
using System;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
 /// <summary>
 /// Defines the contract for IStatusMonitor
 /// </summary>
	public interface IStatusMonitor
	{
		/// <summary>
		/// Gets the Parent
		/// </summary>
		IKeyed Parent { get; }

		/// <summary>
		/// Fires when the status changes
		/// </summary>
		event EventHandler<MonitorStatusChangeEventArgs> StatusChange;

		/// <summary>
		/// Gets the Status
		/// </summary>
		MonitorStatus Status { get; }

		/// <summary>
		/// Gets the Message
		/// </summary>
		string Message { get; }

		/// <summary>
		/// Gets or sets the IsOnlineFeedback
		/// </summary>
		BoolFeedback IsOnlineFeedback { get; set; }

		/// <summary>
		/// Start method
		/// </summary>
		void Start();

		/// <summary>
		/// Stop method
		/// </summary>
		void Stop();
	}


 /// <summary>
 /// Defines the contract for ICommunicationMonitor
 /// </summary>
	public interface ICommunicationMonitor
	{
		/// <summary>
		/// Gets the CommunicationMonitor
		/// </summary>
		StatusMonitorBase CommunicationMonitor { get; }
	}

	/// <summary>
	/// StatusUnknown = 0, IsOk = 1, InWarning = 2, InError = 3
	/// </summary>
	public enum MonitorStatus
	{
		/// <summary>
		/// Status Unknown
		/// </summary>
        StatusUnknown = 0,

		/// <summary>
		/// Is Ok
		/// </summary>
		IsOk = 1,

		/// <summary>
		/// In Warning
		/// </summary>
        InWarning = 2,

		/// <summary>
		/// In Error
		/// </summary>
        InError = 3
	}

	/// <summary>
	/// Represents a MonitorStatusChangeEventArgs
	/// </summary>
	public class MonitorStatusChangeEventArgs : EventArgs
	{
		/// <summary>
		/// Gets or sets the Status
		/// </summary>
		public MonitorStatus Status { get; private set; }

		/// <summary>
		/// Gets or sets the Message
		/// </summary>
		public string Message { get; private set; }
	
		/// <summary>
		/// Constructor for MonitorStatusChangeEventArgs
		/// </summary>
		/// <param name="status">monitor status</param>
		public MonitorStatusChangeEventArgs(MonitorStatus status)
		{
			Status = status;
			Message = status == MonitorStatus.IsOk ? "" : status.ToString();
		}

		/// <summary>
		/// Constructor for MonitorStatusChangeEventArgs
		/// </summary>
		/// <param name="status">monitor status</param>
		/// <param name="message">status message</param>
		public MonitorStatusChangeEventArgs(MonitorStatus status, string message)
		{
			Status = status;
			Message = message;
		}
	}
}