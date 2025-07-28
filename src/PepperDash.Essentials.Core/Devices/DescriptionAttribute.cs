using System;

namespace PepperDash.Essentials.Core.Devices
{
  /// <summary>
  /// Represents a description attribute for a device.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
  public class DescriptionAttribute : Attribute
  {
    /// <summary>
    /// Represents a description attribute for a device.
    /// </summary>
    /// <param name="description"></param>
    public DescriptionAttribute(string description)
    {
      Description = description;
    }

    /// <summary>
    /// Gets the description for the device.
    /// </summary>
    public string Description { get; }
  }

}