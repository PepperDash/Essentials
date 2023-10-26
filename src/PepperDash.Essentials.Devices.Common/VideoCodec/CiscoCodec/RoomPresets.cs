using System.Collections.Generic;
using System.Linq;

using PepperDash.Core;
using PepperDash.Essentials.Core.Presets;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
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
}