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

        #endregion


        public LightingBase(string key, string name)
            : base(key, name)
        {
            LightingScenes = new List<LightingScene>();
        }

        public abstract void SelectScene(LightingScene scene);

        protected void OnLightingSceneChange()
        {
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
    }
}