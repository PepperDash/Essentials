using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Lighting
{
    /// <summary>
    /// Defines the contract for ILightingScenes
    /// </summary>
    public interface ILightingScenes
    {
        event EventHandler<LightingSceneChangeEventArgs> LightingSceneChange;

        List<LightingScene> LightingScenes { get; }

        void SelectScene(LightingScene scene);

        LightingScene CurrentLightingScene { get; }

    }

    /// <summary>
    /// Defines the contract for ILightingScenesDynamic
    /// </summary>
    public interface ILightingScenesDynamic : ILightingScenes
    {
        event EventHandler LightingScenesUpdated;
    }

    /// <summary>
    /// Defines the contract for ILightingMasterRaiseLower
    /// </summary>
    public interface ILightingMasterRaiseLower
    {
        void MasterRaise();
        void MasterLower();
        void MasterRaiseLowerStop();
    }

    /// <summary>
    /// Defines the contract for ILightingLoad
    /// </summary>
    public interface ILightingLoad
    {
        void SetLoadLevel(int level);
        void Raise();
        void Lower();

        IntFeedback LoadLevelFeedback { get; }
        BoolFeedback LoadIsOnFeedback { get; }
    }

    /// <summary>
    /// Represents a LightingSceneChangeEventArgs
    /// </summary>
    public class LightingSceneChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the CurrentLightingScene
        /// </summary>
        public LightingScene CurrentLightingScene { get; private set; }

        public LightingSceneChangeEventArgs(LightingScene scene)
        {
            CurrentLightingScene = scene;
        }
    }

}