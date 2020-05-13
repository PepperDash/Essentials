using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using System.Text.RegularExpressions;
using Crestron.SimplSharp.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials;



namespace PepperDash.Essentials.Devices.Common
{
    [Obsolete("This Device will be moved to a plugin in a future update")]
    public class DigitalLogger : EssentialsBridgeableDevice
    {
        public IBasicCommunication Communication { get; private set; }
        public CommunicationGather PortGather { get; private set; }
        public StatusMonitorBase CommunicationMonitor { get; private set; }

		private HttpClient WebClient;
		public string userName;
		public string password;
		public string address;
		private bool OnlineStatus; 
		public BoolFeedback OnlineFeedback;
		//private ushort CurrentPreset;
		public IntFeedback PresetFeedback;

		public Dictionary<uint, DigitalLoggerCircuit> CircuitStatus;
		public uint CircuitCount;

		public Dictionary<uint, StringFeedback> CircuitNameFeedbacks { get; private set; }
		public Dictionary<uint, BoolFeedback> CircuitIsCritical{ get; private set; }
		public Dictionary<uint, BoolFeedback> CircuitState { get; private set; }

        // new public Dictionary<string, QscDspLevelControl> LevelControlPoints { get; private set; }
		// public List<QscDspPresets> PresetList = new List<QscDspPresets>();

        public bool isSubscribed;

        private CTimer SubscriptionTimer;

        CrestronQueue CommandQueue;

        bool CommandQueueInProgress = false;

        //new public Dictionary<string, DspControlPoint> DialerControlPoints { get; private set; }

        //new public Dictionary<string, DspControlPoint> SwitcherControlPoints { get; private set; }

        /// <summary>
        /// Shows received lines as hex
        /// </summary>
        public bool ShowHexResponse { get; set; }

		public DigitalLogger(string key, string name, DigitalLoggerPropertiesConfig props) :
            base(key, name)
        {
			CircuitCount = 8;
			this.userName = props.userName;
			this.password = props.password;
			CommandQueue = new CrestronQueue(100);

			WebClient = new HttpClient();
			WebClient.UserName = this.userName;
			WebClient.Password = this.password;			
			this.address = props.address;
			WebClient.HostAddress = props.address;
			
			
 
        }

