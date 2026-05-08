using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides a messaging bridge for devices implementing <see cref="IHasContentSharing"/>
    /// </summary>
    public class IHasContentSharingMessenger : MessengerBase
    {
        private readonly IHasContentSharing _sharing;

        public IHasContentSharingMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _sharing = device as IHasContentSharing ?? throw new ArgumentNullException(nameof(device));
            _sharing.SharingContentIsOnFeedback.OutputChange += SharingContentIsOnFeedback_OutputChange;
            _sharing.SharingSourceFeedback.OutputChange += SharingSourceFeedback_OutputChange;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));
            AddAction("/sharingStart", (id, content) => _sharing.StartSharing());
            AddAction("/sharingStop", (id, content) => _sharing.StopSharing());
        }

        private void SharingContentIsOnFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            try
            {
                PostStatusMessage(new IHasContentSharingStateMessage
                {
                    SharingContentIsOn = e.BoolValue
                });
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting sharing content is on");
            }
        }

        private void SharingSourceFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            try
            {
                PostStatusMessage(new IHasContentSharingStateMessage
                {
                    SharingSource = e.StringValue
                });
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting sharing source");
            }
        }

        private void SendFullStatus(string id = null)
        {
            try
            {
                var state = new IHasContentSharingStateMessage
                {
                    SharingContentIsOn = _sharing.SharingContentIsOnFeedback.BoolValue,
                    SharingSource = _sharing.SharingSourceFeedback.StringValue
                };

                Task.Run(() => PostStatusMessage(state, id));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending content sharing full status");
            }
        }
    }

    public class IHasContentSharingStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("sharingContentIsOn", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SharingContentIsOn { get; set; }

        [JsonProperty("sharingSource", NullValueHandling = NullValueHandling.Ignore)]
        public string SharingSource { get; set; }
    }
}
