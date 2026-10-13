using System.Collections.Concurrent;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Devices.Common.Cameras;
using PepperDash.Essentials.Devices.Common.VideoCodec;
using Xunit;

namespace PepperDash.Essentials.Tests.MobileControl;

/// <summary>
/// A codec that is configured but hasn't connected may not have created its far-end camera feedback
/// or preset lists yet (the Cisco plugin builds them after its first sync). A status request must
/// still be answered (issue #1501).
/// </summary>
public class IHasCodecRoomPresetsMessengerTests
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

    /// <summary>A codec before its first sync: no far-end feedback and no preset lists.</summary>
    private sealed class UnconnectedCodec : EssentialsDevice, IHasCodecRoomPresets, IHasFarEndCameraControl
    {
        public UnconnectedCodec() : base("codec-1", "Codec 1") { }

        public event EventHandler<EventArgs>? CodecRoomPresetsListHasChanged;
        public List<CodecRoomPreset> NearEndPresets { get; set; } = null!;
        public List<CodecRoomPreset> FarEndRoomPresets { get; set; } = null!;
        public CameraBase FarEndCamera => null!;
        public BoolFeedback ControllingFarEndCameraFeedback { get; set; } = null!;

        public void CodecRoomPresetSelect(int preset) { }
        public void CodecRoomPresetStore(int preset, string description) { }
        public void SelectFarEndPreset(int preset) { }
        public void RaiseListChanged() => CodecRoomPresetsListHasChanged?.Invoke(this, EventArgs.Empty);
    }

    private static (FakeAppServer server, UnconnectedCodec codec) Create()
    {
        var server = new FakeAppServer();
        var codec = new UnconnectedCodec();
        new IHasCodecRoomPresetsMessenger("codec-1-presets", "/device/codec-1", codec).RegisterWithAppServer(server);
        return (server, codec);
    }

    [Fact]
    public void AStatusRequestIsAnswered_BeforeTheCodecHasConnected()
    {
        var (server, _) = Create();

        server.Handler!("/device/codec-1/fullStatus", "7", new JObject());

        server.Sent.Should().ContainSingle(m => m.Type == "/device/codec-1" && m.ClientId == "7");
    }

    [Fact]
    public void OnceConnected_TheNearEndPresetsAreSent()
    {
        var (server, codec) = Create();
        codec.NearEndPresets = new List<CodecRoomPreset> { new(1, "Wide", true, true) };
        codec.ControllingFarEndCameraFeedback = new BoolFeedback("farEnd", () => false);

        server.Handler!("/device/codec-1/fullStatus", "7", new JObject());

        var reply = server.Sent.Single(m => m.Type == "/device/codec-1");
        reply.Content!["presets"]!.Single()["description"]!.Value<string>().Should().Be("Wide");
    }
}
