using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
  /// <summary>
  /// Defines the contract for IHasMeetingRecordingWithPrompt
  /// </summary>
  public interface IHasMeetingRecordingWithPrompt : IHasMeetingRecording
  {
    /// <summary>
    /// Feedback that indicates whether the recording consent prompt is visible
    /// </summary>
    BoolFeedback RecordConsentPromptIsVisible { get; }

    /// <summary>
    /// Used to agree or disagree to the meeting being recorded when prompted
    /// </summary>
    /// <param name="agree"></param>
    void RecordingPromptAcknowledgement(bool agree);
  }
}