using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials.Devices.Displays
{
	/// <summary>
	/// 
	/// </summary>
	public class SamsungMDC : TwoWayDisplayBase, IBasicVolumeWithFeedback, ICommunicationMonitor
	{
		public IBasicCommunication Communication { get; private set; }
		public CommunicationGather PortGather { get; private set; }
		public StatusMonitorBase CommunicationMonitor { get; private set; }

        private StringBuilder Buffer = new StringBuilder();

		#region Command constants

        public string Ack { get; private set; }
        public string Nak { get; private set; }

        // Volume Commands

        //public string MuteGetCmd { get; private set; }

        //public string MuteOnCmd { get; private set; }

        //public string MuteOffCmd { get; private set; }

        //public string VolumeGetCmd { get; private set; }

        /// <summary>
        /// To be appended with the requested volume level.  Does not include checksum calculation
        /// </summary>
        public string VolumeLevelPartialCmd { get; private set; }

		#endregion


        public byte ID { get; private set; }

        /// <summary>
        /// Builds the command by including the ID of the display and calculating the checksum
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private string AppendChecksum(string command)
        {
            int checksum = 0;

            //var bytes = command.ToCharArray();

            byte[] bytes = Encoding.ASCII.GetBytes(command);

            for (int i = 1; i < bytes.Length; i++)   /*Convert.ToByte(bytes[i]*/
            {
                checksum = checksum + bytes[i];
            }

            if (checksum >= 0x100)            // Check if value is greater than 0x100 and if so, remove the first digit by subtracting 0x100
                checksum = checksum - 0x100;

            var checksumByte = Convert.ToByte(checksum);
            Debug.Console(2, "Append checksum: command={0}, checksum={1}", ComTextHelper.GetEscapedText(command), checksumByte);

            string result = string.Format("{0}{1}", command, (char)checksumByte);

            return result;

        }

		bool _PowerIsOn;
		bool _IsWarmingUp;
		bool _IsCoolingDown;
		ushort _VolumeLevel;
		bool _IsMuted;
        RoutingInputPort _CurrentInputPort;

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
            Communication.TextReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(Communication_TextReceived);
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
            Communication.TextReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(Communication_TextReceived);
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
            Communication.TextReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(Communication_TextReceived);
            ID = id == null ? (byte)0x01 : Convert.ToByte(id, 16); // If id is null, set default value of 0x01, otherwise assign value passed in constructor
            Init();
		}

        void AddRoutingInputPort(RoutingInputPort port, string cmdPrefix, byte fbMatch)
        {
            port.FeedbackMatchObject = fbMatch;
            InputPorts.Add(port);

        }

		void Init()
		{

            CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 30000, 120000, 300000, AppendChecksum(string.Format("\x11{0}\x01\x01", ID)));

            // Build Command Strings

            Ack = AppendChecksum(string.Format("\xAA\xFF{0}\x03A", ID));

            Nak = AppendChecksum(string.Format("\xAA\xFF{0}\x03N", ID));

            string cmdPrefix = string.Format("\x14{0}\x01", ID);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.HdmiIn1, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Hdmi, new Action(InputHdmi1), this), cmdPrefix, 0x21);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.HdmiIn2, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Hdmi, new Action(InputHdmi2), this), cmdPrefix, 0x23);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.HdmiIn3, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Hdmi, new Action(InputHdmi3), this), cmdPrefix, 0x32);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.DisplayPortIn1, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.DisplayPort, new Action(InputDisplayPort1), this), cmdPrefix, 0x25);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.DviIn, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Dvi, new Action(InputDvi1), this), cmdPrefix, 0x18);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.CompositeIn, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Composite, new Action(InputVideo1), this), cmdPrefix, 0x08);

			AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.RgbIn1, eRoutingSignalType.Video,
                eRoutingPortConnectionType.Vga, new Action(InputRgb1), this), cmdPrefix, 0x14);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.RgbIn2, eRoutingSignalType.Video,
                eRoutingPortConnectionType.Rgb, new Action(new Action(InputRgb2)), this), cmdPrefix, 0x1E);

            // Mute Commands

            //MuteGetCmd = AppendChecksum(string.Format("\x13{0}\x00", ID));

            //MuteOnCmd = AppendChecksum(string.Format("\x13{0}\x01\x01", ID));

            //MuteOffCmd = AppendChecksum(string.Format("\x13{0}\x01\x00", ID));

            // Volume Commands

            //VolumeGetCmd = AppendChecksum(string.Format("\x12{0}\x00", ID));

            VolumeLevelPartialCmd = string.Format("\x12{0}\x01", ID);

			VolumeLevelFeedback = new IntFeedback(() => { return _VolumeLevel; });
			MuteFeedback = new BoolFeedback(() => _IsMuted);

            // Query initial device status

            //Send(PowerGetCmd);
            PowerGet();

            InputGet();
            //Send(InputGetCmd);

            VolumeGet();

            MuteGet();
		}

		~SamsungMDC()
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

		public override List<Feedback> Feedbacks
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

        void Communication_TextReceived(object sender, GenericCommMethodReceiveTextArgs e)
        {
            Buffer.Append(e.Text);

            if (Debug.Level == 2)
                Debug.Console(2, this, "Received: '{0}'", ComTextHelper.GetEscapedText(e.Text));

            ParseData();
        }

        void ParseData()
        {
            if(Buffer.Length < 8)       // Message length must be > 8 bytes
                return;

            var sample = Buffer.ToString();

            var messageStart = sample.IndexOf("\xAA\xFF");

            if (messageStart > -1)
            {
                StringBuilder garbage;

                if(messageStart > 0)        // Check for leftover bytes before first full message and remove
                     garbage = Buffer.Remove(0, messageStart - 1);

                var chunk = Buffer.Remove(messageStart, 8);     // Remove first message

                var message = chunk.ToString();

                if (message.IndexOf(Ack) > -1)                  // Check to see if Ack message
                {
                    var bytes = message.ToCharArray();

                    var commandByte = bytes[5];

                    var dataByte = bytes[6];

                    switch (commandByte)
                    {
                        case '\x11':    // Power State
                            {
                                var currentState = _PowerIsOn;

                                if (dataByte == '\x00')
                                    _PowerIsOn = false;

                                else if (dataByte == '\x01')
                                    _PowerIsOn = true;

                                if(currentState != _PowerIsOn)
                                    PowerIsOnFeedback.FireUpdate();

                                break;
                            }
                        case '\x12':    // Volume Level
                            {
                                _VolumeLevel = (ushort)Scale(dataByte, 0, 100, 0, 65535);

                                VolumeLevelFeedback.FireUpdate();
                                break;
                            }
                        case '\x13':    // Mute State
                            {
                                if (dataByte == '\x00')
                                {
                                    _IsMuted = false;
                                }
                                else if (dataByte == '\x01')
                                {
                                    _IsMuted = true;
                                }


                                MuteFeedback.FireUpdate();
                                break;
                            }
                        case '\x14':    // Input State
                            {
                                var currentInput = InputPorts.FirstOrDefault(i => i.FeedbackMatchObject.Equals(dataByte));

                                if (currentInput != null)
                                {
                                    _CurrentInputPort = currentInput;

                                    CurrentInputFeedback.FireUpdate();
                                }

                                break;
                            }
                        case '\x15':    // Aspect Ratio
                            {
                                // Not implemented yet

                                break;
                            }
                        default:
                            {
                                Debug.Console(2, this, "Unreckocnized Response Command Type: {0}", commandByte);
                                break;
                            }
                    }
                }
                else if (message.IndexOf(Nak) > -1)
                {
                    Debug.Console(2, this, "Nak received");
                }

            }
        }

        /// <summary>
        /// Append checksup and send command to device
        /// </summary>
        /// <param name="s"></param>
		void Send(string s)
		{
            var cmd = AppendChecksum(s);

			if (Debug.Level == 2)
                Debug.Console(2, this, "Send: '{0}'", ComTextHelper.GetEscapedText(cmd));
            Communication.SendText(cmd);
		}

        void SendBytes(byte[] b)
        {
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
            Communication.SendBytes(b);
        }





        public override void PowerOn()
		{
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

		public override void PowerOff()
		{
			// If a display has unreliable-power off feedback, just override this and
			// remove this check.
			if (PowerIsOnFeedback.BoolValue && !_IsWarmingUp && !_IsCoolingDown)
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

        public void PowerGet()
        {
            SendBytes(new byte[] { 0xAA, 0x11, 0x00, 0x00, 0x00 });
        }

		public void InputHdmi1()
		{
            SendBytes(new byte[] { 0xAA, 0x14, 0x00, 0x01, 0x21, 0x00 });
		}

		public void InputHdmi2()
		{
            SendBytes(new byte[] { 0xAA, 0x14, 0x00, 0x01, 0x23, 0x00 });
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

		public override void ExecuteSwitch(object selector)
		{
            if (selector is Action)
                (selector as Action).Invoke();
            else
                Debug.Console(1, this, "WARNING: ExecuteSwitch cannot handle type {0}", selector.GetType());
			//Send((string)selector);
		}

        /// <summary>
        /// Scales the level to the range of the display and sends the command
        /// </summary>
        /// <param name="level"></param>
		void SetVolume(ushort level)
		{
            var scaledLevel = Scale(level, 0, 65535, 0, 100);

            var levelString = string.Format("{0}{1}", VolumeLevelPartialCmd, Convert.ToByte(scaledLevel));
            Debug.Console(2, this, "Volume:{0}", ComTextHelper.GetEscapedText(levelString));

            var volByte = BitConverter.GetBytes(scaledLevel) FIX THIS HERE!!!!!!!!!!!
            Send(levelString);
            _VolumeLevel = level;
			VolumeLevelFeedback.FireUpdate();
		}

        double Scale(double input, double inMin, double inMax, double outMin, double outMax)
        {
            Debug.Console(2, this, "Scaling (double) input '{0}' with min '{1}'/max '{2}' to output range min '{3}'/max '{4}'", input, inMin, inMax, outMin, outMax);

            double inputRange = inMax - inMin;

            if (inputRange <= 0)
            {
                throw new ArithmeticException(string.Format("Invalid Input Range '{0}' for Scaling.  Min '{1}' Max '{2}'.", inputRange, inMin, inMax));
            }

            double outputRange = outMax - outMin;

            var output = (((input - inMin) * outputRange) / inputRange) + outMin;

            Debug.Console(2, this, "Scaled output '{0}'", output);

            return output;
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

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="level"></param>
        //void SetVolume(ushort level)
        //{
        //    SetVolume(level);
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
			//throw new NotImplementedException();
//#warning need incrementer for these
			SetVolume(_VolumeLevel++);
		}

        /// <summary>
        /// /
        /// </summary>
        /// <param name="pressRelease"></param>
		public void VolumeUp(bool pressRelease)
		{
			//throw new NotImplementedException();
            SetVolume(_VolumeLevel--);
		}

        public void VolumeGet()
        {
            SendBytes(new byte[] { 0xAA, 0x12, 0x00, 0x00, 0x00 });
        }

		#endregion
	}
}