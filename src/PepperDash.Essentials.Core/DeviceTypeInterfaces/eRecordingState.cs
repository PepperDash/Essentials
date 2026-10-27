namespace PepperDash.Essentials.Core.DeviceTypeInterfaces;

/// <summary>
/// What a recorder is doing, as the device itself reports it.
/// </summary>
/// <remarks>
/// <para>Deliberately describes only confirmed device state. A recorder can take several seconds
/// to acknowledge a transport command, and tracking that wait belongs to the consumer: a room knows
/// it asked for something and has not been answered, while the device only ever knows what is
/// true. A pending value here would let a panel show "recording" for a recording that never
/// started.</para>
/// <para><see cref="Starting"/> keeps to that. It is not a guess made from a button press: it is
/// reported only when the recording service itself holds a recording that is due now and the
/// recorder has not begun it — which can run to a minute while a hardware recorder picks up a new
/// booking.</para>
/// <para>New values are appended, so the existing ones keep their numbers.</para>
/// </remarks>
public enum eRecordingState
{
    /// <summary>Not recording, and nothing in progress.</summary>
    Idle,

    /// <summary>Recording.</summary>
    Recording,

    /// <summary>Recording, but capture is suspended and can be resumed.</summary>
    Paused,

    /// <summary>Unreachable, so what it is doing is unknown. Not the same as idle.</summary>
    Offline,

    /// <summary>
    /// A recording is due now and the recording service is waiting for the recorder to begin it.
    /// Not yet capturing; stopping it cancels the recording.
    /// </summary>
    Starting,
}
