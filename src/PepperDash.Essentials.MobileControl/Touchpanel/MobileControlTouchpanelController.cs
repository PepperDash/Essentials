﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.DeviceInfo;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.UI;
using Serilog.Events;
using Feedback = PepperDash.Essentials.Core.Feedback;

namespace PepperDash.Essentials.Touchpanel
{
    /// <summary>
    /// Mobile Control touchpanel controller that provides app control, Zoom integration, 
    /// and mobile control functionality for Crestron touchpanels.
    /// </summary>
    public class MobileControlTouchpanelController : TouchpanelBase, IHasFeedback, ITswAppControl, ITswZoomControl, IDeviceInfoProvider, IMobileControlCrestronTouchpanelController, ITheme
    {
        private readonly MobileControlTouchpanelProperties localConfig;
        private IMobileControlRoomMessenger _bridge;

        private string _appUrl;

        /// <summary>
        /// Gets or sets the AppUrlFeedback
        /// </summary>
        public StringFeedback AppUrlFeedback { get; private set; }

        private readonly StringFeedback QrCodeUrlFeedback;
        private readonly StringFeedback McServerUrlFeedback;
        private readonly StringFeedback UserCodeFeedback;

        private readonly BoolFeedback _appOpenFeedback;

        /// <summary>
        /// Gets feedback indicating whether an application is currently open on the touchpanel.
        /// </summary>
        public BoolFeedback AppOpenFeedback => _appOpenFeedback;

        private readonly BoolFeedback _zoomIncomingCallFeedback;

        /// <summary>
        /// Gets feedback indicating whether there is an incoming Zoom call.
        /// </summary>
        public BoolFeedback ZoomIncomingCallFeedback => _zoomIncomingCallFeedback;

        private readonly BoolFeedback _zoomInCallFeedback;

        /// <summary>
        /// Event that is raised when device information changes.
        /// </summary>
        public event DeviceInfoChangeHandler DeviceInfoChanged;

        /// <summary>
        /// Gets feedback indicating whether a Zoom call is currently active.
        /// </summary>
        public BoolFeedback ZoomInCallFeedback => _zoomInCallFeedback;

        /// <summary>
        /// Gets or sets the Feedbacks
        /// </summary>
        public FeedbackCollection<Feedback> Feedbacks { get; private set; }

        /// <summary>
        /// Gets or sets the ZoomFeedbacks
        /// </summary>
        public FeedbackCollection<Feedback> ZoomFeedbacks { get; private set; }

        /// <summary>
        /// Gets the default room key for this touchpanel controller.
        /// </summary>
        public string DefaultRoomKey => _config.DefaultRoomKey;

        /// <summary>
        /// Gets a value indicating whether to use direct server communication.
        /// </summary>
        public bool UseDirectServer => localConfig.UseDirectServer;

        /// <summary>
        /// Gets a value indicating whether this touchpanel acts as a Zoom Room controller.
        /// </summary>
        public bool ZoomRoomController => localConfig.ZoomRoomController;

        /// <summary>
        /// Gets the current theme for the touchpanel interface.
        /// </summary>
        public string Theme => localConfig.Theme;

        /// <summary>
        /// Gets or sets the ThemeFeedback
        /// </summary>
        public StringFeedback ThemeFeedback { get; private set; }

        /// <summary>
        /// Gets device information including MAC address and IP address.
        /// </summary>
        public DeviceInfo DeviceInfo => new DeviceInfo();

        public ReadOnlyCollection<ConnectedIpInformation> ConnectedIps => Panel.ConnectedIpList;

        private System.Net.IPAddress csIpAddress;

        private System.Net.IPAddress csSubnetMask;


