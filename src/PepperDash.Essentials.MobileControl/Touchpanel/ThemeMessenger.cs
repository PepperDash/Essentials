﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.AppServer;
using PepperDash.Essentials.AppServer.Messengers;

namespace PepperDash.Essentials.Touchpanel
{
    /// <summary>
    /// Messenger to save the current theme (light/dark) and send to a device
    /// </summary>
    public class ThemeMessenger : MessengerBase
    {
        private readonly ITheme _tpDevice;

        /// <summary>
        /// Create an instance of the <see cref="ThemeMessenger"/> class
        /// </summary>
        /// <param name="key">The key for this messenger</param>
        /// <param name="path">The path for this messenger</param>
        /// <param name="device">The device for this messenger</param>
        public ThemeMessenger(string key, string path, ITheme device) : base(key, path, device as Device)
        {
            _tpDevice = device;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            AddAction("/fullStatus", (id, content) =>
            {
                PostStatusMessage(new ThemeUpdateMessage { Theme = _tpDevice.Theme });
            });

            AddAction("/saveTheme", (id, content) =>
            {
                var theme = content.ToObject<MobileControlSimpleContent<string>>();

                Debug.LogMessage(Serilog.Events.LogEventLevel.Information, "Setting theme to {theme}", this, theme.Value);
                _tpDevice.UpdateTheme(theme.Value);

                PostStatusMessage(JToken.FromObject(new { theme = theme.Value }));
            });
        }
    }

    /// <summary>
    /// Represents a ThemeUpdateMessage
    /// </summary>
    public class ThemeUpdateMessage : DeviceStateMessageBase
    {

        /// <summary>
        /// Gets or sets the Theme
        /// </summary>
        [JsonProperty("theme")]
        public string Theme { get; set; }
    }
}
