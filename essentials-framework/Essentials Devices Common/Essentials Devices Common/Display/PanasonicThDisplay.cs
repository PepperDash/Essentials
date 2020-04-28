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
using Feedback = PepperDash.Essentials.Core.Feedback;

namespace PepperDash.Essentials.Devices.Displays
{
	/// <summary>
	/// 
	/// </summary>
	public class PanasonicThDisplay : TwoWayDisplayBase, IBasicVolumeWithFeedback, ICommunicationMonitor, IBridgeAdvanced
	{
		public IBasicCommunication Communication { get; private set; }
		public CommunicationGather PortGather { get; private set; }
		public StatusMonitorBase CommunicationMonitor { get; private set; }

		#region Command constants
		public const string InputGetCmd = "\x02QMI\x03";
		public const string Hdmi1Cmd = "\x02IMS:HM1\x03";
		public const string Hdmi2Cmd = "\x02IMS:HM2\x03";
		public const string Hdmi3Cmd = "";
		public const string Hdmi4Cmd = "";
		public const string Dp1Cmd = "";
		public const string Dp2Cmd = "";
		public const string Dvi1Cmd = "\x02IMS:DV1";
		public const string Video1Cmd = "";
		public const string VgaCmd = "";
		public const string RgbCmd = "";

		public const string PowerOnCmd = "\x02PON\x03";
		public const string PowerOffCmd = "\x02POF\x03";
		public const string PowerToggleIrCmd = "";

		public const string MuteOffCmd = "\x02AMT:0\x03";
		public const string MuteOnCmd = "\x02AMT:1\x03";
		public const string MuteToggleIrCmd = "\x02AMT\x03";
		public const string MuteGetCmd = "\x02QAM\x03";

		public const string VolumeGetCmd = "\x02QAV\x03";
		public const string VolumeLevelPartialCmd = "\x02AVL:"; //
		public const string VolumeUpCmd = "\x02AUU\x03";
		public const string VolumeDownCmd = "\x02AUD\x03";

		public const string MenuIrCmd = "";
		public const string UpIrCmd = "";
		public const string DownIrCmd = "";
		public const string LeftIrCmd = "";
		public const string RightIrCmd = "";
		public const string SelectIrCmd = "";
		public const string ExitIrCmd = "";
		#endregion

		bool _PowerIsOn;
		bool _IsWarmingUp;
		bool _IsCoolingDown;
		ushort _VolumeLevel;
		bool _IsMuted;

		protected override Func<bool> PowerIsOnFeedbackFunc { get { return () => _PowerIsOn; } }
		protected override Func<bool> IsCoolingDownFeedbackFunc { get { return () => _IsCoolingDown; } }
		protected override Func<bool> IsWarmingUpFeedbackFunc { get { return () => _IsWarmingUp; } }
        protected override Func<string> CurrentInputFeedbackFunc { get { return () => "Not Implemented"; } }


		/// <summary>
		/// Constructor for IBasicCommunication
		/// </summary>
		public PanasonicThDisplay(string key, string name, IBasicCommunication comm)
			: base(key, name)
		{
			Communication = comm;
			Init();
		}
		/// <summary>
		/// Constructor for TCP
		/// </summary>
		public PanasonicThDisplay(string key, string name, string hostname, int port)
			: base(key, name)
		{
			Communication = new GenericTcpIpClient(key + "-tcp", hostname, port, 5000);
			Init();
		}


		/// <summary>
		/// Constructor for COM
		/// </summary>
		public PanasonicThDisplay(string key, string name, ComPort port, ComPort.ComPortSpec spec)
			: base(key, name)
		{
			Communication = new ComPortController(key + "-com", port, spec);
			Init();
		}

		void Init()
		{
			PortGather = new CommunicationGather(Communication, '\x0d');
			PortGather.LineReceived += this.Port_LineReceived;
			CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 30000, 120000, 300000, "\x02QPW\x03"); // Query Power

			InputPorts.Add(new RoutingInputPort(RoutingPortNames.HdmiIn1, eRoutingSignalType.Audio | eRoutingSignalType.Video,
				eRoutingPortConnectionType.Hdmi, new Action(InputHdmi1), this));
			InputPorts.Add(new RoutingInputPort(RoutingPortNames.HdmiIn2, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
				eRoutingPortConnectionType.Hdmi, new Action(InputHdmi2), this));

			InputPorts.Add(new RoutingInputPort(RoutingPortNames.DviIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Dvi, new Action(InputDvi1), this));
			InputPorts.Add(new RoutingInputPort(RoutingPortNames.CompositeIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Composite, new Action(InputVideo1), this));
			InputPorts.Add(new RoutingInputPort(RoutingPortNames.VgaIn, eRoutingSignalType.Video,
                eRoutingPortConnectionType.Vga, new Action(InputVga), this));


			VolumeLevelFeedback = new IntFeedback(() => { return _VolumeLevel; });
			MuteFeedback = new BoolFeedback(() => _IsMuted);

			//    new BoolCueActionPair(CommonBoolCue.Menu, b => { if(b) Send(MenuIrCmd); }),
			//    new BoolCueActionPair(CommonBoolCue.Up, b => { if(b) Send(UpIrCmd); }),
			//    new BoolCueActionPair(CommonBoolCue.Down, b => { if(b) Send(DownIrCmd); }),
			//    new BoolCueActionPair(CommonBoolCue.Left, b => { if(b) Send(LeftIrCmd); }),
			//    new BoolCueActionPair(CommonBoolCue.Right, b => { if(b) Send(RightIrCmd); }),
			//    new BoolCueActionPair(CommonBoolCue.Select, b => { if(b) Send(SelectIrCmd); }),
			//    new BoolCueActionPair(CommonBoolCue.Exit, b => { if(b) Send(ExitIrCmd); }),
			//};
		}

