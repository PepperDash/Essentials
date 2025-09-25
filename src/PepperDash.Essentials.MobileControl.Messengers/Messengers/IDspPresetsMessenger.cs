using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a IDspPresetsMessenger
    /// </summary>
    public class IDspPresetsMessenger : MessengerBase
    {
        private readonly IDspPresets device;

        public IDspPresetsMessenger(string key, string messagePath, IDspPresets device)
            : base(key, messagePath, device as IKeyName)
        {
            this.device = device;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/dspPresetStatus", (id, content) => SendFullStatus(id));

            AddAction("/recallPreset", (id, content) =>
            {
                var presetKey = content.ToObject<string>();


                if (!string.IsNullOrEmpty(presetKey))
                {
                    device.RecallPreset(presetKey);
                }
            });
        }

        private void SendFullStatus(string id = null)
        {
            var message = new IHasDspPresetsStateMessage
            {
                Presets = device.Presets
            };

            PostStatusMessage(message, id);
        }
    }

    /// <summary>
    /// Represents a IHasDspPresetsStateMessage
    /// </summary>
    public class IHasDspPresetsStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("presets")]
        public Dictionary<string, IKeyName> Presets { get; set; }
    }
}
