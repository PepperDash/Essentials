using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    /// <summary>
    /// Defines the contract for IHasMeetingRecording
    /// </summary>
    public interface IHasMeetingRecording
    {
        /// <summary>
        /// Feedback that indicates whether the meeting is being recorded
        /// </summary>
        BoolFeedback MeetingIsRecordingFeedback { get; }

        /// <summary>
        /// Starts recording the meeting
        /// </summary>
        void StartRecording();

        /// <summary>
        /// Stops recording the meeting
        /// </summary>
        void StopRecording();

        /// <summary>
        /// Toggles recording the meeting
        /// </summary>
        void ToggleRecording();
    }
}