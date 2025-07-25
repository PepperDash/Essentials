using System;

namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Represents a ConfigSnippetAttribute
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
  public class ConfigSnippetAttribute : Attribute
  {
    private string _ConfigSnippet;

    /// <summary>
    /// Represents a configuration snippet for the device.
    /// </summary>
    /// <param name="configSnippet"></param>
    public ConfigSnippetAttribute(string configSnippet)
    {
      //Debug.LogMessage(LogEventLevel.Verbose, "Setting Config Snippet {0}", configSnippet);
      _ConfigSnippet = configSnippet;
    }

    /// <summary>
    /// Gets the configuration snippet for the device.
    /// This snippet can be used in the DeviceConfig to instantiate the device.
    /// </summary>
    public string ConfigSnippet
    {
      get { return _ConfigSnippet; }
    }
  }

}