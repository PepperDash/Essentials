using System.Collections.Generic;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common
{
  /// <summary>
  /// Represents a Roku2Factory
  /// </summary>
  public class Roku2Factory : EssentialsDeviceFactory<Roku2>
  {
    /// <summary>
    /// Roku2Factory constructor
    /// </summary>
    public Roku2Factory()
    {
      TypeNames = new List<string>() { "roku" };
    }

    /// <summary>
    /// BuildDevice method
    /// </summary>
    /// <inheritdoc />
    public override EssentialsDevice BuildDevice(DeviceConfig dc)
    {
      Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new Roku Device");
      var irCont = IRPortHelper.GetIrOutputPortController(dc);
      return new Roku2(dc.Key, dc.Name, irCont);

    }
  }

}