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
        ActivationActions = actionDeviceKey == null
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
        var props = Props(
            Scenario("combined", new() { new PartitionState { PartitionKey = "p1", PartitionPresent = false } },
                actions.Key, nameof(ScenarioActions.RunCombined)),
            Scenario("divided", new() { new PartitionState { PartitionKey = "p1", PartitionPresent = true } },
                actions.Key, nameof(ScenarioActions.RunDivided)));
        props.defaultToManualMode = true;
        // No sensor device: the partition is manual, and starts not present.
        props.Partitions = new() { new PartitionConfig { Key = "p1", Name = "p1", DeviceKey = NewKey("noSensor"), AdjacentRoomKeys = new() } };

        // Not activated, so there's no startup scenario; PostActivate sets up the partition.
        var combiner = new EssentialsRoomCombiner(NewKey("combiner"), props);
        combiner.PostActivate();

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
