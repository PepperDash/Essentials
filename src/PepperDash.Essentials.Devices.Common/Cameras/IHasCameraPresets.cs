using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Describes a camera with preset functionality
    /// </summary>
    public interface IHasCameraPresets
    {
        /// <summary>
        /// Event that is raised when the presets list has changed
        /// </summary>
        event EventHandler<EventArgs> PresetsListHasChanged;

        /// <summary>
        /// Gets the list of camera presets
        /// </summary>
        List<CameraPreset> Presets { get; }

        /// <summary>
        /// Selects the specified preset
        /// </summary>
        /// <param name="preset">The preset number to select</param>
        void PresetSelect(int preset);

        /// <summary>
        /// Stores a preset at the specified location with the given description
        /// </summary>
        /// <param name="preset">The preset number to store</param>
        /// <param name="description">The description for the preset</param>
        void PresetStore(int preset, string description);
    }
}