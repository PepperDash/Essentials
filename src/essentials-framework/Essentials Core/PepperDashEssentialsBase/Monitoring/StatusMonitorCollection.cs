﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using System.ComponentModel;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// 
	/// </summary>
	public class StatusMonitorCollection : IStatusMonitor
	{
		public IKeyed Parent { get; private set; }

		List<IStatusMonitor> Monitors = new List<IStatusMonitor>();

		#region IStatusMonitor Members

		public event EventHandler<MonitorStatusChangeEventArgs> StatusChange;

		public MonitorStatus Status { get; protected set; }

		public string Message { get; private set; }

		public BoolFeedback IsOnlineFeedback { get; set; }

		public StatusMonitorCollection(IKeyed parent)
		{
			Parent = parent;
		}

		public void Start()
		{
			foreach (var mon in Monitors)
				mon.StatusChange += mon_StatusChange;
			ProcessStatuses();
		}


		void ProcessStatuses()
		{
			var InError = Monitors.Where(m => m.Status == MonitorStatus.InError);
			var InWarning = Monitors.Where(m => m.Status == MonitorStatus.InWarning);
			var IsOk = Monitors.Where(m => m.Status == MonitorStatus.IsOk);


			MonitorStatus initialStatus;
			string prefix = "0:";
			if (InError.Count() > 0)
			{
				initialStatus = MonitorStatus.InError;
				prefix = "3:";
			}
			else if (InWarning.Count() > 0)
			{
				initialStatus = MonitorStatus.InWarning;
				prefix = "2:";
			}
			else if (IsOk.Count() > 0)
				initialStatus = MonitorStatus.IsOk;
			else
				initialStatus = MonitorStatus.StatusUnknown;

			// Build the error message string
            if (InError.Count() > 0 || InWarning.Count() > 0)
            {
                StringBuilder sb = new StringBuilder(prefix);
                if (InError.Count() > 0)
                {
                    // Do string splits and joins 
                    sb.Append(string.Format("{0} Errors:", InError.Count()));
                    foreach (var mon in InError)
                        sb.Append(string.Format("{0}, ", mon.Parent.Key));
                }
                if (InWarning.Count() > 0)
                {
                    sb.Append(string.Format("{0} Warnings:", InWarning.Count()));
                    foreach (var mon in InWarning)
                        sb.Append(string.Format("{0}, ", mon.Parent.Key));
                }
                Message = sb.ToString();
            }
            else
            {
                Message = "Room Ok.";
            }

			// Want to fire even if status doesn't change because the message may.
			Status = initialStatus;
			OnStatusChange(initialStatus, Message);
		}


		void mon_StatusChange(object sender, MonitorStatusChangeEventArgs e)
		{
			ProcessStatuses();
		}

		public void Stop()
		{
			throw new NotImplementedException();
		}

		#endregion

		public void AddMonitor(IStatusMonitor monitor)
		{
			if (!Monitors.Contains(monitor))
				Monitors.Add(monitor);
		}


		protected void OnStatusChange(MonitorStatus status, string message)
		{
			var handler = StatusChange;
			if (handler != null)
				handler(this, new MonitorStatusChangeEventArgs(status, message));
		}
	}
}