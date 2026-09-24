namespace PepperDash.Essentials.Core.DeviceTypeInterfaces;

/// <summary>
/// What a recorder is doing, as the device itself reports it.
/// </summary>
/// <remarks>
/// Deliberately describes only confirmed device state. A recorder can take several seconds to
/// acknowledge a transport command, and tracking that wait belongs to the consumer: a room knows
/// it asked for something and has not been answered, while the device only ever knows what is
/// true. A pending value here would let a panel show "recording" for a recording that never
/// started.
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
}
