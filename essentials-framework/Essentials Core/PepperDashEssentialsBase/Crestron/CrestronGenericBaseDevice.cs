using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// A bridge class to cover the basic features of GenericBase hardware
	/// </summary>
	public class CrestronGenericBaseDevice : Device, IOnline, IHasFeedback, ICommunicationMonitor, IUsageTracking
	{
		public virtual GenericBase Hardware { get; protected set; }

        /// <summary>
        /// Returns a list containing the Outputs that we want to expose.
        /// </summary>
        public FeedbackCollection<Feedback> Feedbacks { get; private set; }

		public BoolFeedback IsOnline { get; private set; }
		public BoolFeedback IsRegistered { get; private set; }
		public StringFeedback IpConnectionsText { get; private set; }

		/// <summary>
		/// Used by implementing classes to prevent registration with Crestron TLDM. For
		/// devices like RMCs and TXs attached to a chassis.
		/// </summary>
		public bool PreventRegistration { get; protected set; }

		public CrestronGenericBaseDevice(string key, string name, GenericBase hardware)
			: base(key, name)
		{
            Feedbacks = new FeedbackCollection<Feedback>();

			Hardware = hardware;
			IsOnline = new BoolFeedback("IsOnlineFeedback", () => Hardware.IsOnline);
			IsRegistered = new BoolFeedback("IsRegistered", () => Hardware.Registered);
			IpConnectionsText = new StringFeedback("IpConnectionsText", () => 
				string.Join(",", Hardware.ConnectedIpList.Select(cip => cip.DeviceIpAddress).ToArray()));

            AddToFeedbackList(IsOnline, IsRegistered, IpConnectionsText);

			CommunicationMonitor = new CrestronGenericBaseCommunicationMonitor(this, hardware, 120000, 300000);
		}

		/// <summary>
		/// Make sure that overriding classes call this!
		/// Registers the Crestron device, connects up to the base events, starts communication monitor
		/// </summary>
		public override bool CustomActivate()
		{
            Debug.Console(0, this, "Activating");
			if (!PreventRegistration)
			{
                //Debug.Console(1, this, "  Does not require registration. Skipping");

				var response = Hardware.RegisterWithLogging(Key);
				if (response != eDeviceRegistrationUnRegistrationResponse.Success)
				{
					//Debug.Console(0, this, "ERROR: Cannot register Crestron device: {0}", response);
					return false;
				}
			}

            Hardware.OnlineStatusChange += new OnlineStatusChangeEventHandler(Hardware_OnlineStatusChange);
            CommunicationMonitor.Start();    

			return true;
		}

		/// <summary>
		/// This disconnects events and unregisters the base hardware device.
		/// </summary>
		/// <returns></returns>
		public override bool Deactivate()
		{
			CommunicationMonitor.Stop();
			Hardware.OnlineStatusChange -= Hardware_OnlineStatusChange;

			return Hardware.UnRegister() == eDeviceRegistrationUnRegistrationResponse.Success;
		}
	
        /// <summary>
        /// Adds feedback(s) to the list
        /// </summary>
        /// <param name="newFbs"></param>
        public void AddToFeedbackList(params Feedback[] newFbs)
        {
            foreach (var f in newFbs)
            {
                if (f != null)
                {
                    if (!Feedbacks.Contains(f))
                    {
                        Feedbacks.Add(f);
                    }
                }
            }
        }

		void Hardware_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
            if (args.DeviceOnLine)
            {
                foreach (var feedback in Feedbacks)
                {
                    if (feedback != null)
                        feedback.FireUpdate();
                }
            }
		}

		#region IStatusMonitor Members

		public StatusMonitorBase CommunicationMonitor { get; private set; }
		#endregion

        #region IUsageTracking Members

        public UsageTracking UsageTracker { get; set; }

        #endregion
	}

	//***********************************************************************************
	public class CrestronGenericBaseDeviceEventIds
	{
		public const uint IsOnline = 1;
		public const uint IpConnectionsText =2;
	}

	/// <summary>
	/// Adds logging to Register() failure
	/// </summary>
	public static class GenericBaseExtensions
	{
		public static eDeviceRegistrationUnRegistrationResponse RegisterWithLogging(this GenericBase device, string key)
		{
			var result = device.Register();
			var level = result == eDeviceRegistrationUnRegistrationResponse.Success ?
				Debug.ErrorLogLevel.Notice : Debug.ErrorLogLevel.Error;
			Debug.Console(0, level, "Register device result: '{0}', type '{1}', result {2}", key, device, result);
			//if (result != eDeviceRegistrationUnRegistrationResponse.Success)
			//{
			//    Debug.Console(0, Debug.ErrorLogLevel.Error, "Cannot register device '{0}': {1}", key, result);
			//}
			return result;
		}

	}
}