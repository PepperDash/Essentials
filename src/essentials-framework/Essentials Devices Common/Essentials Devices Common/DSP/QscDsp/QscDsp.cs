using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Codec;
using System.Text.RegularExpressions;
using Crestron.SimplSharp.Reflection;

namespace PepperDash.Essentials.Devices.Common.DSP
{

	// QUESTIONS:
	// 
	// When subscribing, just use the Instance ID for Custom Name?
	
	// Verbose on subscriptions?

    // Example subscription feedback responses
	// ! "publishToken":"name" "value":-77.0
	// ! "myLevelName" -77

	public class QscDsp : DspBase
    {
        public IBasicCommunication Communication { get; private set; }
        public CommunicationGather PortGather { get; private set; }
		public GenericCommunicationMonitor CommunicationMonitor { get; private set; }

        new public Dictionary<string, QscDspLevelControl> LevelControlPoints { get; private set; }
		new public Dictionary<string, QscDspDialer> Dialers { get; set; }
		public List<QscDspPresets> PresetList = new List<QscDspPresets>();

        public bool isSubscribed;



        CrestronQueue CommandQueue;

        bool CommandQueueInProgress = false;


        public bool ShowHexResponse { get; set; }

		public QscDsp(string key, string name, IBasicCommunication comm, QscDspPropertiesConfig props) :
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
            PortGather = new CommunicationGather(Communication, "\x0a");
            PortGather.LineReceived += this.Port_LineReceived;

            LevelControlPoints = new Dictionary<string, QscDspLevelControl>();
			Dialers = new Dictionary<string, QscDspDialer>();

			if (props.CommunicationMonitorProperties != null)
			{
				CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, props.CommunicationMonitorProperties);
			}
			else
			{
				CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 20000, 120000, 300000, "cgp 1\x0D\x0A");
			}
			

            foreach (KeyValuePair<string, QscDspLevelControlBlockConfig> block in props.LevelControlBlocks)
            {
				this.LevelControlPoints.Add(block.Key, new QscDspLevelControl(block.Key, block.Value, this));
				Debug.Console(2, this, "Added LevelControlPoint {0}", block.Key);


            }
			foreach (KeyValuePair<string, QscDspPresets> preset in props.presets)
			{
				this.addPreset(preset.Value);
				Debug.Console(2, this, "Added Preset {0} {1}", preset.Value.label, preset.Value.preset);
			}
			foreach (KeyValuePair<string, QscDialerConfig> dialerConfig in props.dialerControlBlocks)
			{
				Debug.Console(2, this, "Added Dialer {0}\n {1}", dialerConfig.Key, dialerConfig.Value);
				this.Dialers.Add(dialerConfig.Key, new QscDspDialer(dialerConfig.Value, this));
		
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
				SubscribeToAttributes();
            }
            else
            {
                // Cleanup items from this session
                CommandQueue.Clear();
                CommandQueueInProgress = false;
            }
        }

        /// <summary>
        /// Initiates the subscription process to the DSP
        /// </summary>
        void SubscribeToAttributes()
        {
			SendLine("cgd 1");
 
            SendLine("cgc 1");
			
            foreach (KeyValuePair<string, QscDspLevelControl> level in LevelControlPoints)
            {
                level.Value.Subscribe();
            }

			foreach (var dialer in Dialers)
			{
				dialer.Value.Subscribe();
			}

			if (CommunicationMonitor != null)
			{

				CommunicationMonitor.Start();
				//CommunicationMonitor = null;
			}
			else
			{
				
			}
			//CommunicationMonitor.Message = "cgp 1\x0D\x0A";
			CommunicationMonitor.StatusChange += (o, a) => { Debug.Console(2, this, "Communication monitor state: {0}", CommunicationMonitor.Status); };
			//CommunicationMonitor.Start();
            if (!CommandQueueInProgress)
                SendNextQueuedCommand();

           // ResetSubscriptionTimer();
        }



        /// <summary>
        /// Handles a response message from the DSP
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="args"></param>
        void Port_LineReceived(object dev, GenericCommMethodReceiveTextArgs args)
        {
            Debug.Console(2, this, "RX: '{0}'", args.Text);
            try
            {
                if (args.Text.IndexOf("sr ") > -1)
                {
                }
                else if (args.Text.IndexOf("cv") > -1)
                {
                    
				var changeMessage = args.Text.Split(null);

				string changedInstance = changeMessage[1].Replace("\"", "");
				Debug.Console(1, this, "cv parse Instance: {0}", changedInstance);
				bool foundItFlag = false;
                foreach (KeyValuePair<string, QscDspLevelControl> controlPoint in LevelControlPoints)
                {
					if (changedInstance == controlPoint.Value.LevelInstanceTag)
                    {
						controlPoint.Value.ParseSubscriptionMessage(changedInstance, changeMessage[4]);
						foundItFlag = true;
                        return;
                    }
					else if (changedInstance == controlPoint.Value.MuteInstanceTag)
					{
						controlPoint.Value.ParseSubscriptionMessage(changedInstance, changeMessage[2].Replace("\"", ""));
						foundItFlag = true;
						return;
					}

                }
				if (!foundItFlag)
				{
					foreach (var dialer in Dialers)
					{
						PropertyInfo[] properties = dialer.Value.Tags.GetType().GetCType().GetProperties();
						//GetPropertyValues(Tags);
						foreach (var prop in properties)
						{
							var propValue = prop.GetValue(dialer.Value.Tags, null) as string;
							if (changedInstance == propValue)
							{
								dialer.Value.ParseSubscriptionMessage(changedInstance, changeMessage[2].Replace("\"", ""));
								foundItFlag = true;
								return;
							}
						}
						if (foundItFlag)
						{
							return;
						}
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

		public void RunPresetNumber(ushort n)
		{
			RunPreset(PresetList[n].preset);
		}

		public void addPreset(QscDspPresets s)
		{
			PresetList.Add(s);
		}
        /// <summary>
        /// Sends a command to execute a preset
        /// </summary>
        /// <param name="name">Preset Name</param>
        public override void RunPreset(string name)
        {
            SendLine(string.Format("ssl {0}", name));
			SendLine("cgp 1");
        }

        public class QueuedCommand
        {
            public string Command { get; set; }
            public string AttributeCode { get; set; }
            public QscDspControlPoint ControlPoint { get; set; }
        }
    }
}