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

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/cameraSelfViewStatus", (id, content) => SendFullStatus(id));

            AddAction("/cameraSelfView", (id, content) => _selfView.SelfViewModeToggle());

            _selfView.SelfviewIsOnFeedback.OutputChange += SelfviewIsOnFeedback_OutputChange;
        }

        private void SelfviewIsOnFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            PostCameraSelfView();
        }

        private void SendFullStatus(string id = null)
        {
            PostCameraSelfView(id);
        }

        private void PostCameraSelfView(string id = null)
        {
            try
            {
                PostStatusMessage(new IHasCodecSelfViewStateMessage
                {
                    CameraSelfViewIsOn = _selfView.SelfviewIsOnFeedback.BoolValue,
                    ShowSelfViewByDefault = _selfView.ShowSelfViewByDefault
                }, id);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting camera self view");
            }
        }
    }

    /// <summary>
    /// State message for <see cref="IHasCodecSelfView"/>
    /// </summary>
    public class IHasCodecSelfViewStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether the codec's self view is currently on. Null if unknown or not applicable.
         ///
        /// </summary>
        [JsonProperty("cameraSelfView", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CameraSelfViewIsOn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the codec is set to show self view by default. Null if unknown or not applicable.
         ///
        /// </summary>
        [JsonProperty("showSelfViewByDefault", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowSelfViewByDefault { get; set; }
    }
}
