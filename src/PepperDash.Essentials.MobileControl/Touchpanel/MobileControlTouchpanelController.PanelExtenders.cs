using System;
using Crestron.SimplSharpPro.UI;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core.DeviceInfo;

namespace PepperDash.Essentials.Touchpanel
{
  /// <summary>
  /// Partial class containing panel extender registration and event handling functionality.
  /// </summary>
  public partial class MobileControlTouchpanelController
  {
    /// <summary>
    /// Registers for extender signals based on the panel type and sets up event handlers
    /// for app control, Zoom room, and Ethernet functionality.
    /// </summary>
    private void RegisterForExtenders()
    {
      if (Panel is TswXX70Base x70Panel)
      {
        RegisterX70PanelExtenders(x70Panel);
        return;
      }

      if (Panel is TswX60WithZoomRoomAppReservedSigs x60withZoomApp)
      {
        RegisterX60PanelExtenders(x60withZoomApp);
      }
    }

    /// <summary>
    /// Registers extender signals and event handlers for TSW X70 series panels.
    /// </summary>
    /// <param name="x70Panel">The X70 panel to register extenders for.</param>
    private void RegisterX70PanelExtenders(TswXX70Base x70Panel)
    {
      // App Control Extender
      x70Panel.ExtenderApplicationControlReservedSigs.DeviceExtenderSigChange += (e, a) =>
      {
        Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, this, $"X70 App Control Device Extender args: {a.Event}:{a.Sig}:{a.Sig.Type}:{a.Sig.BoolValue}:{a.Sig.UShortValue}:{a.Sig.StringValue}");

        UpdateZoomFeedbacks();

        if (!x70Panel.ExtenderApplicationControlReservedSigs.HideOpenedApplicationFeedback.BoolValue)
        {
          x70Panel.ExtenderButtonToolbarReservedSigs.ShowButtonToolbar();
          x70Panel.ExtenderButtonToolbarReservedSigs.Button2On();
        }
        else
        {
          x70Panel.ExtenderButtonToolbarReservedSigs.HideButtonToolbar();
          x70Panel.ExtenderButtonToolbarReservedSigs.Button2Off();
        }
      };

      // Zoom Room App Extender
      x70Panel.ExtenderZoomRoomAppReservedSigs.DeviceExtenderSigChange += (e, a) =>
      {
        Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, this, $"X70 Zoom Room App Device Extender args: {a.Event}:{a.Sig}:{a.Sig.Type}:{a.Sig.BoolValue}:{a.Sig.UShortValue}:{a.Sig.StringValue}");

        if (a.Sig.Number == x70Panel.ExtenderZoomRoomAppReservedSigs.ZoomRoomIncomingCallFeedback.Number)
        {
          ZoomIncomingCallFeedback.FireUpdate();
        }
        else if (a.Sig.Number == x70Panel.ExtenderZoomRoomAppReservedSigs.ZoomRoomActiveFeedback.Number)
        {
          ZoomInCallFeedback.FireUpdate();
        }
      };

      // Ethernet Extender
      x70Panel.ExtenderEthernetReservedSigs.DeviceExtenderSigChange += (e, a) =>
      {
        DeviceInfo.MacAddress = x70Panel.ExtenderEthernetReservedSigs.MacAddressFeedback.StringValue;
        DeviceInfo.IpAddress = x70Panel.ExtenderEthernetReservedSigs.IpAddressFeedback.StringValue;

        Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, this, $"MAC: {DeviceInfo.MacAddress} IP: {DeviceInfo.IpAddress}");

        var handler = DeviceInfoChanged;

        if (handler == null)
        {
          return;
        }

        handler(this, new DeviceInfoEventArgs(DeviceInfo));
      };

      // Initialize extenders
      x70Panel.ExtenderApplicationControlReservedSigs.Use();
      x70Panel.ExtenderZoomRoomAppReservedSigs.Use();
      x70Panel.ExtenderEthernetReservedSigs.Use();
      x70Panel.ExtenderButtonToolbarReservedSigs.Use();

      // Initialize button toolbar
      x70Panel.ExtenderButtonToolbarReservedSigs.Button1Off();
      x70Panel.ExtenderButtonToolbarReservedSigs.Button3Off();
      x70Panel.ExtenderButtonToolbarReservedSigs.Button4Off();
      x70Panel.ExtenderButtonToolbarReservedSigs.Button5Off();
      x70Panel.ExtenderButtonToolbarReservedSigs.Button6Off();
    }

    /// <summary>
    /// Registers extender signals and event handlers for TSW X60 series panels with Zoom room app support.
    /// </summary>
    /// <param name="x60withZoomApp">The X60 panel with Zoom room app to register extenders for.</param>
    private void RegisterX60PanelExtenders(TswX60WithZoomRoomAppReservedSigs x60withZoomApp)
    {
      // App Control Extender
      x60withZoomApp.ExtenderApplicationControlReservedSigs.DeviceExtenderSigChange += (e, a) =>
      {
        Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, this, $"X60 App Control Device Extender args: {a.Event}:{a.Sig}:{a.Sig.Type}:{a.Sig.BoolValue}:{a.Sig.UShortValue}:{a.Sig.StringValue}");

        if (a.Sig.Number == x60withZoomApp.ExtenderApplicationControlReservedSigs.HideOpenApplicationFeedback.Number)
        {
          AppOpenFeedback.FireUpdate();
        }
      };

      // Zoom Room App Extender
      x60withZoomApp.ExtenderZoomRoomAppReservedSigs.DeviceExtenderSigChange += (e, a) =>
      {
        Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, this, $"X60 Zoom Room App Device Extender args: {a.Event}:{a.Sig}:{a.Sig.Type}:{a.Sig.BoolValue}:{a.Sig.UShortValue}:{a.Sig.StringValue}");

        if (a.Sig.Number == x60withZoomApp.ExtenderZoomRoomAppReservedSigs.ZoomRoomIncomingCallFeedback.Number)
        {
          ZoomIncomingCallFeedback.FireUpdate();
        }
        else if (a.Sig.Number == x60withZoomApp.ExtenderZoomRoomAppReservedSigs.ZoomRoomActiveFeedback.Number)
        {
          ZoomInCallFeedback.FireUpdate();
        }
      };

      // Ethernet Extender
      x60withZoomApp.ExtenderEthernetReservedSigs.DeviceExtenderSigChange += (e, a) =>
      {
        DeviceInfo.MacAddress = x60withZoomApp.ExtenderEthernetReservedSigs.MacAddressFeedback.StringValue;
        DeviceInfo.IpAddress = x60withZoomApp.ExtenderEthernetReservedSigs.IpAddressFeedback.StringValue;

        Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, this, $"MAC: {DeviceInfo.MacAddress} IP: {DeviceInfo.IpAddress}");

        var handler = DeviceInfoChanged;

        if (handler == null)
        {
          return;
        }

        handler(this, new DeviceInfoEventArgs(DeviceInfo));
      };

      // Initialize extenders
      x60withZoomApp.ExtenderZoomRoomAppReservedSigs.Use();
      x60withZoomApp.ExtenderApplicationControlReservedSigs.Use();
      x60withZoomApp.ExtenderEthernetReservedSigs.Use();
    }
  }
}