        /// <summary>
        /// Initializes a new instance of the MobileControlTouchpanelController class.
        /// </summary>
        /// <param name="key">The unique key identifier for this touchpanel controller.</param>
        /// <param name="name">The friendly name for this touchpanel controller.</param>
        /// <param name="panel">The touchpanel hardware device.</param>
        /// <param name="config">The configuration properties for this controller.</param>
        public MobileControlTouchpanelController(string key, string name, BasicTriListWithSmartObject panel, MobileControlTouchpanelProperties config) : base(key, name, panel, config)
        {
            localConfig = config;

            AddPostActivationAction(SubscribeForMobileControlUpdates);

            ThemeFeedback = new StringFeedback($"{Key}-theme", () => Theme);
            AppUrlFeedback = new StringFeedback($"{Key}-appUrl", () => _appUrl);
            QrCodeUrlFeedback = new StringFeedback($"{Key}-qrCodeUrl", () => _bridge?.QrCodeUrl);
            McServerUrlFeedback = new StringFeedback($"{Key}-mcServerUrl", () => _bridge?.McServerUrl);
            UserCodeFeedback = new StringFeedback($"{Key}-userCode", () => _bridge?.UserCode);

            _appOpenFeedback = new BoolFeedback($"{Key}-appOpen", () =>
            {
                if (Panel is TswX60BaseClass tsX60)
                {
                    Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, this, $"x60 sending {tsX60.ExtenderApplicationControlReservedSigs.HideOpenApplicationFeedback.BoolValue}");
                    return !tsX60.ExtenderApplicationControlReservedSigs.HideOpenApplicationFeedback.BoolValue;
                }

                if (Panel is TswX70Base tsX70)
                {
                    Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, this, $"x70 sending {tsX70.ExtenderApplicationControlReservedSigs.HideOpenedApplicationFeedback.BoolValue}");
                    return !tsX70.ExtenderApplicationControlReservedSigs.HideOpenedApplicationFeedback.BoolValue;
                }

                return false;
            });

            _zoomIncomingCallFeedback = new BoolFeedback($"{Key}-zoomIncomingCall", () =>
            {
                if (Panel is TswX60WithZoomRoomAppReservedSigs tsX60)
                {
                    return tsX60.ExtenderZoomRoomAppReservedSigs.ZoomRoomIncomingCallFeedback.BoolValue;
                }

                if (Panel is TswX70Base tsX70)
                {
                    return tsX70.ExtenderZoomRoomAppReservedSigs.ZoomRoomIncomingCallFeedback.BoolValue;
                }

                return false;
            });

            _zoomInCallFeedback = new BoolFeedback($"{Key}-zoomInCall", () =>
            {
                if (Panel is TswX60WithZoomRoomAppReservedSigs tsX60)
                {
                    return tsX60.ExtenderZoomRoomAppReservedSigs.ZoomRoomActiveFeedback.BoolValue;
                }

                if (Panel is TswX70Base tsX70)
                {
                    return tsX70.ExtenderZoomRoomAppReservedSigs.ZoomRoomActiveFeedback.BoolValue;
                }

                return false;
            });

            Feedbacks = new FeedbackCollection<Feedback>
            {
                AppUrlFeedback, QrCodeUrlFeedback, McServerUrlFeedback, UserCodeFeedback
            };

            ZoomFeedbacks = new FeedbackCollection<Feedback> {
                AppOpenFeedback, _zoomInCallFeedback, _zoomIncomingCallFeedback
            };

            RegisterForExtenders();

