
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
		BoolFeedback IsOnlineFeedback { get; set; }
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
	/// StatusUnknown = 0, IsOk = 1, InWarning = 2, InError = 3
	/// </summary>
	public enum MonitorStatus
	{
        StatusUnknown = 0,
		IsOk = 1, 
        InWarning = 2, 
        InError = 3
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