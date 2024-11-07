using PepperDash.Core;

namespace PepperDash.Essentials.Core.Device_Info
{
    public interface IDeviceInfoProvider:IKeyed
    {
        DeviceInfo DeviceInfo { get; }

        event DeviceInfoChangeHandler DeviceInfoChanged;

        void UpdateDeviceInfo();
    }

    public delegate void DeviceInfoChangeHandler(IKeyed device, DeviceInfoEventArgs args);
}