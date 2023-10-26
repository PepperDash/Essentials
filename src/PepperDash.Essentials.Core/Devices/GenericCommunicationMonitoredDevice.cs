using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;


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
				Debug.Console(2, this, "Communication monitor status change: {0}", a.Status);
			};

		}

		public GenericCommunicationMonitoredDevice(string key, string name, IBasicCommunication comm, string pollString)
			: this(key, name, comm, pollString, 30000, 120000, 300000)
		{
		}


		/// <summary>
		/// Generic monitor for TCP reachability.  Default with 30s poll, 120s warning and 300s error times
		/// </summary>
		[Obsolete]
		public GenericCommunicationMonitoredDevice(string key, string name, string hostname, int port, string pollString)
			: this(key, name, hostname, port, pollString, 30000, 120000, 300000)
		{
		}

		/// <summary>
		/// Monitor for TCP reachability
		/// </summary>
		[Obsolete]
		public GenericCommunicationMonitoredDevice(string key, string name, string hostname, int port, string pollString, 
			long pollTime, long warningTime, long errorTime)
			: base(key, name)
		{
			Client = new GenericTcpIpClient(key + "-tcp", hostname, port, 512);
			CommunicationMonitor = new GenericCommunicationMonitor(this, Client, pollTime, warningTime, errorTime, pollString);

			// ------------------------------------------------------DELETE THIS
			CommunicationMonitor.StatusChange += (o, a) =>
			{
				Debug.Console(2, this, "Communication monitor status change: {0}", a.Status);
			};
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