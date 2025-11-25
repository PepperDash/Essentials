using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.AppServer.Messengers;


namespace PepperDash.Essentials.Touchpanel
{
    /// <summary>
    /// Messenger to handle Zoom status and control for a TSW panel that supports the Zoom Application
    /// </summary>
    public class ITswZoomControlMessenger : MessengerBase
    {
        private readonly ITswZoomControl _zoomControl;

        /// <summary>
        /// Create an instance of the <see cref="ITswZoomControlMessenger"/> class for the given device
        /// </summary>
        /// <param name="key">The key for this messenger</param>
        /// <param name="messagePath">The message path for this messenger</param>
        /// <param name="device">The device for this messenger</param>
        public ITswZoomControlMessenger(string key, string messagePath, Device device) : base(key, messagePath, device)
        {
            _zoomControl = device as ITswZoomControl;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            if (_zoomControl == null)
            {
                this.LogInformation("{deviceKey} does not implement ITswZoomControl", _device.Key);
                return;
            }

            AddAction($"/fullStatus", (id, context) => SendFullStatus(id));

            AddAction($"/zoomStatus", (id, content) => SendFullStatus(id));


            AddAction($"/endCall", (id, context) => _zoomControl.EndZoomCall());

            _zoomControl.ZoomIncomingCallFeedback.OutputChange += (s, a) =>
            {
                PostStatusMessage(JToken.FromObject(new
                {
                    incomingCall = a.BoolValue,
                    inCall = _zoomControl.ZoomInCallFeedback.BoolValue
                }));
            };


            _zoomControl.ZoomInCallFeedback.OutputChange += (s, a) =>
            {
                PostStatusMessage(JToken.FromObject(
                new
                {
                    inCall = a.BoolValue,
                    incomingCall = _zoomControl.ZoomIncomingCallFeedback.BoolValue
                }));
            };
        }

        private void SendFullStatus(string id = null)
        {
            var message = new TswZoomStateMessage
            {
                InCall = _zoomControl?.ZoomInCallFeedback.BoolValue,
                IncomingCall = _zoomControl?.ZoomIncomingCallFeedback.BoolValue
            };

            PostStatusMessage(message, id);
        }
    }

    /// <summary>
    /// Represents a TswZoomStateMessage
    /// </summary>
    public class TswZoomStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// True if the panel is in a Zoom call
        /// </summary>
        [JsonProperty("inCall", NullValueHandling = NullValueHandling.Ignore)]
        public bool? InCall { get; set; }

        /// <summary>
        /// True if there is an incoming Zoom call
        /// </summary>

        [JsonProperty("incomingCall", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IncomingCall { get; set; }
    }
}
