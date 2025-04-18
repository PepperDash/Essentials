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

    public interface ILightingScenesDynamic : ILightingScenes
    {
        event EventHandler LightingScenesUpdated;
    }

    /// <summary>
    /// Requirements for a device that implements master raise/lower
    /// </summary>
    public interface ILightingMasterRaiseLower
    {
        void MasterRaise();
        void MasterLower();
        void MasterRaiseLowerStop();
    }

    /// <summary>
    /// Requiremnts for controlling a lighting load
    /// </summary>
    public interface ILightingLoad
    {
        void SetLoadLevel(int level);
        void Raise();
        void Lower();

        IntFeedback LoadLevelFeedback { get; }
        BoolFeedback LoadIsOnFeedback { get; }
    }

    public class LightingSceneChangeEventArgs : EventArgs
    {
        public LightingScene CurrentLightingScene { get; private set; }

        public LightingSceneChangeEventArgs(LightingScene scene)
        {
            CurrentLightingScene = scene;
        }
    }

}