using System;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces;

/// <summary>
/// Defines the contract for a recorder that knows what it is booked to record next.
/// </summary>
/// <remarks>
/// Its own interface because the schedule frequently lives somewhere other than the device doing
/// the recording — a room may record through one system while its bookings are held by another.
/// </remarks>
public interface IHasRecordingSchedule
{
    /// <summary>
    /// The next booking that has not started, or null when nothing is scheduled.
    /// </summary>
    RecordingScheduleEntry NextRecording { get; }

    /// <summary>
    /// Raised when <see cref="NextRecording"/> changes.
    /// </summary>
    event EventHandler NextRecordingChanged;
}

/// <summary>
/// One booked recording.
/// </summary>
public class RecordingScheduleEntry
{
    /// <summary>
    /// What the booking is called.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// When it is due to start, in local time.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// When it is due to end, in local time.
    /// </summary>
    public DateTime EndTime { get; set; }
}
