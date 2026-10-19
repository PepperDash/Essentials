using FluentAssertions;
using PepperDash.Essentials.Core;
using Xunit;

namespace PepperDash.Essentials.Tests.Routing;

/// <summary>
/// <see cref="TieLine.Type"/> drives which tie lines the route walk considers for a given signal type,
/// so it must describe what can actually flow between the two ports, not just what the destination accepts.
/// </summary>
public sealed class TieLineTests
{
    private sealed class FakeSource : IRoutingOutputs
    {
        public FakeSource(eRoutingSignalType outType)
        {
            Out = new RoutingOutputPort("out", outType, eRoutingPortConnectionType.Hdmi, "out", this);
            OutputPorts = new RoutingPortCollection<RoutingOutputPort> { Out };
        }

        public string Key => "tieLineTests-source";
        public RoutingOutputPort Out { get; }
        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; }
    }

    private sealed class FakeSink : IRoutingInputs
    {
        public FakeSink(eRoutingSignalType inType)
        {
            In = new RoutingInputPort("in", inType, eRoutingPortConnectionType.Hdmi, "in", this);
            InputPorts = new RoutingPortCollection<RoutingInputPort> { In };
        }

        public string Key => "tieLineTests-sink";
        public RoutingInputPort In { get; }
        public RoutingPortCollection<RoutingInputPort> InputPorts { get; }
    }

    [Theory]
    [InlineData(eRoutingSignalType.AudioVideo, eRoutingSignalType.AudioVideo, eRoutingSignalType.AudioVideo)]
    [InlineData(eRoutingSignalType.Audio, eRoutingSignalType.AudioVideo, eRoutingSignalType.Audio)]
    [InlineData(eRoutingSignalType.AudioVideo, eRoutingSignalType.Video, eRoutingSignalType.Video)]
    [InlineData(eRoutingSignalType.Video, eRoutingSignalType.Audio, (eRoutingSignalType)0)]
    [InlineData(eRoutingSignalType.AudioVideo | eRoutingSignalType.Usb, eRoutingSignalType.Usb, eRoutingSignalType.Usb)]
    public void Type_IsTheIntersectionOfSourceAndDestinationPortTypes(
        eRoutingSignalType sourceType, eRoutingSignalType destinationType, eRoutingSignalType expected)
    {
        var tieLine = new TieLine(new FakeSource(sourceType).Out, new FakeSink(destinationType).In);

        tieLine.Type.Should().Be(expected);
    }

    [Fact]
    public void Type_UsesTheOverrideWhenOneIsSet()
    {
        var tieLine = new TieLine(new FakeSource(eRoutingSignalType.AudioVideo).Out, new FakeSink(eRoutingSignalType.AudioVideo).In, eRoutingSignalType.Audio);

        tieLine.Type.Should().Be(eRoutingSignalType.Audio);
    }

    [Fact]
    public void Type_IgnoresTheDestinationTypeWhenAnOverrideIsSet()
    {
        var tieLine = new TieLine(new FakeSource(eRoutingSignalType.AudioVideo).Out, new FakeSink(eRoutingSignalType.Video).In)
        {
            OverrideType = eRoutingSignalType.AudioVideo
        };

        tieLine.Type.Should().Be(eRoutingSignalType.AudioVideo);
    }
}
