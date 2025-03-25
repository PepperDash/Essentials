using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.Touchpanel
{
    public class ITswAppControlMessenger : MessengerBase
    {
        private readonly ITswAppControl _appControl;

        public ITswAppControlMessenger(string key, string messagePath, Device device) : base(key, messagePath, device)
        {
            _appControl = device as ITswAppControl;
        }

        protected override void RegisterActions()
        {
            if (_appControl == null)
            {
                Debug.Console(0, this, $"{_device.Key} does not implement ITswAppControl");
                return;
            }

            AddAction($"/fullStatus", (id, context) => SendFullStatus());

            AddAction($"/openApp", (id, context) => _appControl.OpenApp());

            AddAction($"/closeApp", (id, context) => _appControl.CloseOpenApp());

            AddAction($"/hideApp", (id, context) => _appControl.HideOpenApp());

            _appControl.AppOpenFeedback.OutputChange += (s, a) =>
            {
                PostStatusMessage(JToken.FromObject(new
                {
                    appOpen = a.BoolValue
                }));
            };
        }

        private void SendFullStatus()
        {
            var message = new TswAppStateMessage
            {
                AppOpen = _appControl.AppOpenFeedback.BoolValue,
            };

            PostStatusMessage(message);
        }
    }

    public class TswAppStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("appOpen", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AppOpen { get; set; }
    }
}
