using PepperDash.Essentials.Core.Presets;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for ITvPresetsProvider
    /// </summary>
    public interface ITvPresetsProvider
    {
        /// <summary>
        /// The TV presets model
        /// </summary>
        DevicePresetsModel TvPresets { get; }
    }
}