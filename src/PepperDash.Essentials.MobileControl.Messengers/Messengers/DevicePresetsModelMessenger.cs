using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Presets;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides messaging capabilities for device preset management operations.
    /// Handles preset selection, recall, and preset list management.
    /// </summary>
    public class DevicePresetsModelMessenger : MessengerBase
    {
        private readonly ITvPresetsProvider _presetsDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevicePresetsModelMessenger"/> class.
        /// </summary>
        /// <param name="key">The unique identifier for this messenger instance.</param>
        /// <param name="messagePath">The message path for preset control messages.</param>
        /// <param name="presetsDevice">The device that provides preset functionality.</param>
        public DevicePresetsModelMessenger(string key, string messagePath, ITvPresetsProvider presetsDevice)
            : base(key, messagePath, presetsDevice as Device)
        {
            _presetsDevice = presetsDevice;
        }

        private void SendPresets()
        {
            PostStatusMessage(new PresetStateMessage
            {
                Favorites = _presetsDevice.TvPresets.PresetsList
            });
        }

        private void RecallPreset(ISetTopBoxNumericKeypad device, string channel)
        {
            _presetsDevice.TvPresets.Dial(channel, device);
        }

        private void SavePresets(List<PresetChannel> presets)
        {
            _presetsDevice.TvPresets.UpdatePresets(presets);
        }


        #region Overrides of MessengerBase

        /// <summary>
        /// Registers actions for handling device preset operations.
        /// Includes preset selection, recall, and full status reporting.
        /// </summary>
        protected override void RegisterActions()

        {
            AddAction("/fullStatus", (id, content) =>
            {
                this.LogInformation("getting full status for client {id}", id);
                try
                {
                    SendPresets();
                }
                catch (Exception ex)
                {
                    Debug.LogMessage(ex, "Exception sending preset full status", this);
                }
            });

            AddAction("/recall", (id, content) =>
            {
                var p = content.ToObject<PresetChannelMessage>();


                if (!(DeviceManager.GetDeviceForKey(p.DeviceKey) is ISetTopBoxNumericKeypad dev))
                {
                    this.LogDebug("Unable to find device with key {0}", p.DeviceKey);
                    return;
                }

                RecallPreset(dev, p.Preset.Channel);
            });

            AddAction("/save", (id, content) =>
            {
                var presets = content.ToObject<List<PresetChannel>>();

                SavePresets(presets);
            });

            _presetsDevice.TvPresets.PresetsSaved += (p) => SendPresets();
        }

        #endregion
    }

    /// <summary>
    /// Represents a preset channel message for device preset operations.
    /// </summary>
    public class PresetChannelMessage
    {
        /// <summary>
        /// Gets or sets the preset channel information.
        /// </summary>
        [JsonProperty("preset")]
        public PresetChannel Preset;

        /// <summary>
        /// Gets or sets the device key associated with the preset.
        /// </summary>
        [JsonProperty("deviceKey")]
        public string DeviceKey;
    }

    /// <summary>
    /// Represents a preset state message containing favorite presets.
    /// </summary>
    public class PresetStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the list of favorite preset channels.
        /// </summary>
        [JsonProperty("favorites", NullValueHandling = NullValueHandling.Ignore)]
        public List<PresetChannel> Favorites { get; set; } = new List<PresetChannel>();
    }
}