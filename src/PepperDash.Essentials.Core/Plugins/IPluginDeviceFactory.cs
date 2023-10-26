using PepperDash.Core;


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
}


