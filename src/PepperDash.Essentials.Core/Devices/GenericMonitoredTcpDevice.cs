using PepperDash.Essentials.Core.Communications;
using PepperDash.Essentials.Core.Monitoring;
using Serilog.Events;


namespace PepperDash.Essentials.Core.Devices
{
 /// <summary>
 /// Represents a GenericCommunicationMonitoredDevice
 /// </summary>
	public class GenericCommunicationMonitoredDevice : Device, ICommunicationMonitor
	{
		IBasicCommunication Client;

  /// <summary>
  /// Gets or sets the CommunicationMonitor
  /// </summary>
		public StatusMonitorBase CommunicationMonitor { get; private set; }

		public GenericCommunicationMonitoredDevice(string key, string name, IBasicCommunication comm, string pollString,
			long pollTime, long warningTime, long errorTime)
			: base(key, name)
		{
			Client = comm;
			CommunicationMonitor = new GenericCommunicationMonitor(this, Client, pollTime, warningTime, errorTime, pollString);

			// ------------------------------------------------------DELETE THIS
			CommunicationMonitor.StatusChange += (o, a) =>
			{
				Debug.LogMessage(LogEventLevel.Verbose, this, "Communication monitor status change: {0}", a.Status);
			};

		}

		public GenericCommunicationMonitoredDevice(string key, string name, IBasicCommunication comm, string pollString)
			: this(key, name, comm, pollString, 30000, 120000, 300000)
		{
		}

  /// <summary>
  /// CustomActivate method
  /// </summary>
  /// <inheritdoc />
		public override bool CustomActivate()
		{
			CommunicationMonitor.Start();
			return true;
		}

  /// <summary>
  /// Deactivate method
  /// </summary>
		public override bool Deactivate()
		{
			CommunicationMonitor.Stop();
			return true;
		}
	}
}