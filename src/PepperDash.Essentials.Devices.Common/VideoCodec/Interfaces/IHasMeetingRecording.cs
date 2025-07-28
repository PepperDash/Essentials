using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    /// <summary>
    /// Defines the contract for IHasMeetingRecording
    /// </summary>
    public interface IHasMeetingRecording
    {
        BoolFeedback MeetingIsRecordingFeedback { get; }

        void StartRecording();
        void StopRecording();
        void ToggleRecording();
    }

    /// <summary>
    /// Defines the contract for IHasMeetingRecordingWithPrompt
    /// </summary>
    public interface IHasMeetingRecordingWithPrompt : IHasMeetingRecording
    {
        BoolFeedback RecordConsentPromptIsVisible { get; }

        /// <summary>
        /// Used to agree or disagree to the meeting being recorded when prompted
        /// </summary>
        /// <param name="agree"></param>
        void RecordingPromptAcknowledgement(bool agree);
    }
}