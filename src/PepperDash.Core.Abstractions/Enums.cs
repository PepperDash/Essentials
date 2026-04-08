namespace PepperDash.Core.Abstractions;

/// <summary>
/// Mirrors Crestron's <c>eDevicePlatform</c> without requiring the Crestron SDK.
/// </summary>
public enum DevicePlatform
{
    /// <summary>Hardware appliance (e.g. CP4, MC4).</summary>
    Appliance,
    /// <summary>Crestron Virtual Control / server runtime.</summary>
    Server,
}

/// <summary>
/// Mirrors Crestron's <c>eRuntimeEnvironment</c>.
/// </summary>
public enum RuntimeEnvironment
{
    /// <summary>SimplSharpPro program slot (hardware 4-series).</summary>
    SimplSharpPro,
    /// <summary>SimplSharp (older 3-series or server environments).</summary>
    SimplSharp,
    /// <summary>Any other environment — check for completeness.</summary>
    Other,
}

/// <summary>
/// Mirrors Crestron's <c>ConsoleAccessLevelEnum</c>.
/// </summary>
public enum ConsoleAccessLevel
{
    AccessAdministrator = 0,
    AccessOperator = 1,
    AccessProgrammer = 2,
}

/// <summary>
/// Mirrors Crestron's <c>eProgramStatusEventType</c>.
/// </summary>
public enum ProgramStatusEventType
{
    Starting,
    Stopping,
    Paused,
    Resumed,
}

/// <summary>
/// Mirrors the event type used by Crestron's <c>EthernetEventArgs</c>.
/// </summary>
public enum EthernetEventType
{
    LinkDown = 0,
    LinkUp = 1,
}

/// <summary>
/// Event args for Crestron ethernet link events.
/// </summary>
public class PepperDashEthernetEventArgs : EventArgs
{
    public EthernetEventType EthernetEventType { get; }
    public short EthernetAdapter { get; }

    public PepperDashEthernetEventArgs(EthernetEventType eventType, short adapter)
    {
        EthernetEventType = eventType;
        EthernetAdapter = adapter;
    }
}

/// <summary>
/// Mirrors the set of <c>CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET</c> values
/// used across this codebase — does not aim to be exhaustive.
/// </summary>
public enum EthernetParameterType
{
    GetCurrentIpAddress,
    GetHostname,
    GetDomainName,
    GetLinkStatus,
    GetCurrentDhcpState,
    GetCurrentIpMask,
    GetCurrentRouter,
    GetMacAddress,
    GetDnsServer,
}

/// <summary>
/// Mirrors Crestron's <c>SocketStatus</c> without requiring the Crestron SDK.
/// </summary>
public enum PepperDashSocketStatus
{
    SocketNotConnected = 0,
    SocketConnected = 2,
    SocketConnectionInProgress = 6,
    SocketConnectFailed = 11,
    SocketDisconnecting = 12,
    SocketBrokenRemotely = 7,
}
