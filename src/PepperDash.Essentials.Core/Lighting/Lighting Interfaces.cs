using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Lighting
{
    /// <summary>
    /// Defines the contract for ILightingScenes
    /// </summary>
    public interface ILightingScenes
    {
        /// <summary>
        /// Fires when the lighting scene changes
        /// </summary>
        event EventHandler<LightingSceneChangeEventArgs> LightingSceneChange;

        /// <summary>
        /// Gets the list of LightingScenes
        /// </summary>
        List<LightingScene> LightingScenes { get; }

        /// <summary>
        /// Selects the given LightingScene
        /// </summary>
        /// <param name="scene">scene to select</param>
        void SelectScene(LightingScene scene);

        /// <summary>
        /// Gets the currently selected LightingScene
        /// </summary>
        LightingScene CurrentLightingScene { get; }

    }

    /// <summary>
    /// Defines the contract for ILightingScenesDynamic
    /// </summary>
    public interface ILightingScenesDynamic : ILightingScenes
    {
        /// <summary>
        /// Fires when the lighting scenes are updated
        /// </summary>
        event EventHandler LightingScenesUpdated;
    }

    /// <summary>
    /// Defines the contract for ILightingMasterRaiseLower
    /// </summary>
    public interface ILightingMasterRaiseLower
    {
        /// <summary>
        /// Raises the master level
        /// </summary>
        void MasterRaise();

        /// <summary>
        /// Lowers the master level
        /// </summary>
        void MasterLower();

        /// <summary>
        /// Stops raising or lowering the master level
        /// </summary>
        void MasterRaiseLowerStop();
    }

    /// <summary>
    /// Defines the contract for ILightingLoad
    /// </summary>
    public interface ILightingLoad
    {
        /// <summary>
        /// Sets the load level
        /// </summary>
        /// <param name="level">level to set</param>
        void SetLoadLevel(int level);

        /// <summary>
        /// Raises the load level
        /// </summary>
        void Raise();

        /// <summary>
        /// Lowers the load level
        /// </summary>
        void Lower();

        /// <summary>
        /// feedback of the current load level
        /// </summary>
        IntFeedback LoadLevelFeedback { get; }

        /// <summary>
        /// feedback of whether the load is on
        /// </summary>
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

        /// <summary>
        /// Constructor for LightingSceneChangeEventArgs
        /// </summary>
        /// <param name="scene">The lighting scene that changed</param>
        public LightingSceneChangeEventArgs(LightingScene scene)
        {
            CurrentLightingScene = scene;
        }
    }

}