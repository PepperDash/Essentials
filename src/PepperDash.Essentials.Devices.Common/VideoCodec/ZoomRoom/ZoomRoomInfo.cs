extern alias Full;
using Full::Newtonsoft.Json;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
    /// <summary>
    /// Zoom Room specific info object
    /// </summary>
    public class ZoomRoomInfo : VideoCodecInfo
    {
        public ZoomRoomInfo(ZoomRoomStatus status, ZoomRoomConfiguration configuration)
        {
            Status        = status;
            Configuration = configuration;
        }

        [JsonIgnore]
        public ZoomRoomStatus Status { get; private set; }
        [JsonIgnore]
        public ZoomRoomConfiguration Configuration { get; private set; }

        public override bool AutoAnswerEnabled
        {
            get { return Status.SystemUnit.RoomInfo.AutoAnswerIsEnabled; }
        }

        public override string E164Alias
        {
            get
            {
                if (!string.IsNullOrEmpty(Status.SystemUnit.MeetingNumber))
                {
                    return Status.SystemUnit.MeetingNumber;
                }
                return string.Empty;
            }
        }

        public override string H323Id
        {
            get
            {
                if (!string.IsNullOrEmpty(Status.Call.Info.meeting_list_item.third_party.h323_address))
                {
                    return Status.Call.Info.meeting_list_item.third_party.h323_address;
                }
                return string.Empty;
            }
        }

        public override string IpAddress
        {
            get
            {
                if (!string.IsNullOrEmpty(Status.SystemUnit.RoomInfo.AccountEmail))
                {
                    return Status.SystemUnit.RoomInfo.AccountEmail;
                }
                return string.Empty;
            }
        }

        public override bool MultiSiteOptionIsEnabled
        {
            get { return true; }
        }

        public override string SipPhoneNumber
        {
            get
            {
                if (!string.IsNullOrEmpty(Status.Call.Info.dialIn))
                {
                    return Status.Call.Info.dialIn;
                }
                return string.Empty;
            }
        }

        public override string SipUri
        {
            get
            {
                if (!string.IsNullOrEmpty(Status.Call.Info.meeting_list_item.third_party.sip_address))
                {
                    return Status.Call.Info.meeting_list_item.third_party.sip_address;
                }
                return string.Empty;
            }
        }
    }
}