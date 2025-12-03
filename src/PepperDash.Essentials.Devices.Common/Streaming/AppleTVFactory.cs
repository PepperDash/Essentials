using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common
{
  /// <summary>
  /// Represents a AppleTVFactory
  /// </summary>
  public class AppleTVFactory : EssentialsDeviceFactory<AppleTV>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="AppleTVFactory"/> class
    /// </summary>
    public AppleTVFactory()
    {
      TypeNames = new List<string>() { "appletv" };
    }

    /// <summary>
    /// BuildDevice method
    /// </summary>
    /// <inheritdoc />
    public override EssentialsDevice BuildDevice(DeviceConfig dc)
    {
      Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new AppleTV Device");
      var irCont = IRPortHelper.GetIrOutputPortController(dc);
      return new AppleTV(dc.Key, dc.Name, irCont);
    }
  }
}