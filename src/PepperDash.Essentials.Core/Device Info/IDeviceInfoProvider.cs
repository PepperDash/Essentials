using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceInfo
{
    /// <summary>
    /// Defines the contract for IDeviceInfoProvider
    /// </summary>
    public interface IDeviceInfoProvider:IKeyed
    {
        DeviceInfo DeviceInfo { get; }

        event DeviceInfoChangeHandler DeviceInfoChanged;

        void UpdateDeviceInfo();
    }

    /// <summary>
    /// Delegate for DeviceInfoChangeHandler
    /// </summary>
    public delegate void DeviceInfoChangeHandler(IKeyed device, DeviceInfoEventArgs args);
}