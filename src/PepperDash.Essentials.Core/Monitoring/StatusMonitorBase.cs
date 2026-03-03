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
	/// Base class for status monitors
	/// </summary>
	public abstract class StatusMonitorBase : IStatusMonitor, IKeyName
	{
		/// <summary>
		/// Event fired when status changes
		/// </summary>
		public event EventHandler<MonitorStatusChangeEventArgs> StatusChange;

        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        public string Key { get { return Parent.Key + "-comMonitor"; } }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get { return "Comm. monitor"; } }

		/// <summary>
		/// Gets or sets the Parent
		/// </summary>
		public IKeyed Parent { get; private set; }

		/// <summary>
		/// Bool feedback for online status
		/// </summary>
		public BoolFeedback IsOnlineFeedback { get; set; }

		/// <summary>
		/// Indicates whether the monitored device is online
		/// </summary>
		public bool IsOnline;

		/// <summary>
		/// Current monitor status
		/// </summary>
		public MonitorStatus Status
		{
			get { return _Status; }
			protected set
			{
				if (value != _Status)
				{
					_Status = value;
					
					OnStatusChange(value);
				}
			}
		}
		MonitorStatus _Status;

		/// <summary>
		/// Current status message
		/// </summary>
		public string Message
		{
			get { return _Message; }
			set 
			{
				if (value == _Message) return;
				_Message = value;
				OnStatusChange(Status, value);
					
			}
		}
		string _Message;

		long WarningTime;
		long ErrorTime;
		CTimer WarningTimer;
		CTimer ErrorTimer;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent">parent device</param>
		/// <param name="warningTime">time in milliseconds before warning status</param>
		/// <param name="errorTime">time in milliseconds before error status</param>
		public StatusMonitorBase(IKeyed parent, long warningTime, long errorTime)
		{
			Parent = parent;
			if (warningTime > errorTime)
				throw new ArgumentException("warningTime must be less than errorTime");
			if (warningTime < 5000 || errorTime < 5000)
				throw new ArgumentException("time values cannot be less that 5000 ms");

			IsOnlineFeedback = new BoolFeedback(() => { return IsOnline; });
			Status = MonitorStatus.StatusUnknown;
			WarningTime = warningTime;
			ErrorTime = errorTime;
		}

		/// <summary>
		/// Starts the monitor
		/// </summary>
		public abstract void Start();

		/// <summary>
		/// Stops the monitor
		/// </summary>
		public abstract void Stop();

		/// <summary>
		/// Fires the StatusChange event
		/// </summary>
		/// <param name="status">monitor status</param>
		protected void OnStatusChange(MonitorStatus status)
		{
			if (_Status == MonitorStatus.IsOk)
				IsOnline = true;
			else
				IsOnline = false;
			IsOnlineFeedback.FireUpdate();
			var handler = StatusChange;
			if (handler != null)
				handler(this, new MonitorStatusChangeEventArgs(status));
		}

		/// <summary>
		/// Fires the StatusChange event with message
		/// </summary>
		/// <param name="status">monitor status</param>
		/// <param name="message">status message</param>
		protected void OnStatusChange(MonitorStatus status, string message)
		{
			if (_Status == MonitorStatus.IsOk)
				IsOnline = true;
			else
				IsOnline = false;
			IsOnlineFeedback.FireUpdate();
			var handler = StatusChange;
			if (handler != null)
				handler(this, new MonitorStatusChangeEventArgs(status, message));
		}

		/// <summary>
		/// Starts the error timers
		/// </summary>
		protected void StartErrorTimers()
		{
			if (WarningTimer == null) WarningTimer = new CTimer(o => { Status = MonitorStatus.InWarning; }, WarningTime);
			if (ErrorTimer == null) ErrorTimer = new CTimer(o => { Status = MonitorStatus.InError; }, ErrorTime);
		}

		/// <summary>
		/// Stops the error timers
		/// </summary>
		protected void StopErrorTimers()
		{
			if (WarningTimer != null) WarningTimer.Stop();
			if (ErrorTimer != null) ErrorTimer.Stop();
			WarningTimer = null;
			ErrorTimer = null;
		}

		/// <summary>
		/// Resets the error timers
		/// </summary>
        protected void ResetErrorTimers()
        {
            if(WarningTimer != null)
                WarningTimer.Reset(WarningTime, WarningTime);
            if(ErrorTimer != null)
                ErrorTimer.Reset(ErrorTime, ErrorTime);

        }
	}
}