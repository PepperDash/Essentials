using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common
{
  /// <summary>
  /// Represents a IRSetTopBoxBaseFactory
  /// </summary>
  public class IRSetTopBoxBaseFactory : EssentialsDeviceFactory<IRSetTopBoxBase>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="IRSetTopBoxBaseFactory"/> class
    /// </summary>
    public IRSetTopBoxBaseFactory()
    {
      TypeNames = new List<string>() { "settopbox" };
    }

    /// <summary>
    /// BuildDevice method
    /// </summary>
    /// <inheritdoc />
    public override EssentialsDevice BuildDevice(DeviceConfig dc)
    {
      Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new SetTopBox Device");
      var irCont = IRPortHelper.GetIrOutputPortController(dc);
      var config = dc.Properties.ToObject<SetTopBoxPropertiesConfig>();
      var stb = new IRSetTopBoxBase(dc.Key, dc.Name, irCont, config);

      var listName = dc.Properties.Value<string>("presetsList");
      if (listName != null)
        stb.LoadPresets(listName);
      return stb;

    }
  }

}