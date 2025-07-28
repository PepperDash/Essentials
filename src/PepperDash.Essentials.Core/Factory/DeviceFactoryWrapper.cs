

using System;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Factory
{
  /// <summary>
  /// Wraps a device factory, providing metadata and a factory method for creating devices.
  /// </summary>
  public class DeviceFactoryWrapper
  {
    /// <summary>
    /// Gets or sets the type associated with the current instance.
    /// </summary>
    public Type Type { get; set; }

    /// <summary>
    /// Gets or sets the description associated with the object.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the factory method used to create an <see cref="IKeyed"/> instance based on the provided <see
    /// cref="DeviceConfig"/>.
    /// </summary>
    /// <remarks>The factory method allows customization of how <see cref="IKeyed"/> instances are created for
    /// specific <see cref="DeviceConfig"/> inputs. Ensure the delegate is not null before invoking it.</remarks>
    public Func<DeviceConfig, IKeyed> FactoryMethod { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceFactoryWrapper"/> class with default values.
    /// </summary>
    /// <remarks>The <see cref="Type"/> property is initialized to <see langword="null"/>, and the <see
    /// cref="Description"/>  property is set to "Not Available".</remarks>
    public DeviceFactoryWrapper()
    {
      Type = null;
      Description = "Not Available";
    }
  }
}
