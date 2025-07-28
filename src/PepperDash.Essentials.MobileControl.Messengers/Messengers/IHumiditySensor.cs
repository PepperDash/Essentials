using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Feedbacks;
using System;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a IHumiditySensorMessenger
    /// </summary>
    public class IHumiditySensorMessenger : MessengerBase
    {
        private readonly IHumiditySensor device;

        public IHumiditySensorMessenger(string key, IHumiditySensor device, string messagePath)
            : base(key, messagePath, device as IKeyName)
        {
            this.device = device;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus());

            device.HumidityFeedback.OutputChange += new EventHandler<FeedbackEventArgs>((o, a) => SendFullStatus());
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

    /// <summary>
    /// Represents a IHumiditySensorStateMessage
    /// </summary>
    public class IHumiditySensorStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("humidity")]
        /// <summary>
        /// Gets or sets the Humidity
        /// </summary>
        public string Humidity { get; set; }
    }
}
