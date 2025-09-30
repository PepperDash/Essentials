using System;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a ITemperatureSensorMessenger
    /// </summary>
    public class ITemperatureSensorMessenger : MessengerBase
    {
        private readonly ITemperatureSensor device;

        public ITemperatureSensorMessenger(string key, ITemperatureSensor device, string messagePath)
            : base(key, messagePath, device as IKeyName)
        {
            this.device = device;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/temperatureStatus", (id, content) => SendFullStatus(id));

            AddAction("/setTemperatureUnitsToCelcius", (id, content) =>
            {
                device.SetTemperatureFormat(true);
            });

            AddAction("/setTemperatureUnitsToFahrenheit", (id, content) =>
            {
                device.SetTemperatureFormat(false);
            });

            device.TemperatureFeedback.OutputChange += new EventHandler<Core.FeedbackEventArgs>((o, a) => SendFullStatus());
            device.TemperatureInCFeedback.OutputChange += new EventHandler<Core.FeedbackEventArgs>((o, a) => SendFullStatus());
        }

        private void SendFullStatus(string id = null)
        {
            // format the temperature to a string with one decimal place
            var tempString = string.Format("{0}.{1}", device.TemperatureFeedback.UShortValue / 10, device.TemperatureFeedback.UShortValue % 10);

            var state = new ITemperatureSensorStateMessage
            {
                Temperature = tempString,
                TemperatureInCelsius = device.TemperatureInCFeedback.BoolValue
            };

            PostStatusMessage(state, id);
        }
    }

    /// <summary>
    /// Represents a ITemperatureSensorStateMessage
    /// </summary>
    public class ITemperatureSensorStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the Temperature
        /// </summary>
        [JsonProperty("temperature")]
        public string Temperature { get; set; }

        /// <summary>
        /// Gets or sets the TemperatureInCelsius
        /// </summary>
        [JsonProperty("temperatureInCelsius")]
        public bool TemperatureInCelsius { get; set; }
    }
}
