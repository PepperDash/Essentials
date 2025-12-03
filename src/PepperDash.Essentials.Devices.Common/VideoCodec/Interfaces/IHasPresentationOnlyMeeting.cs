namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    /// <summary>
    /// Defines the contract for IHasPresentationOnlyMeeting
    /// </summary>
    public interface IHasPresentationOnlyMeeting
    {
        /// <summary>
        /// Starts a presentation only meeting
        /// </summary>
        void StartSharingOnlyMeeting();

        /// <summary>
        /// Starts a presentation only meeting with specified display mode
        /// </summary>
        /// <param name="displayMode">The display mode for the meeting</param>
        void StartSharingOnlyMeeting(eSharingMeetingMode displayMode);

        /// <summary>
        /// Starts a presentation only meeting with specified display mode and duration
        /// </summary>
        /// <param name="displayMode">The display mode for the meeting</param>
        /// <param name="duration">The duration for the meeting</param>
        void StartSharingOnlyMeeting(eSharingMeetingMode displayMode, uint duration);

        /// <summary>
        /// Starts a presentation only meeting with specified display mode, duration, and password
        /// </summary>
        /// <param name="displayMode">The display mode for the meeting</param>
        /// <param name="duration">The duration for the meeting</param>
        /// <param name="password">The password for the meeting</param>
        void StartSharingOnlyMeeting(eSharingMeetingMode displayMode, uint duration, string password);

        /// <summary>
        /// Starts a normal meeting from a sharing only meeting
        /// </summary>
        void StartNormalMeetingFromSharingOnlyMeeting();
    }

    /// <summary>
    /// Enumeration of eSharingMeetingMode values
    /// </summary>
    public enum eSharingMeetingMode
    {
        /// <summary>
        /// No specific sharing mode
        /// </summary>
        None,
        /// <summary>
        /// Laptop sharing mode
        /// </summary>
        Laptop,
        /// <summary>
        /// iOS sharing mode
        /// </summary>
        Ios,
    }
}