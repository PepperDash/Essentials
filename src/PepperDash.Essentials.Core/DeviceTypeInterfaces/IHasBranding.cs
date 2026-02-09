using System;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for IHasBranding
    /// </summary>
    public interface IHasBranding
    {
        /// <summary>
        /// Gets whether branding is enabled
        /// </summary>
        bool BrandingEnabled { get; }

        /// <summary>
        /// Initializes branding for the device
        /// </summary>
        /// <param name="roomKey">The key identifying the room for branding purposes</param>
        void InitializeBranding(string roomKey);
    }
}