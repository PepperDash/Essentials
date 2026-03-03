using System;
using PepperDash.Core;
using System.Threading;
using PepperDash.Core.Logging;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Used for monitoring comms that are IBasicCommunication. Will send a poll string and provide an event when
	/// statuses change.
    /// Default monitoring uses TextReceived event on Client.
	/// </summary>
	public class GenericCommunicationMonitor : StatusMonitorBase
	{
        /// <summary>
        /// Gets the Client being monitored
        /// </summary>
		public IBasicCommunication Client { get; private set; }

        /// <summary>
        /// Will monitor Client.BytesReceived if set to true.  Otherwise the default is to monitor Client.TextReceived
        /// </summary>
        public bool MonitorBytesReceived { get; private set; }

        /// <summary>
        /// Return true if the Client is ISocketStatus
        /// </summary>
        public bool IsSocket => Client is ISocketStatus;

        private readonly string PollString;
        private readonly Action PollAction;
        private readonly long PollTime;

		private Timer PollTimer;

        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// GenericCommunicationMonitor constructor
        /// 
        /// Note: If the client is a socket, the connection status will be monitored and the PollTimer will be started automatically when the client is connected
        /// </summary>
        /// <param name="parent">Parent device</param>
        /// <param name="client">Communications Client</param>
        /// <param name="pollTime">Time in MS for polling</param>
        /// <param name="warningTime">Warning time in MS. If a message is not received before this elapsed time the status will be Warning</param>
        /// <param name="errorTime">Error time in MS. If a message is not received before this elapsed time the status will be Error</param>
        /// <param name="pollString">string to send for polling</param>
        /// <exception cref="ArgumentException">Poll time must be less than warning and error time</exception>
        public GenericCommunicationMonitor(IKeyed parent, IBasicCommunication client, long pollTime, 
			long warningTime, long errorTime, string pollString) :
			base(parent, warningTime, errorTime)
		{
			if (pollTime > warningTime || pollTime > errorTime)
				throw new ArgumentException("pollTime must be less than warning or errorTime");

			Client = client;
			PollTime = pollTime;
			PollString = pollString;

            if (IsSocket)
            {
                (Client as ISocketStatus).ConnectionChange += Socket_ConnectionChange;
            }
		}

        /// <summary>
        /// GenericCommunicationMonitor constructor with a bool to specify whether to monitor BytesReceived
        /// 
        /// Note: If the client is a socket, the connection status will be monitored and the PollTimer will be started automatically when the client is connected
        /// </summary>
        /// <param name="parent">Parent device</param>
        /// <param name="client">Communications Client</param>
        /// <param name="pollTime">Time in MS for polling</param>
        /// <param name="warningTime">Warning time in MS. If a message is not received before this elapsed time the status will be Warning</param>
        /// <param name="errorTime">Error time in MS. If a message is not received before this elapsed time the status will be Error</param>
        /// <param name="pollString">string to send for polling</param>
        /// <param name="monitorBytesReceived">Use bytesReceived event instead of textReceived when true</param>
        public GenericCommunicationMonitor(IKeyed parent, IBasicCommunication client, long pollTime,
            long warningTime, long errorTime, string pollString, bool monitorBytesReceived) :
            this(parent, client, pollTime, warningTime, errorTime, pollString)
        {
            MonitorBytesReceived = monitorBytesReceived;
        }

        /// <summary>
        /// GenericCommunicationMonitor constructor with a poll action instead of a poll string
        /// 
        /// Note: If the client is a socket, the connection status will be monitored and the PollTimer will be started automatically when the client is connected
        /// </summary>
        /// <param name="parent">Parent device</param>
        /// <param name="client">Communications Client</param>
        /// <param name="pollTime">Time in MS for polling</param>
        /// <param name="warningTime">Warning time in MS. If a message is not received before this elapsed time the status will be Warning</param>
        /// <param name="errorTime">Error time in MS. If a message is not received before this elapsed time the status will be Error</param>
        /// <param name="pollAction">Action to execute for polling</param>
        /// <exception cref="ArgumentException">Poll time must be less than warning and error time</exception>
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
                (Client as ISocketStatus).ConnectionChange += Socket_ConnectionChange;
            }

        }

        /// <summary>
        /// GenericCommunicationMonitor constructor with a poll action instead of a poll string and a bool to specify whether to monitor BytesReceived
        /// 
        /// Note: If the client is a socket, the connection status will be monitored and the PollTimer will be started automatically when the client is connected
        /// </summary>
        /// <param name="parent">Parent device</param>
        /// <param name="client">Communications Client</param>
        /// <param name="pollTime">Time in MS for polling</param>
        /// <param name="warningTime">Warning time in MS. If a message is not received before this elapsed time the status will be Warning</param>
        /// <param name="errorTime">Error time in MS. If a message is not received before this elapsed time the status will be Error</param>
        /// <param name="pollAction">Action to execute for polling</param>
        /// <param name="monitorBytesReceived">Use bytesReceived event instead of textReceived when true</param>
        public GenericCommunicationMonitor(IKeyed parent, IBasicCommunication client, long pollTime,
            long warningTime, long errorTime, Action pollAction, bool monitorBytesReceived) :
            this(parent, client, pollTime, warningTime, errorTime, pollAction)
        {
            MonitorBytesReceived = monitorBytesReceived;
        }


        /// <summary>
        /// GenericCommunicationMonitor constructor with a config object
        /// 
        /// Note: If the client is a socket, the connection status will be monitored and the PollTimer will be started automatically when the client is connected
        /// </summary>
        /// <param name="parent">Parent Device</param>
        /// <param name="client">Communications Client</param>
        /// <param name="props"><see cref="CommunicationMonitorConfig">Communication Monitor Config</see> object</param>
        public GenericCommunicationMonitor(IKeyed parent, IBasicCommunication client, 
			CommunicationMonitorConfig props) :
			this(parent, client, props.PollInterval, props.TimeToWarning, props.TimeToError, props.PollString)
		{
            if (IsSocket)
            {
                (Client as ISocketStatus).ConnectionChange += Socket_ConnectionChange;
            }
		}

        /// <summary>
        /// GenericCommunicationMonitor constructor with a config object and a bool to specify whether to monitor BytesReceived
        /// 
        /// Note: If the client is a socket, the connection status will be monitored and the PollTimer will be started automatically when the client is connected
        /// </summary>
        /// <param name="parent">Parent Device</param>
        /// <param name="client">Communications Client</param>
        /// <param name="props"><see cref="CommunicationMonitorConfig">Communication Monitor Config</see> object</param>
        /// <param name="monitorBytesReceived">Use bytesReceived event instead of textReceived when true</param>
        public GenericCommunicationMonitor(IKeyed parent, IBasicCommunication client, CommunicationMonitorConfig props, bool monitorBytesReceived) :
            this(parent, client, props.PollInterval, props.TimeToWarning, props.TimeToError, props.PollString)
        {
            MonitorBytesReceived = monitorBytesReceived;
        }

        /// <summary>
        /// Start the poll cycle
        /// </summary>
		public override void Start()
		{
            if (MonitorBytesReceived) 
            {
			    Client.BytesReceived -= Client_BytesReceived;
                Client.BytesReceived += Client_BytesReceived;
            }
            else
            {
                Client.TextReceived -= Client_TextReceived;
                Client.TextReceived += Client_TextReceived;
            }

            BeginPolling();
		}

        private void Socket_ConnectionChange(object sender, GenericSocketStatusChageEventArgs e)
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
            }
        }

        private void BeginPolling()
        {
            try
            {
                semaphore.Wait();
                {
                    if (PollTimer != null)
                    {
                        return;
                    }

                    PollTimer = new Timer(o => Poll(), null, 0, PollTime);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

  /// <summary>
  /// Stop method
  /// </summary>
  /// <inheritdoc />
		public override void Stop()
		{
            if(MonitorBytesReceived)
            {
			    Client.BytesReceived -= Client_BytesReceived;
            }
            else
            {
                Client.TextReceived -= Client_TextReceived;
            }

            StopErrorTimers();

            if (PollTimer == null)
            {
                return;
            }
            
            PollTimer.Dispose();
            PollTimer = null;
		}

        private void Client_TextReceived(object sender, GenericCommMethodReceiveTextArgs e)
        {
            DataReceived();
        }

		private void Client_BytesReceived(object sender, GenericCommMethodReceiveBytesArgs e)
		{
            DataReceived();
        }

        private void DataReceived()
        {
            Status = MonitorStatus.IsOk;
            ResetErrorTimers();
        }

		private void Poll()
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
				this.LogVerbose("Comm not connected");
			}
		}
	}

    /// <summary>
    /// Represents a CommunicationMonitorConfig
    /// </summary>
	public class CommunicationMonitorConfig
	{
        /// <summary>
        /// Gets or sets the PollInterval
        /// </summary>
		public int PollInterval { get; set; }

        /// <summary>
        /// Gets or sets the TimeToWarning
        /// </summary>
		public int TimeToWarning { get; set; }

        /// <summary>
        /// Gets or sets the TimeToError
        /// </summary>
		public int TimeToError { get; set; }

        /// <summary>
        /// Gets or sets the PollString
        /// </summary>
		public string PollString { get; set; }

        /// <summary>
        /// Default constructor. Sets pollInterval to 30s, TimeToWarning to 120s, and TimeToError to 300s
        /// </summary>
		public CommunicationMonitorConfig()
		{
			PollInterval = 30000;
			TimeToWarning = 120000;
			TimeToError = 300000;
			PollString = "";
		}
	}
}