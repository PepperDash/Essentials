namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces;

public interface IHasPresentationOnlyMeeting
{
    void StartSharingOnlyMeeting();
    void StartSharingOnlyMeeting(eSharingMeetingMode displayMode);
    void StartSharingOnlyMeeting(eSharingMeetingMode displayMode, uint duration);
    void StartSharingOnlyMeeting(eSharingMeetingMode displayMode, uint duration, string password);
    void StartNormalMeetingFromSharingOnlyMeeting();
}

public enum eSharingMeetingMode
{
    None,
    Laptop,
    Ios,
}