using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Devices.Common.Generic;

public class GenericSink : EssentialsDevice, IRoutingSinkWithInputPort
{
    public GenericSink(string key, string name) : base(key, name)
    {
        InputPorts = new RoutingPortCollection<RoutingInputPort>();

        var inputPort = new RoutingInputPort(RoutingPortNames.AnyVideoIn, eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, null, this);            

        InputPorts.Add(inputPort);
    }

    public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

    public string CurrentSourceInfoKey { get; set; }

    private SourceListItem _currentSource;
    public SourceListItem CurrentSourceInfo {
        get => _currentSource;
        set {
            if(value == _currentSource)
            {
                return;
            }

            CurrentSourceChange?.Invoke(_currentSource, ChangeType.WillChange);

            _currentSource = value;

            CurrentSourceChange?.Invoke(_currentSource, ChangeType.DidChange);
        }
    }

    public RoutingInputPort CurrentInputPort => InputPorts[0];

    public event SourceInfoChangeHandler CurrentSourceChange;
}

public class GenericSinkFactory : EssentialsDeviceFactory<GenericSink>
{
    public GenericSinkFactory()
    {
        TypeNames = new List<string>() { "genericsink", "genericdestination" };
    }

    public override EssentialsDevice BuildDevice(DeviceConfig dc)
    {
        Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new Generic Sink Device");
        return new GenericSink(dc.Key, dc.Name);
    }
}
