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

namespace PepperDash.Essentials
{
    public class EssentialsTouchpanelController : EssentialsDevice, IHasBasicTriListWithSmartObject
	{
        private CrestronTouchpanelPropertiesConfig _propertiesConfig;

		public BasicTriListWithSmartObject Panel { get; private set; }

		public PanelDriverBase PanelDriver { get; private set; }

		CTimer BacklightTransitionedOnTimer;

		public EssentialsTouchpanelController(string key, string name, Tswx52ButtonVoiceControl tsw, 
			string projectName, string sgdPath)
			: base(key, name)
		{
			Panel = tsw;

            if (!string.IsNullOrEmpty(sgdPath))
                Panel.LoadSmartObjects(sgdPath);
            else
                Debug.Console(1, this, "No SGD file path defined");



			tsw.SigChange += Panel_SigChange;
		}

        public EssentialsTouchpanelController(string key, string name, Dge100 dge, string projectName, string sgdPath)
            : base(key, name)
        {
            Panel = dge;

            if (!string.IsNullOrEmpty(sgdPath))
                Panel.LoadSmartObjects(sgdPath);
            else
                Debug.Console(1, this, "No SGD file path defined");

            dge.SigChange += Panel_SigChange;
        }

		/// <summary>
		/// Config constructor
		/// </summary>
		public EssentialsTouchpanelController(string key, string name, string type, CrestronTouchpanelPropertiesConfig props, uint id)
			: base(key, name)
		{
            _propertiesConfig = props;

            Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Creating touchpanel hardware...");
			type = type.ToLower();
			try
			{
                if (type == "crestronapp")
                {
                    var app = new CrestronApp(id, Global.ControlSystem);
                    app.ParameterProjectName.Value = props.ProjectName;
                    Panel = app;
                }
                else if (type == "xpanel")
                    Panel = new XpanelForSmartGraphics(id, Global.ControlSystem);
                else if (type == "tsw550")
                    Panel = new Tsw550(id, Global.ControlSystem);
                else if (type == "tsw552")
                    Panel = new Tsw552(id, Global.ControlSystem);
                else if (type == "tsw560")
                    Panel = new Tsw560(id, Global.ControlSystem);
                else if (type == "tsw750")
                    Panel = new Tsw750(id, Global.ControlSystem);
                else if (type == "tsw752")
                    Panel = new Tsw752(id, Global.ControlSystem);
                else if (type == "tsw760")
                    Panel = new Tsw760(id, Global.ControlSystem);
                else if (type == "tsw1050")
                    Panel = new Tsw1050(id, Global.ControlSystem);
                else if (type == "tsw1052")
                    Panel = new Tsw1052(id, Global.ControlSystem);
                else if (type == "tsw1060")
                    Panel = new Tsw1060(id, Global.ControlSystem);
                else if (type == "tsw570")
                    Panel = new Tsw570(id, Global.ControlSystem);
                else if (type == "tsw770")
                    Panel = new Tsw770(id, Global.ControlSystem);
                else if (type == "ts770")
                    Panel = new Ts770(id, Global.ControlSystem);
                else if (type == "tsw1070")
                    Panel = new Tsw1070(id, Global.ControlSystem);
                else if (type == "ts1070")
                    Panel = new Ts1070(id, Global.ControlSystem);
                else
                {
                    Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "WARNING: Cannot create TSW controller with type '{0}'", type);
                    return;
                }
			}
			catch (Exception e)
			{
                Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "WARNING: Cannot create TSW base class. Panel will not function: {0}", e.Message);
				return;				
			}

            // Reserved sigs
            if (Panel is TswFt5ButtonSystem)
            {
                var tsw = Panel as TswFt5ButtonSystem;
                tsw.ExtenderSystemReservedSigs.Use();
                tsw.ExtenderSystemReservedSigs.DeviceExtenderSigChange
                    += ExtenderSystemReservedSigs_DeviceExtenderSigChange;

                tsw.ButtonStateChange += new ButtonEventHandler(Tsw_ButtonStateChange);

            }

            if (Panel.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "WARNING: Registration failed. Continuing, but panel may not function: {0}", Panel.RegistrationFailureReason);

            // Give up cleanly if SGD is not present.
            var sgdName = Global.FilePathPrefix + "sgd" + Global.DirectorySeparator + props.SgdFile;
            if (!File.Exists(sgdName))
            {
                Debug.Console(0, this, "Smart object file '{0}' not present in User folder. Looking for embedded file", sgdName);

                sgdName = Global.ApplicationDirectoryPathPrefix + Global.DirectorySeparator + "SGD" + Global.DirectorySeparator + props.SgdFile;

                if (!File.Exists(sgdName))
                {
                    Debug.Console(0, this, "Unable to find SGD file '{0}' in User sgd or application SGD folder. Exiting touchpanel load.", sgdName);
                    return;
                }
            }

            Panel.LoadSmartObjects(sgdName);
            Panel.SigChange += Panel_SigChange;

