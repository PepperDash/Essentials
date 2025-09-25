using System;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

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

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/humidityStatus", (id, content) => SendFullStatus(id));

            device.HumidityFeedback.OutputChange += new EventHandler<Core.FeedbackEventArgs>((o, a) => SendFullStatus());
        }

        private void SendFullStatus(string id = null)
        {
            var state = new IHumiditySensorStateMessage
            {
                Humidity = string.Format("{0}%", device.HumidityFeedback.UShortValue)
            };

            PostStatusMessage(state, id);
        }
    }

    /// <summary>
    /// Represents a IHumiditySensorStateMessage
    /// </summary>
    public class IHumiditySensorStateMessage : DeviceStateMessageBase
    {

        /// <summary>
        /// Gets or sets the Humidity
        /// </summary>
        [JsonProperty("humidity")]
        public string Humidity { get; set; }
    }
}
