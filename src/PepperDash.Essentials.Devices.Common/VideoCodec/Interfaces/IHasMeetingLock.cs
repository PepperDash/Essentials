using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    /// <summary>
    /// Defines the contract for IHasMeetingLock
    /// </summary>
    public interface IHasMeetingLock
    {
        BoolFeedback MeetingIsLockedFeedback { get; }

        void LockMeeting();
        void UnLockMeeting();
        void ToggleMeetingLock();
    }
}