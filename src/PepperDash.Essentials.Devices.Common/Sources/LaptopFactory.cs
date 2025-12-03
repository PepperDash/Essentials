using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Sources
{
  /// <summary>
  /// Represents a LaptopFactory
  /// </summary>
  public class LaptopFactory : EssentialsDeviceFactory<Laptop>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="LaptopFactory"/> class
    /// </summary>
    public LaptopFactory()
    {
      TypeNames = new List<string>() { "laptop" };
    }

    /// <summary>
    /// BuildDevice method
    /// </summary>
    /// <inheritdoc />
    public override EssentialsDevice BuildDevice(DeviceConfig dc)
    {
      Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new Laptop Device");
      return new Laptop(dc.Key, dc.Name);
    }
  }
}