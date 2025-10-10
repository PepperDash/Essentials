namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    /// <summary>
    /// Describes the ability to start an ad-hoc meeting
    /// </summary>
    public interface IHasStartMeeting
    {
        /// <summary>
        /// The default meeting duration in minutes
        /// </summary>
        uint DefaultMeetingDurationMin { get; }

        /// <summary>
        /// Start an ad-hoc meeting for the specified duration
        /// </summary>
        /// <param name="duration"></param>
        void StartMeeting(uint duration);

        /// <summary>
        /// Leaves a meeting without ending it
        /// </summary>
        void LeaveMeeting();
    }
}