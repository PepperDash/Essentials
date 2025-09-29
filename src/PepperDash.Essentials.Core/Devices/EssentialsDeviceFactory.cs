using System;
using System.Collections.Generic;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Provides the basic needs for a Device Factory
  /// </summary>
  public abstract class EssentialsDeviceFactory<T> : IDeviceFactory where T : EssentialsDevice
  {
    /// <inheritdoc />
    public Type FactoryType => typeof(T);

    /// <summary>
    /// A list of strings that can be used in the type property of a DeviceConfig object to build an instance of this device
    /// </summary>
    public List<string> TypeNames { get; protected set; }

    /// <summary>
    /// Build the device using the configuration
    /// </summary>
    /// <param name="dc">The device config</param>
    /// <returns>An instance of the device</returns>
    public abstract EssentialsDevice BuildDevice(DeviceConfig dc);
  }
}