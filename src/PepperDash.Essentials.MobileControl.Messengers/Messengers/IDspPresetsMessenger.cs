using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides messaging capabilities for DSP preset management operations.
    /// Handles DSP preset selection, recall, and preset list management.
    /// </summary>
    public class IDspPresetsMessenger : MessengerBase
    {
        private readonly IDspPresets device;

        /// <summary>
        /// Initializes a new instance of the <see cref="IDspPresetsMessenger"/> class.
        /// </summary>
        /// <param name="key">The unique identifier for this messenger instance.</param>
        /// <param name="messagePath">The message path for DSP preset control messages.</param>
        /// <param name="device">The device that provides DSP preset functionality.</param>
        public IDspPresetsMessenger(string key, string messagePath, IDspPresets device)
            : base(key, messagePath, device as IKeyName)
        {
            this.device = device;
        }

        /// <summary>
        /// Registers actions for handling DSP preset operations.
        /// Includes preset selection, recall, and full status reporting.
        /// </summary>
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) =>
            {
                var message = new IHasDspPresetsStateMessage
                {
                    Presets = device.Presets
                };

                PostStatusMessage(message);
            });

            AddAction("/recallPreset", (id, content) =>
            {
                var presetKey = content.ToObject<string>();


                if (!string.IsNullOrEmpty(presetKey))
                {
                    device.RecallPreset(presetKey);
                }
            });
        }
    }

    /// <summary>
    /// Represents a DSP presets state message containing available presets.
    /// </summary>
    public class IHasDspPresetsStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the dictionary of available DSP presets.
        /// </summary>
        [JsonProperty("presets")]
        public Dictionary<string, IKeyName> Presets { get; set; }
    }
}
