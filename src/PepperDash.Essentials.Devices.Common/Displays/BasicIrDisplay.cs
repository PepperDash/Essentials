using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Displays
{
	/// <summary>
	/// Represents a BasicIrDisplay
	/// </summary>
	public class BasicIrDisplay : DisplayBase, IBasicVolumeControls, IBridgeAdvanced
	{
		/// <summary>
		/// Gets or sets the IrPort
		/// </summary>
		public IrOutputPortController IrPort { get; private set; }
		/// <summary>
		/// Gets or sets the IrPulseTime
		/// </summary>
		public ushort IrPulseTime { get; set; }

		/// <summary>
		/// Gets the power is on feedback function
		/// </summary>
		protected Func<bool> PowerIsOnFeedbackFunc
		{
			get { return () => _PowerIsOn; }
		}
		/// <summary>
		/// Gets the is cooling down feedback function
		/// </summary>
		protected override Func<bool> IsCoolingDownFeedbackFunc
		{
			get { return () => _IsCoolingDown; }
		}
		/// <summary>
		/// Gets the is warming up feedback function
		/// </summary>
		protected override Func<bool> IsWarmingUpFeedbackFunc
		{
			get { return () => _IsWarmingUp; }
		}

		bool _PowerIsOn;
		bool _IsWarmingUp;
		bool _IsCoolingDown;

		/// <summary>
		/// Initializes a new instance of the BasicIrDisplay class
		/// </summary>
		/// <param name="key">The device key</param>
		/// <param name="name">The device name</param>
		/// <param name="port">The IR output port</param>
		/// <param name="irDriverFilepath">The path to the IR driver file</param>
		public BasicIrDisplay(string key, string name, IROutputPort port, string irDriverFilepath)
			: base(key, name)
		{
			IrPort = new IrOutputPortController(key + "-ir", port, irDriverFilepath);
			DeviceManager.AddDevice(IrPort);

			IsWarmingUpFeedback.OutputChange += (o, a) => Debug.LogMessage(LogEventLevel.Verbose, this, "Warming up={0}", _IsWarmingUp);
			IsCoolingDownFeedback.OutputChange += (o, a) => Debug.LogMessage(LogEventLevel.Verbose, this, "Cooling down={0}", _IsCoolingDown);

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

		/// <summary>
		/// Hdmi1 method
		/// </summary>
		public void Hdmi1()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_HDMI_1, IrPulseTime);
		}

		/// <summary>
		/// Hdmi2 method
		/// </summary>
		public void Hdmi2()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_HDMI_2, IrPulseTime);
		}

		/// <summary>
		/// Hdmi3 method
		/// </summary>
		public void Hdmi3()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_HDMI_3, IrPulseTime);
		}

		/// <summary>
		/// Hdmi4 method
		/// </summary>
		public void Hdmi4()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_HDMI_4, IrPulseTime);
		}

		/// <summary>
		/// Component1 method
		/// </summary>
		public void Component1()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_COMPONENT_1, IrPulseTime);
		}

		/// <summary>
		/// Video1 method
		/// </summary>
		public void Video1()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_VIDEO_1, IrPulseTime);
		}

		/// <summary>
		/// Antenna method
		/// </summary>
		public void Antenna()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_ANTENNA, IrPulseTime);
		}

		#region IPower Members

		/// <summary>
		/// PowerOn method
		/// </summary>
		/// <inheritdoc />
		public override void PowerOn()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_POWER_ON, IrPulseTime);
			_PowerIsOn = true;
		}

		/// <summary>
		/// PowerOff method
		/// </summary>
		public override void PowerOff()
		{
			_PowerIsOn = false;
			IrPort.Pulse(IROutputStandardCommands.IROut_POWER_OFF, IrPulseTime);
		}

		/// <summary>
		/// PowerToggle method
		/// </summary>
		public override void PowerToggle()
		{
			_PowerIsOn = false;
			IrPort.Pulse(IROutputStandardCommands.IROut_POWER, IrPulseTime);
		}

		#endregion

		#region IBasicVolumeControls Members

		/// <summary>
		/// VolumeUp method
		/// </summary>
		public void VolumeUp(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_VOL_PLUS, pressRelease);
		}

		/// <summary>
		/// VolumeDown method
		/// </summary>
		public void VolumeDown(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_VOL_MINUS, pressRelease);
		}

		/// <summary>
		/// MuteToggle method
		/// </summary>
		public void MuteToggle()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_MUTE, 200);
		}

		#endregion

		void StartWarmingTimer()
		{
			_IsWarmingUp = true;
			IsWarmingUpFeedback.FireUpdate();
			new CTimer(o =>
			{
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
		/// <summary>
		/// ExecuteSwitch method
		/// </summary>
		/// <inheritdoc />
		public override void ExecuteSwitch(object inputSelector)
		{
			Debug.LogMessage(LogEventLevel.Verbose, this, "Switching to input '{0}'", (inputSelector as Action).ToString());

			Action finishSwitch = () =>
				{
					var action = inputSelector as Action;
					if (action != null)
						action();
				};

			if (!_PowerIsOn)
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

		/// <summary>
		/// LinkToApi method
		/// </summary>
		public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
		{
			LinkDisplayToApi(this, trilist, joinStart, joinMapKey, bridge);
		}
	}

	/// <summary>
	/// Represents a BasicIrDisplayFactory
	/// </summary>
	public class BasicIrDisplayFactory : EssentialsDeviceFactory<BasicIrDisplay>
	{
		/// <summary>
		/// Initializes a new instance of the BasicIrDisplayFactory class
		/// </summary>
		public BasicIrDisplayFactory()
		{
			TypeNames = new List<string>() { "basicirdisplay" };
		}

		/// <summary>
		/// BuildDevice method
		/// </summary>
		/// <inheritdoc />
		public override EssentialsDevice BuildDevice(DeviceConfig dc)
		{
			Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new BasicIrDisplay Device");
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