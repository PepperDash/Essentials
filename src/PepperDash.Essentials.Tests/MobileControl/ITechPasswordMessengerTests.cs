using System.Collections.Concurrent;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using Xunit;

namespace PepperDash.Essentials.Tests.MobileControl;

/// <summary>
/// A tech PIN validation result must go back to the client that entered the PIN only; broadcasting it
/// unlocked the tech pages on every panel in the room.
/// </summary>
public class ITechPasswordMessengerTests
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

    /// <summary>A room that, like the real ones, raises its result during ValidateTechPassword.</summary>
    private sealed class FakeRoom : ITechPassword, IKeyName
    {
        public string Key => "room1";
        public string Name => "Room 1";
        public int TechPasswordLength => 4;
        public bool RaiseLater { get; init; }
        /// <summary>Report results only when <see cref="ReportDeferred"/> is called, like a device callback.</summary>
        public bool Deferred { get; init; }
        public Task? Later { get; private set; }
        private readonly Queue<TechPasswordEventArgs> deferred = new();

        public event EventHandler<TechPasswordEventArgs>? TechPasswordValidateResult;
        public event EventHandler<EventArgs>? TechPasswordChanged;

        public void ValidateTechPassword(string password)
        {
            var args = new TechPasswordEventArgs(password == "1234");
            if (Deferred)
                deferred.Enqueue(args);
            else if (RaiseLater)
                Later = Task.Run(async () => { await Task.Delay(20); TechPasswordValidateResult?.Invoke(this, args); });
            else
                TechPasswordValidateResult?.Invoke(this, args);
        }

        public void SetTechPassword(string oldPassword, string newPassword) => TechPasswordChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the oldest deferred result from a context that doesn't flow from any request, as a
        /// device callback thread would.
        /// </summary>
        public void ReportDeferred()
        {
            var args = deferred.Dequeue();
            Task task;
            using (ExecutionContext.SuppressFlow())
                task = Task.Run(() => TechPasswordValidateResult?.Invoke(this, args));
            task.Wait();
        }

        /// <summary>Raises a result that no client's request asked for.</summary>
        public void RaiseUnprompted(bool isValid) => TechPasswordValidateResult?.Invoke(this, new TechPasswordEventArgs(isValid));
    }

    private static (FakeAppServer server, FakeRoom room) Create(bool raiseLater = false, bool deferred = false)
    {
        var server = new FakeAppServer();
        var room = new FakeRoom { RaiseLater = raiseLater, Deferred = deferred };
        new ITechPasswordMessenger("room1-techPassword", "/room/room1", room).RegisterWithAppServer(server);
        return (server, room);
    }

    private static void Validate(FakeAppServer server, string clientId, string pin) =>
        server.Handler!("/room/room1/validateTechPassword", clientId, new JObject { ["password"] = pin });

    private static IMobileControlMessage ValidationResult(FakeAppServer server) =>
        server.Sent.Single(m => m.Type == "/event/room/room1/passwordValidationResult");

    [Fact]
    public void SendsTheResultOnlyToTheClientThatEnteredThePin()
    {
        var (server, _) = Create();

        Validate(server, "7", "1234");

        var result = ValidationResult(server);
        result.ClientId.Should().Be("7");
        result.Content!["isValid"]!.Value<bool>().Should().BeTrue();
    }

    [Fact]
    public void AWrongPinIsAlsoReportedOnlyToThatClient()
    {
        var (server, _) = Create();

        Validate(server, "8", "0000");

        var result = ValidationResult(server);
        result.ClientId.Should().Be("8");
        result.Content!["isValid"]!.Value<bool>().Should().BeFalse();
    }

    [Fact]
    public async Task ConcurrentValidationsEachGoToTheirOwnClient()
    {
        var (server, _) = Create();

        await Task.WhenAll(Enumerable.Range(1, 20).Select(i =>
            Task.Run(() => Validate(server, i.ToString(), i % 2 == 0 ? "1234" : "0000"))));

        var results = server.Sent.Where(m => m.Type == "/event/room/room1/passwordValidationResult").ToList();
        results.Should().HaveCount(20);
        results.Should().OnlyContain(m =>
            m.Content!["isValid"]!.Value<bool>() == (int.Parse(m.ClientId) % 2 == 0));
    }

    [Fact]
    public async Task AResultTheRoomRaisesLater_FromWorkTheRequestStarted_StillGoesToThatClient()
    {
        var (server, room) = Create(raiseLater: true);

        Validate(server, "7", "1234");
        await room.Later!;

        ValidationResult(server).ClientId.Should().Be("7");
    }

    [Fact]
    public void AResultRaisedOutsideTheRequest_GoesToTheWaitingClientsInOrder()
    {
        var (server, room) = Create(deferred: true);

        Validate(server, "7", "1234");
        Validate(server, "8", "0000");
        room.ReportDeferred();
        room.ReportDeferred();

        var results = server.Sent.Where(m => m.Type == "/event/room/room1/passwordValidationResult").ToList();
        results.Select(m => m.ClientId).Should().Equal("7", "8");
        results.Select(m => m.Content!["isValid"]!.Value<bool>()).Should().Equal(true, false);
    }

    [Fact]
    public void AResultNoRequestAskedFor_IsNotSent()
    {
        var (server, room) = Create();

        room.RaiseUnprompted(true);

        server.Sent.Should().NotContain(m => m.Type == "/event/room/room1/passwordValidationResult");
    }

    [Fact]
    public void AnAnsweredRequest_IsNotAnsweredAgain()
    {
        var (server, room) = Create();

        Validate(server, "7", "1234");
        room.RaiseUnprompted(true);

        server.Sent.Where(m => m.Type == "/event/room/room1/passwordValidationResult")
            .Should().ContainSingle().Which.ClientId.Should().Be("7");
    }
}
