using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;


namespace PepperDash.Essentials.Core;

	public class ConsoleCommMockDevice : EssentialsDevice, ICommunicationMonitor
	{
  /// <summary>
  /// Gets or sets the Communication
  /// </summary>
		public IBasicCommunication Communication { get; private set; }
  /// <summary>
  /// Gets or sets the PortGather
  /// </summary>
		public CommunicationGather PortGather { get; private set; }
  /// <summary>
  /// Gets or sets the CommunicationMonitor
  /// </summary>
		public StatusMonitorBase CommunicationMonitor { get; private set; }

  /// <summary>
  /// Gets or sets the LineEnding
  /// </summary>
		public string LineEnding { get; set; }

  /// <summary>
  /// Gets or sets the ShowHexResponse
  /// </summary>
		public bool ShowHexResponse { get; set; }

        /// <summary>
        /// Initializes a new instance of the ConsoleCommMockDevice class.
        /// </summary>
        /// <param name="key">The key of the device.</param>
        /// <param name="name">The name of the device.</param>
        /// <param name="props">The properties configuration for the device.</param>
        /// <param name="comm">The communication method for the device.</param>
		public ConsoleCommMockDevice(string key, string name, ConsoleCommMockDevicePropertiesConfig props, IBasicCommunication comm)
			:base(key, name)
		{
			Communication = comm;
			PortGather = new CommunicationGather(Communication, '\x0d');
			//PortGather.LineReceived += this.Port_LineReceived;
			CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, props.CommunicationMonitorProperties);
			LineEnding = props.LineEnding;
		}

  /// <summary>
  /// CustomActivate method
  /// </summary>
  /// <inheritdoc />
		protected override bool CustomActivate()
		{
			Communication.Connect();
			CommunicationMonitor.StatusChange += (o, a) => { Debug.LogMessage(LogEventLevel.Verbose, this, "Communication monitor state: {0}", CommunicationMonitor.Status); };
			CommunicationMonitor.Start();

			CrestronConsole.AddNewConsoleCommand(SendLine, "send" + Key, "", ConsoleAccessLevelEnum.AccessOperator);
			CrestronConsole.AddNewConsoleCommand(s => Communication.Connect(), "con" + Key, "", ConsoleAccessLevelEnum.AccessOperator);
			return true;
		}

		void SendLine(string s)
		{
			//if (Debug.Level == 2)
			//    Debug.LogMessage(LogEventLevel.Verbose, this, "    Send '{0}'", ComTextHelper.GetEscapedText(s));
			Communication.SendText(s + LineEnding);
		}
	}

    /// <summary>
    /// Represents a ConsoleCommMockDevicePropertiesConfig
    /// </summary>
	public class ConsoleCommMockDevicePropertiesConfig
	{
        /// <summary>
        /// Gets or sets the LineEnding
        /// </summary>
		public string LineEnding { get; set; }

        /// <summary>
        /// Gets or sets the CommunicationMonitorProperties
        /// </summary>
		public CommunicationMonitorConfig CommunicationMonitorProperties { get; set; }

        /// <summary>
        /// Initializes a new instance of the ConsoleCommMockDevicePropertiesConfig class.
        /// </summary>
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
        Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new Comm Mock Device");
        var comm = CommFactory.CreateCommForDevice(dc);
        var props = Newtonsoft.Json.JsonConvert.DeserializeObject<ConsoleCommMockDevicePropertiesConfig>(
            dc.Properties.ToString());
        return new ConsoleCommMockDevice(dc.Key, dc.Name, props, comm);
    }
}