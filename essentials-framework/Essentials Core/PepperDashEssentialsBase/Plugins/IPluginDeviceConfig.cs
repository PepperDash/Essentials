using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Plugins
{
    public interface IPluginDeviceConfig
    {
        IKeyed BuildDevice(DeviceConfig dc);
    }
}