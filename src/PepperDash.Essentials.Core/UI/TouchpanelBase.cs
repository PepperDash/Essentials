using System;
using System.Linq;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Room.Combining;
using PepperDash.Essentials.Core.Touchpanels;
using Serilog.Events;

namespace PepperDash.Essentials.Core.UI
{
    public abstract class TouchpanelBase: EssentialsDevice, IHasBasicTriListWithSmartObject
    {
        protected CrestronTouchpanelPropertiesConfig _config;
        public BasicTriListWithSmartObject Panel { get; private set; }

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
        protected TouchpanelBase(string key, string name, BasicTriListWithSmartObject panel, CrestronTouchpanelPropertiesConfig config)
            :base(key, name)
        {

            if (panel == null)
            {
                Debug.LogMessage(LogEventLevel.Information, this, "Panel is not valid. Touchpanel class WILL NOT work correctly");
                return;
            }

            Panel = panel;

            Panel.SigChange += Panel_SigChange;

            if (Panel is TswFt5ButtonSystem)
            {
                var tsw = Panel as TswFt5ButtonSystem;
                tsw.ExtenderSystemReservedSigs.Use();
                tsw.ExtenderSystemReservedSigs.DeviceExtenderSigChange
                    += ExtenderSystemReservedSigs_DeviceExtenderSigChange;

                tsw.ButtonStateChange += Tsw_ButtonStateChange;
            }

            _config = config;            

            AddPreActivationAction(() => {
                if (Panel.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                    Debug.LogMessage(LogEventLevel.Information, this, "WARNING: Registration failed. Continuing, but panel may not function: {0}", Panel.RegistrationFailureReason);

                // Give up cleanly if SGD is not present.
                var sgdName = Global.FilePathPrefix + "sgd" + Global.DirectorySeparator + _config.SgdFile;
                if (!File.Exists(sgdName))
                {
                    Debug.LogMessage(LogEventLevel.Information, this, "Smart object file '{0}' not present in User folder. Looking for embedded file", sgdName);

                    sgdName = Global.ApplicationDirectoryPathPrefix + Global.DirectorySeparator + "SGD" + Global.DirectorySeparator + _config.SgdFile;

                    if (!File.Exists(sgdName))
                    {
                        Debug.LogMessage(LogEventLevel.Information, this, "Unable to find SGD file '{0}' in User sgd or application SGD folder. Exiting touchpanel load.", sgdName);
                        return;
                    }
                }

                Panel.LoadSmartObjects(sgdName);
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
        /// Event handler for System Extender Events
        /// </summary>
        /// <param name="currentDeviceExtender"></param>
        /// <param name="args"></param>
        protected abstract void ExtenderSystemReservedSigs_DeviceExtenderSigChange(DeviceExtender currentDeviceExtender, SigEventArgs args);


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

        private void Panel_SigChange(object currentDevice, SigEventArgs args)
		{
			Debug.LogMessage(LogEventLevel.Verbose, this, "Sig change: {0} {1}={2}", args.Sig.Type, args.Sig.Number, args.Sig.StringValue);
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
    }
}