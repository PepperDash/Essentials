using FluentAssertions;
using PepperDash.Essentials.Core;
using Xunit;

namespace PepperDash.Essentials.Tests.Routing;

/// <summary>
/// An AudioVideo route is stored as two descriptors (Audio and Video). Releasing a destination must
/// release both, or the other one is orphaned along with its output port's in-use registration
/// (issue #1494).
/// </summary>
public class RouteDescriptorCollectionTests
{
    private sealed class FakeSource : IRoutingOutputs
    {
        public FakeSource(string key)
        {
            Key = key;
            Out = new RoutingOutputPort("out", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, "out", this);
            OutputPorts = new RoutingPortCollection<RoutingOutputPort> { Out };
        }

        public string Key { get; }
        public RoutingOutputPort Out { get; }
        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; }
    }

    private sealed class FakeSink : IRoutingInputs
    {
        public FakeSink(string key)
        {
            Key = key;
            HdmiIn1 = new RoutingInputPort("hdmiIn1", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, "hdmiIn1", this);
            HdmiIn2 = new RoutingInputPort("hdmiIn2", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, "hdmiIn2", this);
            InputPorts = new RoutingPortCollection<RoutingInputPort> { HdmiIn1, HdmiIn2 };
        }

        public string Key { get; }
        public RoutingInputPort HdmiIn1 { get; }
        public RoutingInputPort HdmiIn2 { get; }
        public RoutingPortCollection<RoutingInputPort> InputPorts { get; }
    }

    private sealed class FakeMatrix : IRoutingMidpointWithFeedback
    {
        public FakeMatrix()
        {
            In1 = new RoutingInputPort("in1", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, "in1", this);
            Out1 = new RoutingOutputPort("out1", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, "out1", this);
            InputPorts = new RoutingPortCollection<RoutingInputPort> { In1 };
            OutputPorts = new RoutingPortCollection<RoutingOutputPort> { Out1 };
        }

        public string Key => "matrix";
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

    /// <summary>What RunRouteRequest stores for an AudioVideo route: one descriptor per signal.</summary>
    private static (RouteDescriptor audio, RouteDescriptor video) AudioVideoRoute(
        FakeSource source, FakeSink sink, FakeMatrix? matrix = null)
    {
        RouteDescriptor Make(eRoutingSignalType type)
        {
            var descriptor = new RouteDescriptor(source, sink, sink.HdmiIn1, source.Out, type);
            if (matrix != null)
                descriptor.Routes.Add(new RouteSwitchDescriptor(matrix.Out1, matrix.In1));
            return descriptor;
        }

        return (Make(eRoutingSignalType.Audio), Make(eRoutingSignalType.Video));
    }

    [Fact]
    public void RemoveRouteDescriptors_RemovesBothHalvesOfAnAudioVideoRoute()
    {
        var collection = new RouteDescriptorCollection();
        var sink = new FakeSink("display");
        var other = new FakeSink("other-display");
        var (audio, video) = AudioVideoRoute(new FakeSource("laptop"), sink);
        var (otherAudio, _) = AudioVideoRoute(new FakeSource("laptop"), other);
        collection.AddRouteDescriptor(audio);
        collection.AddRouteDescriptor(video);
        collection.AddRouteDescriptor(otherAudio);

        var removed = collection.RemoveRouteDescriptors(sink);

        removed.Should().BeEquivalentTo(new[] { audio, video });
        collection.Descriptors.Should().ContainSingle().Which.Should().BeSameAs(otherAudio);
    }

    [Fact]
    public void RemoveRouteDescriptors_WithAnInputPort_LeavesOtherInputsAlone()
    {
        var collection = new RouteDescriptorCollection();
        var source = new FakeSource("laptop");
        var sink = new FakeSink("codec");
        var onIn1 = new RouteDescriptor(source, sink, sink.HdmiIn1, source.Out, eRoutingSignalType.Video);
        var onIn2 = new RouteDescriptor(source, sink, sink.HdmiIn2, source.Out, eRoutingSignalType.Video);
        collection.AddRouteDescriptor(onIn1);
        collection.AddRouteDescriptor(onIn2);

        collection.RemoveRouteDescriptors(sink, "hdmiIn1").Should().ContainSingle().Which.Should().BeSameAs(onIn1);
        collection.Descriptors.Should().ContainSingle().Which.Should().BeSameAs(onIn2);
    }

    [Fact]
    public void RemoveRouteDescriptors_WhenNothingMatches_ReturnsEmptyWithoutRaisingChanged()
    {
        var collection = new RouteDescriptorCollection();
        var changed = 0;
        collection.RouteDescriptorCollectionChanged += (_, _) => changed++;

        collection.RemoveRouteDescriptors(new FakeSink("display")).Should().BeEmpty();
        changed.Should().Be(0);
    }

    [Fact]
    public void ReleasingEveryRemovedDescriptor_ReturnsTheOutputPortToUnused()
    {
        var collection = new RouteDescriptorCollection();
        var matrix = new FakeMatrix();
        var sink = new FakeSink("display");
        var (audio, video) = AudioVideoRoute(new FakeSource("laptop"), sink, matrix);
        foreach (var descriptor in new[] { audio, video })
        {
            collection.AddRouteDescriptor(descriptor);
            descriptor.ExecuteRoutes();
        }
        matrix.Out1.InUseTracker.InUseCountFeedback.IntValue.Should().Be(2);

        foreach (var descriptor in collection.RemoveRouteDescriptors(sink))
            descriptor.ReleaseRoutes();

        matrix.Out1.InUseTracker.InUseCountFeedback.IntValue.Should().Be(0);
    }

    [Fact]
    public void ChangingSourceRepeatedly_NeverAccumulatesDescriptors()
    {
        // The issue's reproduction: cycle a display through sources, releasing before each route
        // exactly as ReleaseAndMakeRoute does. Each release must act on the previous source.
        var collection = new RouteDescriptorCollection();
        var sink = new FakeSink("display");
        var sources = new[] { "zoom", "appletv", "iptv", "signage", "zoom", "appletv" }
            .Select(key => new FakeSource(key))
            .ToList();

        string? previous = null;
        foreach (var source in sources)
        {
            var released = collection.RemoveRouteDescriptors(sink);
            released.Select(d => d.Source.Key).Distinct().Should().Equal(
                previous == null ? Array.Empty<string>() : new[] { previous });

            var (audio, video) = AudioVideoRoute(source, sink);
            collection.AddRouteDescriptor(audio);
            collection.AddRouteDescriptor(video);
            collection.Descriptors.Should().HaveCount(2);
            previous = source.Key;
        }
    }

    // ── ReplaceRouteDescriptor (issue #1496) ────────────────────────────────────────────────────

    private static RouteDescriptor OnHdmiIn1(FakeSource source, FakeSink sink, eRoutingSignalType type) =>
        new RouteDescriptor(source, sink, sink.HdmiIn1, source.Out, type);

    [Fact]
    public void ReplaceRouteDescriptor_SwapsTheDescriptorForThatPortAndSignalOnly()
    {
        var collection = new RouteDescriptorCollection();
        var sink = new FakeSink("codec");
        var laptop = new FakeSource("laptop");
        var oldAudio = OnHdmiIn1(laptop, sink, eRoutingSignalType.Audio);
        var oldVideo = OnHdmiIn1(laptop, sink, eRoutingSignalType.Video);
        var otherPort = new RouteDescriptor(laptop, sink, sink.HdmiIn2, laptop.Out, eRoutingSignalType.Video);
        collection.AddRouteDescriptor(oldAudio);
        collection.AddRouteDescriptor(oldVideo);
        collection.AddRouteDescriptor(otherPort);

        var newVideo = OnHdmiIn1(new FakeSource("bluray"), sink, eRoutingSignalType.Video);
        collection.ReplaceRouteDescriptor(newVideo).Should().BeTrue();

        collection.Descriptors.Should().BeEquivalentTo(new[] { oldAudio, newVideo, otherPort });
    }

    [Fact]
    public void ReplaceRouteDescriptor_KeepsTheExecutedDescriptorWhenTheSourceIsUnchanged()
    {
        // Feedback confirming the route that was just made must not swap out the descriptor that
        // was executed, which holds the output ports' in-use registrations.
        var collection = new RouteDescriptorCollection();
        var sink = new FakeSink("codec");
        var laptop = new FakeSource("laptop");
        var executed = OnHdmiIn1(laptop, sink, eRoutingSignalType.Video);
        collection.AddRouteDescriptor(executed);
        var changed = 0;
        collection.RouteDescriptorCollectionChanged += (_, _) => changed++;

        collection.ReplaceRouteDescriptor(OnHdmiIn1(laptop, sink, eRoutingSignalType.Video)).Should().BeFalse();

        collection.Descriptors.Should().ContainSingle().Which.Should().BeSameAs(executed);
        changed.Should().Be(0);
    }

    [Fact]
    public void ReplaceRouteDescriptor_IgnoresNull()
    {
        var collection = new RouteDescriptorCollection();
        var sink = new FakeSink("codec");
        var executed = OnHdmiIn1(new FakeSource("laptop"), sink, eRoutingSignalType.Video);
        collection.AddRouteDescriptor(executed);

        collection.ReplaceRouteDescriptor(null!).Should().BeFalse();

        collection.Descriptors.Should().ContainSingle().Which.Should().BeSameAs(executed);
    }

    [Fact]
    public void AFeedbackPassThatCannotRebuildARoute_LeavesItForTheReleaseToTearDown()
    {
        // The #1496 sequence: a port-keyed AudioVideo route is made, feedback for that port arrives
        // but can't name a source (so nothing is replaced), and then the destination is cleared.
        var collection = new RouteDescriptorCollection();
        var matrix = new FakeMatrix();
        var sink = new FakeSink("zoomRoom");
        var (audio, video) = AudioVideoRoute(new FakeSource("tx-zoom"), sink, matrix);
        foreach (var descriptor in new[] { audio, video })
        {
            collection.AddRouteDescriptor(descriptor);
            descriptor.ExecuteRoutes();
        }

        // Feedback pass with no source for either signal: no replacement is offered.

        var released = collection.RemoveRouteDescriptors(sink, "hdmiIn1");
        released.Should().BeEquivalentTo(new[] { audio, video });
        foreach (var descriptor in released)
            descriptor.ReleaseRoutes(clearRoute: true);
        matrix.Out1.InUseTracker.InUseCountFeedback.IntValue.Should().Be(0);
    }
}

