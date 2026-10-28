using System.Collections.Concurrent;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Routing;
using Xunit;

namespace PepperDash.Essentials.Tests.MobileControl;

/// <summary>
/// The routing graph messenger gives a touchpanel the same graph, live routes and routing commands
/// that the Dev Tools app gets from the web API.
/// </summary>
public sealed class RoutingGraphMessengerTests : IDisposable
{
    private sealed class FakeAppServer : IMobileControl
    {
        public Action<string, string, JToken>? Handler { get; private set; }
        public ConcurrentQueue<IMobileControlMessage> Sent { get; } = new();

        public string Key => "appServer";
        public string Host => "";
        public string ClientAppUrl => "";
        public string SystemUuid => "";
        public BoolFeedback ApiOnlineAndAuthorized => null!;

        public void SendMessageObject(IMobileControlMessage o) => Sent.Enqueue(o);
        public void AddAction<T>(T messenger, Action<string, string, JToken> action) where T : IMobileControlMessenger => Handler = action;
        public void RemoveAction(string key) { }
        public void AddDeviceMessenger(IMobileControlMessenger messenger) { }
        public bool CheckForDeviceMessenger(string key) => false;
        public IMobileControlRoomMessenger GetRoomMessenger(string key) => null!;
        public void AddDefaultMessengersForDevice(EssentialsDevice device, IEnumerable<Type>? interfaces = null) { }
    }

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

    /// <summary>A one-in, one-out matrix that records its route and raises RouteChanged, like a real one.</summary>
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
        public event RouteChangedEventHandler? RouteChanged;

        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
        {
            var route = new RouteSwitchDescriptor(Out1, In1);
            CurrentRoutes.Clear();
            CurrentRoutes.Add(route);
            RouteChanged?.Invoke(this, route);
        }

        public void ClearRoute(object outputSelector, eRoutingSignalType signalType)
        {
            CurrentRoutes.Clear();
            RouteChanged?.Invoke(this, null!);
        }
    }

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
        public RoutingInputPort? CurrentInputPort { get; private set; }
        public Dictionary<eRoutingSignalType, IRoutingSource> CurrentSources { get; } = new();
        public Dictionary<eRoutingSignalType, string> CurrentSourceKeys { get; } = new();
#pragma warning disable CS0067 // Required by the interface; these tests never raise it.
        public event InputChangedEventHandler? InputChanged;
