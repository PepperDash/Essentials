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

        /// <summary>
        /// Initializes a new instance of the <see cref="IHumiditySensorMessenger"/> class.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="device"></param>
        /// <param name="messagePath"></param>
        public IHumiditySensorMessenger(string key, IHumiditySensor device, string messagePath)
            : base(key, messagePath, device as IKeyName)
        {
            this.device = device;
        }

        /// <inheritdoc />
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