            try
            {
                var csAdapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetCSAdapter);
                var csSubnetMask = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_MASK, csAdapterId);
                var csIpAddress = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, csAdapterId);

                this.csSubnetMask = System.Net.IPAddress.Parse(csSubnetMask);
                this.csIpAddress = System.Net.IPAddress.Parse(csIpAddress);
            }
            catch (ArgumentException)
            {
                Debug.LogInformation("This processor does not have a CS LAN", this);
            }
            catch (InvalidOperationException)
            {
                Debug.LogInformation("This processor does not have a CS LAN", this);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected exception when checking CS LAN: {ex}", this);
            }
        }

        /// <summary>
        /// Updates the theme setting for this touchpanel controller and persists the change to configuration.
        /// </summary>
        /// <param name="theme">The new theme identifier to apply.</param>
        /// <summary>
        /// UpdateTheme method
        /// </summary>
        public void UpdateTheme(string theme)
        {
            localConfig.Theme = theme;

            var props = JToken.FromObject(localConfig);

            var deviceConfig = ConfigReader.ConfigObject.Devices.FirstOrDefault((d) => d.Key == Key);

            if (deviceConfig == null) { return; }

            deviceConfig.Properties = props;

            ConfigWriter.UpdateDeviceConfig(deviceConfig);
        }

        private void RegisterForExtenders()
        {
            if (Panel is TswXX70Base x70Panel)
            {
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


                x70Panel.ExtenderZoomRoomAppReservedSigs.DeviceExtenderSigChange += (e, a) =>
                {
                    Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, this, $"X70 Zoom Room Ap Device Extender args: {a.Event}:{a.Sig}:{a.Sig.Type}:{a.Sig.BoolValue}:{a.Sig.UShortValue}:{a.Sig.StringValue}");

                    if (a.Sig.Number == x70Panel.ExtenderZoomRoomAppReservedSigs.ZoomRoomIncomingCallFeedback.Number)
                    {
                        ZoomIncomingCallFeedback.FireUpdate();
                    }
                    else if (a.Sig.Number == x70Panel.ExtenderZoomRoomAppReservedSigs.ZoomRoomActiveFeedback.Number)
                    {
                        ZoomInCallFeedback.FireUpdate();
                    }
                };


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

                x70Panel.ExtenderApplicationControlReservedSigs.Use();
                x70Panel.ExtenderZoomRoomAppReservedSigs.Use();
                x70Panel.ExtenderEthernetReservedSigs.Use();
                x70Panel.ExtenderButtonToolbarReservedSigs.Use();

                x70Panel.ExtenderButtonToolbarReservedSigs.Button1Off();
                x70Panel.ExtenderButtonToolbarReservedSigs.Button3Off();
                x70Panel.ExtenderButtonToolbarReservedSigs.Button4Off();
                x70Panel.ExtenderButtonToolbarReservedSigs.Button5Off();
                x70Panel.ExtenderButtonToolbarReservedSigs.Button6Off();

                return;
            }

            if (Panel is TswX60WithZoomRoomAppReservedSigs x60withZoomApp)
            {
                x60withZoomApp.ExtenderApplicationControlReservedSigs.DeviceExtenderSigChange += (e, a) =>
                {
                    Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, this, $"X60 App Control Device Extender args: {a.Event}:{a.Sig}:{a.Sig.Type}:{a.Sig.BoolValue}:{a.Sig.UShortValue}:{a.Sig.StringValue}");

                    if (a.Sig.Number == x60withZoomApp.ExtenderApplicationControlReservedSigs.HideOpenApplicationFeedback.Number)
                    {
                        AppOpenFeedback.FireUpdate();
                    }
                };
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

                x60withZoomApp.ExtenderZoomRoomAppReservedSigs.Use();
                x60withZoomApp.ExtenderApplicationControlReservedSigs.Use();
                x60withZoomApp.ExtenderEthernetReservedSigs.Use();
            }
        }

        /// <summary>
        /// Performs custom activation setup for the touchpanel controller, including 
        /// registering messengers and linking to mobile control.
        /// </summary>
        /// <returns>True if activation was successful; otherwise, false.</returns>
        /// <summary>
        /// CustomActivate method
        /// </summary>
        public override bool CustomActivate()
        {
            var appMessenger = new ITswAppControlMessenger($"appControlMessenger-{Key}", $"/device/{Key}", this);

            var zoomMessenger = new ITswZoomControlMessenger($"zoomControlMessenger-{Key}", $"/device/{Key}", this);

            var themeMessenger = new ThemeMessenger($"themeMessenger-{Key}", $"/device/{Key}", this);

            var mc = DeviceManager.AllDevices.OfType<IMobileControl>().FirstOrDefault();

            if (mc == null)
            {
                return base.CustomActivate();
            }

            if (!(Panel is TswXX70Base) && !(Panel is TswX60WithZoomRoomAppReservedSigs))
            {
                mc.AddDeviceMessenger(themeMessenger);

                return base.CustomActivate();
            }

            mc.AddDeviceMessenger(appMessenger);
            mc.AddDeviceMessenger(zoomMessenger);
            mc.AddDeviceMessenger(themeMessenger);

            return base.CustomActivate();
        }

        /// <summary>
        /// Handles device extender signal changes for system reserved signals.
        /// </summary>
        /// <param name="currentDeviceExtender">The device extender that generated the signal change.</param>
        /// <param name="args">The signal event arguments containing the changed signal information.</param>
        protected override void ExtenderSystemReservedSigs_DeviceExtenderSigChange(DeviceExtender currentDeviceExtender, SigEventArgs args)
        {
            Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, this, $"System Device Extender args: ${args.Event}:${args.Sig}");
        }

        /// <summary>
        /// Sets up the panel drivers and signal mappings for the specified room.
        /// </summary>
        /// <param name="roomKey">The room key to configure the panel drivers for.</param>
        protected override void SetupPanelDrivers(string roomKey)
        {
            AppUrlFeedback.LinkInputSig(Panel.StringInput[1]);
            QrCodeUrlFeedback.LinkInputSig(Panel.StringInput[2]);
            McServerUrlFeedback.LinkInputSig(Panel.StringInput[3]);
            UserCodeFeedback.LinkInputSig(Panel.StringInput[4]);

            Panel.IpInformationChange += (sender, args) =>
            {
                if (args.Connected)
                {
                    this.LogVerbose("Connection from IP: {ip}", args.DeviceIpAddress);
                    this.LogInformation("Sending {appUrl} on join 1", AppUrlFeedback.StringValue);

                    var appUrl = GetUrlWithCorrectIp(_appUrl);
                    Panel.StringInput[1].StringValue = appUrl;

                    SetAppUrl(appUrl);
                }
                else
                {
                    this.LogVerbose("Disconnection from IP: {ip}", args.DeviceIpAddress);
                }
            };

            Panel.OnlineStatusChange += (sender, args) =>
            {
                this.LogInformation("Sending {appUrl} on join 1", AppUrlFeedback.StringValue);

                UpdateFeedbacks();
                Panel.StringInput[1].StringValue = _appUrl;
                Panel.StringInput[2].StringValue = QrCodeUrlFeedback.StringValue;
                Panel.StringInput[3].StringValue = McServerUrlFeedback.StringValue;
                Panel.StringInput[4].StringValue = UserCodeFeedback.StringValue;
            };
        }

        /// <summary>
        /// Gets the URL with the correct IP address based on the connected devices and the Crestron processor's IP address.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
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
                    return csIpAddress.IsInSameSubnet(parsedIp, csSubnetMask);
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
                this.LogInformation("AppURL changed: {appURL}", _bridge.AppUrl);
                SetAppUrl(_bridge.AppUrl);
                UpdateFeedbacks(s, a);
            };

            SetAppUrl(_bridge.AppUrl);
        }

        /// <summary>
        /// Sets the application URL and updates the corresponding feedback.
        /// </summary>
        /// <param name="url">The new application URL to set.</param>
        /// <summary>
        /// SetAppUrl method
        /// </summary>
        public void SetAppUrl(string url)
        {
            _appUrl = GetUrlWithCorrectIp(url);

            AppUrlFeedback.FireUpdate();
        }

        private void UpdateFeedbacks(object sender, EventArgs args)
        {
            UpdateFeedbacks();
        }

        private void UpdateFeedbacks()
        {
            foreach (var feedback in Feedbacks) { this.LogDebug("Updating {feedbackKey}", feedback.Key); feedback.FireUpdate(); }
        }

        private void UpdateZoomFeedbacks()
        {
            foreach (var feedback in ZoomFeedbacks)
            {
                Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, this, $"Updating {feedback.Key}");
                feedback.FireUpdate();
            }
        }

        /// <summary>
        /// HideOpenApp method
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
        /// OpenApp method
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
        /// CloseOpenApp method
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

        /// <summary>
        /// EndZoomCall method
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

        /// <summary>
        /// UpdateDeviceInfo method
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
    }

    /// <summary>
    /// Represents a MobileControlTouchpanelControllerFactory
    /// </summary>
    public class MobileControlTouchpanelControllerFactory : EssentialsPluginDeviceFactory<MobileControlTouchpanelController>
    {
        /// <summary>
        /// Initializes a new instance of the MobileControlTouchpanelControllerFactory class.
        /// Sets up supported device type names and minimum framework version requirements.
        /// </summary>
        public MobileControlTouchpanelControllerFactory()
        {
            TypeNames = new List<string>() { "mccrestronapp", "mctsw550", "mctsw750", "mctsw1050", "mctsw560", "mctsw760", "mctsw1060", "mctsw570", "mctsw770", "mcts770", "mctsw1070", "mcts1070", "mcxpanel", "mcdge1000" };
            MinimumEssentialsFrameworkVersion = "2.0.0";
        }

        /// <summary>
        /// Builds a MobileControlTouchpanelController device from the provided device configuration.
        /// </summary>
        /// <param name="dc">The device configuration containing the device properties and settings.</param>
        /// <returns>A configured MobileControlTouchpanelController instance.</returns>
        /// <summary>
        /// BuildDevice method
        /// </summary>
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            var comm = CommFactory.GetControlPropertiesConfig(dc);
            var props = JsonConvert.DeserializeObject<MobileControlTouchpanelProperties>(dc.Properties.ToString());

            var panel = GetPanelForType(dc.Type, comm.IpIdInt, props.ProjectName);

            if (panel == null)
            {
                Debug.LogMessage(Serilog.Events.LogEventLevel.Information, "Unable to create Touchpanel for type {0}. Touchpanel Controller WILL NOT function correctly", dc.Type);
            }

            Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, "Factory Attempting to create new MobileControlTouchpanelController");

            var panelController = new MobileControlTouchpanelController(dc.Key, dc.Name, panel, props);

            return panelController;
        }

        private BasicTriListWithSmartObject GetPanelForType(string type, uint id, string projectName)
        {
            type = type.ToLower().Replace("mc", "");
            try
            {
                if (type == "crestronapp")
                {
                    var app = new CrestronApp(id, Global.ControlSystem);
                    app.ParameterProjectName.Value = projectName;
                    return app;
                }
                else if (type == "xpanel")
                    return new XpanelForHtml5(id, Global.ControlSystem);
                else if (type == "tsw550")
                    return new Tsw550(id, Global.ControlSystem);
                else if (type == "tsw552")
                    return new Tsw552(id, Global.ControlSystem);
                else if (type == "tsw560")
                    return new Tsw560(id, Global.ControlSystem);
                else if (type == "tsw750")
                    return new Tsw750(id, Global.ControlSystem);
                else if (type == "tsw752")
                    return new Tsw752(id, Global.ControlSystem);
                else if (type == "tsw760")
                    return new Tsw760(id, Global.ControlSystem);
                else if (type == "tsw1050")
                    return new Tsw1050(id, Global.ControlSystem);
                else if (type == "tsw1052")
                    return new Tsw1052(id, Global.ControlSystem);
                else if (type == "tsw1060")
                    return new Tsw1060(id, Global.ControlSystem);
                else if (type == "tsw570")
                    return new Tsw570(id, Global.ControlSystem);
                else if (type == "tsw770")
                    return new Tsw770(id, Global.ControlSystem);
                else if (type == "ts770")
                    return new Ts770(id, Global.ControlSystem);
                else if (type == "tsw1070")
                    return new Tsw1070(id, Global.ControlSystem);
                else if (type == "ts1070")
                    return new Ts1070(id, Global.ControlSystem);
                else if (type == "dge1000")
                    return new Dge1000(id, Global.ControlSystem);
                else

                {
                    Debug.LogMessage(Serilog.Events.LogEventLevel.Warning, "WARNING: Cannot create TSW controller with type '{0}'", type);
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogMessage(Serilog.Events.LogEventLevel.Warning, "WARNING: Cannot create TSW base class. Panel will not function: {0}", e.Message);
                return null;
            }
        }
    }
}
