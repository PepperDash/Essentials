namespace PepperDash.Essentials.Core.DeviceTypeInterfaces;

/// <summary>
/// Defines the contract for a recorder that can suspend and resume capture without ending the
/// recording.
/// </summary>
/// <remarks>
/// Separate from <see cref="IHasRecordingControl"/> because not every recorder can do this, and
/// some expose the methods while throwing when they are called. Declaring the capability is how a
/// consumer knows the difference in advance, rather than at the moment someone presses the button.
/// </remarks>
public interface IHasRecordingPause : IHasRecordingControl
{
    /// <summary>
    /// Suspends capture, leaving the recording open.
    /// </summary>
    void PauseRecording();

    /// <summary>
    /// Resumes capture on a paused recording.
    /// </summary>
    void ResumeRecording();
}
