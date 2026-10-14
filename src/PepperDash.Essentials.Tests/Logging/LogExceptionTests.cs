using System.Collections.Concurrent;
using FluentAssertions;
using PepperDash.Core;
using PepperDash.Core.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit;
using Debug = PepperDash.Core.Debug;

namespace PepperDash.Essentials.Tests.Logging;

/// <summary>
/// LogException writes two entries: an Error with the caller's message plus the exception's message,
/// then a Verbose entry with the stack trace.
/// </summary>
public sealed class LogExceptionTests : IDisposable
{
    private sealed class CaptureSink : ILogEventSink
    {
        public ConcurrentQueue<LogEvent> Events { get; } = new();
        public void Emit(LogEvent logEvent) => Events.Enqueue(logEvent);
    }

    private sealed class FakeDevice : IKeyed
    {
        public FakeDevice(string key) => Key = key;
        public string Key { get; }
    }

    private readonly CaptureSink _sink = new();
    private readonly FakeDevice _device = new($"log-exception-{Guid.NewGuid():N}");

    public LogExceptionTests()
    {
        Debug.UpdateLoggerConfiguration(new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Sink(_sink));
    }

    // Debug.ResetLoggerConfiguration can't be used to restore the default logger: it builds a logger from
    // the default configuration a second time, which Serilog refuses. A logger with no sinks is
    // enough here, since no other test reads the global log.
    public void Dispose() => Debug.UpdateLoggerConfiguration(new LoggerConfiguration().MinimumLevel.Verbose());

    /// <summary>This device's entries (other tests may be logging at the same time).</summary>
    private List<LogEvent> Entries() =>
        _sink.Events
            .Where(e => e.Properties.TryGetValue("Key", out var key) && key.ToString().Trim('"') == _device.Key)
            .ToList();

    private static Exception Thrown(string message)
    {
        try { throw new InvalidOperationException(message); }
        catch (Exception ex) { return ex; }
    }

    [Fact]
    public void LogsAnErrorThenAVerboseStackTrace()
    {
        var ex = Thrown("socket closed");

        _device.LogException(ex, "Error sending text");

        var entries = Entries();
        entries.Select(e => e.Level).Should().Equal(LogEventLevel.Error, LogEventLevel.Verbose);
        entries[0].RenderMessage().Should().Be("Error sending text: socket closed");
        entries[1].RenderMessage().Should().StartWith("Stack Trace:")
            .And.Contain(nameof(Thrown), "the trace names the method that threw")
            .And.NotContain("\"", "the trace is rendered as plain text, not a quoted string");
    }

    [Fact]
    public void TheErrorEntryDoesNotCarryTheStackTrace()
    {
        var ex = Thrown("socket closed");

        _device.LogException(ex, "Error sending text");

        var error = Entries().First(e => e.Level == LogEventLevel.Error);
        error.Exception.Should().BeNull("an attached exception would repeat the stack trace in sinks that print it");
        error.RenderMessage().Should().NotContain(nameof(Thrown));
    }

    [Fact]
    public void ArgumentsStillFillTheCallersTemplate()
    {
        var ex = Thrown("refused");

        // Positional placeholders out of order, as some existing callers use.
        _device.LogException(ex, "{1} failed to connect to {0}", "10.0.0.5", "client-a");

        Entries().First(e => e.Level == LogEventLevel.Error).RenderMessage()
            .Should().Be("\"client-a\" failed to connect to \"10.0.0.5\": refused");
    }

    [Fact]
    public void BracesInTheExceptionMessageAreNotReadAsPlaceholders()
    {
        var ex = Thrown("bad value {0} in {settings}");

        _device.LogException(ex, "Error parsing {name}", "config");

        Entries().First(e => e.Level == LogEventLevel.Error).RenderMessage()
            .Should().Be("Error parsing \"config\": bad value {0} in {settings}");
    }

    [Fact]
    public void AnExceptionThatWasNeverThrown_StillLogsBothEntries()
    {
        _device.LogException(new InvalidOperationException("never thrown"), "Unexpected state");

        var entries = Entries();
        entries.Select(e => e.Level).Should().Equal(LogEventLevel.Error, LogEventLevel.Verbose);
        entries[1].RenderMessage().Should().Contain("(not available)");
    }

    [Fact]
    public void ANullException_LogsOnlyTheErrorEntry()
    {
        _device.LogException(null!, "Something failed");

        var entries = Entries();
        entries.Should().ContainSingle().Which.Level.Should().Be(LogEventLevel.Error);
        entries[0].RenderMessage().Should().Be("Something failed");
    }
}
