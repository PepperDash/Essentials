using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Describes a camera with preset functionality
    /// </summary>
    public interface IHasCameraPresets
    {
        event EventHandler<EventArgs> PresetsListHasChanged;

        List<CameraPreset> Presets { get; }

        void PresetSelect(int preset);

        void PresetStore(int preset, string description);
    }
}