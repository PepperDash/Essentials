using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for devices that implement ITechPassword interface
    /// </summary>
    public class ITechPasswordMessenger : MessengerBase
    {
        private readonly ITechPassword _room;

        /// <summary>
        /// Initializes a new instance of the ITechPasswordMessenger class
        /// </summary>
        /// <param name="key">Unique identifier for the messenger</param>
        /// <param name="messagePath">Path for message routing</param>
        /// <param name="room">Room that implements ITechPassword</param>
        public ITechPasswordMessenger(string key, string messagePath, ITechPassword room)
            : base(key, messagePath, room as IKeyName)
        {
            _room = room;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {

            AddAction("/status", (id, content) =>
            {
                SendFullStatus(id);
            });

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
    /// State message for tech password information
    /// </summary>
    public class ITechPasswordStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the tech password length
        /// </summary>
        [JsonProperty("techPasswordLength", NullValueHandling = NullValueHandling.Ignore)]
        public int? TechPasswordLength { get; set; }
    }

    /// <summary>
    /// Event message for tech password validation result
    /// </summary>
    public class ITechPasswordEventMessage : DeviceEventMessageBase
    {
        /// <summary>
        /// Gets or sets whether the password is valid
        /// </summary>
        [JsonProperty("isValid", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsValid { get; set; }
    }

    /// <summary>
    /// Content for setting tech password
    /// </summary>
    internal class SetTechPasswordContent
    {
        /// <summary>
        /// Gets or sets the old password
        /// </summary>
        [JsonProperty("oldPassword")]
        public string OldPassword { get; set; }

        /// <summary>
        /// Gets or sets the new password
        /// </summary>
        [JsonProperty("newPassword")]
        public string NewPassword { get; set; }
    }

}
