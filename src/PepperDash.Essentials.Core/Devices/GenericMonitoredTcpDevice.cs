using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
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

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key">key of the device</param>
		/// <param name="name">name of the device</param>
		/// <param name="comm">communication client</param>
		/// <param name="pollString">poll string</param>
		/// <param name="pollTime">poll time</param>
		/// <param name="warningTime">warning time</param>
		/// <param name="errorTime">error time</param>
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

		/// <summary>
		/// Constructor with default times
		/// </summary>
		/// <param name="key">key of the device</param>
		/// <param name="name">name of the device</param>
		/// <param name="comm">communication client</param>
		/// <param name="pollString">poll string</param>
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