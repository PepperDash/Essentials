using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Core.Lighting
{
    public class LightingSceneChangeEventArgs : EventArgs
    {
        public LightingScene CurrentLightingScene { get; private set; }

        public LightingSceneChangeEventArgs(LightingScene scene)
        {
            CurrentLightingScene = scene;
        }
    }

}