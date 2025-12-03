using System;
using System.Collections.Generic;
using System.Linq;
using PepperDash.Essentials.Core.Presets;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    /// <summary>
    /// Interface for camera presets
    /// </summary>
    public interface IHasCodecRoomPresets
    {
        /// <summary>
        /// Event that is raised when the list of room presets has changed.
        /// </summary>
        event EventHandler<EventArgs> CodecRoomPresetsListHasChanged;

        /// <summary>
        /// List of near end presets that can be recalled.
        /// </summary>
        List<CodecRoomPreset> NearEndPresets { get; }

        /// <summary>
        /// List of far end presets that can be recalled.
        /// </summary>
        List<CodecRoomPreset> FarEndRoomPresets { get; }

        /// <summary>
        /// Selects a near end preset by its ID.
        /// </summary>
        /// <param name="preset"></param>
        void CodecRoomPresetSelect(int preset);

        /// <summary>
        /// Stores a near end preset with the given ID and description.
        /// </summary>
        /// <param name="preset"></param>
        /// <param name="description"></param>
        void CodecRoomPresetStore(int preset, string description);

        /// <summary>
        /// Selects a far end preset by its ID. This is typically used to recall a preset that has been defined on the far end codec.
        /// </summary>
        /// <param name="preset"></param>
        void SelectFarEndPreset(int preset);
    }

    /// <summary>
    /// Static class for converting non-generic RoomPresets to generic CameraPresets.
    /// </summary>
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
    /// Represents a CodecRoomPreset
    /// </summary>
    public class CodecRoomPreset : PresetBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="description"></param>
        /// <param name="def"></param>
        /// <param name="isDef"></param>
        public CodecRoomPreset(int id, string description, bool def, bool isDef)
            : base(id, description, def, isDef)
        {

        }
    }
}