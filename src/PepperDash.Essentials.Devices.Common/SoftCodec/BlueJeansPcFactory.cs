using System.Collections.Generic;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;


namespace PepperDash.Essentials.Devices.Common.SoftCodec
{
  /// <summary>
  /// Represents a BlueJeansPcFactory
  /// </summary>
  public class BlueJeansPcFactory : EssentialsDeviceFactory<BlueJeansPc>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="BlueJeansPcFactory"/> class
    /// </summary>
    public BlueJeansPcFactory()
    {
      TypeNames = new List<string>() { "bluejeanspc" };
    }

    /// <summary>
    /// BuildDevice method
    /// </summary>
    /// <inheritdoc />
    public override EssentialsDevice BuildDevice(DeviceConfig dc)
    {
      Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new BlueJeansPc Device");
      return new BlueJeansPc(dc.Key, dc.Name);
    }
  }

}