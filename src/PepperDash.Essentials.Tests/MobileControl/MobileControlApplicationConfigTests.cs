using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace PepperDash.Essentials.Tests.MobileControl;

/// <summary>
/// The processor regenerates the user app's <c>_config.local.json</c> from the <c>appServer</c>
/// device's <c>applicationConfig</c> on every start. App-specific settings Essentials doesn't model
/// must survive that round trip instead of being dropped.
/// </summary>
public class MobileControlApplicationConfigTests
{
    private const string AppServerProperties = """
        {
          "applicationConfig": {
            "logoPath": "logo/custom.png",
            "enableDev": true,
            "datadog": {
              "applicationId": "app-id",
              "clientToken": "pub-token",
              "site": "datadoghq.com"
            },
            "perf": { "phase": "baseline" }
          }
        }
        """;

    [Fact]
    public void UnmodelledProperties_AreKeptAlongsideTheModelledOnes()
    {
        var config = JsonConvert.DeserializeObject<MobileControlConfig>(AppServerProperties)!;

        config.ApplicationConfig.LogoPath.Should().Be("logo/custom.png");
        config.ApplicationConfig.EnableDev.Should().BeTrue();
        config.ApplicationConfig.AdditionalProperties.Should().ContainKeys("datadog", "perf")
            .And.NotContainKeys("logoPath", "enableDev");
        config.ApplicationConfig.AdditionalProperties!["datadog"]!["applicationId"]!.Value<string>()
            .Should().Be("app-id");
    }

    [Fact]
    public void TheGeneratedAppConfig_CarriesThemAtTheTopLevel()
    {
        var source = JsonConvert.DeserializeObject<MobileControlConfig>(AppServerProperties)!.ApplicationConfig;

        // What MobileControlWebsocketServer.GetApplicationConfig builds for _config.local.json
        var generated = new MobileControlApplicationConfig
        {
            ApiPath = "http://10.0.0.5:50002/mc/api",
            LogoPath = source.LogoPath,
            AdditionalProperties = new Dictionary<string, JToken>(source.AdditionalProperties!),
        };

        var written = JObject.Parse(JsonConvert.SerializeObject(generated, Formatting.Indented));

        written["apiPath"]!.Value<string>().Should().Be("http://10.0.0.5:50002/mc/api");
        written["logoPath"]!.Value<string>().Should().Be("logo/custom.png");
        written["datadog"]!["clientToken"]!.Value<string>().Should().Be("pub-token");
        written["perf"]!["phase"]!.Value<string>().Should().Be("baseline");
    }

    [Fact]
    public void WithNoExtraProperties_TheGeneratedConfigIsUnchanged()
    {
        var config = JsonConvert.DeserializeObject<MobileControlConfig>(
            """{ "applicationConfig": { "logoPath": "logo/logo.png" } }""")!;

        config.ApplicationConfig.AdditionalProperties.Should().BeNullOrEmpty();

        var written = JObject.Parse(JsonConvert.SerializeObject(new MobileControlApplicationConfig
        {
            ApiPath = "http://10.0.0.5:50002/mc/api",
            AdditionalProperties = config.ApplicationConfig.AdditionalProperties,
        }));

        written.Properties().Select(p => p.Name).Should().NotContain(new[] { "datadog", "perf" });
    }

    [Fact]
    public void PerClientQueues_IsOnUnlessTurnedOff()
    {
        JsonConvert.DeserializeObject<MobileControlConfig>("""{ "directServer": { "enableDirectServer": true } }""")!
            .DirectServer.PerClientQueues.Should().BeTrue();
        JsonConvert.DeserializeObject<MobileControlConfig>("""{ "directServer": { "perClientQueues": false } }""")!
            .DirectServer.PerClientQueues.Should().BeFalse();
    }
}
