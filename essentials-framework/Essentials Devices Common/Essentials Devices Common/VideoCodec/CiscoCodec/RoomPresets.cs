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
        public static List<TDestination> GetGenericPresets<TSource, TDestination>(this List<TSource> presets) where TSource : ConvertiblePreset where TDestination : PresetBase
        {
            return
                presets.Select(preset => preset.ConvertCodecPreset())
                    .Where(newPreset => newPreset != null)
                    .Cast<TDestination>()
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