using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a ITechPasswordMessenger
    /// </summary>
    public class ITechPasswordMessenger : MessengerBase
    {
        private readonly ITechPassword _room;

        /// <summary>
        /// The client whose /validateTechPassword request is being handled, for the code that request
        /// runs. Rooms raise TechPasswordValidateResult during ValidateTechPassword, so the result can
        /// be sent back to that client alone - a correct PIN must not unlock the tech pages on every
        /// panel in the room.
        /// </summary>
        private static readonly AsyncLocal<string> validatingClientId = new AsyncLocal<string>();

        /// <summary>
        /// How long a /validateTechPassword request waits for its result before it is forgotten.
        /// </summary>
        private static readonly TimeSpan PendingTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Clients waiting for a validation result, oldest first. A room may raise the result outside
        /// the request (from a device callback, after ValidateTechPassword has returned), where
        /// <see cref="validatingClientId"/> is not set; that result goes to the oldest waiting client,
        /// never to every client.
        /// </summary>
        private readonly LinkedList<(string ClientId, DateTime RequestedAt)> pendingValidations =
            new LinkedList<(string ClientId, DateTime RequestedAt)>();

        /// <summary>
        /// Constructor for ITechPasswordMessenger
        /// </summary>
        /// <param name="key"></param>
        /// <param name="messagePath"></param>
        /// <param name="room"></param>
        public ITechPasswordMessenger(string key, string messagePath, ITechPassword room)
            : base(key, messagePath, room as IKeyName)
        {
            _room = room;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));
            AddAction("/techPasswordStatus", (id, content) => SendFullStatus(id));

            AddAction("/validateTechPassword", (id, content) =>
            {
                var password = content.Value<string>("password");

                lock (pendingValidations)
                {
                    pendingValidations.AddLast((id, DateTime.UtcNow));
                }

                var previous = validatingClientId.Value;
                validatingClientId.Value = id;
                try
                {
                    _room.ValidateTechPassword(password);
                }
                finally
                {
                    validatingClientId.Value = previous;
                }
            });

            AddAction("/setTechPassword", (id, content) =>
            {
                var response = content.ToObject<SetTechPasswordContent>();

                _room.SetTechPassword(response.OldPassword, response.NewPassword);
            });

            _room.TechPasswordChanged += (sender, args) =>
            {
                PostEventMessage("passwordChangedSuccessfully");
            };

            _room.TechPasswordValidateResult += (sender, args) =>
            {
                var evt = new ITechPasswordEventMessage
                {
                    IsValid = args.IsValid
                };

                // Only to the client that asked. A result raised outside the request goes to the
                // oldest client still waiting; with none waiting it is dropped, never broadcast, so a
                // correct PIN can't unlock the tech pages on panels that didn't enter it.
                var clientId = TakePendingValidation(validatingClientId.Value);
                if (clientId == null)
                {
                    this.LogWarning("Dropped a tech password validation result: no client is waiting for one");
                    return;
                }

                PostEventMessage(evt, "passwordValidationResult", clientId);
            };
        }

        /// <summary>
        /// Removes and returns the waiting client a validation result belongs to: <paramref name="requester"/>
        /// when the result was raised during its request, otherwise the oldest client still waiting.
        /// Returns null when no client is waiting.
        /// </summary>
        private string TakePendingValidation(string requester)
        {
            lock (pendingValidations)
            {
                var expiredBefore = DateTime.UtcNow - PendingTimeout;
                while (pendingValidations.First != null && pendingValidations.First.Value.RequestedAt < expiredBefore)
                {
                    pendingValidations.RemoveFirst();
                }

                for (var node = pendingValidations.First; node != null; node = node.Next)
                {
                    if (requester == null || node.Value.ClientId == requester)
                    {
                        pendingValidations.Remove(node);
                        return node.Value.ClientId;
                    }
                }

                // Raised during a request that has already been answered or timed out: still that
                // client's result.
                return requester;
            }
        }

        private void SendFullStatus(string id = null)
        {
            var status = new ITechPasswordStateMessage
            {
                TechPasswordLength = _room.TechPasswordLength
            };

            PostStatusMessage(status, id);
        }

    }

    /// <summary>
    /// Represents a ITechPasswordStateMessage
    /// </summary>
    public class ITechPasswordStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the TechPasswordLength
        /// </summary>
        [JsonProperty("techPasswordLength", NullValueHandling = NullValueHandling.Ignore)]
        public int? TechPasswordLength { get; set; }
    }

    /// <summary>
    /// Represents a ITechPasswordEventMessage
    /// </summary>
    public class ITechPasswordEventMessage : DeviceEventMessageBase
    {
        /// <summary>
        /// Gets or sets the IsValid
        /// </summary>
        [JsonProperty("isValid", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsValid { get; set; }
    }

    internal class SetTechPasswordContent
    {
        /// <summary>
        /// Gets or sets the OldPassword
        /// </summary>
        [JsonProperty("oldPassword")]
        public string OldPassword { get; set; }

        /// <summary>
        /// Gets or sets the NewPassword
        /// </summary>
        [JsonProperty("newPassword")]
        public string NewPassword { get; set; }
    }

}