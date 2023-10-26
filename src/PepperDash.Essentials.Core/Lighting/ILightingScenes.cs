using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Lighting
{
    /// <summary>
    /// Requirements for a device that implements lighting scene control
    /// </summary>
    public interface ILightingScenes
    {
        event EventHandler<LightingSceneChangeEventArgs> LightingSceneChange;

        List<LightingScene> LightingScenes { get; }

        void SelectScene(LightingScene scene);

        LightingScene CurrentLightingScene { get; }

    }
}