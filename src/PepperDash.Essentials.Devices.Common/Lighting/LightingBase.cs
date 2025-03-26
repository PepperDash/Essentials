

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Lighting;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Lighting
{
    public abstract class LightingBase : EssentialsBridgeableDevice, ILightingScenes
    {
        #region ILightingScenes Members

        public event EventHandler<LightingSceneChangeEventArgs> LightingSceneChange;

        public List<LightingScene> LightingScenes { get; protected set; }

        public LightingScene CurrentLightingScene { get; protected set; }
		
		public IntFeedback CurrentLightingSceneFeedback { get; protected set; }

        #endregion

        protected LightingBase(string key, string name)
            : base(key, name)
        {
            LightingScenes = new List<LightingScene>();

            CurrentLightingScene = new LightingScene();
			//CurrentLightingSceneFeedback = new IntFeedback(() => { return int.Parse(this.CurrentLightingScene.ID); });
        }

        public abstract void SelectScene(LightingScene scene);

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

            trilist.StringInput[(uint) (joinMap.SelectSceneDirect.JoinNumber + index)].StringValue = scene.Name;
            trilist.BooleanInput[(uint) (joinMap.ButtonVisibility.JoinNumber + index)].BoolValue = true;
            scene.IsActiveFeedback.FireUpdate();

            sceneIndex++;
          }
        };

        return joinMap;
      }
    }
}