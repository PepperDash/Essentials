using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Routing;
using Feedback = PepperDash.Essentials.Core.Feedback;

namespace PepperDash.Essentials.Devices.Displays
{
	/// <summary>
	/// 
	/// </summary>
    public class AvocorDisplay : TwoWayDisplayBase, IBasicVolumeWithFeedback, ICommunicationMonitor, IInputDisplayPort1,
        IInputHdmi1, IInputHdmi2, IInputHdmi3, IInputHdmi4, IInputVga1, IBridgeAdvanced
	{
        public IBasicCommunication Communication { get; private set; }
        public CommunicationGather PortGather { get; private set; }
		
        public StatusMonitorBase CommunicationMonitor { get; private set; }

        public byte ID { get; private set; }

        /// <summary>
        /// 0x08
        /// </summary>
        const byte InputVga1Value = 0x00;
        /// <summary>
        /// 0x09
        /// </summary>
        const byte InputHdmi1Value = 0x09;
        /// <summary>
        /// 0x0a
        /// </summary>
        const byte InputHdmi2Value = 0x0a;
        /// <summary>
        /// 0x0b
        /// </summary>
        const byte InputHdmi3Value = 0x0b;
        /// <summary>
        /// 0x0c
        /// </summary>
        const byte InputHdmi4Value = 0x0c;
        /// <summary>
        /// 0c0d
        /// </summary>
        const byte InputDisplayPort1Value = 0x0d;
        /// <summary>
        /// 0x0e
        /// </summary>
        const byte InputIpcOpsValue = 0x0e;
        /// <summary>
        /// 0x11
        /// </summary>
        const byte InputHdmi5Value = 0x11;
        /// <summary>
        /// 0x12
        /// </summary>
        const byte InputMediaPlayerValue = 0x12;


		bool _PowerIsOn;
		bool _IsWarmingUp;
		bool _IsCoolingDown;
		ushort _VolumeLevelForSig;
        int _LastVolumeSent;
        ushort _PreMuteVolumeLevel;
		bool _IsMuted;
        RoutingInputPort _CurrentInputPort;
        ActionIncrementer VolumeIncrementer;
        bool VolumeIsRamping;
        public bool IsInStandby { get; private set; }

		protected override Func<bool> PowerIsOnFeedbackFunc { get { return () => _PowerIsOn; } }
		protected override Func<bool> IsCoolingDownFeedbackFunc { get { return () => _IsCoolingDown; } }
		protected override Func<bool> IsWarmingUpFeedbackFunc { get { return () => _IsWarmingUp; } }
        protected override Func<string> CurrentInputFeedbackFunc { get { return () => _CurrentInputPort.Key; } }    

		/// <summary>
		/// Constructor for IBasicCommunication
		/// </summary>
		public AvocorDisplay(string key, string name, IBasicCommunication comm, string id)
			: base(key, name)
		{
			Communication = comm;
            //Communication.BytesReceived += new EventHandler<GenericCommMethodReceiveBytesArgs>(Communication_BytesReceived);

            PortGather = new CommunicationGather(Communication, '\x08');
            PortGather.IncludeDelimiter = true;
            PortGather.LineReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(PortGather_LineReceived);

            ID = id == null ? (byte)0x01 : Convert.ToByte(id, 16); // If id is null, set default value of 0x01, otherwise assign value passed in constructor
            Init();
		}

		/// <summary>
		/// Constructor for TCP
		/// </summary>
		public AvocorDisplay(string key, string name, string hostname, int port, string id)
			: base(key, name)
		{
			Communication = new GenericTcpIpClient(key + "-tcp", hostname, port, 5000);

            PortGather = new CommunicationGather(Communication, '\x0d');
            PortGather.IncludeDelimiter = true;
            PortGather.LineReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(PortGather_LineReceived);

            ID = id == null ? (byte)0x01 : Convert.ToByte(id, 16); // If id is null, set default value of 0x01, otherwise assign value passed in constructor
            Init();
		}

		/// <summary>
		/// Constructor for COM
		/// </summary>
		public AvocorDisplay(string key, string name, ComPort port, ComPort.ComPortSpec spec, string id)
			: base(key, name)
		{
			Communication = new ComPortController(key + "-com", port, spec);

            PortGather = new CommunicationGather(Communication, '\x0d');
            PortGather.IncludeDelimiter = true;
            PortGather.LineReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(PortGather_LineReceived);

            ID = id == null ? (byte)0x01 : Convert.ToByte(id, 16); // If id is null, set default value of 0x01, otherwise assign value passed in constructor
            Init();
		}

        void AddRoutingInputPort(RoutingInputPort port, byte fbMatch)
        {
            port.FeedbackMatchObject = fbMatch;
            InputPorts.Add(port);
        }

		void Init()
		{
            WarmupTime = 10000;
			CooldownTime = 8000;

            CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 30000, 120000, 300000, StatusGet);
            DeviceManager.AddDevice(CommunicationMonitor);

            VolumeIncrementer = new ActionIncrementer(655, 0, 65535, 800, 80, 
                v => SetVolume((ushort)v), 
                () => _LastVolumeSent);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.HdmiIn1, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, new Action(InputHdmi1), this), InputHdmi1Value);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.HdmiIn2, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, new Action(InputHdmi2), this), InputHdmi2Value);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.HdmiIn3, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, new Action(InputHdmi3), this), InputHdmi3Value);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.HdmiIn4, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, new Action(InputHdmi4), this), InputHdmi4Value);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.HdmiIn5, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, new Action(InputHdmi5), this), InputHdmi5Value);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.DisplayPortIn1, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.DisplayPort, new Action(InputDisplayPort1), this), InputDisplayPort1Value);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.VgaIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Dvi, new Action(InputVga1), this), InputVga1Value);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.IpcOps, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Composite, new Action(InputIpcOps), this), InputIpcOpsValue);

			AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.MediaPlayer, eRoutingSignalType.Video,
                eRoutingPortConnectionType.Vga, new Action(InputMediaPlayer), this), InputMediaPlayerValue);

			VolumeLevelFeedback = new IntFeedback(() => { return _VolumeLevelForSig; });
			MuteFeedback = new BoolFeedback(() => _IsMuted);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public override bool CustomActivate()
		{
			Communication.Connect();

            var socket = Communication as ISocketStatus;
            if (socket != null)
            {
                socket.ConnectionChange += new EventHandler<GenericSocketStatusChageEventArgs>(socket_ConnectionChange);
            }

			CommunicationMonitor.StatusChange += (o, a) => { Debug.Console(2, this, "Communication monitor state: {0}", CommunicationMonitor.Status); };
			CommunicationMonitor.Start();

            StatusGet();

			return true;
		}

	    public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
	    {
	        LinkDisplayToApi(this, trilist, joinStart, joinMapKey, bridge);
	    }

	    void socket_ConnectionChange(object sender, GenericSocketStatusChageEventArgs e)
        {
            if (e.Client.IsConnected)
                StatusGet();
        }

        public override FeedbackCollection<Feedback> Feedbacks
		{
			get
			{
				var list = base.Feedbacks;
				list.AddRange(new List<Feedback>
				{
                    VolumeLevelFeedback,
                    MuteFeedback,
                    CurrentInputFeedback
				});
				return list;
			}
		}

        ///// <summary>
        ///// /
        ///// </summary>
        ///// <param name="sender"></param>
        //void Communication_BytesReceived(object sender, GenericCommMethodReceiveBytesArgs e)
        //{
        //    // This is probably not thread-safe buffering
        //    // Append the incoming bytes with whatever is in the buffer
        //    var newBytes = new byte[IncomingBuffer.Length + e.Bytes.Length];
        //    IncomingBuffer.CopyTo(newBytes, 0);
        //    e.Bytes.CopyTo(newBytes, IncomingBuffer.Length);

        //    if (Debug.Level == 2) // This check is here to prevent following string format from building unnecessarily on level 0 or 1
        //        Debug.Console(2, this, "Received:{0}", ComTextHelper.GetEscapedText(newBytes));

        //    // Need to find AA FF and have 
        //    for (int i = 0; i < newBytes.Length; i++)
        //    {
        //        if (newBytes[i] == 0xAA && newBytes[i + 1] == 0xFF)
        //        {
        //            newBytes = newBytes.Skip(i).ToArray(); // Trim off junk if there's "dirt" in the buffer

        //            // parse it
        //            // If it's at least got the header, then process it, 
        //            while (newBytes.Length > 4 && newBytes[0] == 0xAA && newBytes[1] == 0xFF)
        //            {
        //                var msgLen = newBytes[3];
        //                // if the buffer is shorter than the header (3) + message (msgLen) + checksum (1),
        //                // give and save it for next time 
        //                if (newBytes.Length < msgLen + 4)
        //                    break;

        //                // Good length, grab the message
        //                var message = newBytes.Skip(4).Take(msgLen).ToArray();

        //                // At this point, the ack/nak is the first byte
        //                if (message[0] == 0x41)
        //                {
        //                    switch (message[1]) // type byte
        //                    {
        //                        case 0x00: // General status
        //                            UpdatePowerFB(message[2], message[5]); // "power" can be misrepresented when the display sleeps
        //                            UpdateInputFb(message[5]);
        //                            UpdateVolumeFB(message[3]);
        //                            UpdateMuteFb(message[4]);
        //                            UpdateInputFb(message[5]);
        //                            break;

        //                        case 0x11:
        //                            UpdatePowerFB(message[2]);
        //                            break;

        //                        case 0x12:
        //                            UpdateVolumeFB(message[2]);
        //                            break;

        //                        case 0x13:
        //                            UpdateMuteFb(message[2]);
        //                            break;

        //                        case 0x14:
        //                            UpdateInputFb(message[2]);
        //                            break;

        //                        default:
        //                            break;
        //                    }
        //                }
        //                // Skip over what we've used and save the rest for next time
        //                newBytes = newBytes.Skip(5 + msgLen).ToArray();
        //            }
       
        //            break; // parsing will mean we can stop looking for header in loop
        //        }
        //    }

        //    // Save whatever partial message is here
        //    IncomingBuffer = newBytes;
        //}

        void PortGather_LineReceived(object sender, GenericCommMethodReceiveTextArgs e)
        {
            Debug.Console(1, this, "Receivied: '{0}'", ComTextHelper.GetEscapedText(e.Text));

            if (e.Text.IndexOf("\x50\x4F\x57") > -1)
            {
                // Power Status Response

                var value = e.Text.ToCharArray();

                switch (value[6])
                {
                    case '\x00':
                        {
                            _PowerIsOn = false;
                            break;
                        }
                    case '\x01':
                        {
                            _PowerIsOn = true;
                            break;
                        }
                }

                PowerIsOnFeedback.FireUpdate();
                Debug.Console(1, this, "PowerIsOn State: {0}", PowerIsOnFeedback.BoolValue);

            }
            else if (e.Text.IndexOf("\x4D\x49\x4E") > -1)
            {
                var value = e.Text.ToCharArray();

                var b = value[6];

                var newInput = InputPorts.FirstOrDefault(i => i.FeedbackMatchObject.Equals(b));
                if (newInput != null && newInput != _CurrentInputPort)
                {
                    _CurrentInputPort = newInput;
                    CurrentInputFeedback.FireUpdate();
                    Debug.Console(1, this, "Current Input: {0}", CurrentInputFeedback.StringValue);
                }
                
            }
            else if (e.Text.IndexOf("\x56\x4F\x4C") > -1)
            {
                // Volume Status Response

                var value = e.Text.ToCharArray();

                var b = value[6];

                var newVol = (ushort)NumericalHelpers.Scale((double)b, 0, 100, 0, 65535);
                if (!VolumeIsRamping)
                    _LastVolumeSent = newVol;
                if (newVol != _VolumeLevelForSig)
                {
                    _VolumeLevelForSig = newVol;
                    VolumeLevelFeedback.FireUpdate();

                    if (_VolumeLevelForSig > 0)
                        _IsMuted = false;
                    else
                        _IsMuted = true;

                    MuteFeedback.FireUpdate();

                    Debug.Console(1, this, "Volume Level: {0}", VolumeLevelFeedback.IntValue);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        void UpdatePowerFB(byte powerByte)
        {
            var newVal = powerByte == 1;
            if (newVal != _PowerIsOn)
            {
                _PowerIsOn = newVal;
                PowerIsOnFeedback.FireUpdate();
            }
        }

        /// <summary>
        /// Updates power status from general updates where source is included.
        /// Compensates for errant standby / power off hiccups by ignoring 
        /// power off states with input < 0x10 
        /// </summary>
        void UpdatePowerFB(byte powerByte, byte inputByte)
        {
            // This should reject errant power feedbacks when switching away from input on standby.
            if (powerByte == 0x01 && inputByte < 0x10)
                IsInStandby = true;
            if (powerByte == 0x00 && IsInStandby) // Ignore power off if coming from standby - glitch
            {
                IsInStandby = false;
                return;
            }
        
            UpdatePowerFB(powerByte);
        }

        /// <summary>
        /// 
        /// </summary>
        void UpdateVolumeFB(byte b)
        {
            var newVol = (ushort)NumericalHelpers.Scale((double)b, 0, 100, 0, 65535);
            if (!VolumeIsRamping)
                _LastVolumeSent = newVol;
            if (newVol != _VolumeLevelForSig)
            {
                _VolumeLevelForSig = newVol;
                VolumeLevelFeedback.FireUpdate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void UpdateMuteFb(byte b)
        {
            var newMute = b == 1;
            if (newMute != _IsMuted)
            {
                _IsMuted = newMute;
                MuteFeedback.FireUpdate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void UpdateInputFb(byte b)
        {
            var newInput = InputPorts.FirstOrDefault(i => i.FeedbackMatchObject.Equals(b));
            if (newInput != null && newInput != _CurrentInputPort)
            {
                _CurrentInputPort = newInput;
                CurrentInputFeedback.FireUpdate();
            }
        }

        /// <summary>
        /// Formats an outgoing message. Replaces third byte with ID and replaces last byte with checksum
        /// </summary>
        /// <param name="b"></param>
        void SendBytes(byte[] b)
        {
            b[1] = ID;

            var command = b.ToString();

            Debug.Console(1, this, "Sending: '{0}'",ComTextHelper.GetEscapedText(b));

            Communication.SendBytes(b);
        }


        /// <summary>
        /// 
        /// </summary>
        public void StatusGet()
        {
            PowerGet();

            CrestronEnvironment.Sleep(100);

            InputGet();

            CrestronEnvironment.Sleep(100);

            VolumeGet();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void PowerOn()
		{
            //Send(PowerOnCmd);
            SendBytes(new byte[] { 0x07, ID, 0x02, 0x50, 0x4F, 0x57, 0x01, 0x08, 0x0d });
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

        /// <summary>
        /// 
        /// </summary>
		public override void PowerOff()
		{
			// If a display has unreliable-power off feedback, just override this and
			// remove this check.
            if (!_IsWarmingUp && !_IsCoolingDown) // PowerIsOnFeedback.BoolValue &&
			{
                //Send(PowerOffCmd);
                SendBytes(new byte[] { 0x07, ID, 0x02, 0x50, 0x4F, 0x57, 0x00, 0x08, 0x0d });
				_IsCoolingDown = true;
				_PowerIsOn = false;
				PowerIsOnFeedback.FireUpdate();
				IsCoolingDownFeedback.FireUpdate();
				// Fake cool-down cycle
				CooldownTimer = new CTimer(o =>
					{
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

        public void PowerGet()
        {
            SendBytes(new byte[] { 0x07, ID, 0x01, 0x50, 0x4F, 0x57, 0x08, 0x0d });
        }

		public void InputHdmi1()
		{
            SendBytes(new byte[] { 0x07, ID, 0x02, 0x4D, 0x49, 0x4E, InputHdmi1Value, 0x08, 0x0d });
		}

		public void InputHdmi2()
		{
            SendBytes(new byte[] { 0x07, ID, 0x02, 0x4D, 0x49, 0x4E, InputHdmi2Value, 0x08, 0x0d });
        }

		public void InputHdmi3()
		{
            SendBytes(new byte[] { 0x07, ID, 0x02, 0x4D, 0x49, 0x4E, InputHdmi3Value, 0x08, 0x0d });
        }

        public void InputHdmi4()
        {
            SendBytes(new byte[] { 0x07, ID, 0x02, 0x4D, 0x49, 0x4E, InputHdmi4Value, 0x08, 0x0d });
        }

        public void InputHdmi5()
        {
            SendBytes(new byte[] { 0x07, ID, 0x02, 0x4D, 0x49, 0x4E, InputHdmi5Value, 0x08, 0x0d });
        }

		public void InputDisplayPort1()
		{
            SendBytes(new byte[] { 0x07, ID, 0x02, 0x4D, 0x49, 0x4E, InputDisplayPort1Value, 0x08, 0x0d });
        }

        public void InputVga1()
        {
            SendBytes(new byte[] { 0x07, ID, 0x02, 0x4D, 0x49, 0x4E, InputVga1Value, 0x08, 0x0d });
        }

        public void InputIpcOps()
        {
            SendBytes(new byte[] { 0x07, ID, 0x02, 0x4D, 0x49, 0x4E, InputIpcOpsValue, 0x08, 0x0d });
        }

        public void InputMediaPlayer()
        {
            SendBytes(new byte[] { 0x07, ID, 0x02, 0x4D, 0x49, 0x4E, InputMediaPlayerValue, 0x08, 0x0d });
        }

        public void InputGet()
        {
            SendBytes(new byte[] { 0x07, ID, 0x01, 0x4D, 0x49, 0x4E, 0x08, 0x0d });
        }

        public void VolumeGet()
        {
            SendBytes(new byte[] { 0x07, ID, 0x01, 0x56, 0x4F, 0x4C, 0x08, 0x0d });
        }


        /// <summary>
        /// Executes a switch, turning on display if necessary.
        /// </summary>
        /// <param name="selector"></param>
		public override void ExecuteSwitch(object selector)
		{
            //if (!(selector is Action))
            //    Debug.Console(1, this, "WARNING: ExecuteSwitch cannot handle type {0}", selector.GetType());
         
            if (_PowerIsOn)
                (selector as Action)();
            else // if power is off, wait until we get on FB to send it. 
            {
                // One-time event handler to wait for power on before executing switch
                EventHandler<FeedbackEventArgs> handler = null; // necessary to allow reference inside lambda to handler
                handler = (o, a) =>
                {
                    if (!_IsWarmingUp) // Done warming
                    {
                        IsWarmingUpFeedback.OutputChange -= handler;
                        (selector as Action)();
                    }
                };
                IsWarmingUpFeedback.OutputChange += handler; // attach and wait for on FB
                PowerOn();
            }
		}

        /// <summary>
        /// Scales the level to the range of the display and sends the command
        /// </summary>
        /// <param name="level"></param>
		public void SetVolume(ushort level)
		{
            _LastVolumeSent = level;
            var scaled = (int)NumericalHelpers.Scale(level, 0, 65535, 0, 100);
            // The inputs to Scale ensure that byte won't overflow
            SendBytes(new byte[] { 0x07, ID, 0x02, 0x56, 0x4F, 0x4C, Convert.ToByte(scaled), 0x08, 0x0d });
		}

		#region IBasicVolumeWithFeedback Members
	    
		public IntFeedback VolumeLevelFeedback { get; private set; }

		public BoolFeedback MuteFeedback { get; private set; }

        /// <summary>
        /// 
        /// </summary>
		public void MuteOff()
		{
            SetVolume(_PreMuteVolumeLevel);
        }

        /// <summary>
        /// 
        /// </summary>
		public void MuteOn()
		{
            _PreMuteVolumeLevel = _VolumeLevelForSig;

            SetVolume(0);
		}

        ///// <summary>
        ///// 
        ///// </summary>
        //public void MuteGet()
        //{
        //    SendBytes(new byte[] { 0x07, ID, 0x01, });
        //}

		#endregion

		#region IBasicVolumeControls Members

        /// <summary>
        /// 
        /// </summary>
		public void MuteToggle()
		{
            if (_IsMuted)
                MuteOff();
            else
                MuteOn();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pressRelease"></param>
		public void VolumeDown(bool pressRelease)
		{
            if (pressRelease)
            {
                VolumeIncrementer.StartDown();
                VolumeIsRamping = true;
            }
            else
            {
                VolumeIsRamping = false;
                VolumeIncrementer.Stop();
            }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pressRelease"></param>
		public void VolumeUp(bool pressRelease)
		{
            if (pressRelease)
            {
                VolumeIncrementer.StartUp();
                VolumeIsRamping = true;
            }
            else
            {
                VolumeIsRamping = false;
                VolumeIncrementer.Stop();
            }
        }

		#endregion
	}

    public class AvocorDisplayFactory : EssentialsDeviceFactory<AvocorDisplay>
    {
        public AvocorDisplayFactory()
        {
            TypeNames = new List<string>() { "avocorvtf" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Comm Device");
            var comm = CommFactory.CreateCommForDevice(dc);
            if (comm != null)
                return new AvocorDisplay(dc.Key, dc.Name, comm, null);
            else
                return null;

        }
    }

}