        public override bool CustomActivate()
        {
			/*
			Communication.Connect();
			CommunicationMonitor.StatusChange += (o, a) => { Debug.Console(2, this, "Communication monitor state: {0}", CommunicationMonitor.Status); };
			CommunicationMonitor.Start();
			*/ 

			OnlineFeedback = new BoolFeedback(() => { return OnlineStatus; });
			CircuitStatus = new Dictionary<uint, DigitalLoggerCircuit>();
			CircuitNameFeedbacks = new Dictionary<uint, StringFeedback>();
			CircuitIsCritical = new Dictionary<uint, BoolFeedback>();
			CircuitState = new Dictionary<uint, BoolFeedback>();
			for (uint i = 0; i < CircuitCount; i++)
			{
				uint circuit = i;
				CircuitStatus[circuit] = new DigitalLoggerCircuit();
				CircuitNameFeedbacks[circuit] = new StringFeedback(() => {
					    if (CircuitStatus[circuit].name != null) 
                        {
							return CircuitStatus[circuit].name;
					    }
					    else 
                        {
						    return "";
					    }				
					});
				CircuitIsCritical[circuit] = new BoolFeedback(() =>
				{
					if (CircuitStatus.ContainsKey(circuit))
					{
						return CircuitStatus[circuit].critical;
					}
					else
					{
						return false;
					}
				});
				CircuitState[circuit] = new BoolFeedback(() =>
				{
                    if (CircuitStatus.ContainsKey(circuit))
					{
						return CircuitStatus[circuit].state;
					}
					else
					{
						return false;
					}
				});
				PollCircuit(circuit);	
			}
			
            CrestronConsole.AddNewConsoleCommand(SendLine, "send" + Key, "", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(s => Communication.Connect(), "con" + Key, "", ConsoleAccessLevelEnum.AccessOperator);

			

            return true;
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new DigitalLoggerJoinMap();

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<DigitalLoggerJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            for (uint i = 1; i <= CircuitCount; i++)
            {
                var circuit = i;
                CircuitNameFeedbacks[circuit - 1].LinkInputSig(trilist.StringInput[joinMap.CircuitNames + circuit]);
                CircuitIsCritical[circuit - 1].LinkInputSig(trilist.BooleanInput[joinMap.CircuitIsCritical + circuit]);
                CircuitState[circuit - 1].LinkInputSig(trilist.BooleanInput[joinMap.CircuitState + circuit]);
                trilist.SetSigTrueAction(joinMap.CircuitCycle + circuit, () => CycleCircuit(circuit - 1));
                trilist.SetSigTrueAction(joinMap.CircuitOnCmd + circuit, () => TurnOnCircuit(circuit - 1));
                trilist.SetSigTrueAction(joinMap.CircuitOffCmd + circuit, () => TurnOffCircuit(circuit - 1));

            }
        }

        void socket_ConnectionChange(object sender, GenericSocketStatusChageEventArgs e)
        {
            Debug.Console(2, this, "Socket Status Change: {0}", e.Client.ClientStatus.ToString());

            if (e.Client.IsConnected)
            {
				OnlineStatus = true;
				OnlineFeedback.FireUpdate();
            }
            else
            {
                OnlineStatus = false;
				OnlineFeedback.FireUpdate();
                if (SubscriptionTimer != null)
                {
                    SubscriptionTimer.Stop();
                    SubscriptionTimer = null;
                }

                isSubscribed = false;
                CommandQueue.Clear();
                CommandQueueInProgress = false;
            }
        }

		public void PollCircuit(uint circuit)
		{
			try
			{
				string PollCircuitResponse = SendRequest(String.Format("/restapi/relay/outlets/{0}/", circuit));
				CircuitStatus[circuit] = JsonConvert.DeserializeObject<DigitalLoggerCircuit>(PollCircuitResponse);
				DigitalLoggerCircuit temp = CircuitStatus[circuit];
				Debug.Console(2, this, "DigitalLogger Circuit {0} Name: {1} State:{2}'", circuit, CircuitStatus[circuit].name, CircuitStatus[circuit].state);
				CircuitNameFeedbacks[circuit].FireUpdate();
				CircuitState[circuit].FireUpdate();
				CircuitIsCritical[circuit].FireUpdate();
			}
			catch (Exception e)
			{
				Debug.Console(0, this, "PollCircuit {0}", e);
				
			}
		}
        void Port_LineReceived(string response, HTTP_CALLBACK_ERROR error)
        {


        }

		public string SendRequest(string s)
		{
			HttpClientRequest request = new HttpClientRequest();
			string url = string.Format("http://{0}{1}", this.address, s);
			request.Url = new UrlParser(url);
			HttpClientResponse response = WebClient.Dispatch(request);
			
			Debug.Console(2, this, "DigitalLogger TX:\n'{0}'\nRX:\n'{1}'", url, response.ContentString);
			return response.ContentString;
		}
        /// <summary>
        /// Sends a command to the DSP (with delimiter appended)
        /// </summary>
        /// <param name="s">Command to send</param>
		/// 
        public void SendLine(string s)
        {

			HttpClientRequest request = new HttpClientRequest();
			string url = string.Format("http://{0}{1}", this.address, s);
			request.Url = new UrlParser(url);
			
			HttpClientResponse response = WebClient.Dispatch(request);
			
			Debug.Console(2, this, "DigitalLogger TX:\n'{0}'\nRX:\n'{1}'", url, response.ContentString);
			
        }

		public void CycleCircuit(uint circuit)
		{
			SendLine(String.Format("/outlet?{0}=CCL", circuit));
			//PollCircuit(circuit);
	
		}

		public void TurnOnCircuit(uint circuit)
		{
			SendLine(String.Format("/outlet?{0}=ON", circuit));
			//PollCircuit(circuit);
		}

		public void TurnOffCircuit(uint circuit)
		{
			SendLine(String.Format("/outlet?{0}=Off", circuit));
			//PollCircuit(circuit);
		}

        /// <summary>
        /// Adds a command from a child module to the queue
        /// </summary>
        /// <param name="command">Command object from child module</param>
        public void EnqueueCommand(QueuedCommand commandToEnqueue)
        {
            CommandQueue.Enqueue(commandToEnqueue);
            //Debug.Console(1, this, "Command (QueuedCommand) Enqueued '{0}'.  CommandQueue has '{1}' Elements.", commandToEnqueue.Command, CommandQueue.Count);

            if(!CommandQueueInProgress)
                SendNextQueuedCommand();
        }

        /// <summary>
        /// Adds a raw string command to the queue
        /// </summary>
        /// <param name="command"></param>
        public void EnqueueCommand(string command)
        {
            CommandQueue.Enqueue(command);
            //Debug.Console(1, this, "Command (string) Enqueued '{0}'.  CommandQueue has '{1}' Elements.", command, CommandQueue.Count);

            if (!CommandQueueInProgress)
                SendNextQueuedCommand();
        }

        /// <summary>
        /// Sends the next queued command to the DSP
        /// </summary>
        void SendNextQueuedCommand()
        {
                if (Communication.IsConnected && !CommandQueue.IsEmpty)
                {
                    CommandQueueInProgress = true;

                    if (CommandQueue.Peek() is QueuedCommand)
                    {
                        QueuedCommand nextCommand = new QueuedCommand();

                        nextCommand = (QueuedCommand)CommandQueue.Peek();

                        SendLine(nextCommand.Command);
                    }
                    else
                    {
                        string nextCommand = (string)CommandQueue.Peek();

                        SendLine(nextCommand);
                    }
                }
            
        }



        public void CallPreset(ushort presetNumber)
        {
			SendLine(string.Format("Preset.Take = {0}", presetNumber));
			// SendLine("cgp 1");
        }

        public class QueuedCommand
        {
            public string Command { get; set; }
            public string AttributeCode { get; set; }
           // public QscDspControlPoint ControlPoint { get; set; }
        }

		public class DigitalLoggerCircuit
		{
			public string name;
			public bool locked;
			public bool critical;
			public bool transient_state;
			public bool physical_state;
			//public int cycle_delay;
			public bool state;
		}

    }

    public class DigitalLoggerFactory : EssentialsDeviceFactory<DigitalLogger>
    {
        public DigitalLoggerFactory()
        {
            TypeNames = new List<string>() { "digitallogger" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new DigitalLogger Device");
            var props = JsonConvert.DeserializeObject<DigitalLoggerPropertiesConfig>(
                dc.Properties.ToString());
            return new DigitalLogger(dc.Key, dc.Name, props);
        }
    }
}