#pragma warning restore CS0067
        public event EventHandler<CurrentSourcesChangedEventArgs>? CurrentSourcesChanged;

        public void ExecuteSwitch(object inputSelector) { }

        public void SetCurrentSource(eRoutingSignalType signalType, IRoutingSource sourceDevice)
        {
            CurrentInputPort = sourceDevice == null ? null : HdmiIn1;
            CurrentSources[signalType] = sourceDevice!;
            CurrentSourceKeys[signalType] = sourceDevice?.Key!;
            CurrentSourcesChanged?.Invoke(this, null!);
        }
    }

    private const string Path = "/device/routingGraph";

    private readonly string _id = Guid.NewGuid().ToString("N")[..8];
    private readonly FakeSource _source;
    private readonly FakeMatrix _matrix;
    private readonly FakeSink _sink;
    private readonly List<TieLine> _tieLines;
    private readonly FakeAppServer _server = new();

    public RoutingGraphMessengerTests()
    {
        _source = new FakeSource($"laptop-{_id}");
        _matrix = new FakeMatrix($"matrix-{_id}");
        _sink = new FakeSink($"display-{_id}");

        // Outside a running program, DeviceManager only accepts devices once Initialize has run.
        typeof(DeviceManager).GetProperty(nameof(DeviceManager.AddDeviceEnabled))!.SetValue(null, true);
        DeviceManager.AddDevice(_source);
        DeviceManager.AddDevice(_matrix);
        DeviceManager.AddDevice(_sink);

        _tieLines = new List<TieLine>
        {
            new TieLine(_source.Out, _matrix.In1),
            new TieLine(_matrix.Out1, _sink.HdmiIn1),
        };
        lock (TieLineCollection.Default) TieLineCollection.Default.AddRange(_tieLines);

        new RoutingGraphMessenger($"routingGraphMessenger-{_id}", Path).RegisterWithAppServer(_server);
    }

    public void Dispose()
    {
        lock (TieLineCollection.Default)
            foreach (var tieLine in _tieLines) TieLineCollection.Default.Remove(tieLine);
        DeviceManager.RemoveDevice(_source);
        DeviceManager.RemoveDevice(_matrix);
        DeviceManager.RemoveDevice(_sink);
    }

    private void Send(string action, string clientId, JObject? content = null) =>
        _server.Handler!(Path + action, clientId, content ?? new JObject());

    private List<IMobileControlMessage> SentOfType(string type) => _server.Sent.Where(m => m.Type == type).ToList();

    private JToken RunCommand(string clientId, JObject command)
    {
        Send("/routingCommand", clientId, command);
        return SentOfType(RoutingGraphMessenger.CommandResultType).Last(m => m.ClientId == clientId).Content!;
    }

    private async Task<JToken> NextRoutesUpdate(int previousCount)
    {
        // Updates are debounced, so wait for the next status message after previousCount.
        for (var i = 0; i < 50; i++)
        {
            var updates = SentOfType(Path);
            if (updates.Count > previousCount)
                return updates.Last().Content!;
            await Task.Delay(20);
        }

        throw new TimeoutException("No routing update was sent.");
    }

    [Fact]
    public void FullStatus_SendsTheGraphToTheAskingClient()
    {
        Send("/fullStatus", "7");

        var status = SentOfType(Path).Should().ContainSingle().Which;
        status.ClientId.Should().Be("7");

        var content = status.Content!;
        content["devices"]!.Select(d => d["key"]!.ToString())
            .Should().Contain(new[] { _source.Key, _matrix.Key, _sink.Key });
        content["tieLines"]!.Should().Contain(t =>
            t["sourceDeviceKey"]!.ToString() == _matrix.Key && t["destinationDeviceKey"]!.ToString() == _sink.Key);
    }

    [Fact]
    public void FullStatus_ListsUnroutedMidpointsAndSinksWithEmptyRoutes()
    {
        // The client merges each message into the last, so a device that isn't listed would keep
        // showing whatever route it had before.
        Send("/fullStatus", "7");

        var content = SentOfType(Path).Single().Content!;
        content["midpointRoutes"]![_matrix.Key]!.Should().BeEmpty();
        content["sinkRoutes"]![_sink.Key]!.Should().BeEmpty();
    }

    [Fact]
    public void RoutingCommand_RepliesOnlyToTheSender_WithItsRequestId()
    {
        var result = RunCommand("7", new JObject
        {
            ["requestId"] = "abc",
            ["command"] = "midpointSwitch",
            ["deviceKey"] = _matrix.Key,
            ["inputPortKey"] = "in1",
            ["outputPortKey"] = "out1",
            ["signalType"] = "AudioVideo"
        });

        SentOfType(RoutingGraphMessenger.CommandResultType).Should().OnlyContain(m => m.ClientId == "7");
        result["requestId"]!.ToString().Should().Be("abc");
        result["statusCode"]!.Value<int>().Should().Be(200);
        result["response"]!["status"]!.ToString().Should().Be("executed");
        _matrix.CurrentRoutes.Should().ContainSingle();
    }

    [Fact]
    public void RoutingCommand_PassesErrorsThrough()
    {
        var unknownDevice = RunCommand("7", new JObject
        {
            ["requestId"] = "1",
            ["command"] = "midpointSwitch",
            ["deviceKey"] = $"missing-{_id}",
            ["inputPortKey"] = "in1",
            ["outputPortKey"] = "out1",
            ["signalType"] = "AudioVideo"
        });
        unknownDevice["statusCode"]!.Value<int>().Should().Be(404);
        unknownDevice["response"]!["status"]!.ToString().Should().Be("error");

        var unknownPort = RunCommand("7", new JObject
        {
            ["requestId"] = "2",
            ["command"] = "midpointSwitch",
            ["deviceKey"] = _matrix.Key,
            ["inputPortKey"] = "in9",
            ["outputPortKey"] = "out1",
            ["signalType"] = "AudioVideo"
        });
        unknownPort["statusCode"]!.Value<int>().Should().Be(422);
        unknownPort["response"]!["error"]!["code"]!.ToString().Should().Be("portNotFound");
    }

    [Fact]
    public async Task ARouteChange_SendsTheCurrentRoutesToSubscribedClients()
    {
        Send("/fullStatus", "7");
        var sent = SentOfType(Path).Count;

        _matrix.ExecuteSwitch("in1", "out1", eRoutingSignalType.AudioVideo);
        _sink.SetCurrentSource(eRoutingSignalType.Video, _source);

        var update = await NextRoutesUpdate(sent);
        SentOfType(Path).Last().ClientId.Should().Be("7");
        update["midpointRoutes"]![_matrix.Key]!.Single()["inputPortKey"]!.ToString().Should().Be("in1");
        update["sinkRoutes"]![_sink.Key]!.Single()["sourceDeviceKey"]!.ToString().Should().Be(_source.Key);
    }

    [Fact]
    public async Task AClearedRoute_IsSentAsAnEmptyList()
    {
        Send("/fullStatus", "7");
        _matrix.ExecuteSwitch("in1", "out1", eRoutingSignalType.AudioVideo);
        _sink.SetCurrentSource(eRoutingSignalType.Video, _source);
        await NextRoutesUpdate(1);
        var sent = SentOfType(Path).Count;

        _matrix.ClearRoute("out1", eRoutingSignalType.AudioVideo);
        _sink.SetCurrentSource(eRoutingSignalType.Video, null!);

        var update = await NextRoutesUpdate(sent);
        update["midpointRoutes"]![_matrix.Key]!.Should().BeEmpty();
        update["sinkRoutes"]![_sink.Key]!.Should().BeEmpty();
    }
}
