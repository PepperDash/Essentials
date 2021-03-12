using System;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    public interface IHasBranding
    {
        bool BrandingEnabled { get; }
        void InitializeBranding(string roomKey);
    }
}

namespace PepperDash_Essentials_Core.DeviceTypeInterfaces
{
    [Obsolete("Use PepperDash.Essentials.Core.DeviceTypeInterfaces")]
    public interface IHasBranding
    {
        bool BrandingEnabled { get; }
        void InitializeBranding(string roomKey);
    }
}