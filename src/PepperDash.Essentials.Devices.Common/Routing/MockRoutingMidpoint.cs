using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Routing;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Routing;

/// <summary>
/// A mock midpoint routing device (e.g. a matrix switcher) that implements <see cref="IHasNamedRoutingSlots"/>
/// without any real hardware communication. Its input and output ports are configured via
/// <see cref="MockRoutingMidpointPropertiesConfig"/>, each with a name, signal type, and physical port
/// (connection) type - so it can stand in for a real switching device (such as a DM chassis or a StreamSync
/// matrix) for development and testing of routing logic, including named-routing-slot UI that a bare
/// <see cref="IRoutingMidpointWithFeedback"/> device cannot support.
/// </summary>
[Description("A mock routing midpoint (e.g. matrix switcher) device for testing routing logic without real hardware")]
public class MockRoutingMidpoint : EssentialsDevice, IHasNamedRoutingSlots, ICommunicationMonitor
{
    /// <inheritdoc />
    /// <remarks>Always online: there's no real connection behind this mock to lose.</remarks>
    public StatusMonitorBase CommunicationMonitor { get; }

    /// <summary>
    /// The configuration properties for this device.
    /// </summary>
    public MockRoutingMidpointPropertiesConfig PropertiesConfig { get; private set; }

    /// <inheritdoc />
    public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

    /// <inheritdoc />
    public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

    /// <inheritdoc />
    public List<RouteSwitchDescriptor> CurrentRoutes { get; } = new List<RouteSwitchDescriptor>();

    /// <inheritdoc />
    public event RouteChangedEventHandler RouteChanged;

