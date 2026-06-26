using System;
using Newtonsoft.Json;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for devices implementing <see cref="IHasCodecLayouts"/>
    /// </summary>
    public class IHasCodecLayoutsMessenger : MessengerBase
    {
        private readonly IHasCodecLayouts _layouts;

        /// <summary>
        /// Initializes a new instance of the <see cref="IHasCodecLayoutsMessenger"/> class.
        /// </summary>
        public IHasCodecLayoutsMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _layouts = device as IHasCodecLayouts ?? throw new ArgumentException("device must implement IHasCodecLayouts", nameof(device));
        
            _layouts.LocalLayoutFeedback.OutputChange += LocalLayoutFeedback_OutputChange;
        }

        private void LocalLayoutFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            SendFullStatus();
        }

        private void SendFullStatus(string id = null)
        {
            PostStatusMessage(new IHasCodecLayoutsStateMessage
            {
                CurrentLayout = _layouts.LocalLayoutFeedback.StringValue
            }, id);
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/layoutStatus", (id, content) => SendFullStatus(id));

            AddAction("/cameraRemoteView", (id, content) => _layouts.LocalLayoutToggle());
            AddAction("/cameraLayout", (id, content) => _layouts.LocalLayoutToggle());
        }
    }

    /// <summary>
    /// State message for <see cref="IHasCodecLayoutsMessenger"/>
    /// </summary>
    public class IHasCodecLayoutsStateMessage : DeviceStateMessageBase
    {
        /// <summary> 
        /// Gets or sets the current layout of the codec
        /// </summary>
        [JsonProperty("currentLayout", NullValueHandling = NullValueHandling.Ignore)]
        public string CurrentLayout { get; set; }
    }
}
