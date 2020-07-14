using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials.Core
{
    [Description("Wrapper class for a Basic IR Display")]
    public class BasicIrDisplay : DisplayBase, IBasicVolumeControls, IBridgeAdvanced
	{
		public IrOutputPortController IrPort { get; private set; }
		public ushort IrPulseTime { get; set; }

		protected override Func<bool> PowerIsOnFeedbackFunc 
		{ 
			get { return () => _PowerIsOn; } 
		}
		protected override Func<bool> IsCoolingDownFeedbackFunc
		{
			get { return () => _IsCoolingDown; }
		}
		protected override Func<bool> IsWarmingUpFeedbackFunc
		{
			get { return () => _IsWarmingUp; }
		}

		bool _PowerIsOn;
		bool _IsWarmingUp;
		bool _IsCoolingDown;

		public BasicIrDisplay(string key, string name, IROutputPort port, string irDriverFilepath)
			: base(key, name)
		{
			IrPort = new IrOutputPortController(key + "-ir", port, irDriverFilepath);
			DeviceManager.AddDevice(IrPort);

			PowerIsOnFeedback.OutputChange += (o, a) => {
				Debug.Console(2, this, "Power on={0}", _PowerIsOn);
				if (_PowerIsOn) StartWarmingTimer();
				else StartCoolingTimer();
			};
			IsWarmingUpFeedback.OutputChange += (o, a) => Debug.Console(2, this, "Warming up={0}", _IsWarmingUp);
			IsCoolingDownFeedback.OutputChange += (o, a) => Debug.Console(2, this, "Cooling down={0}", _IsCoolingDown);

			InputPorts.AddRange(new RoutingPortCollection<RoutingInputPort>
			{
				new RoutingInputPort(RoutingPortNames.HdmiIn1, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
					eRoutingPortConnectionType.Hdmi, new Action(Hdmi1), this, false),
				new RoutingInputPort(RoutingPortNames.HdmiIn2, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
					eRoutingPortConnectionType.Hdmi, new Action(Hdmi2), this, false),
				new RoutingInputPort(RoutingPortNames.HdmiIn3, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
					eRoutingPortConnectionType.Hdmi, new Action(Hdmi3), this, false),
				new RoutingInputPort(RoutingPortNames.HdmiIn4, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
					eRoutingPortConnectionType.Hdmi, new Action(Hdmi4), this, false),
				new RoutingInputPort(RoutingPortNames.ComponentIn, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
					eRoutingPortConnectionType.Hdmi, new Action(Component1), this, false),
				new RoutingInputPort(RoutingPortNames.CompositeIn, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
					eRoutingPortConnectionType.Hdmi, new Action(Video1), this, false),
				new RoutingInputPort(RoutingPortNames.AntennaIn, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
					eRoutingPortConnectionType.Hdmi, new Action(Antenna), this, false),
			});
		}

		public void Hdmi1()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_HDMI_1, IrPulseTime);
		}

		public void Hdmi2()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_HDMI_2, IrPulseTime);
		}

		public void Hdmi3()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_HDMI_3, IrPulseTime);
		}

		public void Hdmi4()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_HDMI_4, IrPulseTime);
		}

		public void Component1()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_COMPONENT_1, IrPulseTime);
		}

		public void Video1()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_VIDEO_1, IrPulseTime);
		}

		public void Antenna()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_ANTENNA, IrPulseTime);
		}

		#region IPower Members

		public override void PowerOn()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_POWER_ON, IrPulseTime);
			_PowerIsOn = true;
			PowerIsOnFeedback.FireUpdate();
		}

		public override void PowerOff()
		{
			_PowerIsOn = false;
			PowerIsOnFeedback.FireUpdate();
			IrPort.Pulse(IROutputStandardCommands.IROut_POWER_OFF, IrPulseTime);
		}

		public override void PowerToggle()
		{
			_PowerIsOn = false;
			PowerIsOnFeedback.FireUpdate();
			IrPort.Pulse(IROutputStandardCommands.IROut_POWER, IrPulseTime);
		}

		#endregion

		#region IBasicVolumeControls Members

		public void VolumeUp(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_VOL_PLUS, pressRelease);
		}

		public void VolumeDown(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_VOL_MINUS, pressRelease);
		}

		public void MuteToggle()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_MUTE, 200);
		}

		#endregion

		void StartWarmingTimer()
		{
			_IsWarmingUp = true;
			IsWarmingUpFeedback.FireUpdate();
			new CTimer(o => {
				_IsWarmingUp = false;
				IsWarmingUpFeedback.FireUpdate();
			}, 10000);
		}

		void StartCoolingTimer()
		{
			_IsCoolingDown = true;
			IsCoolingDownFeedback.FireUpdate();
			new CTimer(o =>
			{
				_IsCoolingDown = false;
				IsCoolingDownFeedback.FireUpdate();
			}, 7000);
		}

		#region IRoutingSink Members

		/// <summary>
		/// Typically called by the discovery routing algorithm.
		/// </summary>
		/// <param name="inputSelector">A delegate containing the input selector method to call</param>
		public override void ExecuteSwitch(object inputSelector)
		{
            Debug.Console(2, this, "Switching to input '{0}'", (inputSelector as Action).ToString());

			Action finishSwitch = () =>
				{
					var action = inputSelector as Action;
					if (action != null)
						action();
				};

			if (!PowerIsOnFeedback.BoolValue)
			{
				PowerOn();
				EventHandler<FeedbackEventArgs> oneTimer = null;
				oneTimer = (o, a) =>
					 {
						 if (IsWarmingUpFeedback.BoolValue) return; // Only catch done warming
						 IsWarmingUpFeedback.OutputChange -= oneTimer;
						 finishSwitch();
					 };
				IsWarmingUpFeedback.OutputChange += oneTimer;
			}
			else // Do it!
				finishSwitch();
		}

		#endregion

	    public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
	    {
	        LinkDisplayToApi(this, trilist, joinStart, joinMapKey, bridge);
	    }
	}

    public class BasicIrDisplayFactory : EssentialsDeviceFactory<BasicIrDisplay>
    {
        public BasicIrDisplayFactory()
        {
            TypeNames = new List<string>() { "basicirdisplay" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new BasicIrDisplay Device");
            var ir = IRPortHelper.GetIrPort(dc.Properties);
            if (ir != null)
            {
                var display = new BasicIrDisplay(dc.Key, dc.Name, ir.Port, ir.FileName);
                display.IrPulseTime = 200;       // Set default pulse time for IR commands.
                return display;
            }

            return null;
        }
    }

}