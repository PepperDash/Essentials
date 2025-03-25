using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using System;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class IProjectorScreenLiftControlMessenger: MessengerBase
    {
        private readonly IProjectorScreenLiftControl device;

        public IProjectorScreenLiftControlMessenger(string key, string messagePath, IProjectorScreenLiftControl screenLiftDevice)
            : base(key, messagePath, screenLiftDevice as Device)
        {
            device = screenLiftDevice;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus());

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

        private void SendFullStatus()
        {
            var state = new ScreenLiftStateMessage
            {
                InUpPosition = device.InUpPosition,
                Type = device.Type,
                DisplayDeviceKey = device.DisplayDeviceKey
            };

            PostStatusMessage(state);
        }
    }

    public class ScreenLiftStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("inUpPosition", NullValueHandling = NullValueHandling.Ignore)]
        public bool? InUpPosition { get; set; }

        [JsonProperty("displayDeviceKey", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayDeviceKey { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public eScreenLiftControlType Type { get; set; }
    }
}
