using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core.Config;


namespace PepperDash.Essentials.Core
{
	public class ConsoleCommMockDevice : EssentialsDevice, ICommunicationMonitor
	{
		public IBasicCommunication Communication { get; private set; }
		public CommunicationGather PortGather { get; private set; }
		public StatusMonitorBase CommunicationMonitor { get; private set; }

		/// <summary>
		/// Defaults to \x0a
		/// </summary>
		public string LineEnding { get; set; }

		/// <summary>
		/// Set to true to show responses in full hex
		/// </summary>
		public bool ShowHexResponse { get; set; }

		public ConsoleCommMockDevice(string key, string name, ConsoleCommMockDevicePropertiesConfig props, IBasicCommunication comm)
			:base(key, name)
		{
			Communication = comm;
			PortGather = new CommunicationGather(Communication, '\x0d');
			PortGather.LineReceived += this.Port_LineReceived;
			CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, props.CommunicationMonitorProperties);
			LineEnding = props.LineEnding;
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

		void Port_LineReceived(object dev, GenericCommMethodReceiveTextArgs args)
		{
			if (Debug.Level == 2)
				Debug.Console(2, this, "RX: '{0}'", 
					ShowHexResponse ? ComTextHelper.GetEscapedText(args.Text) : args.Text);
		}

		void SendLine(string s)
		{
			//if (Debug.Level == 2)
			//    Debug.Console(2, this, "    Send '{0}'", ComTextHelper.GetEscapedText(s));
			Communication.SendText(s + LineEnding);
		}
	}

	public class ConsoleCommMockDevicePropertiesConfig
	{
		public string LineEnding { get; set; }
		public CommunicationMonitorConfig CommunicationMonitorProperties { get; set; }

		public ConsoleCommMockDevicePropertiesConfig()
		{
			LineEnding = "\x0a";
		}
	}

    public class ConsoleCommMockDeviceFactory : EssentialsDeviceFactory<ConsoleCommMockDevice>
    {
        public ConsoleCommMockDeviceFactory()
        {
            TypeNames = new List<string>() { "commmock" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Comm Mock Device");
            var comm = CommFactory.CreateCommForDevice(dc);
            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<ConsoleCommMockDevicePropertiesConfig>(
                dc.Properties.ToString());
            return new ConsoleCommMockDevice(dc.Key, dc.Name, props, comm);
        }
    }

}