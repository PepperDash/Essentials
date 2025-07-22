using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Presets;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a DevicePresetsModelMessenger
    /// </summary>
    public class DevicePresetsModelMessenger : MessengerBase
    {
        private readonly ITvPresetsProvider _presetsDevice;

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
    /// Represents a PresetChannelMessage
    /// </summary>
    public class PresetChannelMessage
    {
        [JsonProperty("preset")]
        /// <summary>
        /// Gets or sets the Preset
        /// </summary>
        public PresetChannel Preset;

        [JsonProperty("deviceKey")]
        /// <summary>
        /// Gets or sets the DeviceKey
        /// </summary>
        public string DeviceKey;
    }

    /// <summary>
    /// Represents a PresetStateMessage
    /// </summary>
    public class PresetStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("favorites", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the Favorites
        /// </summary>
        public List<PresetChannel> Favorites { get; set; } = new List<PresetChannel>();
    }
}