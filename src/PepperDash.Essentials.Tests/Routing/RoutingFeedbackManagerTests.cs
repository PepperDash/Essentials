using System.Reflection;
using FluentAssertions;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Routing;
using Xunit;

namespace PepperDash.Essentials.Tests.Routing;

/// <summary>
/// Drives <see cref="RoutingFeedbackManager"/>'s per-destination update directly (it is private and
/// normally runs off a debounce timer) against a source -> matrix -> sink topology. The manager works
/// on the global <see cref="TieLineCollection.Default"/> and <see cref="RouteDescriptorCollection.DefaultCollection"/>,
/// so every device here has a key unique to the test instance and is cleaned up afterwards.
/// </summary>
/// <remarks>
/// Only the path where the walk finds no source is tested here. Any path that rebuilds a route calls
/// <c>Extensions.GetRouteToSource</c>, whose static constructor subscribes to the real Crestron SDK's
/// <c>CrestronEnvironment.ProgramStatusEventHandler</c> - which intermittently hangs off a processor.
/// Replacement itself is covered by <c>RouteDescriptorCollectionTests</c>.
/// </remarks>
public sealed class RoutingFeedbackManagerTests : IDisposable
{
    private sealed class FakeSource : IRoutingSource
    {
        public FakeSource(string key)
        {
            Key = key;
            Out = new RoutingOutputPort("out", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, "out", this);
            OutputPorts = new RoutingPortCollection<RoutingOutputPort> { Out };
        }

        public string Key { get; }
        public string Name => Key;
        public RoutingOutputPort Out { get; }
        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; }
    }

    private sealed class FakeMatrix : IRoutingMidpointWithFeedback
    {
        public FakeMatrix(string key)
        {
            Key = key;
            In1 = new RoutingInputPort("in1", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, "in1", this);
            Out1 = new RoutingOutputPort("out1", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, "out1", this);
            InputPorts = new RoutingPortCollection<RoutingInputPort> { In1 };
            OutputPorts = new RoutingPortCollection<RoutingOutputPort> { Out1 };
        }

        public string Key { get; }
        public RoutingInputPort In1 { get; }
        public RoutingOutputPort Out1 { get; }
        public RoutingPortCollection<RoutingInputPort> InputPorts { get; }
        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; }
        public List<RouteSwitchDescriptor> CurrentRoutes { get; } = new List<RouteSwitchDescriptor>();
#pragma warning disable CS0067 // Required by the interface; these tests never raise it.
        public event RouteChangedEventHandler? RouteChanged;
#pragma warning restore CS0067

        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType) { }
        public void ClearRoute(object outputSelector, eRoutingSignalType signalType) { }
    }

    /// <summary>A sink that, like MockVC, never reports a current input - so routes to it are port-keyed.</summary>
    private sealed class FakeSink : IRoutingSinkWithFeedback
    {
        public FakeSink(string key)
        {
            Key = key;
            HdmiIn1 = new RoutingInputPort("hdmiIn1", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, "hdmiIn1", this);
            InputPorts = new RoutingPortCollection<RoutingInputPort> { HdmiIn1 };
        }

        public string Key { get; }
        public string Name => Key;
        public RoutingInputPort HdmiIn1 { get; }
        public RoutingPortCollection<RoutingInputPort> InputPorts { get; }
        public RoutingInputPort? CurrentInputPort => null;
        public Dictionary<eRoutingSignalType, IRoutingSource> CurrentSources { get; } = new();
        public Dictionary<eRoutingSignalType, string> CurrentSourceKeys { get; } = new();
#pragma warning disable CS0067
        public event InputChangedEventHandler? InputChanged;
        public event EventHandler<CurrentSourcesChangedEventArgs>? CurrentSourcesChanged;
#pragma warning restore CS0067

        public void ExecuteSwitch(object inputSelector) { }

        public void SetCurrentSource(eRoutingSignalType signalType, IRoutingSource sourceDevice)
        {
            CurrentSources[signalType] = sourceDevice;
            CurrentSourceKeys[signalType] = sourceDevice?.Key!;
        }
    }

    private static readonly MethodInfo UpdateDestinationImmediate =
        typeof(RoutingFeedbackManager).GetMethod("UpdateDestinationImmediate", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("RoutingFeedbackManager.UpdateDestinationImmediate not found");

    private readonly string _id = Guid.NewGuid().ToString("N")[..8];
    private readonly FakeSource _zoom;
    private readonly FakeMatrix _matrix;
    private readonly FakeSink _sink;
    private readonly List<TieLine> _tieLines;
    private readonly RoutingFeedbackManager _manager;

    public RoutingFeedbackManagerTests()
    {
        _zoom = new FakeSource($"tx-zoom-{_id}");
        _matrix = new FakeMatrix($"nvx-{_id}");
        _sink = new FakeSink($"zoomRoom-{_id}");
        _tieLines = new List<TieLine>
        {
            new TieLine(_zoom.Out, _matrix.In1),
            new TieLine(_matrix.Out1, _sink.HdmiIn1),
        };
        lock (TieLineCollection.Default) TieLineCollection.Default.AddRange(_tieLines);
        _manager = new RoutingFeedbackManager($"rfm-{_id}", "Routing Feedback Manager");
    }

    public void Dispose()
    {
        lock (TieLineCollection.Default)
            foreach (var tieLine in _tieLines) TieLineCollection.Default.Remove(tieLine);
        RouteDescriptorCollection.DefaultCollection.RemoveRouteDescriptors(_sink);
    }

    private void RunFeedbackPass() => UpdateDestinationImmediate.Invoke(_manager, new object[] { _sink, _sink.HdmiIn1 });

    private (RouteDescriptor audio, RouteDescriptor video) RouteToSink(FakeSource source)
    {
        var audio = new RouteDescriptor(source, _sink, _sink.HdmiIn1, source.Out, eRoutingSignalType.Audio);
        var video = new RouteDescriptor(source, _sink, _sink.HdmiIn1, source.Out, eRoutingSignalType.Video);
        RouteDescriptorCollection.DefaultCollection.AddRouteDescriptor(audio);
        RouteDescriptorCollection.DefaultCollection.AddRouteDescriptor(video);
        return (audio, video);
    }

    private List<RouteDescriptor> DescriptorsForSink() =>
        RouteDescriptorCollection.DefaultCollection.Descriptors.Where(d => d.Destination == _sink).ToList();

    [Fact]
    public void WhenTheWalkFindsNoSource_TheRouteDescriptorsSurviveForTheRelease()
    {
        // Issue #1496: the route was made, but the matrix's feedback doesn't (yet) show it.
        var (audio, video) = RouteToSink(_zoom);

        RunFeedbackPass();

        _sink.CurrentSourceKeys[eRoutingSignalType.Video].Should().BeNull("the walk found nothing routed");
        DescriptorsForSink().Should().BeEquivalentTo(new[] { audio, video });
    }
}
