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
	public abstract class StatusMonitorBase : IStatusMonitor, IKeyName
	{
		public event EventHandler<MonitorStatusChangeEventArgs> StatusChange;

        /// <summary>
        /// Format returned: "parentdevkey-comMonitor"
        /// </summary>
        public string Key { get { return Parent.Key + "-comMonitor"; } }

        public string Name { get { return "Comm. monitor"; } }

		public IKeyed Parent { get; private set; }

		public BoolFeedback IsOnlineFeedback { get; set; }

		public bool IsOnline;

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

		public abstract void Start();
		public abstract void Stop();

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

		protected void StartErrorTimers()
		{
			if (WarningTimer == null) WarningTimer = new CTimer(o => { Status = MonitorStatus.InWarning; }, WarningTime);
			if (ErrorTimer == null) ErrorTimer = new CTimer(o => { Status = MonitorStatus.InError; }, ErrorTime);
		}

		protected void StopErrorTimers()
		{
			if (WarningTimer != null) WarningTimer.Stop();
			if (ErrorTimer != null) ErrorTimer.Stop();
			WarningTimer = null;
			ErrorTimer = null;
		}

        protected void ResetErrorTimers()
        {
            if(WarningTimer != null)
                WarningTimer.Reset(WarningTime, WarningTime);
            if(ErrorTimer != null)
                ErrorTimer.Reset(ErrorTime, ErrorTime);

        }
	}
}