using PepperDash.Essentials.Core.Presets;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    public interface IConvertiblePreset
    {
        PresetBase ReturnConvertedCodecPreset();
    }
}