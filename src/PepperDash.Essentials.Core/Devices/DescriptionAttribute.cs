using System;

namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Represents a description attribute for a device.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
  public class DescriptionAttribute : Attribute
  {
    private string _Description;

    /// <summary>
    /// Represents a description attribute for a device.
    /// </summary>
    /// <param name="description"></param>
    public DescriptionAttribute(string description)
    {
      //Debug.LogMessage(LogEventLevel.Verbose, "Setting Description: {0}", description);
      _Description = description;
    }

    /// <summary>
    /// Gets the description for the device.
    /// </summary>
    public string Description
    {
      get { return _Description; }
    }
  }

}