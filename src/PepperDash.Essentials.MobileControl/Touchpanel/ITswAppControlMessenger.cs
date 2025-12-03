using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.AppServer.Messengers;

namespace PepperDash.Essentials.Touchpanel
{
    /// <summary>
    /// Messenger for controlling the Zoom App on a TSW Panel that supports the Zoom Room Control Application
    /// </summary>
    public class ITswAppControlMessenger : MessengerBase
    {
        private readonly ITswAppControl _appControl;

        /// <summary>
        /// Create an instance of the <see cref="ITswAppControlMessenger"/> class.
        /// </summary>
        /// <param name="key">The key for this messenger</param>
        /// <param name="messagePath">The message path for this messenger</param>
        /// <param name="device">The device for this messenger</param>
        public ITswAppControlMessenger(string key, string messagePath, Device device) : base(key, messagePath, device)
        {
            _appControl = device as ITswAppControl;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            if (_appControl == null)
            {
                this.LogInformation("{deviceKey} does not implement ITswAppControl", _device.Key);
                return;
            }

            AddAction($"/fullStatus", (id, context) => SendFullStatus(id));

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

        private void SendFullStatus(string id = null)
        {
            var message = new TswAppStateMessage
            {
                AppOpen = _appControl.AppOpenFeedback.BoolValue,
            };

            PostStatusMessage(message, id);
        }
    }

    /// <summary>
    /// Represents a TswAppStateMessage
    /// </summary>
    public class TswAppStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// True if the Zoom app is open on a TSW panel
        /// </summary>
        [JsonProperty("appOpen", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AppOpen { get; set; }
    }
}
