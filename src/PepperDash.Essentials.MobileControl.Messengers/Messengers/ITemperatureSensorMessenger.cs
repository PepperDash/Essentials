using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using System;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class ITemperatureSensorMessenger : MessengerBase
    {
        private readonly ITemperatureSensor device;

        public ITemperatureSensorMessenger(string key, ITemperatureSensor device, string messagePath)
            : base(key, messagePath, device as Device)
        {
            this.device = device;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus());

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

        private void SendFullStatus()
        {
            // format the temperature to a string with one decimal place
            var tempString = string.Format("{0}.{1}", device.TemperatureFeedback.UShortValue / 10, device.TemperatureFeedback.UShortValue % 10);

            var state = new ITemperatureSensorStateMessage
            {
                Temperature = tempString,
                TemperatureInCelsius = device.TemperatureInCFeedback.BoolValue
            };

            PostStatusMessage(state);
        }
    }

    public class ITemperatureSensorStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("temperature")]
        public string Temperature { get; set; }

        [JsonProperty("temperatureInCelsius")]
        public bool TemperatureInCelsius { get; set; }
    }
}
