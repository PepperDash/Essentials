using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides a messaging bridge for devices implementing <see cref="IHasMeetingInfo"/>
    /// </summary>
    public class IHasMeetingInfoMessenger : MessengerBase
    {
        private readonly IHasMeetingInfo _meetingInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="IHasMeetingInfoMessenger"/> class.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="messagePath"></param>
        /// <param name="device"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public IHasMeetingInfoMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _meetingInfo = device as IHasMeetingInfo ?? throw new ArgumentNullException(nameof(device));
            _meetingInfo.MeetingInfoChanged += MeetingInfo_Changed;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/meetingInfoStatus", (id, content) => SendFullStatus(id));
        }

        private void MeetingInfo_Changed(object sender, MeetingInfoEventArgs e)
        {
            try
            {
                PostStatusMessage(new IHasMeetingInfoStateMessage
                {
                    MeetingInfo = _meetingInfo.MeetingInfo
                });
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting meeting info");
            }
        }

        private void SendFullStatus(string id = null)
        {
            try
            {
                var state = new IHasMeetingInfoStateMessage
                {
                    MeetingInfo = _meetingInfo.MeetingInfo
                };

                Task.Run(() => PostStatusMessage(state, id));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending meeting info full status");
            }
        }
    }

    /// <summary>
    /// Message class for devices implementing <see cref="IHasMeetingInfo"/>
    /// </summary>
    public class IHasMeetingInfoStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the MeetingInfo
        /// </summary>
        [JsonProperty("meetingInfo", NullValueHandling = NullValueHandling.Ignore)]
        public MeetingInfo MeetingInfo { get; set; }
    }
}
