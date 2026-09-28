using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Lighting;

namespace PepperDash.Essentials.Devices.Common.Lighting;

/// <summary>
/// Scenes to simulate for a <see cref="MockLightingDevice"/>.
/// </summary>
public class MockLightingConfig
{
    /// <summary>
    /// Scenes to simulate, in display order. Defaults to a generic 3-scene set (On/Dim/Off) when
    /// omitted so the device works with no config at all.
    /// </summary>
    [JsonProperty("scenes", NullValueHandling = NullValueHandling.Ignore)]
    public List<MockLightingSceneConfig> Scenes { get; set; }

    /// <summary>
    /// ID of the scene active on startup. Falls back to the first configured scene when unset
    /// or when it does not match any configured scene.
    /// </summary>
    [JsonProperty("defaultSceneId", NullValueHandling = NullValueHandling.Ignore)]
    public string DefaultSceneId { get; set; }
}

/// <summary>
/// One configured scene.
/// </summary>
public class MockLightingSceneConfig
{
    /// <summary>
    /// Scene ID, referenced by the client's /selectScene request
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Display name for the scene
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }
}

/// <summary>
/// Simulated lighting scene controller. No hardware behind it - selecting a scene just updates
/// <see cref="LightingBase.CurrentLightingScene"/> and fires feedback, exactly like a real DSP
/// or lighting processor would once the scene recall is acknowledged.
/// </summary>
public class MockLightingDevice : LightingBase
{
    private static readonly List<MockLightingSceneConfig> DefaultScenes = new List<MockLightingSceneConfig>
    {
        new MockLightingSceneConfig { Id = "on", Name = "On" },
        new MockLightingSceneConfig { Id = "dim", Name = "Dim" },
        new MockLightingSceneConfig { Id = "off", Name = "Off" },
    };

    /// <summary>
    /// Initializes a new instance of the MockLightingDevice class
    /// </summary>
    /// <param name="key">The device key</param>
    /// <param name="name">The device name</param>
    /// <param name="config">Scenes to simulate; null falls back to a generic On/Dim/Off set</param>
    public MockLightingDevice(string key, string name, MockLightingConfig config)
        : base(key, name)
    {
        var scenesConfig = config?.Scenes != null && config.Scenes.Count > 0
            ? config.Scenes
            : DefaultScenes;

        LightingScenes = scenesConfig
            .Select(s => new LightingScene { ID = s.Id, Name = s.Name })
            .ToList();

        CurrentLightingScene = LightingScenes.FirstOrDefault(s => s.ID == config?.DefaultSceneId)
            ?? LightingScenes.FirstOrDefault()
            ?? new LightingScene();

        CurrentLightingSceneFeedback = new IntFeedback("currentLightingScene",
            () => LightingScenes.IndexOf(CurrentLightingScene));

        OnLightingSceneChange();
    }

    /// <inheritdoc />
    public override void SelectScene(LightingScene scene)
    {
        var match = LightingScenes.FirstOrDefault(s => s.ID == scene?.ID);

        if (match == null)
        {
            this.LogWarning("No lighting scene with id '{sceneId}'", scene?.ID);
            return;
        }

        this.LogInformation("Selecting lighting scene '{sceneId}'", match.ID);

        CurrentLightingScene = match;
        CurrentLightingSceneFeedback.FireUpdate();
        OnLightingSceneChange();
    }

    /// <inheritdoc />
    public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge) =>
        LinkLightingToApi(this, trilist, joinStart, joinMapKey, bridge);
}

/// <summary>
/// Builds a <see cref="MockLightingDevice"/> for any config entry typed "mockLightingDevice".
/// </summary>
public class MockLightingDeviceFactory : EssentialsDeviceFactory<MockLightingDevice>
{
    /// <summary>
    /// Initializes a new instance of the MockLightingDeviceFactory class
    /// </summary>
    public MockLightingDeviceFactory()
    {
        TypeNames = new List<string> { "mockLightingDevice" };
    }

    /// <inheritdoc />
    public override EssentialsDevice BuildDevice(DeviceConfig dc)
    {
        var config = dc.Properties?.ToObject<MockLightingConfig>();

        return new MockLightingDevice(dc.Key, dc.Name, config);
    }
}
