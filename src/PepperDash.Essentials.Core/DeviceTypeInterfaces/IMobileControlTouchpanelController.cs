using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
  /// <summary>
  /// Defines the contract for IMobileControlTouchpanelController
  /// </summary>
  public interface IMobileControlTouchpanelController : IKeyed
  {
    /// <summary>
    /// The default room key for the controller
    /// </summary>
    string DefaultRoomKey { get; }

    /// <summary>
    /// Sets the application URL for the controller
    /// </summary>
    /// <param name="url">The application URL</param>
    void SetAppUrl(string url);

    /// <summary>
    /// Indicates whether the controller uses a direct server connection
    /// </summary>
    bool UseDirectServer { get; }

    /// <summary>
    /// Indicates whether the controller is a Zoom Room controller
    /// </summary>
    bool ZoomRoomController { get; }
  }
}