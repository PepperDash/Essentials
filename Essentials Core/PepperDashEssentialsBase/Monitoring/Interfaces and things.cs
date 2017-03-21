
using System;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
	public interface IStatusMonitor
	{
		IKeyed Parent { get; }
		event EventHandler<MonitorStatusChangeEventArgs> StatusChange;
		MonitorStatus Status { get; }
		string Message { get; }
		void Start();
		void Stop();
	}


	/// <summary>
	/// Represents a class that has a basic communication monitoring
	/// </summary>
	public interface ICommunicationMonitor
	{
		StatusMonitorBase CommunicationMonitor { get; }
	}

	/// <summary>
	/// 
	/// </summary>
	public enum MonitorStatus
	{
		IsOk, InWarning, InError, StatusUnknown
	}

	public class MonitorStatusChangeEventArgs : EventArgs
	{
		public MonitorStatus Status { get; private set; }
		public string Message { get; private set; }

		public MonitorStatusChangeEventArgs(MonitorStatus status)
		{
			Status = status;
			Message = status == MonitorStatus.IsOk ? "" : status.ToString();
		}

		public MonitorStatusChangeEventArgs(MonitorStatus status, string message)
		{
			Status = status;
			Message = message;
		}
	}
}