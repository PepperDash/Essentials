namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    public interface IHasPresentationOnlyMeeting
    {
        void StartSharingOnlyMeeting();
        void StartSharingOnlyMeeting(eSharingMeetingMode mode);
        void StartSharingOnlyMeeting(eSharingMeetingMode mode, ushort duration);
        void StartSharingOnlyMeeting(eSharingMeetingMode mode, ushort duration, string password);
        void StartNormalMeetingFromSharingOnlyMeeting();
    }

    public enum eSharingMeetingMode
    {
        None,
        Laptop,
        Ios,
    }
}