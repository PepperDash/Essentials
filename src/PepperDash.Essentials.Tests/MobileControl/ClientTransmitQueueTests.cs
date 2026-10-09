using System.Collections.Concurrent;
using System.Diagnostics;
using FluentAssertions;
using PepperDash.Essentials.WebSocketServer;
using Xunit;

namespace PepperDash.Essentials.Tests.MobileControl;

/// <summary>
/// With <c>directServer.perClientQueues</c> on, each client's messages are sent in order by its own
/// task, so a client that is slow to receive only delays itself.
/// </summary>
public class ClientTransmitQueueTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private static OutboundClientMessage Message(string payload, string? type = null) =>
        new(payload, type!, Stopwatch.GetTimestamp());

    /// <summary>Waits until <paramref name="condition"/> holds, failing rather than hanging.</summary>
    private static async Task Eventually(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow + Timeout;
        while (!condition())
        {
            if (DateTime.UtcNow > deadline) throw new TimeoutException("Condition not met in time");
            await Task.Delay(5);
        }
    }

    [Fact]
    public async Task SendsEachClientsMessagesInOrder()
    {
        var sent = new ConcurrentQueue<string>();
        using var queue = new ClientTransmitQueue(sent.Enqueue, () => true);

        for (var i = 0; i < 200; i++) queue.Enqueue(Message($"m{i}"));

        await Eventually(() => sent.Count == 200);
        sent.Should().Equal(Enumerable.Range(0, 200).Select(i => $"m{i}"));
    }

    [Fact]
    public async Task ASlowClientDoesNotHoldUpAnother()
    {
        using var release = new ManualResetEventSlim();
        var slowSent = new ConcurrentQueue<string>();
        var fastSent = new ConcurrentQueue<string>();

        // The slow client's send blocks, like a websocket whose receiver isn't reading.
        using var slow = new ClientTransmitQueue(p => { release.Wait(Timeout); slowSent.Enqueue(p); }, () => true);
        using var fast = new ClientTransmitQueue(fastSent.Enqueue, () => true);

        slow.Enqueue(Message("slow-1"));
        slow.Enqueue(Message("slow-2"));
        for (var i = 0; i < 20; i++) fast.Enqueue(Message($"fast-{i}"));

        await Eventually(() => fastSent.Count == 20);
        slowSent.Should().BeEmpty("the slow client is still blocked on its first send");

        release.Set();
        await Eventually(() => slowSent.Count == 2);
        slowSent.Should().Equal("slow-1", "slow-2");
    }

    [Fact]
    public async Task EnqueueNeverBlocksOnASlowClient()
    {
        using var release = new ManualResetEventSlim();
        using var queue = new ClientTransmitQueue(_ => release.Wait(Timeout), () => true);

        var timer = Stopwatch.StartNew();
        for (var i = 0; i < 1000; i++) queue.Enqueue(Message($"m{i}"));
        timer.Stop();

        timer.ElapsedMilliseconds.Should().BeLessThan(1000);
        release.Set();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task DropsMessagesWhileTheClientCannotReceive()
    {
        var sent = new ConcurrentQueue<string>();
        var connected = false;
        var refusals = 0;
        using var queue = new ClientTransmitQueue(sent.Enqueue, () =>
        {
            if (connected) return true;
            Interlocked.Increment(ref refusals);
            return false;
        });

        queue.Enqueue(Message("while-disconnected"));
        // Wait until the queue has actually checked the client and found it disconnected.
        await Eventually(() => Volatile.Read(ref refusals) == 1);

        connected = true;
        queue.Enqueue(Message("after-reconnect"));
        await Eventually(() => sent.Count == 1);

        sent.Should().Equal("after-reconnect");
    }

    [Fact]
    public async Task ReportsEachDroppedMessage()
    {
        var dropped = new ConcurrentQueue<string>();
        using var queue = new ClientTransmitQueue(_ => { }, () => false, onDropped: m => dropped.Enqueue(m.Type));

        queue.Enqueue(Message("a", "/system/batchDeviceStatus"));
        queue.Enqueue(Message("b", "/device/x"));

        await Eventually(() => dropped.Count == 2);
        dropped.Should().Equal("/system/batchDeviceStatus", "/device/x");
    }

    [Fact]
    public async Task KeepsGoingAfterASendFails()
    {
        var sent = new ConcurrentQueue<string>();
        var errors = new ConcurrentQueue<Exception>();
        using var queue = new ClientTransmitQueue(
            p => { if (p == "bad") throw new InvalidOperationException("socket error"); sent.Enqueue(p); },
            () => true,
            onError: errors.Enqueue);

        queue.Enqueue(Message("before"));
        queue.Enqueue(Message("bad"));
        queue.Enqueue(Message("after"));

        await Eventually(() => sent.Count == 2);
        sent.Should().Equal("before", "after");
        errors.Should().ContainSingle().Which.Message.Should().Be("socket error");
    }

    [Fact]
    public async Task ReportsHowLongEachMessageWaited()
    {
        var reports = new ConcurrentQueue<(string Type, double QueueMs, double SendMs)>();
        using var queue = new ClientTransmitQueue(
            _ => Thread.Sleep(20),
            () => true,
            onSent: (m, queueMs, sendMs) => reports.Enqueue((m.Type, queueMs, sendMs)));

        queue.Enqueue(Message("first", "/device/a"));
        queue.Enqueue(Message("second", "/system/initialSyncComplete"));

        await Eventually(() => reports.Count == 2);
        var second = reports.Last();
        second.Type.Should().Be("/system/initialSyncComplete");
        second.QueueMs.Should().BeGreaterThanOrEqualTo(15, "it waited behind the first message's send");
        second.SendMs.Should().BeGreaterThanOrEqualTo(15);
    }

    [Fact]
    public void AQueueThatFillsUp_RefusesMoreAndReportsItOnce()
    {
        using var release = new ManualResetEventSlim();
        var overflows = 0;
        using var queue = new ClientTransmitQueue(_ => release.Wait(Timeout), () => true,
            capacity: 3, onOverflow: () => Interlocked.Increment(ref overflows));

        var accepted = Enumerable.Range(0, 10).Count(i => queue.Enqueue(Message($"m{i}")));

        accepted.Should().BeInRange(3, 4, "three wait in the queue, and one may already be sending");
        overflows.Should().Be(1);
        release.Set();
    }

    [Fact]
    public void ADisposedQueue_IsNotReportedAsFull()
    {
        var overflows = 0;
        var queue = new ClientTransmitQueue(_ => { }, () => true,
            capacity: 3, onOverflow: () => Interlocked.Increment(ref overflows));

        queue.Dispose();

        queue.Enqueue(Message("after-dispose")).Should().BeFalse();
        overflows.Should().Be(0);
    }

    [Fact]
    public async Task AfterDisposeItAcceptsNothingAndStops()
    {
        var sent = new ConcurrentQueue<string>();
        var queue = new ClientTransmitQueue(sent.Enqueue, () => true);
        queue.Enqueue(Message("queued-before-dispose"));

        queue.Dispose();

        queue.Enqueue(Message("after-dispose")).Should().BeFalse();
        (await Task.WhenAny(queue.Completion, Task.Delay(Timeout))).Should().BeSameAs(queue.Completion);
        sent.Should().Equal("queued-before-dispose");
    }
}
