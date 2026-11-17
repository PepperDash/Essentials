using System.Collections.Concurrent;
using FluentAssertions;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using Xunit;

namespace PepperDash.Essentials.Tests.Room;

/// <summary>
/// The room combiner applies its startup scenario only once the tie lines are loaded, and applies
/// one scenario change at a time (#1570).
/// </summary>
/// <remarks>
/// A combiner subscribes to the static <see cref="DeviceManager.AllDevicesInitialized"/> and can't
/// unsubscribe, so combiners built by earlier tests run their startup handlers again on each later
/// <see cref="DeviceManager.ActivateAll"/> in this collection. That's harmless here: assertions are
/// made only against each test's own combiner and action devices.
/// </remarks>
[Collection(GlobalRoutingStateCollection.Name)]
public sealed class EssentialsRoomCombinerTests : IDisposable
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    // Longer, as the stuck-change test waits out a 3s combination operation timeout.
    private static readonly TimeSpan StuckTestTimeout = TimeSpan.FromSeconds(10);

    private readonly List<IKeyed> _added = new();

    public EssentialsRoomCombinerTests() => DeviceManager.Initialize(null!);

    public void Dispose()
    {
        foreach (var device in _added)
            DeviceManager.RemoveDevice(device);

        // Leave the static device and tie-lines state as a fresh program would have it.
        DeviceManager.Initialize(null!);
    }

    private T Add<T>(T device) where T : IKeyed
    {
        DeviceManager.AddDevice(device);
        _added.Add(device);
        return device;
    }

    private static RoomCombinationScenarioConfig Scenario(
        string key, List<PartitionState> partitionStates, string? actionDeviceKey = null, string? actionMethod = null,
        string? deactivationMethod = null) => new()
    {
        Key = key,
        Name = key,
        PartitionStates = partitionStates,
        UiMap = new(),
        ActivationActions = Actions(actionDeviceKey, actionMethod),
        DeactivationActions = Actions(actionDeviceKey, deactivationMethod),
    };

    private static List<DeviceActionWrapper> Actions(string? deviceKey, string? method) =>
        deviceKey == null || method == null
            ? new()
            : new() { new DeviceActionWrapper { DeviceKey = deviceKey, MethodName = method } };

    private static EssentialsRoomCombinerPropertiesConfig Props(params RoomCombinationScenarioConfig[] scenarios) => new()
    {
        Partitions = new(),
        RoomKeys = new(),
        Scenarios = scenarios.ToList(),
        defaultScenarioKey = scenarios[0].Key,
    };

    private static string NewKey(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    // A manual combiner with one partition ("p1", no sensor, so it starts not present) and two
    // scenarios: "combined" (p1 not present) and "divided" (p1 present). Not activated, so it has no
    // startup scenario; PostActivate sets up the partition.
    private static EssentialsRoomCombiner ManualCombiner(
        string? actionDeviceKey = null, string? combinedAction = null, string? dividedAction = null,
        int? combinationOperationTimeoutSeconds = null, string? combinedDeactivation = null)
    {
        var props = Props(
            Scenario("combined", new() { new PartitionState { PartitionKey = "p1", PartitionPresent = false } },
                actionDeviceKey, combinedAction, combinedDeactivation),
            Scenario("divided", new() { new PartitionState { PartitionKey = "p1", PartitionPresent = true } },
                actionDeviceKey, dividedAction));
        props.defaultToManualMode = true;
        props.Partitions = new() { new PartitionConfig { Key = "p1", Name = "p1", DeviceKey = NewKey("noSensor"), AdjacentRoomKeys = new() } };
        props.CombinationOperationTimeoutSeconds = combinationOperationTimeoutSeconds;

        var combiner = new EssentialsRoomCombiner(NewKey("combiner"), props);
        combiner.PostActivate();
        return combiner;
    }

    // ---- The startup scenario waits for the tie lines ----

    [Fact]
    public void StartupScenario_WaitsUntilTheTieLinesAreLoaded()
    {
        var combiner = Add(new EssentialsRoomCombiner(NewKey("combiner"), Props(Scenario("divided", new()))));
        using var changed = new ManualResetEventSlim();
        combiner.RoomCombinationScenarioChanged += (_, _) => changed.Set();

        using var initialized = new ManualResetEventSlim();
        // Subscribed after the combiner's own handler, so it runs once the combiner's has.
        EventHandler<EventArgs> onInitialized = (_, _) => initialized.Set();
        DeviceManager.AllDevicesInitialized += onInitialized;
        try
        {
            DeviceManager.ActivateAll();
            initialized.Wait(Timeout).Should().BeTrue("the combiner initializes after activation");

            combiner.CurrentScenario.Should().BeNull("the tie lines aren't loaded yet");

            DeviceManager.NotifyTieLinesLoaded();

            changed.Wait(Timeout).Should().BeTrue("the tie lines are loaded");
            combiner.CurrentScenario!.Key.Should().Be("divided");
        }
        finally
        {
            DeviceManager.AllDevicesInitialized -= onInitialized;
        }
    }

    [Fact]
    public void StartupScenario_IsAppliedWhenTheTieLinesWereLoadedFirst()
    {
        var combiner = Add(new EssentialsRoomCombiner(NewKey("combiner"), Props(Scenario("divided", new()))));
        using var changed = new ManualResetEventSlim();
        combiner.RoomCombinationScenarioChanged += (_, _) => changed.Set();

        DeviceManager.NotifyTieLinesLoaded();
        DeviceManager.ActivateAll();

        changed.Wait(Timeout).Should().BeTrue();
        combiner.CurrentScenario!.Key.Should().Be("divided");
    }

    [Fact]
    public async Task A_PartitionChange_BeforeTheTieLinesLoad_IsAppliedOnceTheyAre()
    {
        var combiner = ManualCombiner();

        combiner.Partitions.Single().SetPartitionStatePresent();
        // What the partition change's debounce timer runs.
        var change = combiner.DetermineRoomCombinationScenario();

        change.IsCompleted.Should().BeFalse("the tie lines aren't loaded yet");
        combiner.CurrentScenario.Should().BeNull();

        DeviceManager.NotifyTieLinesLoaded();

        await change.WaitAsync(Timeout);
        combiner.CurrentScenario!.Key.Should().Be("divided");
    }

    // ---- One scenario change at a time ----

    private sealed class ScenarioActions : IKeyed
    {
        public ScenarioActions(string key) => Key = key;

        public string Key { get; }

        public ConcurrentQueue<string> Log { get; } = new();

        public TaskCompletionSource CombinedGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task RunCombined()
        {
            Log.Enqueue("combined:start");
            await CombinedGate.Task;
            Log.Enqueue("combined:end");
        }

        public void RunDivided() => Log.Enqueue("divided");

        public TaskCompletionSource DividedGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task RunDividedHeld()
        {
            Log.Enqueue("divided:start");
            await DividedGate.Task;
            Log.Enqueue("divided:end");
        }

        // Blocks its thread rather than returning a Task. Bounded, so a regression fails rather than hangs.
        public ManualResetEventSlim BlockGate { get; } = new();

        public void RunCombinedBlocking()
        {
            Log.Enqueue("combined:blocking");
            BlockGate.Wait(TimeSpan.FromSeconds(15));
            Log.Enqueue("combined:end");
        }

        public TaskCompletionSource DeactivateGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task DeactivateCombinedHeld()
        {
            Log.Enqueue("combined:deactivate:start");
            await DeactivateGate.Task;
            Log.Enqueue("combined:deactivate:end");
        }
    }

    [Fact]
    public async Task A_ScenarioChange_WaitsForTheOneInProgress()
    {
        var actions = Add(new ScenarioActions(NewKey("scenarioActions")));
        var combiner = ManualCombiner(actions.Key, nameof(ScenarioActions.RunCombined), nameof(ScenarioActions.RunDivided));
        DeviceManager.NotifyTieLinesLoaded();

        var first = combiner.DetermineRoomCombinationScenario();
        // The change runs on the thread pool; 'combined' is then held in its activation action.
        SpinWait.SpinUntil(() => actions.Log.Contains("combined:start"), Timeout).Should().BeTrue();

        combiner.Partitions.Single().SetPartitionStatePresent();
        var second = combiner.DetermineRoomCombinationScenario();

        actions.Log.Should().Equal(new[] { "combined:start" }, "a change must wait for the one in progress");

        actions.CombinedGate.SetResult();
        await Task.WhenAll(first, second).WaitAsync(Timeout);

        actions.Log.Should().Equal("combined:start", "combined:end", "divided");
        combiner.CurrentScenario!.Key.Should().Be("divided");
    }

    [Fact]
    public async Task A_StuckScenarioChange_IsGivenUpOnPastTheTimeout_AndLaterChangesStillQueue()
    {
        var actions = Add(new ScenarioActions(NewKey("scenarioActions")));
        var combiner = ManualCombiner(actions.Key, nameof(ScenarioActions.RunCombined), nameof(ScenarioActions.RunDividedHeld),
            combinationOperationTimeoutSeconds: 3);
        DeviceManager.NotifyTieLinesLoaded();
        try
        {
            // 'combined' never finishes its activation action (until the cleanup below).
            _ = combiner.DetermineRoomCombinationScenario();
            SpinWait.SpinUntil(() => actions.Log.Contains("combined:start"), StuckTestTimeout).Should().BeTrue();

            combiner.Partitions.Single().SetPartitionStatePresent();
            var second = combiner.DetermineRoomCombinationScenario();

            actions.Log.Should().Equal(new[] { "combined:start" }, "it first waits for the change in progress");

            // Past the timeout the stuck change is given up on, and 'divided' starts (held in its action).
            SpinWait.SpinUntil(() => actions.Log.Contains("divided:start"), StuckTestTimeout).Should().BeTrue();

            combiner.Partitions.Single().SetPartitionStateNotPresent();
            var third = combiner.DetermineRoomCombinationScenario();

            actions.Log.Should().Equal(new[] { "combined:start", "divided:start" },
                "a later change still waits for the change in progress, not just for the stuck one");

            actions.DividedGate.SetResult();
            await second.WaitAsync(StuckTestTimeout);

            SpinWait.SpinUntil(() => actions.Log.Count == 4, StuckTestTimeout).Should().BeTrue();
            actions.Log.Should().Equal("combined:start", "divided:start", "divided:end", "combined:start");
        }
        finally
        {
            actions.CombinedGate.TrySetResult();
            actions.DividedGate.TrySetResult();
        }
    }

    [Fact]
    public async Task An_ActionThatBlocks_IsGivenUpOnPastTheTimeout()
    {
        var actions = Add(new ScenarioActions(NewKey("scenarioActions")));
        var combiner = ManualCombiner(actions.Key, nameof(ScenarioActions.RunCombinedBlocking), nameof(ScenarioActions.RunDivided),
            combinationOperationTimeoutSeconds: 3);
        DeviceManager.NotifyTieLinesLoaded();
        try
        {
            // Returns although the action blocks: the change runs on the thread pool.
            _ = combiner.DetermineRoomCombinationScenario();
            SpinWait.SpinUntil(() => actions.Log.Contains("combined:blocking"), StuckTestTimeout).Should().BeTrue();

            combiner.Partitions.Single().SetPartitionStatePresent();
            await combiner.DetermineRoomCombinationScenario().WaitAsync(StuckTestTimeout);

            actions.Log.Should().Equal(new[] { "combined:blocking", "divided" },
                "past the timeout the blocked change is given up on, while it's still blocked");
            combiner.CurrentScenario!.Key.Should().Be("divided");
        }
        finally
        {
            actions.BlockGate.Set();
        }
    }

    [Fact]
    public async Task A_ChangeGivenUpOn_DoesNotOverwriteALaterOne()
    {
        var actions = Add(new ScenarioActions(NewKey("scenarioActions")));
        var combiner = ManualCombiner(actions.Key, combinedAction: null, dividedAction: nameof(ScenarioActions.RunDivided),
            combinationOperationTimeoutSeconds: 3, combinedDeactivation: nameof(ScenarioActions.DeactivateCombinedHeld));
        DeviceManager.NotifyTieLinesLoaded();
        try
        {
            await combiner.DetermineRoomCombinationScenario().WaitAsync(Timeout);
            combiner.CurrentScenario!.Key.Should().Be("combined");

            var changed = 0;
            combiner.RoomCombinationScenarioChanged += (_, _) => Interlocked.Increment(ref changed);

            // 'combined' -> 'divided' gets stuck deactivating 'combined', and is given up on.
            combiner.Partitions.Single().SetPartitionStatePresent();
            await combiner.DetermineRoomCombinationScenario().WaitAsync(StuckTestTimeout);
            actions.Log.Should().Equal("combined:deactivate:start");

            // The partition is back: 'combined' is still current, so there's nothing to change.
            combiner.Partitions.Single().SetPartitionStateNotPresent();
            await combiner.DetermineRoomCombinationScenario().WaitAsync(Timeout);

            // The stuck change completes its deactivation late; it must not go on to apply 'divided'.
            actions.DeactivateGate.SetResult();
            SpinWait.SpinUntil(() => actions.Log.Contains("combined:deactivate:end"), Timeout).Should().BeTrue();
            SpinWait.SpinUntil(() => actions.Log.Contains("divided") || changed > 0, TimeSpan.FromSeconds(1))
                .Should().BeFalse("a later change has started");

            combiner.CurrentScenario!.Key.Should().Be("combined");
        }
        finally
        {
            actions.DeactivateGate.TrySetResult();
        }
    }
}
