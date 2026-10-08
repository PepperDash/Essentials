using System.Threading;
using Newtonsoft.Json;
using PepperDash.Core;
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

                // Only to the client that asked, when known. A room that raises the result later,
                // outside the request, can't be traced back to it, so that falls back to every client.
                PostEventMessage(evt, "passwordValidationResult", validatingClientId.Value);
            };
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