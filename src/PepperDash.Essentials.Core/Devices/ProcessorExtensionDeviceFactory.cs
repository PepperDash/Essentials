using System;
using System.Collections.Generic;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Represents a factory for creating processor extension devices.
  /// </summary>
  /// <typeparam name="T">The type of the processor extension device.</typeparam>
  [Obsolete("will be removed in a future version")]
  public abstract class ProcessorExtensionDeviceFactory<T> : IProcessorExtensionDeviceFactory where T : EssentialsDevice
  {
    #region IProcessorExtensionDeviceFactory Members

    /// <summary>
    /// Gets or sets the TypeNames
    /// </summary>
    public List<string> TypeNames { get; protected set; }

    /// <summary>
    /// LoadFactories method
    /// </summary>
    public void LoadFactories()
    {
      foreach (var typeName in TypeNames)
      {
        string description = typeof(T).GetCustomAttributes(typeof(DescriptionAttribute), true) is DescriptionAttribute[] descriptionAttribute && descriptionAttribute.Length > 0
            ? descriptionAttribute[0].Description
            : "No description available";

        ProcessorExtensionDeviceFactory.AddFactoryForType(typeName.ToLower(), description, typeof(T), BuildDevice);
      }
    }

    /// <summary>
    /// The method that will build the device
    /// </summary>
    /// <param name="dc">The device config</param>
    /// <returns>An instance of the device</returns>
    public abstract EssentialsDevice BuildDevice(DeviceConfig dc);

    #endregion

  }

}