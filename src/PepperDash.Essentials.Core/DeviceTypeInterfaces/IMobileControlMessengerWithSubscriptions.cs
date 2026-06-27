using System;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
  /// <summary>
  /// Obsolete: messengers are subscription based by default; use IMobileControlMessenger instead.
  /// </summary>
  [Obsolete("This interface is obsolete and will be removed in a future version. All messengers are now subscription based.")]
  public interface IMobileControlMessengerWithSubscriptions : IMobileControlMessenger
  {
    /// <summary>
    /// Unsubscribe a client from this messenger
    /// </summary>
    /// <param name="clientId"></param>
    void UnsubscribeClient(string clientId);

    /// <summary>
    /// Register this messenger with the AppServerController
    /// </summary>
    /// <param name="appServerController">parent for this messenger</param>
    /// <param name="enableMessengerSubscriptions">Enable messenger subscriptions</param>
    void RegisterWithAppServer(IMobileControl appServerController, bool enableMessengerSubscriptions);
  }
}