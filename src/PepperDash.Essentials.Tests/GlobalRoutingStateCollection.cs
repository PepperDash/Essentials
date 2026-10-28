using Xunit;

namespace PepperDash.Essentials.Tests;

/// <summary>
/// Tests that add to or read the process-wide routing state (DeviceManager, TieLineCollection.Default,
/// RouteDescriptorCollection.DefaultCollection). xUnit runs test classes in parallel, and those
/// collections are enumerated without their writers' locks, so these tests must not overlap.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class GlobalRoutingStateCollection
{
    public const string Name = "Global routing state";
}
