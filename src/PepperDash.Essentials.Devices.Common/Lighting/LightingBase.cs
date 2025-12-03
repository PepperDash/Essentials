

using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Lighting;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Lighting
{
  /// <summary>
  /// Base class for lighting devices that support scenes
  /// </summary>
  public abstract class LightingBase : EssentialsBridgeableDevice, ILightingScenes
  {
    #region ILightingScenes Members

    /// <summary>
    /// Event fired when lighting scene changes
    /// </summary>
    public event EventHandler<LightingSceneChangeEventArgs> LightingSceneChange;

    /// <summary>
    /// Gets or sets the LightingScenes
    /// </summary>
    public List<LightingScene> LightingScenes { get; protected set; }

    /// <summary>
    /// Gets or sets the CurrentLightingScene
    /// </summary>
    public LightingScene CurrentLightingScene { get; protected set; }

    /// <summary>
    /// Gets or sets the CurrentLightingSceneFeedback
    /// </summary>
    public IntFeedback CurrentLightingSceneFeedback { get; protected set; }

    #endregion

    /// <summary>
    /// Initializes a new instance of the LightingBase class
    /// </summary>
    /// <param name="key">The device key</param>
    /// <param name="name">The device name</param>
    protected LightingBase(string key, string name)
        : base(key, name)
    {
      LightingScenes = new List<LightingScene>();

      CurrentLightingScene = new LightingScene();
      //CurrentLightingSceneFeedback = new IntFeedback(() => { return int.Parse(this.CurrentLightingScene.ID); });
    }

    /// <summary>
    /// Selects the specified lighting scene
    /// </summary>
    /// <param name="scene">The lighting scene to select</param>
    public abstract void SelectScene(LightingScene scene);

    /// <summary>
    /// SimulateSceneSelect method
    /// </summary>
    public void SimulateSceneSelect(string sceneName)
    {
      Debug.LogMessage(LogEventLevel.Debug, this, "Simulating selection of scene '{0}'", sceneName);

      var scene = LightingScenes.FirstOrDefault(s => s.Name.Equals(sceneName));

      if (scene != null)
      {
        CurrentLightingScene = scene;
        OnLightingSceneChange();
      }
    }

    /// <summary>
    /// Sets the IsActive property on each scene and fires the LightingSceneChange event
    /// </summary>
    protected void OnLightingSceneChange()
    {
      foreach (var scene in LightingScenes)
      {
        if (scene == CurrentLightingScene)
          scene.IsActive = true;

        else
          scene.IsActive = false;
      }
      LightingSceneChange?.Invoke(this, new LightingSceneChangeEventArgs(CurrentLightingScene));
    }

    /// <summary>
    /// Links the lighting device to API with join map configuration
    /// </summary>
    /// <param name="lightingDevice">The lighting device to link</param>
    /// <param name="trilist">The trilist to link to</param>
    /// <param name="joinStart">The starting join number</param>
    /// <param name="joinMapKey">The join map key</param>
    /// <param name="bridge">The EISC API bridge</param>
    /// <returns>The configured join map</returns>
    protected GenericLightingJoinMap LinkLightingToApi(LightingBase lightingDevice, BasicTriList trilist, uint joinStart,
      string joinMapKey, EiscApiAdvanced bridge)
    {
      var joinMap = new GenericLightingJoinMap(joinStart);

      var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

      if (!string.IsNullOrEmpty(joinMapSerialized))
        joinMap = JsonConvert.DeserializeObject<GenericLightingJoinMap>(joinMapSerialized);

      if (bridge != null)
      {
        bridge.AddJoinMap(Key, joinMap);
      }
      else
      {
        Debug.LogMessage(LogEventLevel.Information, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
      }

      return LinkLightingToApi(lightingDevice, trilist, joinMap);
    }

    /// <summary>
    /// Links the lighting device to API using an existing join map
    /// </summary>
    /// <param name="lightingDevice">The lighting device to link</param>
    /// <param name="trilist">The trilist to link to</param>
    /// <param name="joinMap">The join map to use</param>
    /// <returns>The join map used for linking</returns>
    protected GenericLightingJoinMap LinkLightingToApi(LightingBase lightingDevice, BasicTriList trilist, GenericLightingJoinMap joinMap)
    {
      Debug.LogMessage(LogEventLevel.Debug, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

      Debug.LogMessage(LogEventLevel.Information, "Linking to Lighting Type {0}", lightingDevice.GetType().Name.ToString());

      // GenericLighitng Actions & FeedBack
      trilist.SetUShortSigAction(joinMap.SelectScene.JoinNumber, u => lightingDevice.SelectScene(lightingDevice.LightingScenes[u]));

      var sceneIndex = 0;
      foreach (var scene in lightingDevice.LightingScenes)
      {
        var index = sceneIndex;

        trilist.SetSigTrueAction((uint)(joinMap.SelectSceneDirect.JoinNumber + index), () => lightingDevice.SelectScene(lightingDevice.LightingScenes[index]));
        scene.IsActiveFeedback.LinkInputSig(trilist.BooleanInput[(uint)(joinMap.SelectSceneDirect.JoinNumber + index)]);
        trilist.StringInput[(uint)(joinMap.SelectSceneDirect.JoinNumber + index)].StringValue = scene.Name;
        trilist.BooleanInput[(uint)(joinMap.ButtonVisibility.JoinNumber + index)].BoolValue = true;

        sceneIndex++;
      }

      trilist.OnlineStatusChange += (sender, args) =>
      {
        if (!args.DeviceOnLine) return;

        sceneIndex = 0;
        foreach (var scene in lightingDevice.LightingScenes)
        {
          var index = sceneIndex;

          trilist.StringInput[(uint)(joinMap.SelectSceneDirect.JoinNumber + index)].StringValue = scene.Name;
          trilist.BooleanInput[(uint)(joinMap.ButtonVisibility.JoinNumber + index)].BoolValue = true;
          scene.IsActiveFeedback.FireUpdate();

          sceneIndex++;
        }
      };

      return joinMap;
    }
  }
}