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

#if SERIES4
        protected override void RegisterActions()
#else
        protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
#endif
        {
            AddAction("/presets/fullStatus", (id, content) => {
                this.LogInformation("getting full status for client {id}", id);
                try
                {
                    SendPresets();
                } catch(Exception ex)
                {
                    Debug.LogMessage(ex, "Exception sending preset full status", this);
                }
            });

            AddAction("/presets/recall", (id, content) =>
            {
                var p = content.ToObject<PresetChannelMessage>();


                if (!(DeviceManager.GetDeviceForKey(p.DeviceKey) is ISetTopBoxNumericKeypad dev))
                {
                    this.LogDebug("Unable to find device with key {0}", p.DeviceKey);
                    return;
                }

                RecallPreset(dev, p.Preset.Channel);
            });

            AddAction("/presets/save", (id, content) =>
            {
                var presets = content.ToObject<List<PresetChannel>>();

                SavePresets(presets);
            });

            _presetsDevice.TvPresets.PresetsSaved += (p) => SendPresets();
        }

        #endregion
    }

    public class PresetChannelMessage
    {
        [JsonProperty("preset")]
        public PresetChannel Preset;

        [JsonProperty("deviceKey")]
        public string DeviceKey;
    }

    public class PresetStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("favorites", NullValueHandling = NullValueHandling.Ignore)]
        public List<PresetChannel> Favorites { get; set; } = new List<PresetChannel>();
    }
}