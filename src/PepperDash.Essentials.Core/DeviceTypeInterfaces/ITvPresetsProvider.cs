using PepperDash.Essentials.Core.Presets;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    public interface ITvPresetsProvider
    {
        DevicePresetsModel TvPresets { get; }
    }
}