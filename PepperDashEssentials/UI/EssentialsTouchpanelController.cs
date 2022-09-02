using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.PageManagers;
using PepperDash.Essentials.Core.UI;
using Newtonsoft.Json;

namespace PepperDash.Essentials
{
    public class EssentialsTouchpanelController : TouchpanelBase
	{        
		public PanelDriverBase PanelDriver { get; private set; }

		CTimer BacklightTransitionedOnTimer;

		/// <summary>
		/// Config constructor
		/// </summary>
		public EssentialsTouchpanelController(string key, string name, BasicTriListWithSmartObject panel, CrestronTouchpanelPropertiesConfig config)
			: base(key, name, panel, config)
		{
		}


        /// <summary>
        /// Sets up drivers and links them to the room specified
        /// </summary>
        /// <param name="roomKey">key of room to link the drivers to</param>
        protected override void SetupPanelDrivers(string roomKey)
        {
            // Clear out any existing actions
            Panel.ClearAllSigActions();

            Debug.Console(0, this, "Linking TP '{0}' to Room '{1}'", Key, roomKey);

            var mainDriver = new EssentialsPanelMainInterfaceDriver(Panel, _config);
            // Then the sub drivers

            // spin up different room drivers depending on room type
            var room = DeviceManager.GetDeviceForKey(roomKey);
            if (room is IEssentialsHuddleSpaceRoom)
            {
                // Screen Saver Driver

                mainDriver.ScreenSaverController = new ScreenSaverController(mainDriver, _config);

                // Header Driver
                Debug.Console(0, this, "Adding header driver");
                mainDriver.HeaderDriver = new EssentialsHeaderDriver(mainDriver, _config);

                // AV Driver
                Debug.Console(0, this, "Adding huddle space AV driver");
                var avDriver = new EssentialsHuddlePanelAvFunctionsDriver(mainDriver, _config);
                avDriver.DefaultRoomKey = roomKey;
                mainDriver.AvDriver = avDriver;
                avDriver.CurrentRoom = room as IEssentialsHuddleSpaceRoom;

                // Environment Driver
                if (avDriver.CurrentRoom.PropertiesConfig.Environment != null && avDriver.CurrentRoom.PropertiesConfig.Environment.DeviceKeys.Count > 0)
                {
                    Debug.Console(0, this, "Adding environment driver");
                    mainDriver.EnvironmentDriver = new EssentialsEnvironmentDriver(mainDriver, _config);

                    mainDriver.EnvironmentDriver.GetDevicesFromConfig(avDriver.CurrentRoom.PropertiesConfig.Environment);
                }

                mainDriver.HeaderDriver.SetupHeaderButtons(avDriver, avDriver.CurrentRoom);

                if (Panel is TswFt5ButtonSystem)
                {
                    var tsw = Panel as TswFt5ButtonSystem;
                    // Wire up hard keys
                    tsw.Power.UserObject = new Action<bool>(b => { if (!b) avDriver.PowerButtonPressed(); });
                    if (mainDriver.EnvironmentDriver != null)
                        tsw.Lights.UserObject = new Action<bool>(b =>
                        {
                            if (!b)
                            {
                                mainDriver.EnvironmentDriver.Toggle();
                            }
                        });
                    tsw.Up.UserObject = new Action<bool>(avDriver.VolumeUpPress);
                    tsw.Down.UserObject = new Action<bool>(avDriver.VolumeDownPress);
                }
            }
            else if (room is IEssentialsHuddleVtc1Room)
            {
                Debug.Console(0, this, "Adding huddle space VTC AV driver");

                // Screen Saver Driver
                mainDriver.ScreenSaverController = new ScreenSaverController(mainDriver, _config);

                // Header Driver
                mainDriver.HeaderDriver = new EssentialsHeaderDriver(mainDriver, _config);

                // AV Driver
                var avDriver = new EssentialsHuddleVtc1PanelAvFunctionsDriver(mainDriver, _config);

                var codecDriver = new PepperDash.Essentials.UIDrivers.VC.EssentialsVideoCodecUiDriver(Panel, avDriver,
                    (room as IEssentialsHuddleVtc1Room).VideoCodec, mainDriver.HeaderDriver);
                avDriver.SetVideoCodecDriver(codecDriver);
                avDriver.DefaultRoomKey = roomKey;
                mainDriver.AvDriver = avDriver;
                avDriver.CurrentRoom = room as IEssentialsHuddleVtc1Room;

                // Environment Driver
                if (avDriver.CurrentRoom.PropertiesConfig.Environment != null && avDriver.CurrentRoom.PropertiesConfig.Environment.DeviceKeys.Count > 0)
                {
                    Debug.Console(0, this, "Adding environment driver");
                    mainDriver.EnvironmentDriver = new EssentialsEnvironmentDriver(mainDriver, _config);

                    mainDriver.EnvironmentDriver.GetDevicesFromConfig(avDriver.CurrentRoom.PropertiesConfig.Environment);
                }

                mainDriver.HeaderDriver.SetupHeaderButtons(avDriver, avDriver.CurrentRoom);


                if (Panel is TswFt5ButtonSystem)
                {
                    var tsw = Panel as TswFt5ButtonSystem;
                    // Wire up hard keys
                    tsw.Power.UserObject = new Action<bool>(b => { if (!b) avDriver.EndMeetingPress(); });
                    if (mainDriver.EnvironmentDriver != null)
                        tsw.Lights.UserObject = new Action<bool>(b =>
                        {
                            if (!b)
                            {
                                mainDriver.EnvironmentDriver.Toggle();
                            }
                        });
                    tsw.Up.UserObject = new Action<bool>(avDriver.VolumeUpPress);
                    tsw.Down.UserObject = new Action<bool>(avDriver.VolumeDownPress);
                }

                LoadAndShowDriver(mainDriver);
            }
            else
            {
                Debug.Console(0, this, "ERROR: Cannot load AvFunctionsDriver for room '{0}'", roomKey);
            }

        }

