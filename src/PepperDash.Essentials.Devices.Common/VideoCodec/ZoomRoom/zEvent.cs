extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
    /// <summary>
    /// zEvent Class Structure
    /// </summary>
    public class zEvent
    {
        public class StartLocalPresentMeeting
        {
            public bool Success { get; set; }
        }
        public class NeedWaitForHost
        {
            public bool Wait { get; set; }
        }

        public class IncomingCallIndication
        {
            public string callerJID { get; set; }
            public string calleeJID { get; set; }
            public string meetingID { get; set; }
            public string password { get; set; }
            public string meetingOption { get; set; }
            public long MeetingNumber { get; set; }
            public string callerName { get; set; }
            public string avatarURL { get; set; }
            public int lifeTime { get; set; }
            public bool accepted { get; set; }
        }

        public class CallConnectError
        {
            public int error_code { get; set; }
            public string error_message { get; set; }
        }

        public class CallDisconnect
        {
            public bool Successful
            {
                get
                {
                    return success == "on";
                }
            }

            public string success { get; set; }
        }

        public class Layout
        {
            public bool Sharethumb { get; set; }
        }

        public class Call
        {
            public Layout Layout { get; set; }
        }

        public class Client
        {
            public Call Call { get; set; }
        }

        public enum eSharingState
        {
            None,
            Connecting,
            Sending,
            Receiving,
            Send_Receiving
        }

        public class SharingState : NotifiableObject
        {
            private bool _paused;
            private eSharingState _state;

            public bool IsSharing { get; private set; }

            [JsonProperty("paused")]
            public bool Paused
            {
                get
                {
                    return _paused;
                }
                set
                {
                    if (value != _paused)
                    {
                        _paused = value;
                        NotifyPropertyChanged("Paused");
                    }
                }
            }
            [JsonProperty("state")]
            public eSharingState State
            {
                get
                {
                    return _state;
                }
                set
                {
                    if (value != _state)
                    {
                        _state    = value;
                        IsSharing = _state == eSharingState.Sending;
                        NotifyPropertyChanged("State");
                    }
                }
            }
        }

        public class PinStatusOfScreenNotification
        {


            [JsonProperty("can_be_pinned")]
            public bool CanBePinned { get; set; }
            [JsonProperty("can_pin_share")]
            public bool CanPinShare { get; set; }
            [JsonProperty("pinned_share_source_id")]
            public int PinnedShareSourceId { get; set; }
            [JsonProperty("pinned_user_id")]
            public int PinnedUserId { get; set; }
            [JsonProperty("screen_index")]
            public int ScreenIndex { get; set; }
            [JsonProperty("screen_layout")]
            public int ScreenLayout { get; set; }
            [JsonProperty("share_source_type")]
            public int ShareSourceType { get; set; }
            [JsonProperty("why_cannot_pin_share")]
            public string WhyCannotPinShare { get; set; }
        }

        public class PhoneCallStatus : NotifiableObject
        {
            private bool _isIncomingCall;
            private string _peerDisplayName;
            private string _peerNumber;

            private bool _offHook;

            public string CallId { get; set; }
            public bool IsIncomingCall
            {
                get { return _isIncomingCall; }
                set
                {
                    if (value == _isIncomingCall) return;

                    _isIncomingCall = value;
                    NotifyPropertyChanged("IsIncomingCall");
                }
            }

            public string PeerDisplayName
            {
                get { return _peerDisplayName; }
                set
                {
                    if (value == _peerDisplayName) return;
                    _peerDisplayName = value;
                    NotifyPropertyChanged("PeerDisplayName");
                }
            }

            public string PeerNumber
            {
                get { return _peerNumber; }
                set
                {
                    if (value == _peerNumber) return;

                    _peerNumber = value;
                    NotifyPropertyChanged("PeerNumber");
                }
            }

            public string PeerUri { get; set; }

            private ePhoneCallStatus _status;
            public ePhoneCallStatus Status
            {
                get { return _status; }
                set
                {
                    _status = value;
                    OffHook = _status == ePhoneCallStatus.PhoneCallStatus_Accepted ||
                              _status == ePhoneCallStatus.PhoneCallStatus_InCall ||
                              _status == ePhoneCallStatus.PhoneCallStatus_Init ||
                              _status == ePhoneCallStatus.PhoneCallStatus_Ringing;
                }
            }

            public bool OffHook
            {
                get { return _offHook; }
                set
                {
                    if (value == _offHook) return;

                    _offHook = value;
                    NotifyPropertyChanged("OffHook");
                }
            }
        }

        public enum ePhoneCallStatus
        {
            PhoneCallStatus_Ringing,
            PhoneCallStatus_Terminated,
            PhoneCallStatus_Accepted,
            PhoneCallStatus_InCall,
            PhoneCallStatus_Init,
        }

        public class MeetingNeedsPassword
        {
            [JsonProperty("needsPassword")]
            public bool NeedsPassword { get; set; }

            [JsonProperty("wrongAndRetry")]
            public bool WrongAndRetry { get; set; }
        }
    }
}