using Crestron.SimplSharpPro.UI;
using PepperDash.Core;
using PepperDash.Core.Logging;

namespace PepperDash.Essentials.Touchpanel
{
  /// <summary>
  /// Partial class containing app control functionality for managing applications on the touchpanel.
  /// </summary>
  public partial class MobileControlTouchpanelController
  {
    /// <summary>
    /// Sets the application URL for mobile control access.
    /// </summary>
    /// <param name="url">The URL to set for the mobile control application.</param>
    public void SetAppUrl(string url)
    {
      _appUrl = GetUrlWithCorrectIp(url);
      AppUrlFeedback.FireUpdate();
    }

    /// <summary>
    /// Hides the currently open application on the touchpanel.
    /// </summary>
    public void HideOpenApp()
    {
      if (Panel is TswX70Base x70Panel)
      {
        x70Panel.ExtenderApplicationControlReservedSigs.HideOpenedApplication();
        return;
      }

      if (Panel is TswX60BaseClass x60Panel)
      {
        x60Panel.ExtenderApplicationControlReservedSigs.HideOpenApplication();
        return;
      }
    }

    /// <summary>
    /// Opens an application on the touchpanel.
    /// </summary>
    public void OpenApp()
    {
      if (Panel is TswX70Base x70Panel)
      {
        x70Panel.ExtenderApplicationControlReservedSigs.OpenApplication();
        return;
      }

      if (Panel is TswX60WithZoomRoomAppReservedSigs)
      {
        Debug.LogMessage(Serilog.Events.LogEventLevel.Information, this, $"X60 panel does not support zoom app");
        return;
      }
    }

    /// <summary>
    /// Closes the currently open application on the touchpanel.
    /// </summary>
    public void CloseOpenApp()
    {
      if (Panel is TswX70Base x70Panel)
      {
        x70Panel.ExtenderApplicationControlReservedSigs.CloseOpenedApplication();
        return;
      }

      if (Panel is TswX60WithZoomRoomAppReservedSigs x60Panel)
      {
        x60Panel.ExtenderApplicationControlReservedSigs.CloseOpenedApplication();
        return;
      }
    }
  }
}