		public void LoadAndShowDriver(PanelDriverBase driver)
		{
            if (PanelDriver != null)
            {
                var mainDriver = PanelDriver as EssentialsPanelMainInterfaceDriver;
                if (mainDriver != null)
                {
                    mainDriver.Dispose();
                }
            }

			PanelDriver = driver;
			driver.Show();
		}

		protected override void ExtenderSystemReservedSigs_DeviceExtenderSigChange(DeviceExtender currentDeviceExtender, SigEventArgs args)
		{
			// If the sig is transitioning on, mark it in case it was home button that transitioned it
			var blOnSig = (Panel as TswFt5ButtonSystem).ExtenderSystemReservedSigs.BacklightOnFeedback;
			if (args.Sig == blOnSig && blOnSig.BoolValue)
			{
				BacklightTransitionedOnTimer = new CTimer(o =>
				{
					BacklightTransitionedOnTimer = null;
				}, 200);
			}
		}

        public void PulseBool(uint join)
        {
            var act = Panel.BooleanInput[join].UserObject as Action<bool>;
            if (act != null)
            {
                act(true);
                act(false);
            }
        }

        public void SetBoolSig(uint join, bool value)
        {
            var act = Panel.BooleanInput[join].UserObject as Action<bool>;
            if (act != null)
                act(value);
        }

        public void SetIntSig(uint join, ushort value)
        {
            var act = Panel.BooleanInput[join].UserObject as Action<ushort>;
            if (act != null)
            {
                act(value);
            }
        }
	}

    public class EssentialsTouchpanelControllerFactory : EssentialsDeviceFactory<EssentialsTouchpanelController>
    {
        public EssentialsTouchpanelControllerFactory()
        {
            TypeNames = new List<string>() { "crestronapp", "tsw550", "tsw750", "tsw1050", "tsw560", "tsw760", "tsw1060", "tsw570", "tsw770", "ts770", "tsw1070", "ts1070", "xpanel" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            var comm = CommFactory.GetControlPropertiesConfig(dc);
            var props = JsonConvert.DeserializeObject<CrestronTouchpanelPropertiesConfig>(dc.Properties.ToString());

            var panel = GetPanelForType(dc.Type, comm.IpIdInt, props.ProjectName);

            Debug.Console(1, "Factory Attempting to create new EssentialsTouchpanelController");

            var panelController = new EssentialsTouchpanelController(dc.Key, dc.Name, panel, props);

            return panelController;
        }

        private BasicTriListWithSmartObject GetPanelForType(string type, uint id, string projectName)
        {
            type = type.ToLower();
            try
            {
                if (type == "crestronapp")
                {
                    var app = new CrestronApp(id, Global.ControlSystem);
                    app.ParameterProjectName.Value = projectName;
                    return app;
                }
                else if (type == "xpanel")
                    return new XpanelForSmartGraphics(id, Global.ControlSystem);
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
                    Debug.Console(0, Debug.ErrorLogLevel.Notice, "WARNING: Cannot create TSW controller with type '{0}'", type);
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.Console(0, Debug.ErrorLogLevel.Notice, "WARNING: Cannot create TSW base class. Panel will not function: {0}", e.Message);
                return null;
            }
        }
    }
}