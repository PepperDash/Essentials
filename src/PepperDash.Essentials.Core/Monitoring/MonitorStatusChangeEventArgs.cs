
using System;


namespace PepperDash.Essentials.Core
{
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