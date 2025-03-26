using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
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

            AddAction("/status", (id, content) =>
            {
                SendFullStatus();
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

        private void SendFullStatus()
        {
            var status = new ITechPasswordStateMessage
            {
                TechPasswordLength = _room.TechPasswordLength
            };

            PostStatusMessage(status);
        }

    }

    public class ITechPasswordStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("techPasswordLength", NullValueHandling = NullValueHandling.Ignore)]
        public int? TechPasswordLength { get; set; }
    }

    public class ITechPasswordEventMessage : DeviceEventMessageBase
    {
        [JsonProperty("isValid", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsValid { get; set; }
    }

    internal class SetTechPasswordContent
    {
        [JsonProperty("oldPassword")]
        public string OldPassword { get; set; }

        [JsonProperty("newPassword")]
        public string NewPassword { get; set; }
    }

}
