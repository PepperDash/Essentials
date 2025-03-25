using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using System;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class IHumiditySensorMessenger : MessengerBase
    {
        private readonly IHumiditySensor device;

        public IHumiditySensorMessenger(string key, IHumiditySensor device, string messagePath)
            : base(key, messagePath, device as Device)
        {
            this.device = device;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus());

            device.HumidityFeedback.OutputChange += new EventHandler<Core.FeedbackEventArgs>((o, a) => SendFullStatus());
        }

        private void SendFullStatus()
        {
            var state = new IHumiditySensorStateMessage
            {
                Humidity = string.Format("{0}%", device.HumidityFeedback.UShortValue)
            };

            PostStatusMessage(state);
        }
    }

    public class IHumiditySensorStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("humidity")]
        public string Humidity { get; set; }
    }
}
