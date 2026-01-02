using System;
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

  /// <summary>
  /// Gets or sets the Status
  /// </summary>
		public MonitorStatus Status { get; protected set; }

  /// <summary>
  /// Gets or sets the Message
  /// </summary>
		public string Message { get; private set; }

  /// <summary>
  /// Gets or sets the IsOnlineFeedback
  /// </summary>
		public BoolFeedback IsOnlineFeedback { get; set; }

		public StatusMonitorCollection(IKeyed parent)
		{
			Parent = parent;
		}

  /// <summary>
  /// Start method
  /// </summary>
		public void Start()
		{
			foreach (var mon in Monitors)
				mon.StatusChange += mon_StatusChange;
			ProcessStatuses();
		}


		void ProcessStatuses()
		{
			var InError = Monitors.Where(m => m.Status == MonitorStatus.InError).ToList();
			var InWarning = Monitors.Where(m => m.Status == MonitorStatus.InWarning).ToList();
			var IsOk = Monitors.Where(m => m.Status == MonitorStatus.IsOk).ToList();


			MonitorStatus initialStatus;
			string prefix = "0:";
			if (InError.Any())
			{
				initialStatus = MonitorStatus.InError;
				prefix = "3:";
			}
			else if (InWarning.Any())
			{
				initialStatus = MonitorStatus.InWarning;
				prefix = "2:";
			}
			else if (IsOk.Any())
				initialStatus = MonitorStatus.IsOk;
			else
				initialStatus = MonitorStatus.StatusUnknown;

			// Build the error message string
			if (InError.Any() || InWarning.Any())
			{
				var errorNames = InError
					.Select(mon => mon.Parent is IKeyName keyName ? keyName.Name : mon.Parent.Key)
					.ToList();
				var warningNames = InWarning
					.Select(mon => mon.Parent is IKeyName keyName ? keyName.Name : mon.Parent.Key)
					.ToList();

				var sb = new StringBuilder(prefix);

				if (errorNames.Count > 0)
				{
					sb.Append($"{errorNames.Count} Error{(errorNames.Count > 1 ? "s" : "")}: ");
					sb.Append(string.Join(", ", errorNames));
				}
				if (warningNames.Count > 0)
				{
					if (errorNames.Count > 0)
						sb.Append("; ");

					sb.Append($"{warningNames.Count} Warning{(warningNames.Count > 1 ? "s" : "")}: ");
					sb.Append(string.Join(", ", warningNames));
				}

                sb.Append(" Offline");
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

  /// <summary>
  /// Stop method
  /// </summary>
		public void Stop()
		{
			throw new NotImplementedException();
		}

		#endregion

  /// <summary>
  /// AddMonitor method
  /// </summary>
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