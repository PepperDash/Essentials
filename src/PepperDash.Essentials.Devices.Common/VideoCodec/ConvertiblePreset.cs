using PepperDash.Essentials.Core.Presets;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    /// <summary>
    /// Base class for presets that can be converted to PresetBase
    /// </summary>
    public abstract class ConvertiblePreset
    {
        /// <summary>
        /// Converts the preset to a PresetBase
        /// </summary>
        /// <returns><see cref="PresetBase"/></returns>
        public abstract PresetBase ConvertCodecPreset();
    }
}