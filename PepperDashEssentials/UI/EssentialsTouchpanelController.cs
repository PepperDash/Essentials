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
using PepperDash.Essentials.Core.PageManagers;

namespace PepperDash.Essentials
{
    public class EssentialsTouchpanelController : Device, IHasBasicTriListWithSmartObject
	{
		public BasicTriListWithSmartObject Panel { get; private set; }

		public PanelDriverBase PanelDriver { get; private set; }

		CTimer BacklightTransitionedOnTimer;

		public EssentialsTouchpanelController(string key, string name, Tswx52ButtonVoiceControl tsw, 
			string projectName, string sgdPath)
			: base(key, name)
		{
			Panel = tsw;
			tsw.LoadSmartObjects(sgdPath);
			tsw.SigChange += Panel_SigChange;
		}

        public EssentialsTouchpanelController(string key, string name, Dge100 panel, string projectName, string sgdPath)
            : base(key, name)
        {
            Panel = panel;
            panel.LoadSmartObjects(sgdPath);
            panel.SigChange += Panel_SigChange;
        }

		/// <summary>
		/// Config constructor
		/// </summary>
		public EssentialsTouchpanelController(string key, string name, string type, CrestronTouchpanelPropertiesConfig props, uint id)
			: base(key, name)
		{

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
            var sgdName = Global.FilePathPrefix
                + Global.DirectorySeparator + "sgd" + Global.DirectorySeparator + props.SgdFile;
            if (!File.Exists(sgdName))
            {
                Debug.Console(0, this, "ERROR: Smart object file '{0}' not present. Exiting TSW load", sgdName);
                return;
            }

            Panel.LoadSmartObjects(sgdName);
            Panel.SigChange += Panel_SigChange;

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
}