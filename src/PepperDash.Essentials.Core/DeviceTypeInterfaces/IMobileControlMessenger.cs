using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
  /// <summary>
  /// Defines the contract for IMobileControlMessenger
  /// </summary>
  public interface IMobileControlMessenger : IKeyed
  {
    /// <summary>
    /// Parent controller for this messenger
    /// </summary>
    IMobileControl AppServerController { get; }

    /// <summary>
    /// Path to listen for messages
    /// </summary>
    string MessagePath { get; }

    /// <summary>
    /// Key of the device this messenger is associated with
    /// </summary>
    string DeviceKey { get; }

    /// <summary>
    /// Register this messenger with the AppServerController
    /// </summary>
    /// <param name="appServerController"></param>
    void RegisterWithAppServer(IMobileControl appServerController);
  }
}