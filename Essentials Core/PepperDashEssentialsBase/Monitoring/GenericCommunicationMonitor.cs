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
	/// Used for monitoring comms that are IBasicCommunication. Will send a poll string and provide an event when
	/// statuses change.
	/// </summary>
	public class GenericCommunicationMonitor : StatusMonitorBase
	{
		public IBasicCommunication Client { get; private set; }

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
        }


		/// <summary>
		/// Build the monitor from a config object
		/// </summary>
		public GenericCommunicationMonitor(IKeyed parent, IBasicCommunication client, 
			CommunicationMonitorConfig props) :
			this(parent, client, props.PollInterval, props.TimeToWarning, props.TimeToError, props.PollString)
		{
		}

		public override void Start()
		{
			Client.BytesReceived += Client_BytesReceived;
			Poll();
			PollTimer = new CTimer(o => Poll(), null, PollTime, PollTime);
		}

		public override void Stop()
		{
			Client.BytesReceived -= this.Client_BytesReceived;
			PollTimer.Stop();
			PollTimer = null;
			StopErrorTimers();
		}

		/// <summary>
		/// Upon any receipt of data, set everything to ok!
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Client_BytesReceived(object sender, GenericCommMethodReceiveBytesArgs e)
		{
			Status = MonitorStatus.IsOk;
			ResetErrorTimers();
		}

		void Poll()
		{
			StartErrorTimers();
			if (Client.IsConnected)
			{
				//Debug.Console(2, this, "Polling");
                if(PollAction != null)
                    PollAction.Invoke();
                else
                    Client.SendText(PollString);
			}
			else
			{
				Debug.Console(2, this, "Comm not connected");
			}
		}

		/// <summary>
		/// When the client connects, and we're waiting for it, respond and disconect from event
		/// </summary>
		void OneTimeConnectHandler(object o, EventArgs a)
		{
			if (Client.IsConnected)
			{
				//Client.IsConnected -= OneTimeConnectHandler;
				Debug.Console(2, this, "Comm connected");
				Poll();
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