            AddPostActivationAction(() =>
            {
                // Check for IEssentialsRoomCombiner in DeviceManager and if found, subscribe to its event
                var roomCombiner = DeviceManager.AllDevices.FirstOrDefault((d) => d is IEssentialsRoomCombiner) as IEssentialsRoomCombiner;

                if (roomCombiner != null)
                {
                    // Subscribe to the even
                    roomCombiner.RoomCombinationScenarioChanged += new EventHandler<EventArgs>(roomCombiner_RoomCombinationScenarioChanged);

                    // Connect to the initial roomKey
                    if (roomCombiner.CurrentScenario != null)
                    {
                        // Use the current scenario
                        DetermineRoomKeyFromScenario(roomCombiner.CurrentScenario);
                    }
                    else
                    {
                        // Current Scenario not yet set.  Use default 
                        SetupPanelDrivers(_propertiesConfig.DefaultRoomKey);
                    }
                }
                else
                {
                    // No room combiner, use the default key
                    SetupPanelDrivers(_propertiesConfig.DefaultRoomKey);
                }
            });
		}

        void roomCombiner_RoomCombinationScenarioChanged(object sender, EventArgs e)
        {
            var roomCombiner = sender as IEssentialsRoomCombiner;

            DetermineRoomKeyFromScenario(roomCombiner.CurrentScenario);
        }

        /// <summary>
        /// Determines the room key to use based on the scenario
        /// </summary>
        /// <param name="scenario"></param>
        void DetermineRoomKeyFromScenario(IRoomCombinationScenario scenario)
        {
            string newRoomKey = null;

            if (scenario.UiMap.ContainsKey(Key))
            {
                newRoomKey = scenario.UiMap[Key];
            }
            else if (scenario.UiMap.ContainsKey(_propertiesConfig.DefaultRoomKey))
            {
                newRoomKey = scenario.UiMap[_propertiesConfig.DefaultRoomKey];
            }

            SetupPanelDrivers(newRoomKey);
        }


        /// <summary>
        /// Sets up drivers and links them to the room specified
        /// </summary>
        /// <param name="roomKey">key of room to link the drivers to</param>
        void SetupPanelDrivers(string roomKey)
        {
            // Clear out any existing actions
            Panel.ClearAllSigActions();

            Debug.Console(0, this, "Linking TP '{0}' to Room '{1}'", Key, roomKey);

            var mainDriver = new EssentialsPanelMainInterfaceDriver(Panel, _propertiesConfig);
            // Then the sub drivers

            // spin up different room drivers depending on room type
            var room = DeviceManager.GetDeviceForKey(roomKey);
            if (room is IEssentialsHuddleSpaceRoom)
            {
                // Screen Saver Driver

                mainDriver.ScreenSaverController = new ScreenSaverController(mainDriver, _propertiesConfig);

                // Header Driver
                Debug.Console(0, this, "Adding header driver");
                mainDriver.HeaderDriver = new EssentialsHeaderDriver(mainDriver, _propertiesConfig);

                // AV Driver
                Debug.Console(0, this, "Adding huddle space AV driver");
                var avDriver = new EssentialsHuddlePanelAvFunctionsDriver(mainDriver, _propertiesConfig);
                avDriver.DefaultRoomKey = roomKey;
                mainDriver.AvDriver = avDriver;
                avDriver.CurrentRoom = room as IEssentialsHuddleSpaceRoom;

                // Environment Driver
                if (avDriver.CurrentRoom.PropertiesConfig.Environment != null && avDriver.CurrentRoom.PropertiesConfig.Environment.DeviceKeys.Count > 0)
                {
                    Debug.Console(0, this, "Adding environment driver");
                    mainDriver.EnvironmentDriver = new EssentialsEnvironmentDriver(mainDriver, _propertiesConfig);

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
                mainDriver.ScreenSaverController = new ScreenSaverController(mainDriver, _propertiesConfig);

                // Header Driver
                mainDriver.HeaderDriver = new EssentialsHeaderDriver(mainDriver, _propertiesConfig);

                // AV Driver
                var avDriver = new EssentialsHuddleVtc1PanelAvFunctionsDriver(mainDriver, _propertiesConfig);

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
                    mainDriver.EnvironmentDriver = new EssentialsEnvironmentDriver(mainDriver, _propertiesConfig);

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

		void HomePressed()
		{
			if (BacklightTransitionedOnTimer == null)
				PanelDriver.BackButtonPressed();
		}

		void ExtenderSystemReservedSigs_DeviceExtenderSigChange(DeviceExtender currentDeviceExtender, SigEventArgs args)
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

		void Panel_SigChange(object currentDevice, Crestron.SimplSharpPro.SigEventArgs args)
		{
			if (Debug.Level == 2)
				Debug.Console(2, this, "Sig change: {0} {1}={2}", args.Sig.Type, args.Sig.Number, args.Sig.StringValue);
			var uo = args.Sig.UserObject;
			if (uo is Action<bool>)
				(uo as Action<bool>)(args.Sig.BoolValue);
			else if (uo is Action<ushort>)
				(uo as Action<ushort>)(args.Sig.UShortValue);
			else if (uo is Action<string>)
				(uo as Action<string>)(args.Sig.StringValue);
		}
	
		void Tsw_ButtonStateChange(GenericBase device, ButtonEventArgs args)
		{
			var uo = args.Button.UserObject;
			if(uo is Action<bool>)
				(uo as Action<bool>)(args.Button.State == eButtonState.Pressed);
		}
	}

    public class EssentialsTouchpanelControllerFactory : EssentialsDeviceFactory<EssentialsTouchpanelController>
    {
        public EssentialsTouchpanelControllerFactory()
        {
            TypeNames = new List<string>() { "tsw550", "tsw750", "tsw1050", "tsw560", "tsw760", "tsw1060", "tsw570", "tsw770", "ts770", "tsw1070", "ts1070", "xpanel" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            var comm = CommFactory.GetControlPropertiesConfig(dc);
            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<CrestronTouchpanelPropertiesConfig>(dc.Properties.ToString());

            Debug.Console(1, "Factory Attempting to create new EssentialsTouchpanelController");

            var panelController = new EssentialsTouchpanelController(dc.Key, dc.Name, dc.Type, props, comm.IpIdInt);

            return panelController;
        }
    }
}