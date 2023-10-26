using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
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