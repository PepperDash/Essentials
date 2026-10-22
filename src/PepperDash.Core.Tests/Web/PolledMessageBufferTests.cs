using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using PepperDash.Core.Web;
using Xunit;

namespace PepperDash.Core.Tests.Web;

/// <summary>
/// Tests for <see cref="PolledMessageBuffer"/>, which carries the debug console and routing feedback
/// streams over plain HTTP where their WebSocket ports can't be reached.
/// </summary>
public class PolledMessageBufferTests
{
    private DateTime _now = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);

    private PolledMessageBuffer Create(int capacity = 10) =>
        new PolledMessageBuffer(capacity, TimeSpan.FromSeconds(60), () => _now);

    [Fact]
    public void Add_IgnoresMessagesUntilAClientPolls()
    {
        var buffer = Create();

        buffer.Add("before").Should().BeFalse();
        buffer.LatestId.Should().Be(0);

        var cursor = buffer.Activate();
        buffer.Add("after").Should().BeTrue();

        var result = buffer.Poll(cursor, TimeSpan.Zero);
        result.Messages.Should().Equal("after");
        result.Next.Should().Be(1);
        result.Missed.Should().BeFalse();
    }

    [Fact]
    public void Poll_ReturnsOnlyMessagesAfterTheCursor()
    {
        var buffer = Create();
        buffer.Activate();
        buffer.Add("a");
        buffer.Add("b");
        buffer.Add("c");

        var result = buffer.Poll(1, TimeSpan.Zero);

        result.Messages.Should().Equal("b", "c");
        result.Next.Should().Be(3);
        result.Missed.Should().BeFalse();
    }

    [Fact]
    public void Poll_WithNothingNew_KeepsTheCursor()
    {
        var buffer = Create();
        buffer.Activate();
        buffer.Add("a");

        var result = buffer.Poll(1, TimeSpan.Zero);

        result.Messages.Should().BeEmpty();
        result.Next.Should().Be(1);
        result.Missed.Should().BeFalse();
    }

    [Fact]
    public void Poll_LimitsTheBatchAndContinuesFromItsEnd()
    {
        var buffer = Create();
        buffer.Activate();
        foreach (var m in new[] { "a", "b", "c", "d" }) buffer.Add(m);

        var first = buffer.Poll(0, TimeSpan.Zero, maxMessages: 3);
        var second = buffer.Poll(first.Next, TimeSpan.Zero, maxMessages: 3);

        first.Messages.Should().Equal("a", "b", "c");
        second.Messages.Should().Equal("d");
        second.Next.Should().Be(4);
    }

    [Fact]
    public void Poll_ReportsMessagesDroppedByTheCapacity()
    {
        var buffer = Create(capacity: 2);
        buffer.Activate();
        foreach (var m in new[] { "a", "b", "c", "d" }) buffer.Add(m);

        var result = buffer.Poll(0, TimeSpan.Zero);

        result.Messages.Should().Equal("c", "d");
        result.Missed.Should().BeTrue();
        result.Next.Should().Be(4);
    }

    [Fact]
    public void Poll_TreatsACursorAheadOfTheBufferAsARestart()
    {
        var buffer = Create();
        buffer.Activate();
        buffer.Add("a");

        var result = buffer.Poll(50, TimeSpan.Zero);

        result.Messages.Should().BeEmpty();
        result.Missed.Should().BeTrue();
        result.Next.Should().Be(1);
    }

    [Fact]
    public void Buffer_StopsCollectingAfterTheIdleTimeout()
    {
        var buffer = Create();
        buffer.Activate();
        buffer.Add("a").Should().BeTrue();

        _now = _now.AddSeconds(61);

        buffer.IsActive.Should().BeFalse();
        buffer.Add("b").Should().BeFalse();
    }

    [Fact]
    public void Poll_AfterGoingIdle_ReportsMissedMessages()
    {
        var buffer = Create();
        var cursor = buffer.Activate();
        buffer.Add("a");

        // A background tab that stopped polling for longer than the idle timeout
        _now = _now.AddSeconds(90);

        var result = buffer.Poll(cursor, TimeSpan.FromSeconds(5));

        result.Missed.Should().BeTrue();
        result.Messages.Should().BeEmpty();
        result.Next.Should().Be(1);
        buffer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Poll_AfterStop_ReportsMissedMessages()
    {
        var buffer = Create();
        var cursor = buffer.Activate();
        buffer.Deactivate();

        buffer.Poll(cursor, TimeSpan.Zero).Missed.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ClearsMessagesAndTheGapIsReportedAsMissed()
    {
        var buffer = Create();
        buffer.Activate();
        buffer.Add("a");
        buffer.Deactivate();

        buffer.IsActive.Should().BeFalse();

        buffer.Activate();
        buffer.Add("b");

        var result = buffer.Poll(0, TimeSpan.Zero);
        result.Messages.Should().Equal("b");
        result.Missed.Should().BeTrue();
        result.Next.Should().Be(2);
    }

    [Fact]
    public async Task Poll_ReturnsAsSoonAsAMessageArrives()
    {
        var buffer = new PolledMessageBuffer(10, TimeSpan.FromSeconds(60));
        var cursor = buffer.Activate();
        var stopwatch = Stopwatch.StartNew();

        var poll = Task.Run(() => buffer.Poll(cursor, TimeSpan.FromSeconds(5)));
        await Task.Delay(200);
        buffer.Add("late");

        var result = await poll;
        stopwatch.Stop();

        result.Messages.Should().Equal("late");
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void Poll_GivesUpAfterTheWait()
    {
        var buffer = new PolledMessageBuffer(10, TimeSpan.FromSeconds(60));
        var cursor = buffer.Activate();
        var stopwatch = Stopwatch.StartNew();

        var result = buffer.Poll(cursor, TimeSpan.FromMilliseconds(300));

        stopwatch.Elapsed.Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(250));
        result.Messages.Should().BeEmpty();
        result.Next.Should().Be(cursor);
    }

    [Fact]
    public async Task Deactivate_ReleasesAWaitingPoll()
    {
        var buffer = new PolledMessageBuffer(10, TimeSpan.FromSeconds(60));
        var cursor = buffer.Activate();
        var stopwatch = Stopwatch.StartNew();

        var poll = Task.Run(() => buffer.Poll(cursor, TimeSpan.FromSeconds(5)));
        await Task.Delay(200);
        buffer.Deactivate();

        var result = await poll;
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3));
        result.Messages.Should().BeEmpty();

        // The released poll must not switch the stopped session back on
        buffer.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Poll_ClampsTheWait()
    {
        PolledMessageBuffer.MaxWait.Should().BeLessThanOrEqualTo(TimeSpan.FromSeconds(10));
        var buffer = Create();
        buffer.Activate();
        buffer.Add("a");

        // A message is already waiting, so even a huge wait returns at once
        var result = buffer.Poll(0, TimeSpan.FromHours(1));
        result.Messages.Single().Should().Be("a");
    }
}
