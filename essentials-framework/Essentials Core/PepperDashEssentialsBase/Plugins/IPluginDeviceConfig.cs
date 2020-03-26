using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines a class that is capable of loading custom plugin device types
    /// </summary>
    public interface IPluginDeviceFactory : IDeviceFactory
    {
        /// <summary>
        /// Required to define the minimum version for Essentials in the format xx.yy.zz
        /// </summary>
        string MinimumEssentialsFrameworkVersion { get; }

    }

    /// <summary>
    /// Defines a class that is capable of loading device types
    /// </summary>
    public interface IDeviceFactory
    {
        /// <summary>
        /// Will be called when the plugin is loaded by Essentials.  Must add any new types to the DeviceFactory using DeviceFactory.AddFactoryForType() for each new type
        /// </summary>
        void LoadTypeFactories();
    }
}