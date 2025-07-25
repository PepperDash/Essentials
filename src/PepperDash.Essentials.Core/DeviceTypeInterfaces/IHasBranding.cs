using System;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for IHasBranding
    /// </summary>
    public interface IHasBranding
    {
        bool BrandingEnabled { get; }
        void InitializeBranding(string roomKey);
    }
}