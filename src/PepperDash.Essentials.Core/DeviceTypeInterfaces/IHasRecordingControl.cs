using System;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces;

/// <summary>
/// Defines the contract for a device that records, and can be started and stopped.
/// </summary>
/// <remarks>
/// <para>The smallest useful recording capability, so that a device which can do nothing but start
/// and stop still takes part. Pausing, the detail of what is being recorded, and what is scheduled
/// next are separate interfaces, because recorders differ in which of those they support and a
/// consumer should be able to ask rather than find out by exception.</para>
/// <para>This is lecture and AV recording, as distinct from
/// <c>IHasMeetingRecording</c>, which is a video codec recording a call.</para>
/// </remarks>
public interface IHasRecordingControl
{
    /// <summary>
    /// What the recorder is doing now, as it reports it.
    /// </summary>
    eRecordingState RecordingState { get; }

    /// <summary>
    /// Raised when <see cref="RecordingState"/> changes.
    /// </summary>
    event EventHandler RecordingStateChanged;

    /// <summary>
    /// Begins recording.
    /// </summary>
    void StartRecording();

    /// <summary>
    /// Ends the current recording.
    /// </summary>
    void StopRecording();
}
