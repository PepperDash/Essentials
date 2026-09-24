using System;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces;

/// <summary>
/// Defines the contract for a recorder that can describe the recording it is making, and how long
/// it is set to run.
/// </summary>
/// <remarks>
/// Independent of <see cref="IHasRecordingControl"/>, not derived from it, because the two occur
/// separately in both directions: a recorder kept as a backup can start and stop while knowing
/// nothing about what it is capturing, and a device may describe a recording it is not the one
/// controlling.
/// </remarks>
public interface IHasRecordingInfo
{
    /// <summary>
    /// What the current recording is called, or empty when nothing is recording.
    /// </summary>
    string RecordingTitle { get; }

    /// <summary>
    /// When the current recording is due to end, or null when nothing is recording.
    /// </summary>
    DateTime? RecordingEndTime { get; }

    /// <summary>
    /// How long a recording started now would run, in minutes.
    /// </summary>
    int RecordingLengthMinutes { get; }

    /// <summary>
    /// Sets how long a recording started from now on should run.
    /// </summary>
    /// <param name="minutes">Length in minutes. A device may clamp this to what it supports.</param>
    void SetRecordingLength(int minutes);

    /// <summary>
    /// Adds time to the recording in progress.
    /// </summary>
    /// <param name="minutes">Minutes to add to the current end time.</param>
    void ExtendRecording(int minutes);
}
