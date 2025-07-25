using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.AppServer.Messengers;


namespace PepperDash.Essentials.Touchpanel
{
    /// <summary>
    /// Represents a ITswZoomControlMessenger
    /// </summary>
    public class ITswZoomControlMessenger : MessengerBase
    {
        private readonly ITswZoomControl _zoomControl;

        public ITswZoomControlMessenger(string key, string messagePath, Device device) : base(key, messagePath, device)
        {
            _zoomControl = device as ITswZoomControl;
        }

        protected override void RegisterActions()
        {
            if (_zoomControl == null)
            {
                this.LogInformation("{deviceKey} does not implement ITswZoomControl", _device.Key);
                return;
            }

            AddAction($"/fullStatus", (id, context) => SendFullStatus());


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

        private void SendFullStatus()
        {
            var message = new TswZoomStateMessage
            {
                InCall = _zoomControl?.ZoomInCallFeedback.BoolValue,
                IncomingCall = _zoomControl?.ZoomIncomingCallFeedback.BoolValue
            };

            PostStatusMessage(message);
        }
    }

    /// <summary>
    /// Represents a TswZoomStateMessage
    /// </summary>
    public class TswZoomStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("inCall", NullValueHandling = NullValueHandling.Ignore)]
        public bool? InCall { get; set; }

        [JsonProperty("incomingCall", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IncomingCall { get; set; }
    }
}
