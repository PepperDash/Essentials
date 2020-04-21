using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using System.Text.RegularExpressions;


namespace PepperDash.Essentials.Devices.Common.DSP
{

	// QUESTIONS:
	// 
	// When subscribing, just use the Instance ID for Custom Name?
	
	// Verbose on subscriptions?

    // Example subscription feedback responses
	// ! "publishToken":"name" "value":-77.0
	// ! "myLevelName" -77

    public class BiampTesiraForteDsp : DspBase
    {
        public IBasicCommunication Communication { get; private set; }
        public CommunicationGather PortGather { get; private set; }
        public StatusMonitorBase CommunicationMonitor { get; private set; }

        new public Dictionary<string, TesiraForteLevelControl> LevelControlPoints { get; private set; }

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

        public BiampTesiraForteDsp(string key, string name, IBasicCommunication comm, BiampTesiraFortePropertiesConfig props) :
            base(key, name)
        {
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
            PortGather = new CommunicationGather(Communication, "\x0d\x0a");
            PortGather.LineReceived += this.Port_LineReceived;
            if (props.CommunicationMonitorProperties != null)
            {
                CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, props.CommunicationMonitorProperties);
            }
            else
            {
//#warning Need to deal with this poll string
                CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 120000, 120000, 300000, "SESSION get aliases\x0d\x0a");
            }

            LevelControlPoints = new Dictionary<string, TesiraForteLevelControl>();

            foreach (KeyValuePair<string, BiampTesiraForteLevelControlBlockConfig> block in props.LevelControlBlocks)
            {
                this.LevelControlPoints.Add(block.Key, new TesiraForteLevelControl(block.Key, block.Value, this));
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
            Debug.Console(2, this, "Socket Status Change: {0}", e.Client.ClientStatus.ToString());

            if (e.Client.IsConnected)
            {
                // Tasks on connect
            }
            else
            {
                // Cleanup items from this session

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
        void SubscribeToAttributes()
        {
            SendLine("SESSION set verbose true");

            foreach (KeyValuePair<string, TesiraForteLevelControl> level in LevelControlPoints)
            {
                level.Value.Subscribe();
            }

            if (!CommandQueueInProgress)
                SendNextQueuedCommand();

            ResetSubscriptionTimer();
        }

        /// <summary>
        /// Resets or Sets the subscription timer
        /// </summary>
        void ResetSubscriptionTimer()
        {
            isSubscribed = true;

            if (SubscriptionTimer != null)
            {
                SubscriptionTimer = new CTimer(o => SubscribeToAttributes(), 30000);
                SubscriptionTimer.Reset();

            }
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

            Debug.Console(1, this, "RX: '{0}'", args.Text);

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
                    if (args.Text == "+OK" || args.Text.IndexOf("list\":") > -1 )       // Check for a simple "+OK" only 'ack' repsonse or a list response and ignore
                        return;

                    // response is not from a subscribed attribute.  From a get/set/toggle/increment/decrement command
                    
                    if (!CommandQueue.IsEmpty)
                    {
                        if (CommandQueue.Peek() is QueuedCommand)
                        {
                            // Expected response belongs to a child class
                            QueuedCommand tempCommand = (QueuedCommand)CommandQueue.TryToDequeue();
                            //Debug.Console(1, this, "Command Dequeued. CommandQueue Size: {0}", CommandQueue.Count);

                            tempCommand.ControlPoint.ParseGetMessage(tempCommand.AttributeCode, args.Text);
                        }
                        else
                        {
                            // Expected response belongs to this class
                            string temp = (string)CommandQueue.TryToDequeue();
                            //Debug.Console(1, this, "Command Dequeued. CommandQueue Size: {0}", CommandQueue.Count);

                        }

                        if (CommandQueue.IsEmpty)
                            CommandQueueInProgress = false;
                        else
                            SendNextQueuedCommand();

                    }


                }
                else if (args.Text.IndexOf("-ERR") > -1)
                {
                    // Error response

                    switch (args.Text)
                    {
                        case "-ERR ALREADY_SUBSCRIBED":
                            {
                                ResetSubscriptionTimer();
                                break;
                            }
                        default:
                            {
                                Debug.Console(0, this, "Error From DSP: '{0}'", args.Text);
                                break;
                            }
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
            Debug.Console(1, this, "TX: '{0}'", s);
            Communication.SendText(s + "\x0a");
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
            //Debug.Console(2, this, "Attempting to send next queued command. CommandQueueInProgress: {0}  Communication isConnected: {1}", CommandQueueInProgress, Communication.IsConnected);

                //if (CommandQueue.IsEmpty)
                //    CommandQueueInProgress = false;

                //Debug.Console(1, this, "CommandQueue has {0} Elements:\n", CommandQueue.Count);

                //foreach (object o in CommandQueue)
                //{
                //    if (o is string)
                //        Debug.Console(1, this, "{0}", o);
                //    else if(o is QueuedCommand)
                //    {
                //        var item = (QueuedCommand)o;
                //        Debug.Console(1, this, "{0}", item.Command);
                //    }
                //}

                //Debug.Console(1, this, "End of CommandQueue");

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

    public class BiampTesiraForteDspFactory : EssentialsDeviceFactory<BiampTesiraForteDsp>
    {
        public BiampTesiraForteDspFactory()
        {
            TypeNames = new List<string>() { "biamptesira" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new BiampTesira Device");
            var comm = CommFactory.CreateCommForDevice(dc);
            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<BiampTesiraFortePropertiesConfig>(
                dc.Properties.ToString());
            return new BiampTesiraForteDsp(dc.Key, dc.Name, comm, props);
        }
    }

}