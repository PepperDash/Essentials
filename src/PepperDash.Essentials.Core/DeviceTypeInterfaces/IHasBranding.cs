using System;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces;

public interface IHasBranding
{
    bool BrandingEnabled { get; }
    void InitializeBranding(string roomKey);
}