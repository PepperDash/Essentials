using PepperDash.Essentials.Core.Config;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines a class that is capable of loading device types
    /// </summary>
    public interface IDeviceFactory
    {
        /// <summary>
        /// Gets the type of the factory associated with the current instance.
        /// </summary>
        Type FactoryType { get; }

        /// <summary>
        /// Gets a list of type names associated with the current plugin.
        /// </summary>
        List<string> TypeNames { get; }

        /// <summary>
        /// Builds and returns an <see cref="EssentialsDevice"/> instance based on the provided configuration.
        /// </summary>
        /// <param name="deviceConfig">The configuration settings used to initialize the device. This parameter cannot be null.</param>
        /// <returns>An <see cref="EssentialsDevice"/> instance configured according to the specified <paramref
        /// name="deviceConfig"/>.</returns>
        EssentialsDevice BuildDevice(DeviceConfig deviceConfig);
    }
}