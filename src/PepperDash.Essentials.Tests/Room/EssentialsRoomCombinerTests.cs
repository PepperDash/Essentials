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
        string key, List<PartitionState> partitionStates, string? actionDeviceKey = null, string? actionMethod = null) => new()
    {
        Key = key,
        Name = key,
        PartitionStates = partitionStates,
        UiMap = new(),
        ActivationActions = actionDeviceKey == null || actionMethod == null
            ? new()
            : new() { new DeviceActionWrapper { DeviceKey = actionDeviceKey, MethodName = actionMethod } },
        DeactivationActions = new(),
    };

    private static EssentialsRoomCombinerPropertiesConfig Props(params RoomCombinationScenarioConfig[] scenarios) => new()
    {
        Partitions = new(),
        RoomKeys = new(),
        Scenarios = scenarios.ToList(),
        defaultScenarioKey = scenarios[0].Key,
    };

    private static string NewKey(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    // Manual mode, one partition ("p1", no sensor, so it starts not present) and two scenarios:
    // "combined" (p1 not present, the default) and "divided" (p1 present).
    private static EssentialsRoomCombinerPropertiesConfig ManualProps(
        string? actionDeviceKey = null, string? combinedAction = null, string? dividedAction = null)
    {
        var props = Props(
            Scenario("combined", new() { new PartitionState { PartitionKey = "p1", PartitionPresent = false } },
                actionDeviceKey, combinedAction),
            Scenario("divided", new() { new PartitionState { PartitionKey = "p1", PartitionPresent = true } },
                actionDeviceKey, dividedAction));
        props.defaultToManualMode = true;
        props.Partitions = new() { new PartitionConfig { Key = "p1", Name = "p1", DeviceKey = NewKey("noSensor"), AdjacentRoomKeys = new() } };
        return props;
    }

    // A combiner from ManualProps that isn't activated, so it has no startup scenario; PostActivate
    // sets up the partition.
    private static EssentialsRoomCombiner ManualCombiner(
        string? actionDeviceKey = null, string? combinedAction = null, string? dividedAction = null)
    {
        var combiner = new EssentialsRoomCombiner(NewKey("combiner"), ManualProps(actionDeviceKey, combinedAction, dividedAction));
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
    public void StartupScenario_InManualMode_IsAppliedWhenThePartitionsAlreadyMatchTheDefault()
    {
        // p1 starts not present, which is what the default scenario ('combined') expects, so setting
        // the default scenario's partition states changes nothing (#1572).
        var combiner = Add(new EssentialsRoomCombiner(NewKey("combiner"), ManualProps()));
        using var changed = new ManualResetEventSlim();
        combiner.RoomCombinationScenarioChanged += (_, _) => changed.Set();

        DeviceManager.NotifyTieLinesLoaded();
        DeviceManager.ActivateAll();

        changed.Wait(Timeout).Should().BeTrue();
        combiner.CurrentScenario!.Key.Should().Be("combined");
    }

    [Fact]
    public async Task A_ScenarioNamingAnUnknownPartition_IsStillApplied()
    {
        var props = ManualProps();
        // A typo'd partition key in the scenario that should match (#1573).
        props.Scenarios[0].PartitionStates.Add(new PartitionState { PartitionKey = "noSuchPartition", PartitionPresent = true });
        var combiner = new EssentialsRoomCombiner(NewKey("combiner"), props);
        combiner.PostActivate();
        DeviceManager.NotifyTieLinesLoaded();

        await combiner.DetermineRoomCombinationScenario().WaitAsync(Timeout);

        combiner.CurrentScenario!.Key.Should().Be("combined", "an unknown partition key is ignored");
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
    }

    [Fact]
    public async Task A_ScenarioChange_WaitsForTheOneInProgress()
    {
        var actions = Add(new ScenarioActions(NewKey("scenarioActions")));
        var combiner = ManualCombiner(actions.Key, nameof(ScenarioActions.RunCombined), nameof(ScenarioActions.RunDivided));
        DeviceManager.NotifyTieLinesLoaded();

        var first = combiner.DetermineRoomCombinationScenario();
        actions.Log.Should().Equal(new[] { "combined:start" }, "'combined' is held in its activation action");

        combiner.Partitions.Single().SetPartitionStatePresent();
        var second = combiner.DetermineRoomCombinationScenario();

        actions.Log.Should().Equal(new[] { "combined:start" }, "a change must wait for the one in progress");

        actions.CombinedGate.SetResult();
        await Task.WhenAll(first, second).WaitAsync(Timeout);

        actions.Log.Should().Equal("combined:start", "combined:end", "divided");
        combiner.CurrentScenario!.Key.Should().Be("divided");
    }
}
