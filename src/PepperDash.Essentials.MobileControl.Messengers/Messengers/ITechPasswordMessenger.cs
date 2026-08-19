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

        // Captures the id of the client whose /validateTechPassword request is in flight, so the
        // TechPasswordValidateResult handler below can reply to only that client instead of
        // broadcasting to every connected panel. Relies on ValidateTechPassword firing the event
        // synchronously (true for all known ITechPassword implementations); if a future
        // implementation validates asynchronously, _pendingClientId will already be null when the
        // handler runs and this degrades to the previous broadcast behavior.
        private readonly object _pendingLock = new object();
        private string _pendingClientId;

        public ITechPasswordMessenger(string key, string messagePath, ITechPassword room)
            : base(key, messagePath, room as IKeyName)
        {
            _room = room;
        }

        protected override void RegisterActions()
        {

            AddAction("/status", (id, content) => SendFullStatus(id));

            AddAction("/techPasswordStatus", (id, content) => SendFullStatus(id));

            AddAction("/validateTechPassword", (id, content) =>
            {
                var password = content.Value<string>("password");

                lock (_pendingLock)
                {
                    _pendingClientId = id;
                    _room.ValidateTechPassword(password);
                    _pendingClientId = null;
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
                string clientId;
                lock (_pendingLock)
                {
                    clientId = _pendingClientId;
                }

                var evt = new ITechPasswordEventMessage
                {
                    IsValid = args.IsValid
                };

                PostEventMessage(evt, "passwordValidationResult", clientId);
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
        [JsonProperty("techPasswordLength", NullValueHandling = NullValueHandling.Ignore)]
        public int? TechPasswordLength { get; set; }
    }

    /// <summary>
    /// Represents a ITechPasswordEventMessage
    /// </summary>
    public class ITechPasswordEventMessage : DeviceEventMessageBase
    {
        [JsonProperty("isValid", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsValid { get; set; }
    }

    internal class SetTechPasswordContent
    {
        [JsonProperty("oldPassword")]
        /// <summary>
        /// Gets or sets the OldPassword
        /// </summary>
        public string OldPassword { get; set; }

        [JsonProperty("newPassword")]
        /// <summary>
        /// Gets or sets the NewPassword
        /// </summary>
        public string NewPassword { get; set; }
    }

}
