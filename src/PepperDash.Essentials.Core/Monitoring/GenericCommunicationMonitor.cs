using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using System.ComponentModel;

using PepperDash.Core;
using Serilog.Events;


namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Used for monitoring comms that are IBasicCommunication. Will send a poll string and provide an event when
	/// statuses change.
    /// Default monitoring uses TextReceived event on Client.
	/// </summary>
	public class GenericCommunicationMonitor : StatusMonitorBase
	{
		public IBasicCommunication Client { get; private set; }

        /// <summary>
        /// Will monitor Client.BytesReceived if set to true.  Otherwise the default is to monitor Client.TextReceived
        /// </summary>
        public bool MonitorBytesReceived { get; private set; }

        /// <summary>
        /// Return true if the Client is ISocketStatus
        /// </summary>
        public bool IsSocket
        {
            get
            {
                return Client is ISocketStatus;
            }
        }

		long PollTime;
		CTimer PollTimer;
		string PollString;
        Action PollAction;
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="client"></param>
		/// <param name="pollTime">in MS, >= 5000</param>
		/// <param name="warningTime">in MS, >= 5000</param>
		/// <param name="errorTime">in MS, >= 5000</param>
		/// <param name="pollString">String to send to comm</param>
		public GenericCommunicationMonitor(IKeyed parent, IBasicCommunication client, long pollTime, 
			long warningTime, long errorTime, string pollString) :
			base(parent, warningTime, errorTime)
		{
			if (pollTime > warningTime || pollTime > errorTime)
				throw new ArgumentException("pollTime must be less than warning or errorTime");
            //if (pollTime < 5000)
            //    throw new ArgumentException("pollTime cannot be less than 5000 ms");

			Client = client;
			PollTime = pollTime;
			PollString = pollString;

            if (IsSocket)
            {
                (Client as ISocketStatus).ConnectionChange += new EventHandler<GenericSocketStatusChageEventArgs>(socket_ConnectionChange);
            }
		}

        public GenericCommunicationMonitor(IKeyed parent, IBasicCommunication client, long pollTime,
            long warningTime, long errorTime, string pollString, bool monitorBytesReceived) :
            this(parent, client, pollTime, warningTime, errorTime, pollString)
        {
            SetMonitorBytesReceived(monitorBytesReceived);
        }

        /// <summary>
        /// Poll is a provided action instead of string
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="client"></param>
        /// <param name="pollTime"></param>
        /// <param name="warningTime"></param>
        /// <param name="errorTime"></param>
        /// <param name="pollBytes"></param>
        public GenericCommunicationMonitor(IKeyed parent, IBasicCommunication client, long pollTime,
            long warningTime, long errorTime, Action pollAction) :
            base(parent, warningTime, errorTime)
        {
            if (pollTime > warningTime || pollTime > errorTime)
                throw new ArgumentException("pollTime must be less than warning or errorTime");
            //if (pollTime < 5000)
            //    throw new ArgumentException("pollTime cannot be less than 5000 ms");

            Client = client;
            PollTime = pollTime;
            PollAction = pollAction;

            if (IsSocket)
            {
                (Client as ISocketStatus).ConnectionChange += new EventHandler<GenericSocketStatusChageEventArgs>(socket_ConnectionChange);
            }

        }

        public GenericCommunicationMonitor(IKeyed parent, IBasicCommunication client, long pollTime,
            long warningTime, long errorTime, Action pollAction, bool monitorBytesReceived) :
            this(parent, client, pollTime, warningTime, errorTime, pollAction)
        {
            SetMonitorBytesReceived(monitorBytesReceived);
        }


		/// <summary>
		/// Build the monitor from a config object
		/// </summary>
		public GenericCommunicationMonitor(IKeyed parent, IBasicCommunication client, 
			CommunicationMonitorConfig props) :
			this(parent, client, props.PollInterval, props.TimeToWarning, props.TimeToError, props.PollString)
		{
            if (IsSocket)
            {
                (Client as ISocketStatus).ConnectionChange += new EventHandler<GenericSocketStatusChageEventArgs>(socket_ConnectionChange);
            }
		}

        /// <summary>
        /// Builds the monitor from a config object and takes a bool to specify whether to monitor BytesReceived
        /// Default is to monitor TextReceived
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="client"></param>
        /// <param name="props"></param>
        /// <param name="monitorBytesReceived"></param>
        public GenericCommunicationMonitor(IKeyed parent, IBasicCommunication client, CommunicationMonitorConfig props, bool monitorBytesReceived) :
            this(parent, client, props.PollInterval, props.TimeToWarning, props.TimeToError, props.PollString)
        {
            SetMonitorBytesReceived(monitorBytesReceived);
        }

        void SetMonitorBytesReceived(bool monitorBytesReceived)
        {
            MonitorBytesReceived = monitorBytesReceived;
        }

		public override void Start()
		{
            if (MonitorBytesReceived) 
            {
			    Client.BytesReceived += Client_BytesReceived;
            }
            else
            {
                Client.TextReceived += Client_TextReceived;
            }

            if (!IsSocket)
            {
                BeginPolling();
            }
		}

        void  socket_ConnectionChange(object sender, GenericSocketStatusChageEventArgs e)
        {
            if (!e.Client.IsConnected)
            {
                // Immediately stop polling and notify that device is offline
                Stop();
                Status = MonitorStatus.InError;
                ResetErrorTimers();
            }
            else
            {
                // Start polling and set status to unknow and let poll result update the status to IsOk when a response is received
                Status = MonitorStatus.StatusUnknown;
                Start();
                BeginPolling();
            }
        }

        void BeginPolling()
        {
            Poll();
            PollTimer = new CTimer(o => Poll(), null, PollTime, PollTime);
        }

		public override void Stop()
		{
            if(MonitorBytesReceived)
            {
			    Client.BytesReceived -= this.Client_BytesReceived;
            }
            else
            {
                Client.TextReceived -= Client_TextReceived;
            }

            if (PollTimer != null)
            {
                PollTimer.Stop();
                PollTimer = null;
                StopErrorTimers();
            }
		}

        void  Client_TextReceived(object sender, GenericCommMethodReceiveTextArgs e)
        {
            DataReceived();
        }

		/// <summary>
		/// Upon any receipt of data, set everything to ok!
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Client_BytesReceived(object sender, GenericCommMethodReceiveBytesArgs e)
		{
            DataReceived();
        }

        void DataReceived()
        {
            Status = MonitorStatus.IsOk;
            ResetErrorTimers();
        }

		void Poll()
		{
			StartErrorTimers();
			if (Client.IsConnected)
			{
				//Debug.LogMessage(LogEventLevel.Verbose, this, "Polling");
                if(PollAction != null)
                    PollAction.Invoke();
                else
                    Client.SendText(PollString);
			}
			else
			{
				Debug.LogMessage(LogEventLevel.Verbose, this, "Comm not connected");
			}
		}
	}


	public class CommunicationMonitorConfig
	{
		public int PollInterval { get; set; }
		public int TimeToWarning { get; set; }
		public int TimeToError { get; set; }
		public string PollString { get; set; }

		public CommunicationMonitorConfig()
		{
			PollInterval = 30000;
			TimeToWarning = 120000;
			TimeToError = 300000;
			PollString = "";
		}
	}
}