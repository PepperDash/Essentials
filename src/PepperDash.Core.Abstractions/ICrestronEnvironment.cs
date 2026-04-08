namespace PepperDash.Core.Abstractions;

/// <summary>
/// Abstracts <c>Crestron.SimplSharp.CrestronEnvironment</c> to allow unit testing
/// without the Crestron SDK.
/// </summary>
public interface ICrestronEnvironment
{
    /// <summary>Gets the platform the program is executing on.</summary>
    DevicePlatform DevicePlatform { get; }

    /// <summary>Gets the current runtime environment.</summary>
    RuntimeEnvironment RuntimeEnvironment { get; }

    /// <summary>Gets the platform-appropriate newline string.</summary>
    string NewLine { get; }

    /// <summary>Gets the application number (program slot).</summary>
    uint ApplicationNumber { get; }

    /// <summary>Gets the room ID (used in Crestron Virtual Control / server environments).</summary>
    uint RoomId { get; }

    /// <summary>Raised when program status changes (starting, stopping, etc.).</summary>
    event EventHandler<ProgramStatusEventArgs> ProgramStatusChanged;

    /// <summary>Raised when the ethernet link changes state.</summary>
    event EventHandler<PepperDashEthernetEventArgs> EthernetEventReceived;

    /// <summary>Gets the application root directory path.</summary>
    string GetApplicationRootDirectory();
}

/// <summary>
/// Event args for <see cref="ICrestronEnvironment.ProgramStatusChanged"/>.
/// </summary>
public class ProgramStatusEventArgs : EventArgs
{
    public ProgramStatusEventType EventType { get; }

    public ProgramStatusEventArgs(ProgramStatusEventType eventType)
    {
        EventType = eventType;
    }
}
