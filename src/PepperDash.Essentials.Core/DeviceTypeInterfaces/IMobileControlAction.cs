using System;
using Newtonsoft.Json.Linq;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
  /// <summary>
  /// Defines the contract for IMobileControlAction
  /// </summary>
  public interface IMobileControlAction
  {
    /// <summary>
    /// The messenger to use for mobile control actions
    /// </summary>
    IMobileControlMessenger Messenger { get; }

    /// <summary>
    /// The action to perform for mobile control actions
    /// </summary>
    Action<string, string, JToken> Action { get; }
  }
}