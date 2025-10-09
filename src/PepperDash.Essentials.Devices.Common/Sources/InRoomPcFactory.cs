using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Sources
{
  /// <summary>
  /// Represents a InRoomPcFactory
  /// </summary>
  public class InRoomPcFactory : EssentialsDeviceFactory<InRoomPc>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="InRoomPcFactory"/> class
    /// </summary>
    public InRoomPcFactory()
    {
      TypeNames = new List<string>() { "inroompc" };
    }

    /// <summary>
    /// BuildDevice method
    /// </summary>
    /// <inheritdoc />
    public override EssentialsDevice BuildDevice(DeviceConfig dc)
    {
      Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new InRoomPc Device");
      return new InRoomPc(dc.Key, dc.Name);
    }
  }

}