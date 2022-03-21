using PepperDash.Core;
using PepperDash.Essentials.Core.DeviceInfo;

namespace PepperDash.Essentials.Core.Interfaces
{
    public interface IDeviceInfoProvider:IKeyed
    {
        DeviceInfo.DeviceInfo DeviceInfo { get; }

        event DeviceInfoChangeHandler DeviceInfoChanged;

        void UpdateDeviceInfo();
    }
}