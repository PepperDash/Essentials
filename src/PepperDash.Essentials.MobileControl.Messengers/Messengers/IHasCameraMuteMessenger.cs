using System;
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
    /// Provides a messaging bridge for devices implementing <see cref="IHasCameraMute"/>
    /// </summary>
    public class IHasCameraMuteMessenger : MessengerBase
    {
        private readonly IHasCameraMute _cameraMute;

        /// <summary>
        /// Initializes a new instance of the <see cref="IHasCameraMuteMessenger"/> class.
        /// </summary>
        /// <param name="key">The key for the messenger.</param>
        /// <param name="messagePath">The message path for the messenger.</param>
        /// <param name="device">The device implementing <see cref="IHasCameraMute"/>.</param>
        public IHasCameraMuteMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _cameraMute = device as IHasCameraMute ?? throw new ArgumentNullException(nameof(device));
            _cameraMute.CameraIsMutedFeedback.OutputChange += CameraIsMutedFeedback_OutputChange;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));
            AddAction("/cameraMuteStatus", (id, content) => SendFullStatus(id));

            AddAction("/cameraMuteOn", (id, content) => _cameraMute.CameraMuteOn());
            AddAction("/cameraMuteOff", (id, content) => _cameraMute.CameraMuteOff());
            AddAction("/cameraMuteToggle", (id, content) => _cameraMute.CameraMuteToggle());
        }

        private void CameraIsMutedFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            try
            {
                PostStatusMessage(new IHasCameraMuteStateMessage
                {
                    CameraIsMuted = e.BoolValue
                });
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting camera mute state");
            }
        }

        private void SendFullStatus(string id = null)
        {
            try
            {
                var state = new IHasCameraMuteStateMessage
                {
                    CameraIsMuted = _cameraMute.CameraIsMutedFeedback.BoolValue
                };

                Task.Run(() => PostStatusMessage(state, id));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending camera mute full status");
            }
        }
    }

    /// <summary>
    /// State message for <see cref="IHasCameraMute"/>
    /// </summary>
    public class IHasCameraMuteStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether the camera is muted. Null if unknown or not applicable.
        /// </summary>
        [JsonProperty("cameraIsMuted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CameraIsMuted { get; set; }
    }
}
