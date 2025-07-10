using System;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for temperature sensor devices
    /// </summary>
    public class ITemperatureSensorMessenger : MessengerBase
    {
        private readonly ITemperatureSensor device;

        /// <summary>
        /// Initializes a new instance of the ITemperatureSensorMessenger class
        /// This messenger provides a mobile control interface for temperature sensor devices.
        /// It allows clients to retrieve the current temperature and change the temperature format between Celsius and Fahrenheit.
        /// </summary>
        /// <param name="key">Unique identifier for the messenger</param>
        /// <param name="device">Device that implements ITemperatureSensor</param>
        /// <param name="messagePath">Path for message routing</param>
        public ITemperatureSensorMessenger(string key, ITemperatureSensor device, string messagePath)
            : base(key, messagePath, device as IKeyName)
        {
            this.device = device;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

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
    /// State message for temperature sensor devices
    /// </summary>
    public class ITemperatureSensorStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the current temperature reading from the sensor.
        /// The temperature is represented as a string formatted to one decimal place.
        /// For example, "22.5" for 22.5 degrees.
        /// </summary>
        [JsonProperty("temperature")]
        public string Temperature { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the temperature is in Celsius.
        /// This property is true if the temperature is in Celsius, and false if it is in Fahrenheit.
        /// </summary>
        [JsonProperty("temperatureInCelsius")]
        public bool TemperatureInCelsius { get; set; }
    }
}
