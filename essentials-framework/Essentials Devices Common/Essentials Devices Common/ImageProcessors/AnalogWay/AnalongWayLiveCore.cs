using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using System.Text.RegularExpressions;


namespace PepperDash.Essentials.Devices.Common
{

    public class AnalogWayLiveCore : EssentialsDevice
    {
        public IBasicCommunication Communication { get; private set; }
        public CommunicationGather PortGather { get; private set; }
        public StatusMonitorBase CommunicationMonitor { get; private set; }
		public string userName;
		public string password;
		private bool OnlineStatus; 
		public BoolFeedback OnlineFeedback;
		private ushort CurrentPreset;
		public IntFeedback PresetFeedback;

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

		public AnalogWayLiveCore(string key, string name, IBasicCommunication comm, AnalogWayLiveCorePropertiesConfig props) :
            base(key, name)
        {

			this.userName = props.userName;
			this.password = props.password;
			CommandQueue = new CrestronQueue(100);


            Communication = comm;
		
            var socket = comm as ISocketStatus;
            if (socket != null)
            {
                // This instance uses IP control
                socket.ConnectionChange += new EventHandler<GenericSocketStatusChageEventArgs>(socket_ConnectionChange);
            }
            else
            {
                // This instance uses RS-232 control
            }
            PortGather = new CommunicationGather(Communication, "\x0a");
            PortGather.LineReceived += this.Port_LineReceived;
			if (props.CommunicationMonitorProperties != null)
			{
				CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, props.CommunicationMonitorProperties);
			}
			else
			{
				//#warning Need to deal with this poll string
				CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 120000, 120000, 300000, "System.Status\x0A\x0D");
			}

        }

        public override bool CustomActivate()
        {

			Communication.Connect();
			CommunicationMonitor.StatusChange += (o, a) => { Debug.Console(2, this, "Communication monitor state: {0}", CommunicationMonitor.Status); };
			CommunicationMonitor.Start();

			OnlineFeedback = new BoolFeedback(() => { return OnlineStatus; });
			PresetFeedback = new IntFeedback(() => { return CurrentPreset; });

            CrestronConsole.AddNewConsoleCommand(SendLine, "send" + Key, "", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(s => Communication.Connect(), "con" + Key, "", ConsoleAccessLevelEnum.AccessOperator);
            return true;
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

        /// <summary>
        /// Initiates the subscription process to the DSP
        /// </summary>




        /// <summary>
        /// Handles a response message from the DSP
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="args"></param>
        void Port_LineReceived(object dev, GenericCommMethodReceiveTextArgs args)
        {
            Debug.Console(2, this, "TVOneCurio RX: '{0}'", args.Text);
            try
            {
                if (args.Text.IndexOf("login") > -1)
                {
					SendLine(string.Format("Login({0},{1})", this.userName, this.password));
                }
				else if (args.Text.IndexOf("!Done Preset.Take =") > -1)
                {

					string presetNumberParse = args.Text.Remove(0, args.Text.IndexOf("=") + 2);

					Debug.Console(1, this, "Preset Parse: {0}", presetNumberParse);
					CurrentPreset = ushort.Parse(presetNumberParse);
					PresetFeedback.FireUpdate();


                }

                
            }
            catch (Exception e)
            {
                if (Debug.Level == 2)
                    Debug.Console(2, this, "Error parsing response: '{0}'\n{1}", args.Text, e);
            }

        }

        /// <summary>
        /// Sends a command to the DSP (with delimiter appended)
        /// </summary>
        /// <param name="s">Command to send</param>
        public void SendLine(string s)
        {
            Debug.Console(1, this, "TVOne Cusio TX: '{0}'", s);
            Communication.SendText(s + "\x0d\x0a");
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
    }

    public class AnalogWayLiveCoreFactory : EssentialsDeviceFactory<AnalogWayLiveCore>
    {
        public AnalogWayLiveCoreFactory()
        {
            TypeNames = new List<string>() { "analogwaylivecore" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new AnalogWayLiveCore Device");
            var comm = CommFactory.CreateCommForDevice(dc);
            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<AnalogWayLiveCorePropertiesConfig>(
                dc.Properties.ToString());
            return new AnalogWayLiveCore(dc.Key, dc.Name, comm, props);
        }
    }

}