namespace PepperDash.Essentials.Core.DeviceTypeInterfaces;

/// <summary>
/// Defines the contract for a recorder that can suspend and resume capture without ending the
/// recording.
/// </summary>
/// <remarks>
/// <para>Independent of <see cref="IHasRecordingControl"/>, not derived from it. Pausing and
/// starting are separate capabilities and real devices have them separately: one recorder in hand
/// can pause a recording another device started, but its own start command force-begins the next
/// booking, which is never what a user pressing Record means. Deriving would have obliged it to
/// offer a start it should not.</para>
/// <para>The same split guards the other direction: a recorder may expose pause methods and throw
/// when they are called. Declaring the capability is how a consumer knows in advance rather than
/// at the moment someone presses the button.</para>
/// </remarks>
public interface IHasRecordingPause
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
