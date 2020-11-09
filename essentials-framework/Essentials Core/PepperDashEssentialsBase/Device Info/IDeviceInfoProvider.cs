using System;

namespace PepperDash.Essentials.Core.DeviceInfo
{
    public interface IDeviceInfoProvider
    {
        DeviceInfo DeviceInfo { get; }

        event EventHandler<DeviceInfoEventArgs> DeviceInfoChanged;

        void UpdateDeviceInfo();
    }
}