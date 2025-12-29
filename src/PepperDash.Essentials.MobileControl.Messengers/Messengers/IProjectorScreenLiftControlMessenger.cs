using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a IProjectorScreenLiftControlMessenger
    /// </summary>
    public class IProjectorScreenLiftControlMessenger : MessengerBase
    {
        private readonly IProjectorScreenLiftControl device;

        /// <summary>
        /// Initializes a new instance of the <see cref="IProjectorScreenLiftControlMessenger"/> class.
        /// </summary>
        /// <param name="key">message key</param>
        /// <param name="messagePath">message path</param>
        /// <param name="screenLiftDevice">screen lift device</param>
        public IProjectorScreenLiftControlMessenger(string key, string messagePath, IProjectorScreenLiftControl screenLiftDevice)
            : base(key, messagePath, screenLiftDevice as IKeyName)
        {
            device = screenLiftDevice;
        }

        /// <summary>
        /// Registers the actions for the messenger.
        /// </summary>
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/screenliftStatus", (id, content) => SendFullStatus(id));

            AddAction("/raise", (id, content) =>
            {

                device.Raise();

            });

            AddAction("/lower", (id, content) =>
            {

                device.Lower();

            });

            device.PositionChanged += Device_PositionChanged;

        }

        private void Device_PositionChanged(object sender, EventArgs e)
        {
            var state = new
            {
                inUpPosition = device.InUpPosition
            };
            PostStatusMessage(JToken.FromObject(state));
        }

        private void SendFullStatus(string id = null)
        {
            var state = new ScreenLiftStateMessage
            {
                InUpPosition = device.InUpPosition,
                Type = device.Type,
                DisplayDeviceKey = device.DisplayDeviceKey
            };

            PostStatusMessage(state, id);
        }
    }

    /// <summary>
    /// Represents a ScreenLiftStateMessage
    /// </summary>
    public class ScreenLiftStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the InUpPosition
        /// </summary>
        [JsonProperty("isInUpPosition", NullValueHandling = NullValueHandling.Ignore)]
        public bool? InUpPosition { get; set; }

        /// <summary>
        /// Gets or sets the DisplayDeviceKey
        /// </summary>
        [JsonProperty("displayDeviceKey", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayDeviceKey { get; set; }

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public eScreenLiftControlType Type { get; set; }
    }
}
