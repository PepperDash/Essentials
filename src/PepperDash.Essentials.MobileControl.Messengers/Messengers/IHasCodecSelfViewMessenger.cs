using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for devices implementing <see cref="IHasCodecSelfView"/>
    /// </summary>
    public class IHasCodecSelfViewMessenger : MessengerBase
    {
        private readonly IHasCodecSelfView _selfView;

        /// <summary>
        /// Initializes a new instance of the <see cref="IHasCodecSelfViewMessenger"/> class.
        /// </summary>
        public IHasCodecSelfViewMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _selfView = device as IHasCodecSelfView ?? throw new ArgumentException("device must implement IHasCodecSelfView", nameof(device));
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/cameraSelfView", (id, content) => _selfView.SelfViewModeToggle());

            _selfView.SelfviewIsOnFeedback.OutputChange += SelfviewIsOnFeedback_OutputChange;
        }

        private void SelfviewIsOnFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            PostCameraSelfView();
        }

        private void PostCameraSelfView()
        {
            try
            {
                PostStatusMessage(new IHasCodecSelfViewStateMessage
                {
                    CameraSelfViewIsOn = _selfView.SelfviewIsOnFeedback.BoolValue
                });
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting camera self view");
            }
        }
    }

    public class IHasCodecSelfViewStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("cameraSelfView", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CameraSelfViewIsOn { get; set; }
    }
}
