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

                _room.ValidateTechPassword(password);
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

                PostEventMessage(evt, "passwordValidationResult");
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
