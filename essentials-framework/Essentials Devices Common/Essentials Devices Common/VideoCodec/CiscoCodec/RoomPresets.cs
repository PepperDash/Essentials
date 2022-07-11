using System;
using System.Collections.Generic;
using System.Linq;

using PepperDash.Core;
using PepperDash.Essentials.Core.Presets;

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

    public static class RoomPresets
    {
        /// <summary>
        /// Converts non-generic RoomPresets to generic CameraPresets
        /// </summary>
        /// <param name="presets"></param>
        /// <returns></returns>
        public static List<PresetBase> GetGenericPresets(List<IConvertiblePreset> presets)
        {
            Debug.Console(2, "Presets List:");


            return
                presets.Select(preset => preset.ReturnConvertedCodecPreset())
                    .Where(newPreset => newPreset != null)
                    .ToList();
        }
    }

    /// <summary>
    /// Represents a room preset on a video codec.  Typically stores camera position(s) and video routing.  Can be recalled by Far End if enabled.
    /// </summary>
    public class CodecRoomPreset : PresetBase
    {
        public CodecRoomPreset(int id, string description, bool def, bool isDef)
            : base(id, description, def, isDef)
        {

        }
    }
}