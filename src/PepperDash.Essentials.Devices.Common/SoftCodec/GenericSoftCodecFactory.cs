using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.SoftCodec
{
  /// <summary>
  /// Represents a GenericSoftCodecFactory
  /// </summary>
  public class GenericSoftCodecFactory : EssentialsDeviceFactory<GenericSoftCodec>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="GenericSoftCodecFactory"/> class
    /// </summary>
    public GenericSoftCodecFactory()
    {
      TypeNames = new List<string> { "genericsoftcodec" };
    }

    /// <summary>
    /// BuildDevice method
    /// </summary>
    /// <inheritdoc />
    public override EssentialsDevice BuildDevice(DeviceConfig dc)
    {
      Debug.LogMessage(LogEventLevel.Debug, "Attempting to create new Generic SoftCodec Device");

      var props = dc.Properties.ToObject<GenericSoftCodecProperties>();

      return new GenericSoftCodec(dc.Key, dc.Name, props);
    }
  }
}
