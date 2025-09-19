using Crestron.SimplSharpPro.UI;
using PepperDash.Core;
using PepperDash.Core.Logging;

namespace PepperDash.Essentials.Touchpanel
{
  /// <summary>
  /// Partial class containing Zoom integration functionality for managing Zoom calls and room control.
  /// </summary>
  public partial class MobileControlTouchpanelController
  {
    /// <summary>
    /// Updates all Zoom-related feedback values to reflect current state.
    /// </summary>
    private void UpdateZoomFeedbacks()
    {
      foreach (var feedback in ZoomFeedbacks)
      {
        Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, this, $"Updating {feedback.Key}");
        feedback.FireUpdate();
      }
    }

    /// <summary>
    /// Ends the current Zoom call on the touchpanel.
    /// </summary>
    public void EndZoomCall()
    {
      if (Panel is TswX70Base x70Panel)
      {
        x70Panel.ExtenderZoomRoomAppReservedSigs.ZoomRoomEndCall();
        return;
      }

      if (Panel is TswX60WithZoomRoomAppReservedSigs x60Panel)
      {
        x60Panel.ExtenderZoomRoomAppReservedSigs.ZoomRoomEndCall();
        return;
      }
    }
  }
}