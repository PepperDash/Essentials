using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Cameras;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides a messaging bridge for devices implementing <see cref="IHasCodecRoomPresets"/>
    /// </summary>
    public class IHasCodecRoomPresetsMessenger : MessengerBase
    {
        private readonly IHasCodecRoomPresets _presets;
        private readonly EssentialsDevice _device;

        public IHasCodecRoomPresetsMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _presets = device as IHasCodecRoomPresets ?? throw new ArgumentNullException(nameof(device));
            _presets.CodecRoomPresetsListHasChanged += Presets_ListHasChanged;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));
        }

        private void Presets_ListHasChanged(object sender, EventArgs e)
        {
            try
            {
                PostStatusMessage(new IHasCodecRoomPresetsStateMessage
                {
                    Presets = GetCurrentPresets()
                });
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting codec room presets");
            }
        }

        private void SendFullStatus(string id = null)
        {
            try
            {
                var state = new IHasCodecRoomPresetsStateMessage
                {
                    Presets = GetCurrentPresets()
                };

                Task.Run(() => PostStatusMessage(state, id));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending room presets full status");
            }
        }

        private List<CodecRoomPreset> GetCurrentPresets()
        {
            if (_device is IHasFarEndCameraControl farEndControl &&
                farEndControl.ControllingFarEndCameraFeedback.BoolValue)
                return _presets.FarEndRoomPresets;

            return _presets.NearEndPresets;
        }
    }

    public class IHasCodecRoomPresetsStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("presets", NullValueHandling = NullValueHandling.Ignore)]
        public List<CodecRoomPreset> Presets { get; set; }
    }
}
