using System;
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
    /// Defines a class that is capable of loading custom plugin device types for development purposes
    /// </summary>
    [Obsolete("This interface is obsolete and will be removed in a future version." +
              " Use IPluginDeviceFactory instead and check Global.IsRunningDevelopmentVersion to determine if the Essentials framework is in development mode.")]
    public interface IPluginDevelopmentDeviceFactory : IPluginDeviceFactory
    {
        /// <summary>
        /// Gets a list of all the development versions of the Essentials framework that are supported by this factory.
        /// </summary>
        List<string> DevelopmentEssentialsFrameworkVersions { get; }
    }
}


