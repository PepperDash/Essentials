using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Plugins
{
    /// <summary>
    /// Defines a class that is capable of loading custom plugin device types
    /// </summary>
    public interface IPluginDeviceConfig
    {
        string MinimumEssentialsFrameworkVersion { get; }
        void LoadPlugin();
        IKeyed BuildDevice(DeviceConfig dc);
    }
}