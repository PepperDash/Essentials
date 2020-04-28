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
	public class NecPSXMDisplay : TwoWayDisplayBase, IBasicVolumeWithFeedback, ICommunicationMonitor, IBridgeAdvanced
	{
		public IBasicCommunication Communication { get; private set; }
		public CommunicationGather PortGather { get; private set; }
		public StatusMonitorBase CommunicationMonitor { get; private set; }

		#region Command constants
		public const string InputGetCmd = "\x01\x30\x41\x30\x43\x30\x36\x02\x30\x30\x36\x30\x03\x03\x0D";
		public const string Hdmi1Cmd = "\x01\x30\x41\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x31\x31\x03\x72\x0d";
		public const string Hdmi2Cmd = "\x01\x30\x41\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x31\x32\x03\x71\x0D";
		public const string Hdmi3Cmd = "\x01\x30\x41\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x38\x32\x03\x78\x0D";
		public const string Hdmi4Cmd = "\x01\x30\x41\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x38\x33\x03\x79\x0D";
		public const string Dp1Cmd = "\x01\x30\x41\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x30\x46\x03\x04\x0D";
		public const string Dp2Cmd = "\x01\x30\x41\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x31\x30\x03\x73\x0D";
		public const string Dvi1Cmd = "\x01\x30\x41\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x30\x33\x03\x71\x0d";
		public const string Video1Cmd = "\x01\x30\x41\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x30\x35\x03\x77\x0D";
		public const string VgaCmd = "\x01\x30\x41\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x30\x31\x03\x73\x0D";
		public const string RgbCmd = "\x01\x30\x41\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x30\x32\x03\x70\x0D";

		public const string PowerOnCmd = "\x01\x30\x41\x30\x41\x30\x43\x02\x43\x32\x30\x33\x44\x36\x30\x30\x30\x31\x03\x73\x0D";
		public const string PowerOffCmd = "\x01\x30\x41\x30\x41\x30\x43\x02\x43\x32\x30\x33\x44\x36\x30\x30\x30\x34\x03\x76\x0D";
		public const string PowerToggleIrCmd = "\x01\x30\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x30\x33\x30\x33\x03\x02\x0D";

		public const string MuteOffCmd = "\x01\x30\x41\x30\x45\x30\x41\x02\x30\x30\x38\x44\x30\x30\x30\x30\x03\x08\x0D";
		public const string MuteOnCmd = "\x01\x30\x41\x30\x45\x30\x41\x02\x30\x30\x38\x44\x30\x30\x30\x31\x03\x09\x0D";
		public const string MuteToggleIrCmd = "\x01\x30\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x31\x42\x30\x33\x03\x72\x0D";
		public const string MuteGetCmd = "\x01\x30\x41\x30\x43\x30\x36\x02\x30\x30\x38\x44\x03\x79\x0D";

		public const string VolumeGetCmd = "\x01\x30\x41\x30\x43\x30\x36\x02\x30\x30\x36\x32\x03\x01\x0D";
		public const string VolumeLevelPartialCmd = "\x01\x30\x41\x30\x45\x30\x41\x02\x30\x30\x36\x32"; //\x46\x46\x46\x46\x03\xNN\x0D
		public const string VolumeUpCmd = "\x01\x30\x41\x30\x45\x30\x41\x02\x31\x30\x41\x44\x30\x30\x30\x31\x03\x71\x0D";
		public const string VolumeDownCmd = "\x01\x30\x41\x30\x45\x30\x41\x02\x31\x30\x41\x44\x30\x30\x30\x32\x03\x72\x0D";

		public const string MenuIrCmd = "\x01\x30\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x32\x30\x30\x33\x03\x03\x0D";
		public const string UpIrCmd = "\x01\x30\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x31\x35\x30\x33\x03\x05\x0D";
		public const string DownIrCmd = "\x01\x30\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x31\x34\x30\x33\x03\x04\x0D";
		public const string LeftIrCmd = "\x01\x30\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x32\x31\x30\x33\x03\x02\x0D";
		public const string RightIrCmd = "\x01\x30\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x32\x32\x30\x33\x03\x01\x0D";
		public const string SelectIrCmd = "\x01\x30\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x32\x33\x30\x33\x03\x00\x0D";
		public const string ExitIrCmd = "\x01\x30\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x31\x46\x30\x33\x03\x76\x0D";
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
		public NecPSXMDisplay(string key, string name, IBasicCommunication comm)
			: base(key, name)
		{
			Communication = comm;
			Init();
		}
		/// <summary>
		/// Constructor for TCP
		/// </summary>
		public NecPSXMDisplay(string key, string name, string hostname, int port)
			: base(key, name)
		{
			Communication = new GenericTcpIpClient(key + "-tcp", hostname, port, 5000);
			Init();
		}


		/// <summary>
		/// Constructor for COM
		/// </summary>
		public NecPSXMDisplay(string key, string name, ComPort port, ComPort.ComPortSpec spec)
			: base(key, name)
		{
			Communication = new ComPortController(key + "-com", port, spec);
			Init();
		}

		void Init()
		{
			PortGather = new CommunicationGather(Communication, '\x0d');
			PortGather.LineReceived += this.Port_LineReceived;
			CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 30000, 120000, 300000, "xx\x0d");

			InputPorts.Add(new RoutingInputPort(RoutingPortNames.HdmiIn1, eRoutingSignalType.Audio | eRoutingSignalType.Video,
				eRoutingPortConnectionType.Hdmi, new Action(InputHdmi1), this));
			InputPorts.Add(new RoutingInputPort(RoutingPortNames.HdmiIn2, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
				eRoutingPortConnectionType.Hdmi, new Action(InputHdmi2), this));
			InputPorts.Add(new RoutingInputPort(RoutingPortNames.HdmiIn3, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, new Action(InputHdmi3), this));
			InputPorts.Add(new RoutingInputPort(RoutingPortNames.HdmiIn4, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, new Action(InputHdmi4), this));
			InputPorts.Add(new RoutingInputPort(RoutingPortNames.DisplayPortIn1, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.DisplayPort, new Action(InputDisplayPort1), this));
			InputPorts.Add(new RoutingInputPort(RoutingPortNames.DisplayPortIn2, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.DisplayPort, new Action(InputDisplayPort2), this));
			InputPorts.Add(new RoutingInputPort(RoutingPortNames.DviIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Dvi, new Action(InputDvi1), this));
			InputPorts.Add(new RoutingInputPort(RoutingPortNames.CompositeIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Composite, new Action(InputVideo1), this));
			InputPorts.Add(new RoutingInputPort(RoutingPortNames.VgaIn, eRoutingSignalType.Video,
                eRoutingPortConnectionType.Vga, new Action(InputVga), this));
			InputPorts.Add(new RoutingInputPort(RoutingPortNames.RgbIn, eRoutingSignalType.Video,
                eRoutingPortConnectionType.Rgb, new Action(new Action(InputRgb)), this));

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

		~NecPSXMDisplay()
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

		void AppendChecksumAndSend(string s)
		{
			int x = 0;
			for (int i = 1; i < s.Length; i++)
				x = x ^ s[i];

			string send = s + (char)x + '\x0d';
			Send(send);
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
			var levelString = string.Format("{0}{1:X4}\x03", VolumeLevelPartialCmd, level);
			AppendChecksumAndSend(levelString);
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

    public class NecPSXMDisplayFactory : EssentialsDeviceFactory<NecPSXMDisplay>
    {
        public NecPSXMDisplayFactory()
        {
            TypeNames = new List<string>() { "necmpsx" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Comm Device");
            var comm = CommFactory.CreateCommForDevice(dc);
            if (comm != null)
                return new NecPSXMDisplay(dc.Key, dc.Name, comm);
            else
                return null;
        }
    }

}