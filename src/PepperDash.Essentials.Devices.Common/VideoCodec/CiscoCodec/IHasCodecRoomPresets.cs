using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    /// <summary>
    /// Interface for camera presets
    /// </summary>
    public interface IHasCodecRoomPresets
    {
        event EventHandler<EventArgs> CodecRoomPresetsListHasChanged;

        List<CodecRoomPreset> NearEndPresets { get; }

        List<CodecRoomPreset> FarEndRoomPresets { get; }

        void CodecRoomPresetSelect(int preset);

        void CodecRoomPresetStore(int preset, string description);

        void SelectFarEndPreset(int preset);        
    }
}