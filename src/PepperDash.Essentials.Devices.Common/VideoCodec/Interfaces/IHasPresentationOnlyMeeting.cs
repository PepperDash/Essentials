namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    /// <summary>
    /// Defines the contract for IHasPresentationOnlyMeeting
    /// </summary>
    public interface IHasPresentationOnlyMeeting
    {
        void StartSharingOnlyMeeting();
        void StartSharingOnlyMeeting(eSharingMeetingMode displayMode);
        void StartSharingOnlyMeeting(eSharingMeetingMode displayMode, uint duration);
        void StartSharingOnlyMeeting(eSharingMeetingMode displayMode, uint duration, string password);
        void StartNormalMeetingFromSharingOnlyMeeting();
    }

    /// <summary>
    /// Enumeration of eSharingMeetingMode values
    /// </summary>
    public enum eSharingMeetingMode
    {
        None,
        Laptop,
        Ios,
    }
}