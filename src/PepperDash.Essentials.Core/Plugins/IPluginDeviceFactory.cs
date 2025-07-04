using System.Collections.Generic;


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
    /// Defines a factory for creating plugin development devices, including support for specific framework versions.
    /// </summary>
    /// <remarks>This interface extends <see cref="IPluginDeviceFactory"/> to provide additional functionality
    /// specific to plugin development environments.</remarks>
    public interface IPluginDevelopmentDeviceFactory : IPluginDeviceFactory
    {
        /// <summary>
        /// Gets a list of Essentials versions that this device is compatible with.
        /// </summary>
        List<string> DevelopmentEssentialsFrameworkVersions { get; }
    }
}


