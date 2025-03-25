using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.AppServer;
using PepperDash.Essentials.AppServer.Messengers;

namespace PepperDash.Essentials.Touchpanel
{
    public class ThemeMessenger : MessengerBase
    {
        private readonly ITheme _tpDevice;

        public ThemeMessenger(string key, string path, ITheme device) : base(key, path, device as Device)
        {
            _tpDevice = device;
        }

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

                PostStatusMessage(JToken.FromObject(new {theme =  theme.Value}));
            });
        }
    }

    public class ThemeUpdateMessage:DeviceStateMessageBase
    {
        [JsonProperty("theme")]
        public string Theme { get; set; }
    }
}
