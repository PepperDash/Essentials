using System.Collections.ObjectModel;
using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
  /// <summary>
  /// Describes a MobileControl Crestron Touchpanel Controller
  /// This interface extends the IMobileControlTouchpanelController to include connected IP information
  /// </summary>
  public interface IMobileControlCrestronTouchpanelController : IMobileControlTouchpanelController
  {
    /// <summary>
    /// Gets a collection of connected IP information for the touchpanel controller
    /// </summary>
    ReadOnlyCollection<ConnectedIpInformation> ConnectedIps { get; }
  }
}