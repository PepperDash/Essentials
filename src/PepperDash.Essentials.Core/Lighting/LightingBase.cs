using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Core.Lighting
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
            Debug.Console(1, this, "Simulating selection of scene '{0}'", sceneName);

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

            var handler = LightingSceneChange;
            if (handler != null)
            {
                handler(this, new LightingSceneChangeEventArgs(CurrentLightingScene));
            }
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
                Debug.Console(0, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            Debug.Console(0, "Linking to Lighting Type {0}", lightingDevice.GetType().Name.ToString());

            // GenericLighitng Actions & FeedBack
            trilist.SetUShortSigAction(joinMap.SelectScene.JoinNumber, u => lightingDevice.SelectScene(lightingDevice.LightingScenes[u]));

            var sceneIndex = 0;
            foreach (var scene in lightingDevice.LightingScenes)
            {
                var index1 = sceneIndex;
                trilist.SetSigTrueAction((uint)(joinMap.SelectSceneDirect.JoinNumber + sceneIndex), () =>
                {
                    var index = index1;
                    Debug.Console(2, this, "LightingDevice: sceneIndex: {0} index: {1} > inside action", index1, index);
                    lightingDevice.SelectScene(lightingDevice.LightingScenes[index]);
                });
                scene.IsActiveFeedback.LinkInputSig(trilist.BooleanInput[(uint)(joinMap.SelectSceneDirect.JoinNumber + sceneIndex)]);
                trilist.StringInput[(uint)(joinMap.SelectSceneDirect.JoinNumber + sceneIndex)].StringValue = scene.Name;
                trilist.BooleanInput[(uint)(joinMap.ButtonVisibility.JoinNumber + sceneIndex)].BoolValue = true;
                sceneIndex++;
            }
            return joinMap;
        }

    }
}