using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a IBasicVideoMuteWithFeedbackMessenger
    /// </summary>
    public class IBasicVideoMuteWithFeedbackMessenger : MessengerBase
    {
        private readonly IBasicVideoMuteWithFeedback device;

        /// <summary>
        /// Initializes a new instance of the <see cref="IBasicVideoMuteWithFeedbackMessenger"/> class.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="messagePath"></param>
        /// <param name="device"></param>
        public IBasicVideoMuteWithFeedbackMessenger(string key, string messagePath, IBasicVideoMuteWithFeedback device)
            : base(key, messagePath, device as IKeyName)
        {
            this.device = device;
        }

        /// <summary>
        /// SendFullStatus method
        /// </summary>
        public void SendFullStatus(string id = null)
        {
            var messageObj = new IBasicVideoMuteWithFeedbackMessage
            {
                VideoMuteState = device.VideoMuteIsOn.BoolValue
            };

            PostStatusMessage(messageObj, id);
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/videoMuteStatus", (id, content) => SendFullStatus(id));

            AddAction("/videoMuteToggle", (id, content) =>
            {
                device.VideoMuteToggle();
            });

            AddAction("/videoMuteOn", (id, content) =>
            {
                device.VideoMuteOn();
            });

            AddAction("/videoMuteOff", (id, content) =>
            {
                device.VideoMuteOff();
            });

            device.VideoMuteIsOn.OutputChange += VideoMuteIsOnFeedback_OutputChange;
        }

        private void VideoMuteIsOnFeedback_OutputChange(object sender, FeedbackEventArgs args)
        {
            PostStatusMessage(JToken.FromObject(new
            {
                videoMuteState = args.BoolValue
            })
            );
        }
    }

    /// <summary>
    /// Represents a IBasicVideoMuteWithFeedbackMessage
    /// </summary>
    public class IBasicVideoMuteWithFeedbackMessage : DeviceStateMessageBase
    {
        [JsonProperty("videoMuteState")]
        public bool VideoMuteState { get; set; }
    }
}
