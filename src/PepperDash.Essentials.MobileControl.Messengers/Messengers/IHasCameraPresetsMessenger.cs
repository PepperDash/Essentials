using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Cameras;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides a messaging bridge for devices implementing <see cref="IHasCameraPresets"/>
    /// </summary>
    public class IHasCameraPresetsMessenger : MessengerBase
    {
        private readonly IHasCameraPresets _cameraPresets;

        /// <summary>
        /// Initializes a new instance of the <see cref="IHasCameraPresetsMessenger"/> class.
        /// </summary>
        /// <param name="key">The key for the messenger.</param>
        /// <param name="messagePath">The message path for the messenger.</param>
        /// <param name="device">The device implementing <see cref="IHasCameraPresets"/>.</param>
        public IHasCameraPresetsMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _cameraPresets = device as IHasCameraPresets ?? throw new ArgumentNullException(nameof(device));
            _cameraPresets.PresetsListHasChanged += PresetsListHasChanged;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));
            AddAction("/presetsStatus", (id, content) => SendFullStatus(id));

            AddAction("/recallPreset", (id, content) =>
            {
                var msg = content.ToObject<MobileControlSimpleContent<int>>();
                _cameraPresets.PresetSelect(msg.Value);
            });

            AddAction("/savePreset", (id, content) =>
            {
                var msg = content.ToObject<MobileControlSimpleContent<int>>();
                _cameraPresets.PresetStore(msg.Value, string.Empty);
            });
        }

        private void PresetsListHasChanged(object sender, EventArgs e)
        {
            try
            {
                PostStatusMessage(new IHasCameraPresetsStateMessage
                {
                    Presets = _cameraPresets.Presets
                });
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting camera presets state");
            }
        }

        private void SendFullStatus(string id = null)
        {
            try
            {
                var state = new IHasCameraPresetsStateMessage
                {
                    Presets = _cameraPresets.Presets
                };

                Task.Run(() => PostStatusMessage(state, id));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending camera presets full status");
            }
        }
    }

    /// <summary>
    /// State message for <see cref="IHasCameraPresets"/>
    /// </summary>
    public class IHasCameraPresetsStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the list of camera presets.
        /// </summary>
        [JsonProperty("presets", NullValueHandling = NullValueHandling.Ignore)]
        public List<CameraPreset> Presets { get; set; }
    }
}
