using FluentAssertions;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.AppServer.Messengers;
using Xunit;

namespace PepperDash.Essentials.Tests.MobileControl;

/// <summary>
/// An aggregated batch status request collects the requesting client's replies, which messengers send
/// through SendMessageObject from inside the batch's handlers, and sends them as one message.
/// </summary>
public class BatchStatusCaptureTests
{
    private static MobileControlMessage Reply(string clientId, string type, int value = 0) =>
        new() { Type = type, ClientId = clientId, Content = new JObject { ["value"] = value } };

    [Fact]
    public void CapturesTheClientsRepliesSentFromInsideTheBatch()
    {
        var capture = new BatchStatusCapture("7");

        capture.Run(() =>
        {
            BatchStatusCapture.TryCapture(Reply("7", "/device/display-1", 1)).Should().BeTrue();
            BatchStatusCapture.TryCapture(Reply("7", "/room/room1", 2)).Should().BeTrue();
        });

        var captured = capture.Close();
        captured.Select(m => m.Type).Should().Equal("/device/display-1", "/room/room1");
        captured[0].Content!["value"]!.Value<int>().Should().Be(1);
    }

    [Fact]
    public async Task AReplyFromWorkThatOutlivesTheBatch_IsSentSeparatelyAndReported()
    {
        var capture = new BatchStatusCapture("7");
        using var release = new ManualResetEventSlim();
        Task<(bool Captured, bool Missed)>? late = null;

        capture.Run(() => late = Task.Run(() =>
        {
            release.Wait(TimeSpan.FromSeconds(5));
            var captured = BatchStatusCapture.TryCapture(Reply("7", "/device/codec-1"), out var missed);
            return (captured, missed);
        }));

        capture.Close().Should().BeEmpty();
        release.Set();

        (await late!).Should().Be((false, true));
    }

    [Fact]
    public void LeavesOtherClientsMessagesAlone()
    {
        var capture = new BatchStatusCapture("7");

        capture.Run(() =>
            BatchStatusCapture.TryCapture(Reply("8", "/device/display-1")).Should().BeFalse());

        capture.Close().Should().BeEmpty();
    }

    [Fact]
    public void CapturesNothingOutsideABatch()
    {
        BatchStatusCapture.TryCapture(Reply("7", "/device/display-1")).Should().BeFalse();
    }

    [Fact]
    public async Task FollowsTheHandlersOwnBackgroundWork_UntilClosed()
    {
        var capture = new BatchStatusCapture("7");
        var startLate = new ManualResetEventSlim();
        Task? early = null, late = null;
        bool earlyCaptured = false, lateCaptured = true;

        capture.Run(() =>
        {
            // A messenger that posts its full status from its own task
            early = Task.Run(() => earlyCaptured = BatchStatusCapture.TryCapture(Reply("7", "/device/early")));
            late = Task.Run(() =>
            {
                startLate.Wait();
                lateCaptured = BatchStatusCapture.TryCapture(Reply("7", "/device/late"));
            });
        });
        await early!;

        var captured = capture.Close();
        startLate.Set();
        await late!;

        earlyCaptured.Should().BeTrue();
        captured.Select(m => m.Type).Should().Equal("/device/early");
        // After the batch has been sent, a straggler goes out normally rather than being lost.
        lateCaptured.Should().BeFalse();
    }

    [Fact]
    public async Task ConcurrentBatchesForDifferentClientsStaySeparate()
    {
        var a = new BatchStatusCapture("1");
        var b = new BatchStatusCapture("2");

        await Task.WhenAll(
            Task.Run(() => a.Run(() =>
            {
                for (var i = 0; i < 50; i++) BatchStatusCapture.TryCapture(Reply("1", $"/device/a{i}"));
            })),
            Task.Run(() => b.Run(() =>
            {
                for (var i = 0; i < 50; i++) BatchStatusCapture.TryCapture(Reply("2", $"/device/b{i}"));
            })));

        a.Close().Should().HaveCount(50).And.OnlyContain(m => m.Type.StartsWith("/device/a"));
        b.Close().Should().HaveCount(50).And.OnlyContain(m => m.Type.StartsWith("/device/b"));
    }

    [Fact]
    public void RunRestoresThePreviousCapture()
    {
        var capture = new BatchStatusCapture("7");

        capture.Run(() => { });

        BatchStatusCapture.TryCapture(Reply("7", "/device/display-1")).Should().BeFalse();
    }
}
