extern alias Full;
using System;
using System.Collections.Generic;
using Full::Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
    /// <summary>
    /// zStatus class stucture
    /// </summary>
    public class zStatus
    {
        public class Login
        {
            [JsonProperty("ZAAPI Release")]
            public string ZAAPIRelease { get; set; }
            [JsonProperty("Zoom Room Release")]
            public string ZoomRoomRelease { get; set; }
        }

        public class SystemUnit
        {
            [JsonProperty("email")]
            public string Email { get; set; }
            [JsonProperty("login_type")]
            public string LoginType { get; set; }
            [JsonProperty("meeting_number")]
            public string MeetingNumber { get; set; }
            [JsonProperty("platform")]
            public string Platform { get; set; }
            [JsonProperty("room_info")]
            public RoomInfo RoomInfo { get; set; }
            [JsonProperty("room_version")]
            public string RoomVersion { get; set; }

            public SystemUnit()
            {
                RoomInfo = new RoomInfo();
            }
        }

        public class RoomInfo
        {
            [JsonProperty("account_email")]
            public string AccountEmail { get; set; }
            [JsonProperty("display_version")]
            public string DisplayVersion { get; set; }
            [JsonProperty("is_auto_answer_enabled")]
            public bool AutoAnswerIsEnabled { get; set; }
            [JsonProperty("is_auto_answer_selected")]
            public bool AutoAnswerIsSelected { get; set; }
            [JsonProperty("room_name")]
            public string RoomName { get; set; }
        }

        public class CloudPbxInfo
        {
            [JsonProperty("company_number")]
            public string CompanyNumber { get; set; }
            [JsonProperty("extension")]
            public string Extension { get; set; }
            [JsonProperty("isValid")]
            public bool IsValid { get; set; }
        }

        public enum ePresence
        {
            PRESENCE_OFFLINE,
            PRESENCE_ONLINE,
            PRESENCE_AWAY,
            PRESENCE_BUSY,
            PRESENCE_DND
        }

        public class Contact
        {
            [JsonProperty("avatarURL")]
            public string AvatarURL { get; set; }
            [JsonProperty("cloud_pbx_info")]
            public CloudPbxInfo CloudPbxInfo { get; set; }
            [JsonProperty("email")]
            public string Email { get; set; }
            [JsonProperty("firstName")]
            public string FirstName { get; set; }
            [JsonProperty("index")]
            public int Index { get; set; }
            [JsonProperty("isLegacy")]
            public bool IsLegacy { get; set; }
            [JsonProperty("isZoomRoom")]
            public bool IsZoomRoom { get; set; }
            [JsonProperty("jid")]
            public string Jid { get; set; }
            [JsonProperty("lastName")]
            public string LastName { get; set; }
            [JsonProperty("onDesktop")]
            public bool OnDesktop { get; set; }
            [JsonProperty("onMobile")]
            public bool OnMobile { get; set; }
            [JsonProperty("phoneNumber")]
            public string PhoneNumber { get; set; }
            [JsonProperty("presence")]
            public ePresence Presence { get; set; }
            [JsonProperty("presence_status")]
            public int PresenceStatus { get; set; }
            [JsonProperty("screenName")]
            public string ScreenName { get; set; }
            [JsonProperty("sip_phone_number")]
            public string SipPhoneNumber { get; set; }


            public Contact()
            {
                CloudPbxInfo = new CloudPbxInfo();
            }
        }

        public class Phonebook
        {
            [JsonProperty("Contacts")]
            public List<Contact> Contacts { get; set; }

            public Phonebook()
            {
                Contacts = new List<Contact>();
            }

            /// <summary>
            /// Converts from zStatus.Contact types to generic directory items
            /// </summary>
            /// <returns></returns>
            public static CodecDirectory ConvertZoomContactsToGeneric(List<Contact> zoomContacts)
            {
                var directory = new CodecDirectory();

                var folders = new List<DirectoryItem>();

                var roomFolder = new DirectoryFolder();

                var contactFolder = new DirectoryFolder();

                var contacts = new List<DirectoryItem>();

                // Check if there are any zoom rooms
                var zoomRooms = zoomContacts.FindAll(c => c.IsZoomRoom);

                if (zoomRooms.Count > 0)
                {
                    // If so, setup a rooms and contacts folder and add them.

                    directory.ResultsFolderId = "root";

                    roomFolder.Name           = "Rooms";
                    roomFolder.ParentFolderId = "root";
                    roomFolder.FolderId       = "rooms";

                    contactFolder.Name           = "Contacts";
                    contactFolder.ParentFolderId = "root";
                    contactFolder.FolderId       = "contacts";

                    folders.Add(roomFolder);
                    folders.Add(contactFolder);

                    directory.AddFoldersToDirectory(folders);
                }

                try
                {
                    if (zoomContacts.Count == 0)
                    {
                        return directory;
                    }
					
                    foreach (Contact c in zoomContacts)
                    {
                        var contact = new InvitableDirectoryContact { Name = c.ScreenName, ContactId = c.Jid };

                        contact.ContactMethods.Add(new ContactMethod()
                        {
                            Number          = c.Jid, 
                            Device          = eContactMethodDevice.Video, 
                            CallType        = eContactMethodCallType.Video, 
                            ContactMethodId = c.Jid
                        });

                        if (folders.Count > 0)
                        {
                            contact.ParentFolderId = c.IsZoomRoom
                                ? roomFolder.FolderId // "rooms" 
                                : contactFolder.FolderId; // "contacts"
                        }

                        contacts.Add(contact);
                    }

                    directory.AddContactsToDirectory(contacts);
                }
                catch (Exception e)
                {
                    Debug.Console(1, "Error converting Zoom Phonebook results to generic: {0}", e);
                }

                return directory;
            }
        }

        public enum eCallStatus
        {
            UNKNOWN,
            NOT_IN_MEETING,
            CONNECTING_MEETING,
            IN_MEETING,
            LOGGED_OUT
        }

        public class ClosedCaption
        {
            public bool Available { get; set; }
        }

        public class Call : NotifiableObject
        {
            private eCallStatus _status;
            private List<zCommand.ListParticipant> _participants;

            public bool IsInCall;

            public eCallStatus Status
            {
                get
                {
                    return _status;
                }
                set
                {
                    if (value != _status)
                    {
                        _status  = value;
                        IsInCall = _status == eCallStatus.IN_MEETING || _status == eCallStatus.CONNECTING_MEETING;
                        NotifyPropertyChanged("Status");
                    }
                }
            }
            public ClosedCaption ClosedCaption { get; set; }
            public List<zCommand.ListParticipant> Participants
            {
                get
                {
                    return _participants;
                }
                set
                {
                    _participants = value;
                    NotifyPropertyChanged("Participants");
                }
            }
            public zEvent.SharingState Sharing { get; set; }

            public CallRecordInfo CallRecordInfo { get; set; }

            private zCommand.InfoResult _info;

            public zCommand.InfoResult Info
            {
                get
                {
                    return _info;
                }
                set
                {
                    _info = value;
                    NotifyPropertyChanged("Info");
                }
            }

            public Call()
            {
                ClosedCaption  = new ClosedCaption();
                Participants   = new List<zCommand.ListParticipant>();
                Sharing        = new zEvent.SharingState();
                CallRecordInfo = new CallRecordInfo();
                Info           = new zCommand.InfoResult();
            }
        }

        public class Capabilities
        {
            public bool aec_Setting_Stored_In_ZR { get; set; }
            public bool can_Dtmf_For_Invite_By_Phone { get; set; }
            public bool can_Mute_On_Entry { get; set; }
            public bool can_Ringing_In_Pstn_Call { get; set; }
            public bool can_Switch_To_Specific_Camera { get; set; }
            public bool is_Airhost_Disabled { get; set; }
            public bool pstn_Call_In_Local_resentation { get; set; }
            public bool support_Claim_Host { get; set; }
            public bool support_Out_Room_Display { get; set; }
            public bool support_Pin_And_Spotlight { get; set; }
            public bool supports_Audio_Checkup { get; set; }
            public bool supports_CheckIn { get; set; }
            public bool supports_Cloud_PBX { get; set; }
            public bool supports_Encrypted_Connection { get; set; }
            public bool supports_Expel_User_Permanently { get; set; }
            public bool supports_H323_DTMF { get; set; }
            public bool supports_Hdmi_Cec_Control { get; set; }
            public bool supports_Highly_Reverberant_Room { get; set; }
            public bool supports_Loading_Contacts_Dynamically { get; set; }
            public bool supports_Loading_Participants_Dynamically { get; set; }
            public bool supports_Mic_Record_Test { get; set; }
            public bool supports_Multi_Share { get; set; }
            public bool supports_ShareCamera { get; set; }
            public bool supports_Share_For_Floating_And_Content_Only { get; set; }
            public bool supports_Sip_Call_out { get; set; }
            public bool supports_Software_Audio_Processing { get; set; }
            public bool supports_Web_Settings_Push { get; set; }
        }

        public enum eDisplayState
        {
            None,
            Laptop,
            IOS,
        }

        public class Sharing : NotifiableObject
        {
            private eDisplayState _dispState;
            private string _password;
            private bool _isAirHostClientConnected;
            private bool _isSharingBlackMagic;
            private bool _isDirectPresentationConnected;
            private bool _isBlackMagicConnected;


            public string directPresentationPairingCode { get; set; }
            /// <summary>
            /// Laptop client sharing key
            /// </summary>
            public string directPresentationSharingKey { get; set; }
            public eDisplayState dispState
            {
                get
                {
                    return _dispState;
                }
                set
                {
                    if (value != _dispState)
                    {
                        _dispState = value;
                        NotifyPropertyChanged("dispState");
                    }
                }
            }

            public bool isAirHostClientConnected 
            {
                get { return _isAirHostClientConnected; }
                set
                {
                    if (value != _isAirHostClientConnected)
                    {
                        _isAirHostClientConnected = value;
                        NotifyPropertyChanged("isAirHostClientConnected");
                    }
                }
            }

            public bool isBlackMagicConnected
            {
                get { return _isBlackMagicConnected; }
                set
                {
                    if (value != _isBlackMagicConnected)
                    {
                        _isBlackMagicConnected = value;
                        NotifyPropertyChanged("isBlackMagicConnected");
                    }
                }
            }
            public bool isBlackMagicDataAvailable { get; set; }

            public bool isDirectPresentationConnected
            {
                get { return _isDirectPresentationConnected; }
                set
                {
                    if (value != _isDirectPresentationConnected)
                    {
                        _isDirectPresentationConnected = value;
                        NotifyPropertyChanged("isDirectPresentationConnected");
                    }
                }
            }

            public bool isSharingBlackMagic
            {
                get { return _isSharingBlackMagic; }
                set
                {
                    if (value != _isSharingBlackMagic)
                    {
                        _isSharingBlackMagic = value;
                        NotifyPropertyChanged("isSharingBlackMagic");
                    }
                }
            }

            /// <summary>
            /// IOS Airplay code
            /// </summary>
            public string password
            {
                get
                {
                    return _password;
                }
                set
                {
                    if (value != _password)
                    {
                        _password = value;
                        NotifyPropertyChanged("password");
                    }
                }
            }
            public string serverName { get; set; }
            public string wifiName { get; set; }
        }

        public class NumberOfScreens : NotifiableObject
        {
            private int _numOfScreens;

            [JsonProperty("NumberOfCECScreens")]
            public int NumOfCECScreens { get; set; }
            [JsonProperty("NumberOfScreens")]
            public int NumOfScreens
            {
                get
                {
                    return _numOfScreens;
                }
                set
                {
                    if (value != _numOfScreens)
                    {
                        _numOfScreens = value;
                        NotifyPropertyChanged("NumberOfScreens");
                    }
                }
            }
        }

        /// <summary>
        /// AudioInputLine/AudioOutputLine/VideoCameraLine list item
        /// </summary>
        public class AudioVideoInputOutputLineItem
        {
            public string Alias { get; set; }
            public string Name { get; set; }
            public bool Selected { get; set; }
            public bool combinedDevice { get; set; }
            public string id { get; set; }
            public bool manuallySelected { get; set; }
            public int numberOfCombinedDevices { get; set; }
            public int ptzComId { get; set; }
        }

        public class Video
        {
            public bool Optimizable { get; set; }
        }

        public class CameraShare : NotifiableObject
        {
            private bool _canControlCamera;
            private bool _isSharing;

            [JsonProperty("can_Control_Camera")]
            public bool CanControlCamera
            {
                get
                {
                    return _canControlCamera;
                }
                set
                {
                    if (value != _canControlCamera)
                    {
                        _canControlCamera = value;
                        NotifyPropertyChanged("CanControlCamera");
                    }
                }
            }
            public string id { get; set; }
            public bool is_Mirrored { get; set; }
            [JsonProperty("is_Sharing")]
            public bool IsSharing
            {
                get
                {
                    return _isSharing;
                }
                set
                {
                    if (value != _isSharing)
                    {
                        _isSharing = value;
                        NotifyPropertyChanged("IsSharing");
                    }
                }
            }
            public int pan_Tilt_Speed { get; set; }

        }

        public class Layout : NotifiableObject
        {
            // backer variables
            private bool _can_Switch_Speaker_View;
            private bool _can_Switch_Wall_View;
            private bool _can_Switch_Strip_View;
            private bool _can_Switch_Share_On_All_Screens;
            private bool _can_Switch_Floating_Share_Content;
            private bool _is_In_First_Page;
            private bool _is_In_Last_Page;
            private string _video_type;


            public bool can_Adjust_Floating_Video { get; set; }


            public bool can_Switch_Floating_Share_Content
            {
                get
                {
                    return _can_Switch_Floating_Share_Content;
                }
                set
                {
                    if (value != _can_Switch_Floating_Share_Content)
                    {
                        _can_Switch_Floating_Share_Content = value;
                        NotifyPropertyChanged("can_Switch_Floating_Share_Content");
                    }
                }
            }


            /// <summary>
            /// [on/off] // Set to On if it is possible to invoke zConfiguration Call Layout Style: ShareAll, to switch to the ShareAll mode, where the content sharing is shown full screen on all monitors.
            /// </summary>
            [JsonProperty("can_Switch_Share_On_All_Screens")]
            public bool can_Switch_Share_On_All_Screens
            {
                get
                {
                    return _can_Switch_Share_On_All_Screens;
                }
                set
                {
                    if (value != _can_Switch_Share_On_All_Screens)
                    {
                        _can_Switch_Share_On_All_Screens = value;
                        NotifyPropertyChanged("can_Switch_Share_On_All_Screens");
                    }
                }
            }

            /// <summary>
            /// [on/off] // Set to On if it is possible to switch to Speaker view by invoking zConfiguration Call Layout Style: Speaker. The active speaker is shown full screen, and other video streams, like self-view, are shown in thumbnails.
            /// </summary>
            [JsonProperty("can_Switch_Speaker_View")]
            public bool can_Switch_Speaker_View
            {
                get
                {
                    return _can_Switch_Speaker_View;
                }
                set
                {
                    if (value != _can_Switch_Speaker_View)
                    {
                        _can_Switch_Speaker_View = value;
                        NotifyPropertyChanged("can_Switch_Speaker_View");
                    }
                }
            }

            /// <summary>
            /// [on/off] On if it is possible to invoke zConfiguration Call Layout Style: Gallery, to switch to the Gallery mode, showing video participants in tiled windows: The Zoom Room shows up to a 5x5 array of tiled windows per page.
            /// </summary>
            [JsonProperty("can_Switch_Wall_View")]
            public bool can_Switch_Wall_View
            {
                get
                {
                    return _can_Switch_Wall_View;
                }
                set
                {
                    if (value != _can_Switch_Wall_View)
                    {
                        _can_Switch_Wall_View = value;
                        NotifyPropertyChanged("can_Switch_Wall_View");
                    }
                }
            }

            [JsonProperty("can_Switch_Strip_View")]
            public bool can_Switch_Strip_View
            {
                get
                {
                    return _can_Switch_Strip_View;
                }
                set
                {
                    if (value != _can_Switch_Strip_View)
                    {
                        _can_Switch_Strip_View = value;
                        NotifyPropertyChanged("can_Switch_Strip_View");
                    }
                }
            }

            [JsonProperty("is_In_First_Page")]
            public bool is_In_First_Page
            {
                get
                {
                    return _is_In_First_Page;
                }
                set
                {
                    if (value != _is_In_First_Page)
                    {
                        _is_In_First_Page = value;
                        NotifyPropertyChanged("is_In_First_Page");
                    }
                }
            }

            [JsonProperty("is_In_Last_Page")]
            public bool is_In_Last_Page
            {
                get
                {
                    return _is_In_Last_Page;
                }
                set
                {
                    if (value != _is_In_Last_Page)
                    {
                        _is_In_Last_Page = value;
                        NotifyPropertyChanged("is_In_Last_Page");
                    }
                }
            }

            public bool is_supported { get; set; }
            public int video_Count_In_Current_Page { get; set; }

            /// <summary>
            ///  [Gallery | Strip] Indicates which mode applies: Strip or Gallery.
            /// </summary>
            [JsonProperty("video_type")]
            public string video_type
            {
                get
                {
                    return _video_type;
                }
                set
                {
                    if (value != _video_type)
                    {
                        _video_type = value;
                        NotifyPropertyChanged("video_type");
                    }
                }
            }
        }

        public class CallRecordInfo : NotifiableObject
        {
            private bool _meetingIsBeingRecorded;
            private bool _canRecord;
            private bool _emailRequired;

            public bool amIRecording { get; set; }

            public bool canRecord
            {
                get
                {
                    return _canRecord;
                }
                set
                {
                    if (value != _canRecord)
                    {
                        _canRecord = value;
                        NotifyPropertyChanged("canRecord");
                    }
                }
            }

            public bool emailRequired
            {
                get
                {
                    return _emailRequired;
                }
                set
                {
                    if (value != _emailRequired)
                    {
                        _emailRequired = value;
                        NotifyPropertyChanged("emailRequired");
                    }
                }
            }

            public bool meetingIsBeingRecorded 
            {
                get
                {
                    return _meetingIsBeingRecorded;
                }
                set
                {
                    //Debug.Console(2, "************************************setting value of meetingIsBeingRecorded to: {0}", value);
                    if (value != _meetingIsBeingRecorded)
                    {
                        _meetingIsBeingRecorded = value;
                        //Debug.Console(2, "********************************set value of meetingIsBeingRecorded to: {0}", _meetingIsBeingRecorded);
                        NotifyPropertyChanged("meetingIsBeingRecorded");
                    }
                }
            }

            /// <summary>
            /// Indicates if recording is allowed (when meeting capable and and email is not required to be entered by the user)
            /// </summary>
            public bool AllowRecord
            {
                get
                {
                    return canRecord && !emailRequired;
                }
            }

            public CallRecordInfo()
            {
                Debug.Console(2, Debug.ErrorLogLevel.Notice, "********************************************* CallRecordInfo() ******************************************");
            }
        }
    }
}