		~PanasonicThDisplay()
		{
			PortGather = null;
		}

		public override bool CustomActivate()
		{
			Communication.Connect();
			CommunicationMonitor.StatusChange += (o, a) => { Debug.Console(2, this, "Communication monitor state: {0}", CommunicationMonitor.Status); };
			CommunicationMonitor.Start();
			return true;
		}

	    public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
	    {
	        LinkDisplayToApi(this, trilist, joinStart, joinMapKey, bridge);
	    }

	    public override FeedbackCollection<Feedback> Feedbacks
		{
			get
			{
				var list = base.Feedbacks;
				list.AddRange(new List<Feedback>
				{

				});
				return list;
			}
		}

		void Port_LineReceived(object dev, GenericCommMethodReceiveTextArgs args)
		{
			if (Debug.Level == 2)
				Debug.Console(2, this, "Received: '{0}'", ComTextHelper.GetEscapedText(args.Text));

			if (args.Text=="DO SOMETHING HERE EVENTUALLY")
			{
				_IsMuted = true;
				MuteFeedback.FireUpdate();
			}
		}

		void Send(string s)
		{
			if (Debug.Level == 2)
				Debug.Console(2, this, "Send: '{0}'", ComTextHelper.GetEscapedText(s));
			Communication.SendText(s);
		}


		public override void PowerOn()
		{
			Send(PowerOnCmd);
			if (!PowerIsOnFeedback.BoolValue && !_IsWarmingUp && !_IsCoolingDown)
			{
				_IsWarmingUp = true;
				IsWarmingUpFeedback.FireUpdate();
				// Fake power-up cycle
				WarmupTimer = new CTimer(o =>
					{
						_IsWarmingUp = false;
						_PowerIsOn = true;
						IsWarmingUpFeedback.FireUpdate();
						PowerIsOnFeedback.FireUpdate();
					}, WarmupTime);
			}
		}

		public override void PowerOff()
		{
			// If a display has unreliable-power off feedback, just override this and
			// remove this check.
			if (PowerIsOnFeedback.BoolValue && !_IsWarmingUp && !_IsCoolingDown)
			{
				Send(PowerOffCmd);
				_IsCoolingDown = true;
				_PowerIsOn = false;
				PowerIsOnFeedback.FireUpdate();
				IsCoolingDownFeedback.FireUpdate();
				// Fake cool-down cycle
				CooldownTimer = new CTimer(o =>
					{
						Debug.Console(2, this, "Cooldown timer ending");
						_IsCoolingDown = false;
						IsCoolingDownFeedback.FireUpdate();
					}, CooldownTime);
			}
		}		
		
		public override void PowerToggle()
		{
			if (PowerIsOnFeedback.BoolValue && !IsWarmingUpFeedback.BoolValue)
				PowerOff();
			else if (!PowerIsOnFeedback.BoolValue && !IsCoolingDownFeedback.BoolValue)
				PowerOn();
		}

		public void InputHdmi1()
		{
			Send(Hdmi1Cmd);
		}

		public void InputHdmi2()
		{
			Send(Hdmi2Cmd);
		}

		public void InputHdmi3()
		{
			Send(Hdmi3Cmd);
		}

		public void InputHdmi4()
		{
			Send(Hdmi4Cmd);
		}

		public void InputDisplayPort1()
		{
			Send(Dp1Cmd);
		}

		public void InputDisplayPort2()
		{
			Send(Dp2Cmd);
		}

		public void InputDvi1()
		{
			Send(Dvi1Cmd);
		}

		public void InputVideo1()
		{
			Send(Video1Cmd);
		}

		public void InputVga()
		{
			Send(VgaCmd);
		}

		public void InputRgb()
		{
			Send(RgbCmd);
		}

		public override void ExecuteSwitch(object selector)
		{
            if (selector is Action)
                (selector as Action).Invoke();
            else
                Debug.Console(1, this, "WARNING: ExecuteSwitch cannot handle type {0}", selector.GetType());
			//Send((string)selector);
		}

		void SetVolume(ushort level)
		{
			var levelString = string.Format("{0}{1:X3}\x03", VolumeLevelPartialCmd, level);
			
			//Debug.Console(2, this, "Volume:{0}", ComTextHelper.GetEscapedText(levelString));
			_VolumeLevel = level;
			VolumeLevelFeedback.FireUpdate();
		}

		#region IBasicVolumeWithFeedback Members
	
		public IntFeedback VolumeLevelFeedback { get; private set; }

		public BoolFeedback MuteFeedback { get; private set; }

		public void MuteOff()
		{
			Send(MuteOffCmd);
		}

		public void MuteOn()
		{
			Send(MuteOnCmd);
		}

		void IBasicVolumeWithFeedback.SetVolume(ushort level)
		{
			SetVolume(level);
		}

		#endregion

		#region IBasicVolumeControls Members

		public void MuteToggle()
		{
			Send(MuteToggleIrCmd);
		}

		public void VolumeDown(bool pressRelease)
		{
			//throw new NotImplementedException();
//#warning need incrementer for these
			SetVolume(_VolumeLevel++);
		}

		public void VolumeUp(bool pressRelease)
		{
			//throw new NotImplementedException();
            SetVolume(_VolumeLevel--);
		}

		#endregion
	}

    public class PanasonicThDisplayFactory : EssentialsDeviceFactory<PanasonicThDisplay>
    {
        public PanasonicThDisplayFactory()
        {
            TypeNames = new List<string>() { "panasonicthef" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Comm Device");
            var comm = CommFactory.CreateCommForDevice(dc);
            if (comm != null)
                return new PanasonicThDisplay(dc.Key, dc.Name, comm);
            else
                return null;
        }
    }

}