using FluentAssertions;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core.Config;
using Xunit;

namespace PepperDash.Essentials.Tests.Config;

/// <summary>
/// A config can be built, deserialized and loaded off a processor (issue #1553): <see cref="InfoConfig"/>
/// takes the app number from the registered environment instead of the Crestron SDK, and
/// <see cref="ConfigReader.LoadConfig(EssentialsConfig)"/> supplies a config without a config file.
/// </summary>
public sealed class OffProcessorConfigTests
{
    [Fact]
    public void NewConfig_TakesTheAppNumberFromTheRegisteredEnvironment()
    {
        var config = new EssentialsConfig();

        config.Info.AppNumber.Should().Be(TestInitializer.Environment.ApplicationNumber);
        config.Info.HostName.Should().BeEmpty("the registered environment isn't real hardware");
    }

    [Fact]
    public void ConfigJson_Deserializes()
    {
        var json = JObject.Parse("""
            {
              "info": { "name": "Test system" },
              "devices": [ { "key": "display-1", "name": "Display", "type": "mockDisplay", "group": "displays" } ]
            }
            """);

        var config = json.ToObject<EssentialsConfig>()!;

        config.Info.Name.Should().Be("Test system");
        config.Devices.Should().ContainSingle(d => d.Key == "display-1");
    }

    [Fact]
    public void LoadConfig_UsesTheGivenConfig()
    {
        var config = new EssentialsConfig();

        ConfigReader.LoadConfig(config);

        ConfigReader.ConfigObject.Should().BeSameAs(config);
    }

    [Fact]
    public void LoadConfig_RejectsNull()
    {
        var act = () => ConfigReader.LoadConfig(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
