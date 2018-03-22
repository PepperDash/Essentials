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

        event EventHandler<LightingSceneChangeEventArgs> ILightingScenes.LightingSceneChange
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        public List<LightingScene> LightingScenes { get; protected set; }

        public LightingScene CurrentLightingScene { get; protected set; }

        #endregion


        public LightingBase(string key, string name) :
            base(key, name)
        {
            LightingScenes = new List<LightingScene>();
        }


        public abstract void SelectScene(LightingScene scene);

    }

    public class LightingScene
    {
        public string Name { get; set; }
    }
}