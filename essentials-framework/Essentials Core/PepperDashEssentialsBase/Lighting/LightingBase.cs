using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using PepperDash.Core;

namespace PepperDash.Essentials.Core.Lighting
{
    public abstract class LightingBase : Device, ILightingScenes
    {
        #region ILightingScenes Members

        public event EventHandler<LightingSceneChangeEventArgs> LightingSceneChange;

        public List<LightingScene> LightingScenes { get; protected set; }

        public LightingScene CurrentLightingScene { get; protected set; }
		
		public IntFeedback CurrentLightingSceneFeedback { get; protected set; }

        #endregion


        public LightingBase(string key, string name)
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

    }

    public class LightingScene
    {
        public string Name { get; set; }
        public string ID { get; set; }
        bool _IsActive;
        public bool IsActive 
        {
            get
            {
                return _IsActive;
            }
            set
            {
                _IsActive = value;
                IsActiveFeedback.FireUpdate();
            }
        }
        public BoolFeedback IsActiveFeedback { get; set; }

        public LightingScene()
        {
            IsActiveFeedback = new BoolFeedback(new Func<bool>(() => IsActive));
        }
    }
}