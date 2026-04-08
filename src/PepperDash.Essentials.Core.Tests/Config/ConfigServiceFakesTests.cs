using FluentAssertions;
using PepperDash.Core.Abstractions;
using PepperDash.Core.Tests.Fakes;
using Xunit;

namespace PepperDash.Essentials.Core.Tests.Config;

/// <summary>
/// Tests for the configuration loading abstractions.
/// These verify behaviour of the test fakes and interfaces independently of
/// any Crestron SDK types (ConfigReader itself will be tested here once it
/// is migrated from Crestron.SimplSharp.CrestronIO to System.IO — see plan Phase 4).
/// </summary>
public class ConfigServiceFakesTests
{
    [Fact]
    public void DataStore_MultipleKeys_AreStoredIndependently()
    {
        var store = new InMemoryCrestronDataStore();
        store.InitStore();

        store.SetLocalInt("KeyA", 1);
        store.SetLocalInt("KeyB", 2);

        store.TryGetLocalInt("KeyA", out var a);
        store.TryGetLocalInt("KeyB", out var b);

        a.Should().Be(1);
        b.Should().Be(2);
    }

    [Fact]
    public void DataStore_OverwriteKey_ReturnsNewValue()
    {
        var store = new InMemoryCrestronDataStore();
        store.SetLocalInt("Level", 1);
        store.SetLocalInt("Level", 5);

        store.TryGetLocalInt("Level", out var level);
        level.Should().Be(5);
    }

    [Fact]
    public void FakeEnvironment_CanBeConfiguredForServer()
    {
        var env = new FakeCrestronEnvironment
        {
            DevicePlatform = DevicePlatform.Server,
            ApplicationNumber = 1,
            RoomId = 42,
        };

        env.DevicePlatform.Should().Be(DevicePlatform.Server);
        env.RoomId.Should().Be(42u);
    }

    [Fact]
    public void FakeEthernetHelper_SeedAndRetrieve()
    {
        var eth = new FakeEthernetHelper()
            .Seed(EthernetParameterType.GetCurrentIpAddress, "192.168.1.100")
            .Seed(EthernetParameterType.GetHostname, "MC4-TEST");

        eth.GetEthernetParameter(EthernetParameterType.GetCurrentIpAddress, 0)
            .Should().Be("192.168.1.100");

        eth.GetEthernetParameter(EthernetParameterType.GetHostname, 0)
            .Should().Be("MC4-TEST");

        eth.GetEthernetParameter(EthernetParameterType.GetDomainName, 0)
            .Should().BeEmpty("unseeded parameter should return empty string");
    }
}
