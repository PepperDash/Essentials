using System.Collections.Generic;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
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
        //Debug.LogMessage(LogEventLevel.Verbose, "Getting Description Attribute from class: '{0}'", typeof(T).FullName);
        var descriptionAttribute = typeof(T).GetCustomAttributes(typeof(DescriptionAttribute), true) as DescriptionAttribute[];
        string description = descriptionAttribute[0].Description;
        var snippetAttribute = typeof(T).GetCustomAttributes(typeof(ConfigSnippetAttribute), true) as ConfigSnippetAttribute[];
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