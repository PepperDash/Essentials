using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
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
public class MockRoutingMidpoint : EssentialsDevice, IHasNamedRoutingSlots
{
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
                inputSlots[portConfig.Name] = new MockRoutingSlotInfo(
                    portConfig.Name, portConfig.Label ?? portConfig.Name, slotNumber, portConfig.SignalType);
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

            // Remove any existing route to this output before making or clearing the new one.
            var existingRoute = CurrentRoutes.FirstOrDefault(r => r.OutputPort?.Key == outputPort.Key);
            if (existingRoute != null)
            {
                CurrentRoutes.Remove(existingRoute);
            }

            if (inputSelector == null)
            {
                this.LogInformation("Clearing route to output {output} on {key}", outputPort.Key, Key);

                if (_outputSlotsByKey.TryGetValue(outputPort.Key, out var clearedSlot))
                {
                    clearedSlot.ClearRoute(signalType);
                }

                var clearedDescriptor = new RouteSwitchDescriptor(outputPort, null);
                RouteChanged?.Invoke(this, clearedDescriptor);
                return;
            }

            var inputPort = InputPorts.FirstOrDefault(p => Equals(p.Selector, inputSelector));

            if (inputPort == null)
            {
                this.LogWarning("Unable to find input port for selector {selector} on {key}", inputSelector, Key);
                return;
            }

            var descriptor = new RouteSwitchDescriptor(outputPort, inputPort);
            CurrentRoutes.Add(descriptor);

            if (_outputSlotsByKey.TryGetValue(outputPort.Key, out var routedSlot))
            {
                routedSlot.SetRoute(signalType, inputPort.Key);
            }

            this.LogInformation("Executed switch: {input} -> {output} ({signalType}) on {key}",
                inputPort.Key, outputPort.Key, signalType, Key);

            RouteChanged?.Invoke(this, descriptor);
        }
        catch (Exception ex)
        {
            this.LogException(ex, "Error executing switch on mock midpoint {0}", Key);
        }
    }

    /// <inheritdoc />
    public void ClearRoute(object outputSelector, eRoutingSignalType signalType)
    {
        ExecuteSwitch(null, outputSelector, signalType);
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
    public void SetRoute(eRoutingSignalType signalType, string inputKey)
    {
        _currentRouteInputKeys[signalType] = inputKey;
        OutputSlotChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Clears the routed input key for the given signal type and raises <see cref="OutputSlotChanged"/> if it changed.
    /// </summary>
    public void ClearRoute(eRoutingSignalType signalType)
    {
        if (_currentRouteInputKeys.Remove(signalType))
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
