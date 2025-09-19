using System;
using System.Linq;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceInfo;

namespace PepperDash.Essentials.Touchpanel
{
  /// <summary>
  /// Partial class containing device information, IP address handling, and network configuration functionality.
  /// </summary>
  public partial class MobileControlTouchpanelController
  {
    /// <summary>
    /// Updates device information including MAC address and IP address from panel extenders.
    /// </summary>
    public void UpdateDeviceInfo()
    {
      if (Panel is TswXX70Base x70Panel)
      {
        DeviceInfo.MacAddress = x70Panel.ExtenderEthernetReservedSigs.MacAddressFeedback.StringValue;
        DeviceInfo.IpAddress = x70Panel.ExtenderEthernetReservedSigs.IpAddressFeedback.StringValue;

        var handler = DeviceInfoChanged;

        if (handler == null)
        {
          return;
        }

        handler(this, new DeviceInfoEventArgs(DeviceInfo));
      }

      if (Panel is TswX60WithZoomRoomAppReservedSigs x60Panel)
      {
        DeviceInfo.MacAddress = x60Panel.ExtenderEthernetReservedSigs.MacAddressFeedback.StringValue;
        DeviceInfo.IpAddress = x60Panel.ExtenderEthernetReservedSigs.IpAddressFeedback.StringValue;

        var handler = DeviceInfoChanged;

        if (handler == null)
        {
          return;
        }

        handler(this, new DeviceInfoEventArgs(DeviceInfo));
      }

      Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, this, $"MAC: {DeviceInfo.MacAddress} IP: {DeviceInfo.IpAddress}");
    }

    /// <summary>
    /// Gets the URL with the correct IP address based on the connected devices and the Crestron processor's IP address.
    /// </summary>
    /// <param name="url">The original URL to process.</param>
    /// <returns>The URL with the correct IP address for the panel's network connection.</returns>
    private string GetUrlWithCorrectIp(string url)
    {
      var lanAdapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetLANAdapter);

      var processorIp = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, lanAdapterId);

      if (csIpAddress == null || csSubnetMask == null || url == null)
      {
        this.LogWarning("CS IP Address Subnet Mask or url is null, cannot determine correct IP for URL");
        return url;
      }

      this.LogVerbose("Processor IP: {processorIp}, CS IP: {csIpAddress}, CS Subnet Mask: {csSubnetMask}", processorIp, csIpAddress, csSubnetMask);
      this.LogVerbose("Connected IP Count: {connectedIps}", ConnectedIps.Count);

      var ip = ConnectedIps.Any(ipInfo =>
      {
        if (System.Net.IPAddress.TryParse(ipInfo.DeviceIpAddress, out var parsedIp))
        {
          return parsedIp.IsInSameSubnet(csIpAddress, csSubnetMask);
        }
        this.LogWarning("Invalid IP address: {deviceIpAddress}", ipInfo.DeviceIpAddress);
        return false;
      }) ? csIpAddress.ToString() : processorIp;

      var match = Regex.Match(url, @"^http://([^:/]+):\d+/mc/app\?token=.+$");
      if (match.Success)
      {
        string ipa = match.Groups[1].Value;
        // ip will be "192.168.1.100"
      }

      // replace ipa with ip but leave the rest of the string intact
      var updatedUrl = Regex.Replace(url, @"^http://[^:/]+", $"http://{ip}");

      this.LogVerbose("Updated URL: {updatedUrl}", updatedUrl);

      return updatedUrl;
    }

    /// <summary>
    /// Subscribes to mobile control updates and bridge events for feedback updates.
    /// </summary>
    private void SubscribeForMobileControlUpdates()
    {
      foreach (var dev in DeviceManager.AllDevices)
      {
        Debug.LogMessage(Serilog.Events.LogEventLevel.Information, this, $"{dev.Key}:{dev.GetType().Name}");
      }

      var mcList = DeviceManager.AllDevices.OfType<MobileControlSystemController>().ToList();

      if (mcList.Count == 0)
      {
        Debug.LogMessage(Serilog.Events.LogEventLevel.Information, this, $"No Mobile Control controller found");
        return;
      }

      // use first in list, since there should only be one.
      var mc = mcList[0];

      var bridge = mc.GetRoomBridge(_config.DefaultRoomKey);

      if (bridge == null)
      {
        Debug.LogMessage(Serilog.Events.LogEventLevel.Information, this, $"No Mobile Control bridge for {_config.DefaultRoomKey} found ");
        return;
      }

      _bridge = bridge;

      _bridge.UserCodeChanged += UpdateFeedbacks;
      _bridge.AppUrlChanged += (s, a) =>
      {
        SetAppUrl(_bridge.AppUrl);
      };

      UpdateFeedbacks();
    }
  }
}