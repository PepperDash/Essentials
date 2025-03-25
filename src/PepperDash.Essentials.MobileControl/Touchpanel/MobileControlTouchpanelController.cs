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
using PepperDash.Essentials.Touchpanel;
using System;
using System.Collections.Generic;
using System.Linq;
using Feedback = PepperDash.Essentials.Core.Feedback;

namespace PepperDash.Essentials.Devices.Common.TouchPanel
{
    //public interface IMobileControlTouchpanelController 
    //{
    //    StringFeedback AppUrlFeedback { get; }
    //    string DefaultRoomKey { get; }
    //    string DeviceKey { get; }
    //}


    public class MobileControlTouchpanelController : TouchpanelBase, IHasFeedback, ITswAppControl, ITswZoomControl, IDeviceInfoProvider, IMobileControlTouchpanelController, ITheme
    {
        private readonly MobileControlTouchpanelProperties localConfig;
        private IMobileControlRoomMessenger _bridge;

        private string _appUrl;

        public StringFeedback AppUrlFeedback { get; private set; }
        private readonly StringFeedback QrCodeUrlFeedback;
        private readonly StringFeedback McServerUrlFeedback;
        private readonly StringFeedback UserCodeFeedback;

        private readonly BoolFeedback _appOpenFeedback;

        public BoolFeedback AppOpenFeedback => _appOpenFeedback;

        private readonly BoolFeedback _zoomIncomingCallFeedback;

        public BoolFeedback ZoomIncomingCallFeedback => _zoomIncomingCallFeedback;

        private readonly BoolFeedback _zoomInCallFeedback;

        public event DeviceInfoChangeHandler DeviceInfoChanged;

        public BoolFeedback ZoomInCallFeedback => _zoomInCallFeedback;


        public FeedbackCollection<Feedback> Feedbacks { get; private set; }

        public FeedbackCollection<Feedback> ZoomFeedbacks { get; private set; }

        public string DefaultRoomKey => _config.DefaultRoomKey;

        public bool UseDirectServer => localConfig.UseDirectServer;

        public bool ZoomRoomController => localConfig.ZoomRoomController;

        public string Theme => localConfig.Theme;

        public StringFeedback ThemeFeedback { get; private set; }

        public DeviceInfo DeviceInfo => new DeviceInfo();

        public MobileControlTouchpanelController(string key, string name, BasicTriListWithSmartObject panel, MobileControlTouchpanelProperties config) : base(key, name, panel, config)
        {
            localConfig = config;

            AddPostActivationAction(SubscribeForMobileControlUpdates);

            ThemeFeedback = new StringFeedback($"{Key}-theme",() => Theme);
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
        }

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


        protected override void ExtenderSystemReservedSigs_DeviceExtenderSigChange(DeviceExtender currentDeviceExtender, SigEventArgs args)
        {
            Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, this, $"System Device Extender args: ${args.Event}:${args.Sig}");
        }

        protected override void SetupPanelDrivers(string roomKey)
        {
            AppUrlFeedback.LinkInputSig(Panel.StringInput[1]);
            QrCodeUrlFeedback.LinkInputSig(Panel.StringInput[2]);
            McServerUrlFeedback.LinkInputSig(Panel.StringInput[3]);
            UserCodeFeedback.LinkInputSig(Panel.StringInput[4]);

            Panel.OnlineStatusChange += (sender, args) =>
            {
                UpdateFeedbacks();

                this.LogInformation("Sending {appUrl} on join 1", AppUrlFeedback.StringValue);

                Panel.StringInput[1].StringValue = AppUrlFeedback.StringValue;
                Panel.StringInput[2].StringValue = QrCodeUrlFeedback.StringValue;
                Panel.StringInput[3].StringValue = McServerUrlFeedback.StringValue;
                Panel.StringInput[4].StringValue = UserCodeFeedback.StringValue;
            };
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
            _bridge.AppUrlChanged += (s, a) => { 
               this.LogInformation("AppURL changed");
                SetAppUrl(_bridge.AppUrl);
                UpdateFeedbacks(s, a); 
            };

            SetAppUrl(_bridge.AppUrl);
        }

        public void SetAppUrl(string url)
        {
            _appUrl = url;
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

    public class MobileControlTouchpanelControllerFactory : EssentialsPluginDeviceFactory<MobileControlTouchpanelController>
    {
        public MobileControlTouchpanelControllerFactory()
        {
            TypeNames = new List<string>() { "mccrestronapp", "mctsw550", "mctsw750", "mctsw1050", "mctsw560", "mctsw760", "mctsw1060", "mctsw570", "mctsw770", "mcts770", "mctsw1070", "mcts1070", "mcxpanel" };
            MinimumEssentialsFrameworkVersion = "2.0.0";
        }

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
