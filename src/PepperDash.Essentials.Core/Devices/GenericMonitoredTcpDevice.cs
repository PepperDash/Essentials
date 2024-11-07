using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core.Monitoring;
using Serilog.Events;


namespace PepperDash.Essentials.Core.Devices
{
	public class GenericCommunicationMonitoredDevice : Device, ICommunicationMonitor
	{
		IBasicCommunication Client;

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

		public override bool CustomActivate()
		{
			CommunicationMonitor.Start();
			return true;
		}

		public override bool Deactivate()
		{
			CommunicationMonitor.Stop();
			return true;
		}
	}
}