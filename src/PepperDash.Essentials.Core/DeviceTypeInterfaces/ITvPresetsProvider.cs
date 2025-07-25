using PepperDash.Essentials.Core.Presets;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for ITvPresetsProvider
    /// </summary>
    public interface ITvPresetsProvider
    {
        DevicePresetsModel TvPresets { get; }
    }
}