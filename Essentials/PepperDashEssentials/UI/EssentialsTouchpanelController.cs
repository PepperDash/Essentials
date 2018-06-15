﻿using System;
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
using PepperDash.Essentials.Core.PageManagers;

namespace PepperDash.Essentials
{
	public class EssentialsTouchpanelController : Device, ICommunicationMonitor
	{
		public BasicTriListWithSmartObject Panel { get; private set; }

        public StatusMonitorBase CommunicationMonitor { get; private set; }

        public bool IncludeInFusionRoomHealth { get; private set; }

		public PanelDriverBase PanelDriver { get; private set; }

		CTimer BacklightTransitionedOnTimer;

		public EssentialsTouchpanelController(string key, string name, Tswx52ButtonVoiceControl tsw, 
			string projectName, string sgdPath)
			: base(key, name)
		{
			Panel = tsw;
			tsw.LoadSmartObjects(sgdPath);
			tsw.SigChange += new Crestron.SimplSharpPro.DeviceSupport.SigEventHandler(Tsw_SigChange);
		}

		/// <summary>
		/// Config constructor
		/// </summary>
		public EssentialsTouchpanelController(string key, string name, string type, CrestronTouchpanelPropertiesConfig props, uint id)
			: base(key, name)
		{
            IncludeInFusionRoomHealth = props.IncludeInFusionRoomHealth;

            Debug.Console(0, this, "Creating hardware...");
            type = type.ToLower();
            try
            {
                if (type == "crestronapp")
                {
                    var app = new CrestronApp(id, Global.ControlSystem);
                    app.ParameterProjectName.Value = props.ProjectName;
                    Panel = app;
                }
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
                else if (type == "xpanel")
                    Panel = new XpanelForSmartGraphics(id, Global.ControlSystem);
                else
                {
                    Debug.Console(0, this, "WARNING: Cannot create TSW controller with type '{0}'", type);
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.Console(0, this, "WARNING: Cannot create TSW base class. Panel will not function: {0}", e.Message);
                return;
            }

            CommunicationMonitor = new CrestronGenericBaseCommunicationMonitor(this, Panel, 30000, 120000);

			AddPostActivationAction(() =>
				{
                    //Debug.Console(0, this, "Creating hardware...");
                    //type = type.ToLower();
                    //try
                    //{
                    //    if (type == "crestronapp")
                    //    {
                    //        var app = new CrestronApp(id, Global.ControlSystem);
                    //        app.ParameterProjectName.Value = props.ProjectName;
                    //        Panel = app;
                    //    }
                    //    else if (type == "tsw550")
                    //        Panel = new Tsw550(id, Global.ControlSystem);
                    //    else if (type == "tsw552")
                    //        Panel = new Tsw552(id, Global.ControlSystem);
                    //    else if (type == "tsw560")
                    //        Panel = new Tsw560(id, Global.ControlSystem);
                    //    else if (type == "tsw750")
                    //        Panel = new Tsw750(id, Global.ControlSystem);
                    //    else if (type == "tsw752")
                    //        Panel = new Tsw752(id, Global.ControlSystem);
                    //    else if (type == "tsw760")
                    //        Panel = new Tsw760(id, Global.ControlSystem);
                    //    else if (type == "tsw1050")
                    //        Panel = new Tsw1050(id, Global.ControlSystem);
                    //    else if (type == "tsw1052")
                    //        Panel = new Tsw1052(id, Global.ControlSystem);
                    //    else if (type == "tsw1060")
                    //        Panel = new Tsw1060(id, Global.ControlSystem);
                    //    else if (type == "xpanel")
                    //        Panel = new XpanelForSmartGraphics(id, Global.ControlSystem);
                    //    else
                    //    {
                    //        Debug.Console(0, this, "WARNING: Cannot create TSW controller with type '{0}'", type);
                    //        return;
                    //    }
                    //}
                    //catch (Exception e)
                    //{
                    //    Debug.Console(0, this, "WARNING: Cannot create TSW base class. Panel will not function: {0}", e.Message);
                    //    return;				
                    //}


                    // Reserved sigs
                    if (Panel is TswFt5ButtonSystem)
                    {
                        var tsw = Panel as TswFt5ButtonSystem;
                        tsw.ExtenderSystemReservedSigs.Use();
                        tsw.ExtenderSystemReservedSigs.DeviceExtenderSigChange
                            += ExtenderSystemReservedSigs_DeviceExtenderSigChange;
                    }

					//CrestronInvoke.BeginInvoke(o =>
					//    {
                            var regSuccess = Panel.Register();
                            if (regSuccess != eDeviceRegistrationUnRegistrationResponse.Success)
                                Debug.Console(0, this, "WARNING: Registration failed. Continuing, but panel may not function: {0}", regSuccess);

                            // Give up cleanly if SGD is not present.
                            var sgdName = @"\NVRAM\Program" + InitialParametersClass.ApplicationNumber
                                + @"\sgd\" + props.SgdFile;
                            if (!File.Exists(sgdName))
                            {
                                Debug.Console(0, this, "ERROR: Smart object file '{0}' not present. Exiting TSW load", sgdName);
                                return;
                            }

                            Panel.LoadSmartObjects(sgdName);
                            Panel.SigChange += Tsw_SigChange;

                            var mainDriver = new EssentialsPanelMainInterfaceDriver(Panel, props);
                            // Then the AV driver

                            // spin up different room drivers depending on room type
                            var room = DeviceManager.GetDeviceForKey(props.DefaultRoomKey);
                            if (room is EssentialsHuddleSpaceRoom)
                            {
                                Debug.Console(0, this, "Adding huddle space driver");
                                var avDriver = new EssentialsHuddlePanelAvFunctionsDriver(mainDriver, props);
                                avDriver.CurrentRoom = room as EssentialsHuddleSpaceRoom;
                                avDriver.DefaultRoomKey = props.DefaultRoomKey;
                                mainDriver.AvDriver = avDriver;
                                LoadAndShowDriver(mainDriver);  // This is a little convoluted.

                                if (Panel is TswFt5ButtonSystem)
                                {
                                    var tsw = Panel as TswFt5ButtonSystem;
                                    // Wire up hard keys
                                    tsw.Power.UserObject = new Action<bool>(b => { if (!b) avDriver.PowerButtonPressed(); });
                                    //tsw.Home.UserObject = new Action<bool>(b => { if (!b) HomePressed(); });
                                    tsw.Up.UserObject = new Action<bool>(avDriver.VolumeUpPress);
                                    tsw.Down.UserObject = new Action<bool>(avDriver.VolumeDownPress);
                                    tsw.ButtonStateChange += new ButtonEventHandler(Tsw_ButtonStateChange);
                                }
                            }
                            else if (room is EssentialsPresentationRoom)
                            {
                                Debug.Console(0, this, "Adding presentation room driver");
                                var avDriver = new EssentialsPresentationPanelAvFunctionsDriver(mainDriver, props);
                                avDriver.CurrentRoom = room as EssentialsPresentationRoom;
                                avDriver.DefaultRoomKey = props.DefaultRoomKey;
                                mainDriver.AvDriver = avDriver;
                                LoadAndShowDriver(mainDriver);
                                
                                if (Panel is TswFt5ButtonSystem)
                                {
                                    var tsw = Panel as TswFt5ButtonSystem;
                                    // Wire up hard keys
                                    tsw.Power.UserObject = new Action<bool>(b => { if (!b) avDriver.PowerButtonPressed(); });
                                    //tsw.Home.UserObject = new Action<bool>(b => { if (!b) HomePressed(); });
                                    tsw.Up.UserObject = new Action<bool>(avDriver.VolumeUpPress);
                                    tsw.Down.UserObject = new Action<bool>(avDriver.VolumeDownPress);
                                    tsw.ButtonStateChange += new ButtonEventHandler(Tsw_ButtonStateChange);
                                }
                            }
                            else if (room is EssentialsHuddleVtc1Room)
                            {
                                Debug.Console(0, this, "Adding huddle space driver");
                                var avDriver = new EssentialsHuddleVtc1PanelAvFunctionsDriver(mainDriver, props);
                                var codecDriver = new PepperDash.Essentials.UIDrivers.VC.EssentialsVideoCodecUiDriver(Panel, avDriver,
                                    (room as EssentialsHuddleVtc1Room).VideoCodec);
                                avDriver.SetVideoCodecDriver(codecDriver);
                                avDriver.CurrentRoom = room as EssentialsHuddleVtc1Room;
                                avDriver.DefaultRoomKey = props.DefaultRoomKey;
                                mainDriver.AvDriver = avDriver;
                                LoadAndShowDriver(mainDriver);  // This is a little convoluted.

                                if (Panel is TswFt5ButtonSystem)
                                {
                                    var tsw = Panel as TswFt5ButtonSystem;
                                    // Wire up hard keys
                                    tsw.Power.UserObject = new Action<bool>(b => { if (!b) avDriver.EndMeetingPress(); });
                                    //tsw.Home.UserObject = new Action<bool>(b => { if (!b) HomePressed(); });
                                    tsw.Up.UserObject = new Action<bool>(avDriver.VolumeUpPress);
                                    tsw.Down.UserObject = new Action<bool>(avDriver.VolumeDownPress);
                                    tsw.ButtonStateChange += new ButtonEventHandler(Tsw_ButtonStateChange);
                                }
                            }
                            else
                            {
                                Debug.Console(0, this, "ERROR: Cannot load AvFunctionsDriver for room '{0}'", props.DefaultRoomKey);
                            }
						//}, 0);
				});
		}

		public void LoadAndShowDriver(PanelDriverBase driver)
		{
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

		void Tsw_SigChange(object currentDevice, Crestron.SimplSharpPro.SigEventArgs args)
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
}