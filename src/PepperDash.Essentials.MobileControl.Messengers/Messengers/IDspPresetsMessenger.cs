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

        /// <summary>
        /// Initializes a new instance of the <see cref="IDspPresetsMessenger"/> class.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="messagePath"></param>
        /// <param name="device"></param>
        public IDspPresetsMessenger(string key, string messagePath, IDspPresets device)
            : base(key, messagePath, device as IKeyName)
        {
            this.device = device;
        }


        /// <inheritdoc />
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
        /// <summary>
        /// Gets or sets the presets.
         /// The key is the preset key, and the value is the preset name
        /// </summary>
        [JsonProperty("presets")]
        public Dictionary<string, IKeyName> Presets { get; set; }
    }
}