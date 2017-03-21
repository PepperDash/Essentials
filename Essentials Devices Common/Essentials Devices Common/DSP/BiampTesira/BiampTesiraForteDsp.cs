using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using System.Text.RegularExpressions;


namespace PepperDash.Essentials.Devices.Common.DSP
{

	// QUESTIONS:
	// 
	// When subscribing, just use the Instance ID for Custom Name?
	
	// Verbose on subscriptions?

	// ! "publishToken":"name" "value":-77.0
	// ! "myLevelName" -77


	public class BiampTesiraForteDsp : DspBase
	{
		public IBasicCommunication Communication { get; private set; }
		public CommunicationGather PortGather { get; private set; }
		public StatusMonitorBase CommunicationMonitor { get; private set; }

        new public Dictionary<string, TesiraForteLevelControl> LevelControlPoints { get; private set; }

        public bool isSubscribed;

        CrestronQueue CommandQueue;

        //new public Dictionary<string, DspControlPoint> DialerControlPoints { get; private set; }

        //new public Dictionary<string, DspControlPoint> SwitcherControlPoints { get; private set; }

		/// <summary>
		/// Shows received lines as hex
		/// </summary>
		public bool ShowHexResponse { get; set; }

		public BiampTesiraForteDsp(string key, string name, IBasicCommunication comm, BiampTesiraFortePropertiesConfig props) :
			base(key, name)
		{
            CommandQueue = new CrestronQueue(20);

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
			PortGather = new CommunicationGather(Communication, '\x0a');
			PortGather.LineReceived += this.Port_LineReceived;
            if (props.CommunicationMonitorProperties != null)
            {
                CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, props.CommunicationMonitorProperties);
            }
            else
            {
#warning Need to deal with this poll string
                CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 120000, 120000, 300000, "");
            }

            LevelControlPoints = new Dictionary<string, TesiraForteLevelControl>();

            foreach (KeyValuePair<string, BiampTesiraForteLevelControlBlockConfig> block in props.LevelControlBlocks)
            {
                this.LevelControlPoints.Add(block.Key, new TesiraForteLevelControl(block.Value, this));    
            }

		}

		public override bool CustomActivate()
		{
			Communication.Connect();
			CommunicationMonitor.StatusChange += (o, a) => { Debug.Console(2, this, "Communication monitor state: {0}", CommunicationMonitor.Status); };
			CommunicationMonitor.Start();

			CrestronConsole.AddNewConsoleCommand(SendLine, "send" + Key, "", ConsoleAccessLevelEnum.AccessOperator);
			CrestronConsole.AddNewConsoleCommand(s => Communication.Connect(), "con" + Key, "", ConsoleAccessLevelEnum.AccessOperator);
			return true;
		}

        void socket_ConnectionChange(object sender, GenericSocketStatusChageEventArgs e)
        {
            if (e.Client.IsConnected)
            {
                Debug.Console(2, this, "Socket Status Change: {0}", e.Client.ClientStatus.ToString());
                // Maybe wait for message indicating valid TTP session
            }
        }

        /// <summary>
        /// Initiates the subscription process to the DSP
        /// </summary>
        void SubscribeToAttributes()
        {
            EnqueueCommand("SESSION set verbose true");

            foreach (KeyValuePair<string, TesiraForteLevelControl> level in LevelControlPoints)
            {
                level.Value.Subscribe();
            }

            ResetSubscriptionTimer();
        }

        

        void ResetSubscriptionTimer()
        {
#warning Add code to create/reset a CTimer to periodically check for subscribtion status

            isSubscribed = true;

            //CTimer SubscribtionTimer = new CTimer(
        }

        /// <summary>
        /// Handles a response message from the DSP
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="args"></param>
		void Port_LineReceived(object dev, GenericCommMethodReceiveTextArgs args)
		{
			if (Debug.Level == 2)
				Debug.Console(2, this, "RX: '{0}'",
					ShowHexResponse ? ComTextHelper.GetEscapedText(args.Text) : args.Text);               

                try
                {
                    if (args.Text.IndexOf("Welcome to the Tesira Text Protocol Server...") > -1)    
                    {
                        // Indicates a new TTP session

                        SubscribeToAttributes();
                    }
                    else if (args.Text.IndexOf("publishToken") > -1)
                    {
                        // response is from a subscribed attribute

                        string pattern = "! \"publishToken\":[\"](.*)[\"] \"value\":(.*)";

                        Match match = Regex.Match(args.Text, pattern);

                        if (match.Success)
                        {

                            string key;

                            string customName;

                            string value;

                            customName = match.Groups[1].Value;

                            // Finds the key (everything before the '~' character
                            key = customName.Substring(0, customName.IndexOf("~", 0) - 1);

                            value = match.Groups[2].Value;

                            foreach (KeyValuePair<string, TesiraForteLevelControl> controlPoint in LevelControlPoints)
                            {
                                if (customName == controlPoint.Value.LevelCustomName || customName == controlPoint.Value.MuteCustomName)
                                {
                                    controlPoint.Value.ParseSubscriptionMessage(customName, value);
                                    return;
                                }

                            }
                        }

                        /// same for dialers
                        /// same for switchers

                    }
                    else if (args.Text.IndexOf("+OK") > -1)
                    {
                        if (args.Text.Length > 4)       // Check for a simple "+OK" only 'ack' repsonse
                            return;

                        // response is not from a subscribed attribute.  From a get/set/toggle/increment/decrement command

                        if (!CommandQueue.IsEmpty)
                        {
                            if(CommandQueue.Peek() is QueuedCommand)
                            {
                                // Expected response belongs to a child class
                                QueuedCommand tempCommand = (QueuedCommand)CommandQueue.Dequeue();

                                tempCommand.ControlPoint.ParseGetMessage(tempCommand.AttributeCode, args.Text);
                            }
                            else
                            {
                                // Expected response belongs to this class
                                string temp = (string)CommandQueue.Dequeue();

                            }

                            SendNextQueuedCommand();

                        }


                    }
                    else if (args.Text.IndexOf("-ERR") > -1)
                    {
                        // Error response

                        if (args.Text == "-ERR ALREADY_SUBSCRIBED")
                        {
                            // Subscription still valid
                            ResetSubscriptionTimer();
                        }
                        else
                        {
                            SubscribeToAttributes();
                        }

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
            Debug.Console(2, this, "TX: '{0}'", s);
			Communication.SendText(s + "\x0a");
		}

        /// <summary>
        /// Adds a command from a child module to the queue
        /// </summary>
        /// <param name="command">Command object from child module</param>
        public void EnqueueCommand(QueuedCommand commandToEnqueue)
        {
            CommandQueue.Enqueue(commandToEnqueue);         
        }

        /// <summary>
        /// Adds a raw string command to the queue
        /// </summary>
        /// <param name="command"></param>
        public void EnqueueCommand(string command)
        {
            CommandQueue.Enqueue(command);
        }

        /// <summary>
        /// Sends the next queued command to the DSP
        /// </summary>
        void SendNextQueuedCommand()
        {
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

        /// <summary>
        /// Sends a command to execute a preset
        /// </summary>
        /// <param name="name">Preset Name</param>
		public override void RunPreset(string name)
		{
            SendLine(string.Format("DEVICE recallPreset {0}", name));
		}

        public class QueuedCommand
        {
            public string Command { get; set; }
            public string AttributeCode { get; set; }
            public TesiraForteControlPoint ControlPoint { get; set; }
        }
	}
}