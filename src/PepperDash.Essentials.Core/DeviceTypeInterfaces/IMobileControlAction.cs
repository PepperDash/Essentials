using System;
using Newtonsoft.Json.Linq;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
  /// <summary>
  /// Defines the contract for IMobileControlAction
  /// </summary>
  public interface IMobileControlAction
  {
    IMobileControlMessenger Messenger { get; }

    Action<string, string, JToken> Action { get; }
  }
}