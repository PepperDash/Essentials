using System.Collections.Generic;
using Newtonsoft.Json;

namespace PepperDash.Essentials.AppServer.Messengers
{
  /// <summary>
  /// Represents a DeviceStateMessageBase
  /// </summary>
  public class DeviceStateMessageBase : DeviceMessageBase
  {
    /// <summary>
    /// The interfaces implmented by the device sending the messsage
    /// </summary>
    [JsonProperty("interfaces")]
    public List<string> Interfaces { get; private set; }

    /// <summary>
    /// Sets the interfaces implemented by the device sending the message
    /// </summary>
    /// <param name="interfaces"></param>
    public void SetInterfaces(List<string> interfaces)
    {
      Interfaces = interfaces;
    }
  }

}