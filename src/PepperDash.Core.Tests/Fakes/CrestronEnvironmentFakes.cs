using PepperDash.Core.Abstractions;

namespace PepperDash.Core.Tests.Fakes;

/// <summary>
/// Configurable ICrestronEnvironment for unit tests.
/// Defaults: Appliance / SimplSharpPro / ApplicationNumber=1.
/// </summary>
public class FakeCrestronEnvironment : ICrestronEnvironment
{
    public DevicePlatform DevicePlatform { get; set; } = DevicePlatform.Appliance;
    public RuntimeEnvironment RuntimeEnvironment { get; set; } = RuntimeEnvironment.SimplSharpPro;
    public string NewLine { get; set; } = "\r\n";
    public uint ApplicationNumber { get; set; } = 1;
    public uint RoomId { get; set; } = 0;

    public event EventHandler<ProgramStatusEventArgs>? ProgramStatusChanged;
    public event EventHandler<PepperDashEthernetEventArgs>? EthernetEventReceived;

    public string GetApplicationRootDirectory() => System.IO.Path.GetTempPath();

    /// <inheritdoc/>
    public bool IsHardwareRuntime => false;

    /// <summary>Simulates a program status event for tests.</summary>
    public void RaiseProgramStatus(ProgramStatusEventType type) =>
        ProgramStatusChanged?.Invoke(this, new ProgramStatusEventArgs(type));

    /// <summary>Simulates an ethernet event for tests.</summary>
    public void RaiseEthernetEvent(EthernetEventType type, short adapter = 0) =>
        EthernetEventReceived?.Invoke(this, new PepperDashEthernetEventArgs(type, adapter));
}

/// <summary>
/// No-op IEthernetHelper that returns configurable values.
/// </summary>
public class FakeEthernetHelper : IEthernetHelper
{
    private readonly Dictionary<EthernetParameterType, string> _values = new();

    public FakeEthernetHelper Seed(EthernetParameterType param, string value)
    {
        _values[param] = value;
        return this;
    }

    public string GetEthernetParameter(EthernetParameterType parameter, short ethernetAdapterId) =>
        _values.TryGetValue(parameter, out var v) ? v : string.Empty;
}