    private readonly Dictionary<string, MockRoutingOutputSlotInfo> _outputSlotsByKey = new Dictionary<string, MockRoutingOutputSlotInfo>();
    private readonly List<MockRoutingInputSlotInfo> _syncAwareInputSlots = new List<MockRoutingInputSlotInfo>();

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IRoutingSlotInfo> InputSlots { get; private set; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IRoutingOutputSlotInfo> OutputSlots { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MockRoutingMidpoint"/> class from a <see cref="DeviceConfig"/>.
    /// </summary>
    /// <param name="config">The device configuration, whose Properties are deserialized as <see cref="MockRoutingMidpointPropertiesConfig"/>.</param>
    public MockRoutingMidpoint(DeviceConfig config)
        : base(config.Key, config.Name)
    {
        CommunicationMonitor = new MockCommunicationMonitor(this);

        PropertiesConfig = config.Properties != null
            ? JsonConvert.DeserializeObject<MockRoutingMidpointPropertiesConfig>(config.Properties.ToString())
            : null;
        PropertiesConfig ??= new MockRoutingMidpointPropertiesConfig();

        InputPorts = new RoutingPortCollection<RoutingInputPort>();
        OutputPorts = new RoutingPortCollection<RoutingOutputPort>();

        BuildPorts();
    }

    /// <summary>
    /// Builds the input and output ports (and their <see cref="IHasNamedRoutingSlots"/> slot info) from
    /// <see cref="PropertiesConfig"/>. Each port's Key and Selector are both set to the configured port
    /// name, so selectors passed to <see cref="ExecuteSwitch"/> and <see cref="ClearRoute"/> can simply be
    /// looked up by matching against the port's Selector. Slot number is the 1-based position of the port
    /// within its input/output list.
    /// </summary>
    private void BuildPorts()
    {
        try
        {
            var inputSlots = new Dictionary<string, IRoutingSlotInfo>();
            var outputSlots = new Dictionary<string, IRoutingOutputSlotInfo>();
            var slotNumber = 0;

            foreach (var portConfig in PropertiesConfig.InputPorts)
            {
                if (string.IsNullOrEmpty(portConfig.Name))
                {
                    this.LogWarning("Skipping input port with no name configured for {key}", Key);
                    continue;
                }

                var port = new RoutingInputPort(portConfig.Name, portConfig.SignalType, portConfig.PortType, portConfig.Name, this);
                InputPorts.Add(port);

                slotNumber++;

                // Video sync is a video concept - an audio-only input (e.g. a mic) gets the bare
                // slot info, with no IRoutingInputSlotInfo/sync status at all, rather than a
                // meaningless always-true dot.
                if (portConfig.SignalType.HasFlag(eRoutingSignalType.Video))
                {
                    var syncSlot = new MockRoutingInputSlotInfo(
                        portConfig.Name, portConfig.Label ?? portConfig.Name, slotNumber, portConfig.SignalType,
                        portConfig.TxDeviceKey, portConfig.StartsWithSync);
                    inputSlots[portConfig.Name] = syncSlot;
                    _syncAwareInputSlots.Add(syncSlot);
                }
                else
                {
                    inputSlots[portConfig.Name] = new MockRoutingSlotInfo(
                        portConfig.Name, portConfig.Label ?? portConfig.Name, slotNumber, portConfig.SignalType);
                }
            }

            slotNumber = 0;
            foreach (var portConfig in PropertiesConfig.OutputPorts)
            {
                if (string.IsNullOrEmpty(portConfig.Name))
                {
                    this.LogWarning("Skipping output port with no name configured for {key}", Key);
                    continue;
                }

                var port = new RoutingOutputPort(portConfig.Name, portConfig.SignalType, portConfig.PortType, portConfig.Name, this);
                OutputPorts.Add(port);

                slotNumber++;
                var outputSlot = new MockRoutingOutputSlotInfo(
                    portConfig.Name, portConfig.Label ?? portConfig.Name, slotNumber, portConfig.SignalType);
                _outputSlotsByKey[portConfig.Name] = outputSlot;
                outputSlots[portConfig.Name] = outputSlot;
            }

            InputSlots = inputSlots;
            OutputSlots = outputSlots;

            this.LogInformation("Built {inputCount} input port(s) and {outputCount} output port(s) for mock midpoint {key}",
                InputPorts.Count, OutputPorts.Count, Key);
        }
        catch (Exception ex)
        {
            this.LogException(ex, "Error building ports for mock midpoint {0}", Key);
        }
    }

    /// <summary>
    /// Links each input slot's <see cref="MockRoutingInputSlotInfo.TxDeviceKey"/> (when configured) to
    /// that device's real video sync, once every device exists - a plain constructor-time lookup
    /// can't do this since the device it names may not have been built yet.
    /// </summary>
    protected override bool CustomActivate()
    {
        foreach (var slot in _syncAwareInputSlots)
        {
            if (string.IsNullOrEmpty(slot.TxDeviceKey)) continue;

            if (!(DeviceManager.GetDeviceForKey(slot.TxDeviceKey) is IVideoSync txDevice))
            {
                this.LogWarning("Tx device '{txDeviceKey}' for input slot '{slot}' not found or does not implement IVideoSync",
                    slot.TxDeviceKey, slot.Key);
                continue;
            }

            slot.SetVideoSyncDetected(txDevice.VideoSyncDetected);
            txDevice.VideoSyncChanged += (sender, args) => slot.SetVideoSyncDetected(txDevice.VideoSyncDetected);
        }

        return base.CustomActivate();
    }

    /// <inheritdoc />
    public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
    {
        try
        {
            var outputPort = OutputPorts.FirstOrDefault(p => Equals(p.Selector, outputSelector));

            if (outputPort == null)
            {
                this.LogWarning("Unable to find output port for selector {selector} on {key}", outputSelector, Key);
                return;
            }

            if (inputSelector == null)
            {
                ClearRouteOnOutput(outputPort, signalType);
                return;
            }

            var inputPort = InputPorts.FirstOrDefault(p => Equals(p.Selector, inputSelector));

            if (inputPort == null)
            {
                this.LogWarning("Unable to find input port for selector {selector} on {key}", inputSelector, Key);
                return;
            }

            if (_outputSlotsByKey.TryGetValue(outputPort.Key, out var routedSlot))
            {
                routedSlot.SetRoute(signalType, inputPort.Key);
            }

            this.LogInformation("Executed switch: {input} -> {output} ({signalType}) on {key}",
                inputPort.Key, outputPort.Key, signalType, Key);

            RefreshCurrentRoutes();

            RouteChanged?.Invoke(this, new RouteSwitchDescriptor(outputPort, inputPort));
        }
        catch (Exception ex)
        {
            this.LogException(ex, "Error executing switch on mock midpoint {0}", Key);
        }
    }

    /// <inheritdoc />
    public void ClearRoute(object outputSelector, eRoutingSignalType signalType)
    {
        var outputPort = OutputPorts.FirstOrDefault(p => Equals(p.Selector, outputSelector));

        if (outputPort == null)
        {
            this.LogWarning("Unable to find output port for selector {selector} on {key}", outputSelector, Key);
            return;
        }

        ClearRouteOnOutput(outputPort, signalType);
    }

    private void ClearRouteOnOutput(RoutingOutputPort outputPort, eRoutingSignalType signalType)
    {
        this.LogInformation("Clearing route to output {output} on {key} ({signalType})", outputPort.Key, Key, signalType);

        if (_outputSlotsByKey.TryGetValue(outputPort.Key, out var clearedSlot))
        {
            clearedSlot.ClearRoute(signalType);
        }

        RefreshCurrentRoutes();

        RouteChanged?.Invoke(this, new RouteSwitchDescriptor(outputPort, null));
    }

    /// <summary>
    /// Rebuilds the flat <see cref="CurrentRoutes"/> list (required by <see cref="IRoutingMidpointWithFeedback"/>,
    /// consumed by clients that only understand the bare contract) from the per-signal-type routes each
    /// output slot actually tracks - one descriptor per (output, signal type) currently routed, so an
    /// output with independent audio/video sources yields two descriptors rather than one overwriting
    /// the other.
    /// </summary>
    private void RefreshCurrentRoutes()
    {
        CurrentRoutes.Clear();

        foreach (var outputPort in OutputPorts)
        {
            if (!_outputSlotsByKey.TryGetValue(outputPort.Key, out var slot)) continue;

            foreach (var route in slot.CurrentRouteInputKeys)
            {
                var inputPort = InputPorts.FirstOrDefault(p => p.Key == route.Value);
                if (inputPort != null)
                {
                    CurrentRoutes.Add(new RouteSwitchDescriptor(outputPort, inputPort));
                }
            }
        }
    }
}

/// <summary>
/// Named routing slot info for a <see cref="MockRoutingMidpoint"/> input or output port.
/// </summary>
class MockRoutingSlotInfo : IRoutingSlotInfo
{
    /// <inheritdoc />
    public string Key { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public int SlotNumber { get; }

    /// <inheritdoc />
    public eRoutingSignalType SupportedSignalTypes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MockRoutingSlotInfo"/> class.
    /// </summary>
    public MockRoutingSlotInfo(string key, string name, int slotNumber, eRoutingSignalType supportedSignalTypes)
    {
        Key = key;
        Name = name;
        SlotNumber = slotNumber;
        SupportedSignalTypes = supportedSignalTypes;
    }
}

/// <summary>
/// Named input routing slot info for a <see cref="MockRoutingMidpoint"/> input port that supports
/// video, adding the <see cref="IRoutingInputSlotInfo"/> status (video sync, online state,
/// transmitter device key) the mobile-control named-routing-slots messenger surfaces when present.
/// </summary>
class MockRoutingInputSlotInfo : MockRoutingSlotInfo, IRoutingInputSlotInfo
{
    private bool _videoSyncDetected;

    /// <inheritdoc />
    public string TxDeviceKey { get; }

    /// <inheritdoc />
    public BoolFeedback IsOnline { get; }

    /// <inheritdoc />
    public bool VideoSyncDetected => _videoSyncDetected;

    /// <inheritdoc />
    public event EventHandler VideoSyncChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockRoutingInputSlotInfo"/> class.
    /// </summary>
    public MockRoutingInputSlotInfo(string key, string name, int slotNumber, eRoutingSignalType supportedSignalTypes,
        string txDeviceKey, bool startsWithSync)
        : base(key, name, slotNumber, supportedSignalTypes)
    {
        TxDeviceKey = txDeviceKey ?? string.Empty;
        _videoSyncDetected = startsWithSync;

        // No real endpoint behind a mock port, so this is always online - only VideoSyncDetected
        // (fixed config value, or live-linked via TxDeviceKey - see MockRoutingMidpoint.CustomActivate)
        // varies.
        IsOnline = new BoolFeedback(() => true);
        IsOnline.FireUpdate();
    }

    /// <summary>
    /// Sets <see cref="VideoSyncDetected"/> and raises <see cref="VideoSyncChanged"/> if it changed -
    /// called from config-seeded startup and, when <see cref="TxDeviceKey"/> names a device
    /// implementing <c>IVideoSync</c>, to mirror that device's real sync state live.
    /// </summary>
    public void SetVideoSyncDetected(bool detected)
    {
        if (_videoSyncDetected == detected) return;

        _videoSyncDetected = detected;
        VideoSyncChanged?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// Named output routing slot info for a <see cref="MockRoutingMidpoint"/> output port, tracking the
/// currently routed input key per signal type since the mock's flat <see cref="MockRoutingMidpoint.CurrentRoutes"/>
/// list does not carry signal type.
/// </summary>
class MockRoutingOutputSlotInfo : MockRoutingSlotInfo, IRoutingOutputSlotInfo
{
    private readonly Dictionary<eRoutingSignalType, string> _currentRouteInputKeys = new Dictionary<eRoutingSignalType, string>();

    /// <inheritdoc />
    public IReadOnlyDictionary<eRoutingSignalType, string> CurrentRouteInputKeys => _currentRouteInputKeys;

    /// <inheritdoc />
    public event EventHandler OutputSlotChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockRoutingOutputSlotInfo"/> class.
    /// </summary>
    public MockRoutingOutputSlotInfo(string key, string name, int slotNumber, eRoutingSignalType supportedSignalTypes)
        : base(key, name, slotNumber, supportedSignalTypes)
    {
    }

    /// <summary>
    /// Records the input key routed to this output for the given signal type and raises <see cref="OutputSlotChanged"/>.
    /// </summary>
    /// <remarks>
    /// Decomposes a combined signal type (e.g. <see cref="eRoutingSignalType.AudioVideo"/>) into
    /// separate Audio/Video entries - the same way <c>DisplayBase.SetCurrentSource</c> does - rather
    /// than keying the dictionary by the combined value itself. Without this, routing "Audio &amp;
    /// Video" (the common case) would record a single entry under the key <c>AudioVideo</c> that an
    /// output slot's independent Audio/Video crosspoint feedback would never match, since a later
    /// audio-only or video-only switch looks up <c>Audio</c>/<c>Video</c> individually.
    /// </remarks>
    public void SetRoute(eRoutingSignalType signalType, string inputKey)
    {
        var changed = false;

        if (signalType.HasFlag(eRoutingSignalType.Audio))
        {
            _currentRouteInputKeys[eRoutingSignalType.Audio] = inputKey;
            changed = true;
        }

        if (signalType.HasFlag(eRoutingSignalType.Video))
        {
            _currentRouteInputKeys[eRoutingSignalType.Video] = inputKey;
            changed = true;
        }

        if (changed)
        {
            OutputSlotChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Clears the routed input key for the given signal type (decomposed the same way as
    /// <see cref="SetRoute"/>) and raises <see cref="OutputSlotChanged"/> if anything changed.
    /// </summary>
    public void ClearRoute(eRoutingSignalType signalType)
    {
        var changed = false;

        if (signalType.HasFlag(eRoutingSignalType.Audio))
        {
            changed |= _currentRouteInputKeys.Remove(eRoutingSignalType.Audio);
        }

        if (signalType.HasFlag(eRoutingSignalType.Video))
        {
            changed |= _currentRouteInputKeys.Remove(eRoutingSignalType.Video);
        }

        if (changed)
        {
            OutputSlotChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

/// <summary>
/// Factory for building <see cref="MockRoutingMidpoint"/> devices.
/// </summary>
public class MockRoutingMidpointFactory : EssentialsDeviceFactory<MockRoutingMidpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MockRoutingMidpointFactory"/> class.
    /// </summary>
    public MockRoutingMidpointFactory()
    {
        TypeNames = new List<string> { "mockroutingmidpoint", "mockmidpoint" };
    }

    /// <inheritdoc />
    public override EssentialsDevice BuildDevice(DeviceConfig dc)
    {
        Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new MockRoutingMidpoint Device");
        return new MockRoutingMidpoint(dc);
    }
}
