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
				
        public StatusMonitorBase CommunicationMonitor { get; private set; }

        public byte ID { get; private set; }

		bool _PowerIsOn;
		bool _IsWarmingUp;
		bool _IsCoolingDown;
		ushort _VolumeLevel;
		bool _IsMuted;
        RoutingInputPort _CurrentInputPort;
        //CTimer StatusTimer;

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
            //Communication.TextReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(Communication_TextReceived);
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
            //Communication.TextReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(Communication_TextReceived);
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
            CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 30000, 120000, 300000, StatusGet);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.HdmiIn1, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Hdmi, new Action(InputHdmi1), this), 0x21);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.HdmiIn2, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Hdmi, new Action(InputHdmi2), this), 0x23);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.HdmiIn3, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Hdmi, new Action(InputHdmi3), this), 0x32);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.DisplayPortIn1, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.DisplayPort, new Action(InputDisplayPort1), this), 0x25);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.DviIn, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Dvi, new Action(InputDvi1), this), 0x18);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.CompositeIn, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Composite, new Action(InputVideo1), this), 0x08);

			AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.RgbIn1, eRoutingSignalType.Video,
                eRoutingPortConnectionType.Vga, new Action(InputRgb1), this), 0x14);

            AddRoutingInputPort(new RoutingInputPort(RoutingPortNames.RgbIn2, eRoutingSignalType.Video,
                eRoutingPortConnectionType.Rgb, new Action(new Action(InputRgb2)), this), 0x1E);

			VolumeLevelFeedback = new IntFeedback(() => { return _VolumeLevel; });
			MuteFeedback = new BoolFeedback(() => _IsMuted);

            StatusGet();
            //StatusTimer = new CTimer(o => StatusGet(), null, 0, 30000);
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

		public override List<Feedback> Feedbacks
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
        /// <param name="e"></param>
        void Communication_BytesReceived(object sender, GenericCommMethodReceiveBytesArgs e)
        {
            var b = e.Bytes;
            Debug.Console(0, this, "Parsing: {0}", ComTextHelper.GetEscapedText(b));
            if (b[0] == 0xAA && b[1] == 0xFF)
            {
                if (b[4] == 0x41)
                {
                    var typeByte = b[5];
                    switch (typeByte)
                    {
                        case 0x00: // General status
                            UpdatePowerFB(b[6]);
                            UpdateVolumeFB(b[7]);
                            UpdateMuteFb(b[8]);
                            UpdateInputFb(b[9]);
                            break;

                        case 0x11:
                            UpdatePowerFB(b[6]);
                            break;

                        case 0x12:
                            UpdateVolumeFB(b[6]);
                            break;
                            
                        case 0x13:
                            UpdateMuteFb(b[6]);
                            break;

                        case 0x14:
                            UpdateInputFb(b[6]);
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        void UpdatePowerFB(byte b)
        {
            var newVal = b == 1;
            if (newVal != _PowerIsOn)
            {
                _PowerIsOn = newVal;
                PowerIsOnFeedback.FireUpdate();
            }

        }

        void UpdateVolumeFB(byte b)
        {
            var newVol = (ushort)Scale((double)b, 0, 100, 0, 65535);
            if (newVol != _VolumeLevel)
            {
                _VolumeLevel = newVol;
                VolumeLevelFeedback.FireUpdate();
            }
        }

        void UpdateMuteFb(byte b)
        {
            var newMute = b == 1;
            if (newMute != _IsMuted)
            {
                _IsMuted = newMute;
                MuteFeedback.FireUpdate();
            }
        }

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
            CommunicationMonitor.ResetErrorTimers(); // Helps prevent message collisions
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
            SendBytes(new byte[] { 0xAA, 0x14, 0x00, 0x01, 0x22, 0x00 });
		}

		public void InputHdmi2()
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
            if (!(selector is Action))
                Debug.Console(1, this, "WARNING: ExecuteSwitch cannot handle type {0}", selector.GetType());
         
            if (_PowerIsOn)
                (selector as Action)();
            else // if power is off, wait until we get on FB to send it. 
            {
                // One-time event handler to wait for power on before executing switch
                EventHandler<EventArgs> handler = null; // necessary to allow reference inside lambda to handler
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
            var volByte = Convert.ToByte(Scale(level, 0, 65535, 0, 100)); // The inputs to Scale ensures it won't overflow
            SendBytes(new byte[] { 0xAA, 0x12, 0x00, 0x01, volByte, 0x00 });
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="inMin"></param>
        /// <param name="inMax"></param>
        /// <param name="outMin"></param>
        /// <param name="outMax"></param>
        /// <returns></returns>
        double Scale(double input, double inMin, double inMax, double outMin, double outMax)
        {
            //Debug.Console(2, this, "Scaling (double) input '{0}' with min '{1}'/max '{2}' to output range min '{3}'/max '{4}'", input, inMin, inMax, outMin, outMax);

            double inputRange = inMax - inMin;

            if (inputRange <= 0)
            {
                throw new ArithmeticException(string.Format("Invalid Input Range '{0}' for Scaling.  Min '{1}' Max '{2}'.", inputRange, inMin, inMax));
            }

            double outputRange = outMax - outMin;

            var output = (((input - inMin) * outputRange) / inputRange) + outMin;

           // Debug.Console(2, this, "Scaled output '{0}'", output);

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
			throw new NotImplementedException();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pressRelease"></param>
		public void VolumeUp(bool pressRelease)
		{
			throw new NotImplementedException();
		}

        public void VolumeGet()
        {
            SendBytes(new byte[] { 0xAA, 0x12, 0x00, 0x00, 0x00 });
        }

		#endregion
	}
}