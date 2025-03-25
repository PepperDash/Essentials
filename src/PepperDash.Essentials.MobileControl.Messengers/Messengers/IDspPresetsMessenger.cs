using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using System.Collections.Generic;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class IDspPresetsMessenger : MessengerBase
    {
        private IDspPresets _device;

        public IDspPresetsMessenger(string key, string messagePath, IDspPresets device)
            : base(key, messagePath, device as Device)
        {
            _device = device;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) =>
            {
                var message = new IHasDspPresetsStateMessage
                {
                    Presets = _device.Presets
                };

                PostStatusMessage(message);
            });

            AddAction("/recallPreset", (id, content) =>
            {
                var presetKey = content.ToObject<string>();


                if (!string.IsNullOrEmpty(presetKey))
                {
                    _device.RecallPreset(presetKey);
                }
            });
        }
    }

    public class IHasDspPresetsStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("presets")]
        public Dictionary<string, IKeyName> Presets { get; set; }
    }
}
