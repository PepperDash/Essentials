using System;
using Crestron.SimplSharp;
using PdCore = PepperDash.Core.Abstractions;

namespace PepperDash.Core.Adapters;

/// <summary>
/// Production adapter — delegates ICrestronEnvironment calls to the real Crestron SDK.
/// </summary>
public sealed class CrestronEnvironmentAdapter : PdCore.ICrestronEnvironment
{
    // Subscribe once in constructor and re-raise as our event types.
    private event EventHandler<PdCore.ProgramStatusEventArgs>? _programStatusChanged;
    private event EventHandler<PdCore.PepperDashEthernetEventArgs>? _ethernetEventReceived;

    public CrestronEnvironmentAdapter()
    {
        CrestronEnvironment.ProgramStatusEventHandler += type =>
            _programStatusChanged?.Invoke(this, new PdCore.ProgramStatusEventArgs(MapProgramStatus(type)));

        CrestronEnvironment.EthernetEventHandler += args =>
            _ethernetEventReceived?.Invoke(this, new PdCore.PepperDashEthernetEventArgs(
                args.EthernetEventType == eEthernetEventType.LinkDown
                    ? PdCore.EthernetEventType.LinkDown
                    : PdCore.EthernetEventType.LinkUp,
                (short)args.EthernetAdapter));
    }

    public PdCore.DevicePlatform DevicePlatform =>
        CrestronEnvironment.DevicePlatform == eDevicePlatform.Appliance
            ? PdCore.DevicePlatform.Appliance
            : PdCore.DevicePlatform.Server;

    public PdCore.RuntimeEnvironment RuntimeEnvironment =>
        CrestronEnvironment.RuntimeEnvironment == eRuntimeEnvironment.SimplSharpPro
            ? PdCore.RuntimeEnvironment.SimplSharpPro
            : PdCore.RuntimeEnvironment.Other;

    public string NewLine => CrestronEnvironment.NewLine;

    public uint ApplicationNumber => InitialParametersClass.ApplicationNumber;

    public uint RoomId => uint.TryParse(InitialParametersClass.RoomId, out var r) ? r : 0;

    public event EventHandler<PdCore.ProgramStatusEventArgs> ProgramStatusChanged
    {
        add => _programStatusChanged += value;
        remove => _programStatusChanged -= value;
    }

    public event EventHandler<PdCore.PepperDashEthernetEventArgs> EthernetEventReceived
    {
        add => _ethernetEventReceived += value;
        remove => _ethernetEventReceived -= value;
    }

    public string GetApplicationRootDirectory() =>
        Crestron.SimplSharp.CrestronIO.Directory.GetApplicationRootDirectory();

    /// <inheritdoc/>
    public bool IsHardwareRuntime => true;

    private static PdCore.ProgramStatusEventType MapProgramStatus(eProgramStatusEventType type) =>
        type switch
        {
            eProgramStatusEventType.Stopping => PdCore.ProgramStatusEventType.Stopping,
            eProgramStatusEventType.Paused => PdCore.ProgramStatusEventType.Paused,
            eProgramStatusEventType.Resumed => PdCore.ProgramStatusEventType.Resumed,
            _ => PdCore.ProgramStatusEventType.Starting,
        };
}
