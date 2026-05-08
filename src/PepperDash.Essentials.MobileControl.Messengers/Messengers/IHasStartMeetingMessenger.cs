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
    /// Provides a messaging bridge for devices implementing <see cref="IHasStartMeeting"/>
    /// </summary>
    public class IHasStartMeetingMessenger : MessengerBase
    {
        private readonly IHasStartMeeting _startMeeting;

        /// <summary>
        /// Initializes a new instance of the <see cref="IHasStartMeetingMessenger"/> class.
        /// </summary>
        /// <param name="key">The key for the messenger.</param>
        /// <param name="messagePath">The message path for the messenger.</param>
        /// <param name="device">The device implementing <see cref="IHasStartMeeting"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if the device does not implement <see cref="IHasStartMeeting"/>.</exception>
        public IHasStartMeetingMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _startMeeting = device as IHasStartMeeting ?? throw new ArgumentNullException(nameof(device));
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/startMeetingStatus", (id, content) => SendFullStatus(id));

            AddAction("/startMeeting", (id, content) =>
            {
                var msg = content.ToObject<MobileControlSimpleContent<uint>>();
                _startMeeting.StartMeeting(msg?.Value ?? _startMeeting.DefaultMeetingDurationMin);
            });

            AddAction("/leaveMeeting", (id, content) => _startMeeting.LeaveMeeting());
        }

        private void SendFullStatus(string id = null)
        {
            try
            {
                var state = new IHasStartMeetingStateMessage
                {
                    DefaultMeetingDurationMin = _startMeeting.DefaultMeetingDurationMin
                };

                Task.Run(() => PostStatusMessage(state, id));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending start meeting full status");
            }
        }
    }

    /// <summary>
    /// State message for devices implementing <see cref="IHasStartMeeting"/>
    /// </summary>
    public class IHasStartMeetingStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Indicates whether the device supports ad-hoc meetings (meetings started from the device rather than an external calendar invite)
         ///
        /// </summary>
        [JsonProperty("supportsAdHocMeeting", NullValueHandling = NullValueHandling.Ignore)]
        public bool SupportsAdHocMeeting { get; set; } = true;

        /// <summary>
        /// The default meeting duration in minutes for meetings started from the device. Null if unknown or not applicable.
        /// </summary>
        [JsonProperty("defaultMeetingDurationMin", NullValueHandling = NullValueHandling.Ignore)]
        public uint DefaultMeetingDurationMin { get; set; }
    }
}
