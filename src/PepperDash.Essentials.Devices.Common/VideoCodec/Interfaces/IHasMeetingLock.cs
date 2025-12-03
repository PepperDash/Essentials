using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    /// <summary>
    /// Defines the contract for IHasMeetingLock
    /// </summary>
    public interface IHasMeetingLock
    {
        /// <summary>
        /// Feedback that indicates whether the meeting is locked
        /// </summary>
        BoolFeedback MeetingIsLockedFeedback { get; }

        /// <summary>
        /// Locks the meeting
        /// </summary>
        void LockMeeting();

        /// <summary>
        /// Unlocks the meeting
        /// </summary>
        void UnLockMeeting();

        /// <summary>
        /// Toggles the meeting lock state
        /// </summary>
        void ToggleMeetingLock();
    }
}