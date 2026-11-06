using FluentAssertions;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Routing;
using Xunit;

namespace PepperDash.Essentials.Tests.Routing;

/// <summary>
/// Routing runs off a processor (issue #1553): <c>ReleaseAndMakeRoute</c> goes through
/// <c>Extensions</c>' static route queue, a <c>GenericQueue</c>, which used to need the Crestron SDK's
/// <c>CrestronEnvironment</c> just to be created.
/// </summary>
[Collection(GlobalRoutingStateCollection.Name)]
public sealed class ReleaseAndMakeRouteTests : IDisposable
{
    private sealed class FakeSource : IRoutingSource
    {
        public FakeSource(string key)
        {
            Key = key;
            HdmiOut1 = new RoutingOutputPort("hdmiOut1", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, "hdmiOut1", this);
            OutputPorts = new RoutingPortCollection<RoutingOutputPort> { HdmiOut1 };
        }

        public string Key { get; }
        public string Name => Key;
        public RoutingOutputPort HdmiOut1 { get; }
        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; }
    }

    /// <summary>A display: switching selects one of its inputs.</summary>
    private sealed class FakeDisplay : IRoutingSinkWithFeedback
    {
        public FakeDisplay(string key)
        {
            Key = key;
            HdmiIn1 = new RoutingInputPort("hdmiIn1", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, "hdmiIn1Selector", this);
            InputPorts = new RoutingPortCollection<RoutingInputPort> { HdmiIn1 };
        }

        public string Key { get; }
        public string Name => Key;
        public RoutingInputPort HdmiIn1 { get; }
        public RoutingPortCollection<RoutingInputPort> InputPorts { get; }
        public RoutingInputPort? CurrentInputPort => null;
        public Dictionary<eRoutingSignalType, IRoutingSource> CurrentSources { get; } = new();
        public Dictionary<eRoutingSignalType, string> CurrentSourceKeys { get; } = new();
#pragma warning disable CS0067 // Required by the interface; these tests never raise it.
        public event InputChangedEventHandler? InputChanged;
        public event EventHandler<CurrentSourcesChangedEventArgs>? CurrentSourcesChanged;
#pragma warning restore CS0067

        public object? SelectedInput { get; private set; }
        public ManualResetEventSlim Switched { get; } = new();

        public void ExecuteSwitch(object inputSelector)
        {
            SelectedInput = inputSelector;
            Switched.Set();
        }

        public void SetCurrentSource(eRoutingSignalType signalType, IRoutingSource sourceDevice)
        {
            CurrentSources[signalType] = sourceDevice;
            CurrentSourceKeys[signalType] = sourceDevice?.Key!;
        }
    }

    private readonly string _id = Guid.NewGuid().ToString("N")[..8];
    private readonly FakeSource _codec;
    private readonly FakeDisplay _display;
    private readonly TieLine _tieLine;

    public ReleaseAndMakeRouteTests()
    {
        _codec = new FakeSource($"codec-{_id}");
        _display = new FakeDisplay($"display-{_id}");
        _tieLine = new TieLine(_codec.HdmiOut1, _display.HdmiIn1);
        lock (TieLineCollection.Default) TieLineCollection.Default.Add(_tieLine);
    }

    public void Dispose()
    {
        lock (TieLineCollection.Default) TieLineCollection.Default.Remove(_tieLine);
        RouteDescriptorCollection.DefaultCollection.RemoveRouteDescriptors(_display);
        _display.Switched.Dispose();
    }

    [Fact]
    public void SwitchesTheDisplayToTheTiedInput()
    {
        _display.ReleaseAndMakeRoute(_codec, eRoutingSignalType.AudioVideo);

        _display.Switched.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue("the route queue should run the route");
        _display.SelectedInput.Should().Be("hdmiIn1Selector");
    }
}
