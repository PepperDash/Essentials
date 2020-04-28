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

using Newtonsoft.Json.Linq;

namespace PepperDash.Essentials.Devices.Displays
{
	/// <summary>
	/// 
	/// </summary>
    public class SamsungMDC : TwoWayDisplayBase, IBasicVolumeWithFeedback, ICommunicationMonitor, IInputDisplayPort1, IInputDisplayPort2,
        IInputHdmi1, IInputHdmi2, IInputHdmi3, IInputHdmi4, IBridgeAdvanced
	{
		public IBasicCommunication Communication { get; private set; }
				
        public StatusMonitorBase CommunicationMonitor { get; private set; }

        public byte ID { get; private set; }

        bool LastCommandSentWasVolume;

		bool _PowerIsOn;
		bool _IsWarmingUp;
		bool _IsCoolingDown;
		ushort _VolumeLevelForSig;
        int _LastVolumeSent;
		bool _IsMuted;
        RoutingInputPort _CurrentInputPort;
        byte[] IncomingBuffer = new byte[]{};
        ActionIncrementer VolumeIncrementer;
        bool VolumeIsRamping;
        public bool IsInStandby { get; private set; }
		bool IsPoweringOnIgnorePowerFb;

		protected override Func<bool> PowerIsOnFeedbackFunc { get { return () => _PowerIsOn; } }
		protected override Func<bool> IsCoolingDownFeedbackFunc { get { return () => _IsCoolingDown; } }
		protected override Func<bool> IsWarmingUpFeedbackFunc { get { return () => _IsWarmingUp; } }
        protected override Func<string> CurrentInputFeedbackFunc { get { return () => _CurrentInputPort.Key; } }    

		/// <summary>
		/// Constructor for IBasicCommunication
		/// </summary>
		public SamsungMDC(string key, string name, IBasicCommunication comm, string id)
			: base(key, name)
		{
			Communication = comm;
            Communication.BytesReceived += new EventHandler<GenericCommMethodReceiveBytesArgs>(Communication_BytesReceived);

            ID = id == null ? (byte)0x01 : Convert.ToByte(id, 16); // If id is null, set default value of 0x01, otherwise assign value passed in constructor
            Init();
		}

		/// <summary>
		/// Constructor for TCP
		/// </summary>
		public SamsungMDC(string key, string name, string hostname, int port, string id)
			: base(key, name)
		{
			Communication = new GenericTcpIpClient(key + "-tcp", hostname, port, 5000);
            ID = id == null ? (byte)0x01 : Convert.ToByte(id, 16); // If id is null, set default value of 0x01, otherwise assign value passed in constructor
            Init();
		}

		/// <summary>
		/// Constructor for COM
		/// </summary>
		public SamsungMDC(string key, string name, ComPort port, ComPort.ComPortSpec spec, string id)
			: base(key, name)
		{
			Communication = new ComPortController(key + "-com", port, spec);
            //Communication.TextReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(Communication_TextReceived);

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

            CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 2000, 120000, 300000, StatusGet);
            DeviceManager.AddDevice(CommunicationMonitor);

            VolumeIncrementer = new ActionIncrementer(655, 0, 65535, 800, 80, 
                v => SetVolume((ushort)v), 
                () => _LastVolumeSent);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.HdmiIn1, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, new Action(InputHdmi1), this), 0x21);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.HdmiIn1PC, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, new Action(InputHdmi1PC), this), 0x22);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.HdmiIn2, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, new Action(InputHdmi2), this), 0x23);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.HdmiIn2PC, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, new Action(InputHdmi2PC), this), 0x24);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.HdmiIn3, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, new Action(InputHdmi3), this), 0x32);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.DisplayPortIn1, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.DisplayPort, new Action(InputDisplayPort1), this), 0x25);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.DviIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Dvi, new Action(InputDvi1), this), 0x18);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.CompositeIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Composite, new Action(InputVideo1), this), 0x08);

			AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.RgbIn1, eRoutingSignalType.Video,
                eRoutingPortConnectionType.Vga, new Action(InputRgb1), this), 0x14);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.RgbIn2, eRoutingSignalType.Video,
                eRoutingPortConnectionType.Rgb, new Action(new Action(InputRgb2)), this), 0x1E);

			VolumeLevelFeedback = new IntFeedback(() => { return _VolumeLevelForSig; });
			MuteFeedback = new BoolFeedback(() => _IsMuted);

            StatusGet();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
                    VolumeLevelFeedback,
                    MuteFeedback,
                    CurrentInputFeedback
				});
				return list;
			}
		}

        /// <summary>
        /// /
        /// </summary>
        /// <param name="sender"></param>
        void Communication_BytesReceived(object sender, GenericCommMethodReceiveBytesArgs e)
        {
            try
            {
                // This is probably not thread-safe buffering
                // Append the incoming bytes with whatever is in the buffer
                var newBytes = new byte[IncomingBuffer.Length + e.Bytes.Length];
                IncomingBuffer.CopyTo(newBytes, 0);
                e.Bytes.CopyTo(newBytes, IncomingBuffer.Length);

                if (Debug.Level == 2) // This check is here to prevent following string format from building unnecessarily on level 0 or 1
                    Debug.Console(2, this, "Received:{0}", ComTextHelper.GetEscapedText(newBytes));

                // Need to find AA FF and have 
                for (int i = 0; i < newBytes.Length; i++)
                {
                    if (newBytes[i] == 0xAA && newBytes[i + 1] == 0xFF)
                    {
                        newBytes = newBytes.Skip(i).ToArray(); // Trim off junk if there's "dirt" in the buffer

                        // parse it
                        // If it's at least got the header, then process it, 
                        while (newBytes.Length > 4 && newBytes[0] == 0xAA && newBytes[1] == 0xFF)
                        {
                            var msgLen = newBytes[3];
                            // if the buffer is shorter than the header (3) + message (msgLen) + checksum (1),
                            // give and save it for next time 
                            if (newBytes.Length < msgLen + 4)
                                break;

                            // Good length, grab the message
                            var message = newBytes.Skip(4).Take(msgLen).ToArray();

                            // At this point, the ack/nak is the first byte
                            if (message[0] == 0x41)
                            {
                                switch (message[1]) // type byte
                                {
                                    case 0x00: // General status
                                        //UpdatePowerFB(message[2], message[5]); // "power" can be misrepresented when the display sleeps

                                        // Handle the first power on fb when waiting for it.
                                        if (IsPoweringOnIgnorePowerFb && message[2] == 0x01)
                                            IsPoweringOnIgnorePowerFb = false;
                                        // Ignore general-status power off messages when powering up
                                        if (!(IsPoweringOnIgnorePowerFb && message[2] == 0x00))
                                            UpdatePowerFB(message[2]);
                                        UpdateVolumeFB(message[3]);
                                        UpdateMuteFb(message[4]);
                                        UpdateInputFb(message[5]);
                                        break;

                                    case 0x11:
                                        UpdatePowerFB(message[2]);
                                        break;

                                    case 0x12:
                                        UpdateVolumeFB(message[2]);
                                        break;

                                    case 0x13:
                                        UpdateMuteFb(message[2]);
                                        break;

                                    case 0x14:
                                        UpdateInputFb(message[2]);
                                        break;

                                    default:
                                        break;
                                }
                            }
                            // Skip over what we've used and save the rest for next time
                            newBytes = newBytes.Skip(5 + msgLen).ToArray();
                        }

                        break; // parsing will mean we can stop looking for header in loop
                    }
                }

                // Save whatever partial message is here
                IncomingBuffer = newBytes;
            }
            catch (Exception err)
            {
                Debug.Console(2, this, "Error parsing feedback: {0}", err);
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
                Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "Feedback Power State: {0}", _PowerIsOn);
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
            if (LastCommandSentWasVolume)   // If the last command sent was volume
                if (b[1] != 0x12)           // Check if this command is volume, and if not, delay this command 
                    CrestronEnvironment.Sleep(100);

            b[2] = ID;
            // append checksum by adding all bytes, except last which should be 00
            int checksum = 0;
            for (var i = 1; i < b.Length - 1; i++) // add 2nd through 2nd-to-last bytes
            {
                checksum += b[i];
            }
            checksum = checksum & 0x000000FF; // mask off MSBs
            b[b.Length - 1] = (byte)checksum;
            if(Debug.Level == 2) // This check is here to prevent following string format from building unnecessarily on level 0 or 1
                Debug.Console(2, this, "Sending:{0}", ComTextHelper.GetEscapedText(b));

            if (b[1] == 0x12)
                LastCommandSentWasVolume = true;
            else
                LastCommandSentWasVolume = false;

            Communication.SendBytes(b);
        }


        /// <summary>
        /// 
        /// </summary>
        public void StatusGet()
        {
            SendBytes(new byte[] { 0xAA, 0x00, 0x00, 0x00, 0x00 });
        }

        /// <summary>
        /// 
        /// </summary>
        public override void PowerOn()
		{
            Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "Powering On Display");

			IsPoweringOnIgnorePowerFb = true;
            //Send(PowerOnCmd);
            SendBytes(new byte[] { 0xAA, 0x11, 0x00, 0x01, 0x01, 0x00 });
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
            Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "Powering Off Display");

			IsPoweringOnIgnorePowerFb = false;
			// If a display has unreliable-power off feedback, just override this and
			// remove this check.
            if (!_IsWarmingUp && !_IsCoolingDown) // PowerIsOnFeedback.BoolValue &&
			{
                //Send(PowerOffCmd);
                SendBytes(new byte[] { 0xAA, 0x11, 0x00, 0x01, 0x00, 0x00 });
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
            SendBytes(new byte[] { 0xAA, 0x11, 0x00, 0x00, 0x00 });
        }

		public void InputHdmi1()
		{
            SendBytes(new byte[] { 0xAA, 0x14, 0x00, 0x01, 0x21, 0x00 });
		}

        public void InputHdmi1PC()
        {
            SendBytes(new byte[] { 0xAA, 0x14, 0x00, 0x01, 0x22, 0x00 });
        }

		public void InputHdmi2()
		{
            SendBytes(new byte[] { 0xAA, 0x14, 0x00, 0x01, 0x23, 0x00 });
        }

        public void InputHdmi2PC()
        {
            SendBytes(new byte[] { 0xAA, 0x14, 0x00, 0x01, 0x24, 0x00 });
        }

		public void InputHdmi3()
		{
            SendBytes(new byte[] { 0xAA, 0x14, 0x00, 0x01, 0x32, 0x00 });
        }

        public void InputHdmi4()
        {
            SendBytes(new byte[] { 0xAA, 0x14, 0x00, 0x01, 0x34, 0x00 });
        }

		public void InputDisplayPort1()
		{
            SendBytes(new byte[] { 0xAA, 0x14, 0x00, 0x01, 0x25, 0x00 });
        }

        public void InputDisplayPort2()
        {
            SendBytes(new byte[] { 0xAA, 0x14, 0x00, 0x01, 0x26, 0x00 });
        }

		public void InputDvi1()
		{
            SendBytes(new byte[] { 0xAA, 0x14, 0x00, 0x01, 0x18, 0x00 });
        }

		public void InputVideo1()
		{
            SendBytes(new byte[] { 0xAA, 0x14, 0x00, 0x01, 0x08, 0x00 });
        }

		public void InputRgb1()
		{
            SendBytes(new byte[] { 0xAA, 0x14, 0x00, 0x01, 0x14, 0x00 });
        }

        public void InputRgb2()
        {
            SendBytes(new byte[] { 0xAA, 0x14, 0x00, 0x01, 0x1E, 0x00 });
        }

        public void InputGet()
        {
            SendBytes(new byte[] { 0xAA, 0x14, 0x00, 0x00, 0x00 });
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
            SendBytes(new byte[] { 0xAA, 0x12, 0x00, 0x01, Convert.ToByte(scaled), 0x00 });
		}

		#region IBasicVolumeWithFeedback Members
	    
		public IntFeedback VolumeLevelFeedback { get; private set; }

		public BoolFeedback MuteFeedback { get; private set; }

        /// <summary>
        /// 
        /// </summary>
		public void MuteOff()
		{
            SendBytes(new byte[] { 0xAA, 0x13, 0x00, 0x01, 0x00, 0x00 });
        }

        /// <summary>
        /// 
        /// </summary>
		public void MuteOn()
		{
            SendBytes(new byte[] { 0xAA, 0x13, 0x00, 0x01, 0x01, 0x00 });
		}

        /// <summary>
        /// 
        /// </summary>
        public void MuteGet()
        {
            SendBytes(new byte[] { 0xAA, 0x13, 0x00, 0x00, 0x00 });
        }

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

        /// <summary>
        /// 
        /// </summary>
        public void VolumeGet()
        {
            SendBytes(new byte[] { 0xAA, 0x12, 0x00, 0x00, 0x00 });
        }

		#endregion
	}

    public class SamsungMDCFactory : EssentialsDeviceFactory<SamsungMDC>
    {
        public SamsungMDCFactory()
        {
            TypeNames = new List<string>() { "samsungmdc" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Comm Device");
            var comm = CommFactory.CreateCommForDevice(dc);
            if (comm != null)
                return new SamsungMDC(dc.Key, dc.Name, comm, dc.Properties["id"].Value<string>());
            else
                return null;
        }
    }

}