using FluentAssertions;
using PepperDash.Essentials.Core.Queues;
using Xunit;

namespace PepperDash.Essentials.Tests.Queues;

/// <summary>
/// <see cref="GenericQueue"/> must work off a processor (issue #1553): it watches for the program
/// stopping through the registered <c>ICrestronEnvironment</c>, not the Crestron SDK's
/// <c>CrestronEnvironment</c>, whose type initializer needs assemblies that only exist on a processor.
/// </summary>
public sealed class GenericQueueTests
{
    private sealed class SignalMessage : IQueueMessage
    {
        private readonly ManualResetEventSlim _done;
        public SignalMessage(ManualResetEventSlim done) => _done = done;
        public void Dispatch() => _done.Set();
    }

    [Fact]
    public void ProcessesEnqueuedMessages()
    {
        using var queue = new GenericQueue($"genericQueueTests-{Guid.NewGuid():N}");
        using var done = new ManualResetEventSlim();

        queue.Enqueue(new SignalMessage(done));

        done.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue("the queue's worker should dispatch the message");
    }

    [Fact]
    public void WatchesForProgramStopThroughTheRegisteredEnvironment()
    {
        var before = TestInitializer.Environment.ProgramStatusHandlerCount;

        using var queue = new GenericQueue($"genericQueueTests-{Guid.NewGuid():N}");

        TestInitializer.Environment.ProgramStatusHandlerCount.Should().BeGreaterThan(before);
    }

    [Fact]
    public void DisposeStopsTheQueue()
    {
        var queue = new GenericQueue($"genericQueueTests-{Guid.NewGuid():N}");

        queue.Dispose();

        queue.Disposed.Should().BeTrue();
    }
}
