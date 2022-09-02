using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using Crestron.SimplSharpPro.UI;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Core.UI
{
    public abstract class TouchpanelBase: EssentialsDevice, IHasBasicTriListWithSmartObject
    {
        protected CrestronTouchpanelPropertiesConfig _config;
        public BasicTriListWithSmartObject Panel { get; private set; }

        /// <summary>
        /// Constructor for pre-created Panel. This constructor attempts to load the SGD file from the provided SGD path and subscribes to the 
        /// `SigChange` event for the provided panel.
        /// </summary>
        /// <param name="key">Essentials Device Key</param>
        /// <param name="name">Essentials Device Name</param>
        /// <param name="tsw">Provided TSW Panel</param>
        /// <param name="sgdPath">Path to SGD file</param>
        protected TouchpanelBase(string key, string name, Tswx52ButtonVoiceControl tsw, string sgdPath)
            :base(key, name)
        {
            Panel = tsw;

            //moving loading of SGD to preactivation action...should make creation quicker
            AddPreActivationAction(() => {
                if(!string.IsNullOrEmpty(sgdPath))
                {
                    Debug.Console(1, this, "Loading sgd file from {0}", sgdPath);
                    Panel.LoadSmartObjects(sgdPath);
                    return;
                }

                Debug.Console(1, this, "No SGD file path defined");
            });

            Panel.SigChange += Panel_SigChange;
        }

        /// <summary>
        /// Constructor for use with DGE panels. This constructor attempts to load the provided SGD file from the provided SGD path and subscribes to the
        /// `SigChange` event for the provided DGE.
        /// </summary>
        /// <param name="key">Essentials Device Key</param>
        /// <param name="name">Essentials Device Name</param>
        /// <param name="dge">Provided DGE</param>        
        /// <param name="sgdPath">Path to SGD file</param>
        protected TouchpanelBase(string key, string name, Dge100 dge, string sgdPath):base(key, name)
        {
            Panel = dge;

            AddPreActivationAction(() =>
            {
                if (!string.IsNullOrEmpty(sgdPath))
                {
                    Debug.Console(1, this, "Loading sgd file from {0}", sgdPath);
                    Panel.LoadSmartObjects(sgdPath);
                    return;
                }

                Debug.Console(1, this, "No SGD file path defined");
            });

            Panel.SigChange += Panel_SigChange;
        }

        /// <summary>
        /// Constructor for use with device Factory. A touch panel device will be created based on the provided IP-ID and the
        /// type of the panel. The SGD File path can be specified using the config property, or a default one located in the program directory if none
        /// is provided.
        /// </summary>
        /// <param name="key">Essentials Device Key</param>
        /// <param name="name">Essentials Device Name</param>
        /// <param name="type">Touchpanel Type to build</param>
        /// <param name="config">Touchpanel Configuration</param>
        /// <param name="id">IP-ID to use for touch panel</param>
        protected TouchpanelBase(string key, string name, string type, CrestronTouchpanelPropertiesConfig config, uint id)
            :base(key, name)
        {
            var panel = GetPanelForType(type, id);
            if(panel != null)
            {
                Panel = panel;
            }

            if (Panel is TswFt5ButtonSystem)
            {
                var tsw = Panel as TswFt5ButtonSystem;
                tsw.ExtenderSystemReservedSigs.Use();
                tsw.ExtenderSystemReservedSigs.DeviceExtenderSigChange
                    += ExtenderSystemReservedSigs_DeviceExtenderSigChange;

                tsw.ButtonStateChange += Tsw_ButtonStateChange;
            }
            

            AddPreActivationAction(() => {
                if (Panel.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                    Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "WARNING: Registration failed. Continuing, but panel may not function: {0}", Panel.RegistrationFailureReason);

                // Give up cleanly if SGD is not present.
                var sgdName = Global.FilePathPrefix + "sgd" + Global.DirectorySeparator + _config.SgdFile;
                if (!File.Exists(sgdName))
                {
                    Debug.Console(0, this, "Smart object file '{0}' not present in User folder. Looking for embedded file", sgdName);

                    sgdName = Global.ApplicationDirectoryPathPrefix + Global.DirectorySeparator + "SGD" + Global.DirectorySeparator + config.SgdFile;

                    if (!File.Exists(sgdName))
                    {
                        Debug.Console(0, this, "Unable to find SGD file '{0}' in User sgd or application SGD folder. Exiting touchpanel load.", sgdName);
                        return;
                    }
                }
            });

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
                        SetupPanelDrivers(_config.DefaultRoomKey);
                    }
                }
                else
                {
                    // No room combiner, use the default key
                    SetupPanelDrivers(_config.DefaultRoomKey);
                }
            });
        }

        /// <summary>
        /// Setup Panel operation
        /// </summary>
        /// <param name="roomKey">Room Key for this panel</param>
        protected abstract void SetupPanelDrivers(string roomKey);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void roomCombiner_RoomCombinationScenarioChanged(object sender, EventArgs e)
        {
            var roomCombiner = sender as IEssentialsRoomCombiner;

            DetermineRoomKeyFromScenario(roomCombiner.CurrentScenario);
        }

        /// <summary>
        /// Determines the room key to use based on the scenario
        /// </summary>
        /// <param name="scenario"></param>
        protected virtual void DetermineRoomKeyFromScenario(IRoomCombinationScenario scenario)
        {
            string newRoomKey = null;

            if (scenario.UiMap.ContainsKey(Key))
            {
                newRoomKey = scenario.UiMap[Key];
            }
            else if (scenario.UiMap.ContainsKey(_config.DefaultRoomKey))
            {
                newRoomKey = scenario.UiMap[_config.DefaultRoomKey];
            }

            SetupPanelDrivers(newRoomKey);
        }

        private BasicTriListWithSmartObject GetPanelForType(string type, uint id)
        {
            type = type.ToLower();
			try
			{
                if (type == "crestronapp")
                {
                    var app = new CrestronApp(id, Global.ControlSystem);
                    app.ParameterProjectName.Value = _config.ProjectName;
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
                    Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "WARNING: Cannot create TSW controller with type '{0}'", type);
                    return null;
                }
			}
			catch (Exception e)
			{
                Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "WARNING: Cannot create TSW base class. Panel will not function: {0}", e.Message);
				return null;				
			}
        }

        private void Panel_SigChange(object currentDevice, Crestron.SimplSharpPro.SigEventArgs args)
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
	
		private void Tsw_ButtonStateChange(GenericBase device, ButtonEventArgs args)
		{
			var uo = args.Button.UserObject;
			if(uo is Action<bool>)
				(uo as Action<bool>)(args.Button.State == eButtonState.Pressed);
		}

        protected abstract void ExtenderSystemReservedSigs_DeviceExtenderSigChange(DeviceExtender currentDeviceExtender, SigEventArgs args);
    }
}