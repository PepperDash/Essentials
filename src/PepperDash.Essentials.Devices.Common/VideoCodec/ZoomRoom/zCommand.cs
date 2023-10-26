extern alias Full;
using System;
using System.Collections.Generic;
using System.Linq;
using Full::Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
    /// <summary>
    /// zCommand class structure
    /// </summary>
    public class zCommand
    {
        public class BookingsListResult
        {
            [JsonProperty("accessRole")]
            public string AccessRole { get; set; }
            [JsonProperty("calendarChangeKey")]
            public string CalendarChangeKey { get; set; }
            [JsonProperty("calendarID")]
            public string CalendarId { get; set; }
            [JsonProperty("checkIn")]
            public bool CheckIn { get; set; }
            [JsonProperty("creatorEmail")]
            public string CreatorEmail { get; set; }
            [JsonProperty("creatorName")]
            public string CreatorName { get; set; }
            [JsonProperty("endTime")]
            public DateTime EndTime { get; set; }
            [JsonProperty("hostName")]
            public string HostName { get; set; }
            [JsonProperty("isInstantMeeting")]
            public bool IsInstantMeeting { get; set; }
            [JsonProperty("isPrivate")]
            public bool IsPrivate { get; set; }
            [JsonProperty("location")]
            public string Location { get; set; }
            [JsonProperty("meetingName")]
            public string MeetingName { get; set; }
            [JsonProperty("meetingNumber")]
            public string MeetingNumber { get; set; }
            [JsonProperty("scheduledFrom")]
            public string ScheduledFrom { get; set; }
            [JsonProperty("startTime")]
            public DateTime StartTime { get; set; }
            [JsonProperty("third_party")]
            public ThirdParty ThirdParty { get; set; }
        }

        public static List<Meeting> GetGenericMeetingsFromBookingResult(List<BookingsListResult> bookings,
            int minutesBeforeMeetingStart)
        {
            var rv = GetGenericMeetingsFromBookingResult(bookings);

            foreach (var meeting in rv)
            {
                meeting.MinutesBeforeMeeting = minutesBeforeMeetingStart;
            }

            return rv;
        }
        /// <summary>
        /// Extracts the necessary meeting values from the Zoom bookings response and converts them to the generic class
        /// </summary>
        /// <param name="bookings"></param>
        /// <returns></returns>
        public static List<Meeting> GetGenericMeetingsFromBookingResult(List<BookingsListResult> bookings)
        {
            var meetings = new List<Meeting>();

            if (Debug.Level > 0)
            {
                Debug.Console(1, "Meetings List:\n");
            }

            foreach (var b in bookings)
            {
                var meeting = new Meeting();

                if (b.MeetingNumber != null)
                    meeting.Id = b.MeetingNumber;
                if (b.CreatorName != null)
                    meeting.Organizer = b.CreatorName;
                if (b.MeetingName != null)
                    meeting.Title = b.MeetingName;
                //if (b.Agenda != null)
                //    meeting.Agenda = b.Agenda.Value;
                if (b.StartTime != null)
                    meeting.StartTime = b.StartTime;
                if (b.EndTime != null)
                    meeting.EndTime = b.EndTime;

                meeting.Privacy = b.IsPrivate ? eMeetingPrivacy.Private : eMeetingPrivacy.Public;

                // No meeting.Calls data exists for Zoom Rooms.  Leaving out for now.
                var now = DateTime.Now;
                if (meeting.StartTime < now && meeting.EndTime < now)
                {
                    Debug.Console(1, "Skipping meeting {0}. Meeting is in the past.", meeting.Title);
                    continue;
                }

                meetings.Add(meeting);

                if (Debug.Level > 0)
                {
                    Debug.Console(1, "Title: {0}, ID: {1}, Organizer: {2}", meeting.Title, meeting.Id, meeting.Organizer);
                    Debug.Console(1, "    Start Time: {0}, End Time: {1}, Duration: {2}", meeting.StartTime, meeting.EndTime, meeting.Duration);
                    Debug.Console(1, "    Joinable: {0}\n", meeting.Joinable);
                }
            }

            meetings.OrderBy(m => m.StartTime);

            return meetings;
        }

        public class HandStatus
        {
            // example return of the "hand_status" object
            // !!!! Note the properties contain ': ' within the property name !!!
            //"hand_status": {
            //  "is_raise_hand: ": false,
            //  "is_valid: ": "on",
            //  "time_stamp: ": "11825083"
            //},
            [JsonProperty("is_raise_hand: ")]
            public bool IsRaiseHand { get; set; }
            [JsonProperty("is_valid: ")]
            public string IsValid { get; set; }
            [JsonProperty("time_stamp: ")]
            public string TimeStamp { get; set; }
            /// <summary>
            /// Retuns a boolean value if the participant hand state is raised and is valid (both need to be true)
            /// </summary>
            public bool HandIsRaisedAndValid
            {
                get { return IsValid != null && IsValid == "on" && IsRaiseHand; }
            }
        }
        public class ListParticipant
        {
            [JsonProperty("audio_status state")]
            public string AudioStatusState { get; set; }
            [JsonProperty("audio_status type")]
            public string AudioStatusType { get; set; }
            [JsonProperty("avatar_url")]
            public string AvatarUrl { get; set; }
            [JsonProperty("camera_status am_i_controlling")]
            public bool CameraStatusAmIControlling { get; set; }
            [JsonProperty("camera_status can_i_request_control")]
            public bool CameraStatusCanIRequestConrol { get; set; }
            [JsonProperty("camera_status can_move_camera")]
            public bool CameraStatusCanMoveCamera { get; set; }
            [JsonProperty("camera_status can_switch_camera")]
            public bool CameraStatusCanSwitchCamera { get; set; }
            [JsonProperty("camera_status can_zoom_camera")]
            public bool CameraStatusCanZoomCamera { get; set; }
            [JsonProperty("can_edit_closed_caption")]
            public bool CanEditClosedCaption { get; set; }
            [JsonProperty("can_record")]
            public bool CanRecord { get; set; }
            [JsonProperty("event")]
            public string Event { get; set; }
            [JsonProperty("hand_status")]
            public HandStatus HandStatus { get; set; }
            [JsonProperty("isCohost")]
            public bool IsCohost { get; set; }
            [JsonProperty("is_client_support_closed_caption")]
            public bool IsClientSupportClosedCaption { get; set; }
            [JsonProperty("is_client_support_coHost")]
            public bool IsClientSupportCoHost { get; set; }
            [JsonProperty("is_host")]
            public bool IsHost { get; set; }
            [JsonProperty("is_myself")]
            public bool IsMyself { get; set; }
            [JsonProperty("is_recording")]
            public bool IsRecording { get; set; }
            [JsonProperty("is_video_can_mute_byHost")]
            public bool IsVideoCanMuteByHost { get; set; }
            [JsonProperty("is_video_can_unmute_byHost")]
            public bool IsVideoCanUnmuteByHost { get; set; }
            [JsonProperty("local_recording_disabled")]
            public bool LocalRecordingDisabled { get; set; }
            [JsonProperty("user_id")]
            public int UserId { get; set; }
            [JsonProperty("user_name")]
            public string UserName { get; set; }
            [JsonProperty("user_type")]
            public string UserType { get; set; }
            [JsonProperty("video_status has_source")]
            public bool VideoStatusHasSource { get; set; }
            [JsonProperty("video_status is_receiving")]
            public bool VideoStatusIsReceiving { get; set; }
            [JsonProperty("video_status is_sending")]
            public bool VideoStatusIsSending { get; set; }

            public ListParticipant()
            {
                HandStatus = new HandStatus();
            }

            /// <summary>
            /// Converts ZoomRoom pariticpant list response to an Essentials participant list
            /// </summary>
            /// <param name="participants"></param>
            /// <returns></returns>
            public static List<Participant> GetGenericParticipantListFromParticipantsResult(
                List<ListParticipant> participants)
            {
                if (participants.Count == 0)
                {
                    return new List<Participant>();
                }
                //return participants.Select(p => new Participant
                //            {
                //                UserId = p.UserId,
                //                Name = p.UserName,
                //                IsHost = p.IsHost,
                //                CanMuteVideo = p.IsVideoCanMuteByHost,
                //                CanUnmuteVideo = p.IsVideoCanUnmuteByHost,
                //                AudioMuteFb = p.AudioStatusState == "AUDIO_MUTED",
                //                VideoMuteFb = p.VideoStatusIsSending,
                //                HandIsRaisedFb = p.HandStatus.HandIsRaisedAndValid,
                //            }).ToList();

                var sortedParticipants = SortParticipantListByHandStatus(participants);
                return sortedParticipants.Select(p => new Participant
                {
                    UserId         = p.UserId,
                    Name           = p.UserName,
                    IsHost         = p.IsHost,
                    IsMyself       = p.IsMyself,
                    CanMuteVideo   = p.IsVideoCanMuteByHost,
                    CanUnmuteVideo = p.IsVideoCanUnmuteByHost,
                    AudioMuteFb    = p.AudioStatusState == "AUDIO_MUTED",
                    VideoMuteFb    = !p.VideoStatusIsSending,
                    HandIsRaisedFb = p.HandStatus.HandIsRaisedAndValid,
                }).ToList();
            }

            /// <summary>
            /// Will sort by hand-raise status and then alphabetically
            /// </summary>
            /// <param name="participants">Zoom Room response list of participants</param>
            /// <returns>List</returns>
            public static List<ListParticipant> SortParticipantListByHandStatus(List<ListParticipant> participants)
            {
                if (participants == null)
                {
                    //Debug.Console(1, "SortParticiapntListByHandStatu(participants == null)");
                    return null;
                }

                // debug testing
                //foreach (ListParticipant participant in participants)
                //{
                //    Debug.Console(1, "{0} | IsValid: {1} | IsRaiseHand: {2} | HandIsRaisedAndValid: {3}", 
                //        participant.UserName, participant.HandStatus.IsValid, participant.HandStatus.IsRaiseHand.ToString(), participant.HandStatus.HandIsRaisedAndValid.ToString());
                //}

                List<ListParticipant> handRaisedParticipantsList = participants.Where(p => p.HandStatus.HandIsRaisedAndValid).ToList();

                if (handRaisedParticipantsList != null)
                {
                    IOrderedEnumerable<ListParticipant> orderByDescending = handRaisedParticipantsList.OrderByDescending(p => p.HandStatus.TimeStamp);

                    //foreach (var participant in handRaisedParticipantsList)
                    //    Debug.Console(1, "handRaisedParticipantList: {0} | {1}", participant.UserName, participant.UserId);
                }

                List<ListParticipant> allOtherParticipantsList = participants.Where(p => !p.HandStatus.HandIsRaisedAndValid).ToList();

                if (allOtherParticipantsList != null)
                {
                    allOtherParticipantsList.OrderBy(p => p.UserName);

                    //foreach (var participant in allOtherParticipantsList)
                    //    Debug.Console(1, "allOtherParticipantsList: {0} | {1}", participant.UserName, participant.UserId);
                }

                // merge the lists
                List<ListParticipant> sortedList = handRaisedParticipantsList.Union(allOtherParticipantsList).ToList();

                // return the sorted list
                return sortedList;				
            }

        }

        public class CallinCountryList
        {
            public int code { get; set; }
            public string display_number { get; set; }
            public string id { get; set; }
            public string name { get; set; }
            public string number { get; set; }
        }

        public class CalloutCountryList
        {
            public int code { get; set; }
            public string display_number { get; set; }
            public string id { get; set; }
            public string name { get; set; }
            public string number { get; set; }
        }

        public class TollFreeCallinList
        {
            public int code { get; set; }
            public string display_number { get; set; }
            public string id { get; set; }
            public string name { get; set; }
            public string number { get; set; }
        }

        public class Info
        {
            public List<CallinCountryList> callin_country_list { get; set; }
            public List<CalloutCountryList> callout_country_list { get; set; }
            public List<TollFreeCallinList> toll_free_callin_list { get; set; }
        }

        public class ThirdParty
        {
            public string h323_address { get; set; }
            public string meeting_number { get; set; }
            public string service_provider { get; set; }
            public string sip_address { get; set; }
        }

        public class MeetingListItem
        {
            public string accessRole { get; set; }
            public string calendarChangeKey { get; set; }
            public string calendarID { get; set; }
            public bool checkIn { get; set; }
            public string creatorEmail { get; set; }
            public string creatorName { get; set; }
            public string endTime { get; set; }
            public string hostName { get; set; }
            public bool isInstantMeeting { get; set; }
            public bool isPrivate { get; set; }
            public string location { get; set; }
            public string meetingName { get; set; }
            public string meetingNumber { get; set; }
            public string scheduledFrom { get; set; }
            public string startTime { get; set; }
            public ThirdParty third_party { get; set; }

            public MeetingListItem()
            {
                third_party = new ThirdParty();
            }
        }

        public class InfoResult
        {
            public Info Info { get; set; }
            public bool am_i_original_host { get; set; }
            public string default_callin_country { get; set; }
            public string dialIn { get; set; }
            public string international_url { get; set; }
            public string invite_email_content { get; set; }
            public string invite_email_subject { get; set; }
            public bool is_callin_country_list_available { get; set; }
            public bool is_calling_room_system_enabled { get; set; }
            public bool is_toll_free_callin_list_available { get; set; }
            public bool is_view_only { get; set; }
            public bool is_waiting_room { get; set; }
            public bool is_webinar { get; set; }
            public string meeting_id { get; set; }
            public MeetingListItem meeting_list_item { get; set; }
            public string meeting_password { get; set; }
            public string meeting_type { get; set; }
            public int my_userid { get; set; }
            public int participant_id { get; set; }
            public string real_meeting_id { get; set; }
            public string schedule_option { get; set; }
            public string schedule_option2 { get; set; }
            public string support_callout_type { get; set; }
            public string toll_free_number { get; set; }
            public string user_type { get; set; }

            public InfoResult()
            {
                Info              = new Info();
                meeting_list_item = new MeetingListItem();
            }
        }

        public class Phonebook
        {
            public List<zStatus.Contact> Contacts { get; set; }
            public int Limit { get; set; }
            public int Offset { get; set; }
        }
    }
}