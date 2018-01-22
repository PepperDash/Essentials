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

		public BoolFeedback IsOnline { get; private set; }
		public BoolFeedback IsRegistered { get; private set; }
		public StringFeedback IpConnectionsText { get; private set; }

		public CrestronGenericBaseDevice(string key, string name, GenericBase hardware)
			: base(key, name)
		{
			Hardware = hardware;
			IsOnline = new BoolFeedback(CommonBoolCue.IsOnlineFeedback, () => Hardware.IsOnline);
			IsRegistered = new BoolFeedback(new Cue("IsRegistered", 0, eCueType.Bool), () => Hardware.Registered);
			IpConnectionsText = new StringFeedback(CommonStringCue.IpConnectionsText, () => 
				string.Join(",", Hardware.ConnectedIpList.Select(cip => cip.DeviceIpAddress).ToArray()));
			CommunicationMonitor = new CrestronGenericBaseCommunicationMonitor(this, hardware, 120000, 300000);
		}

		/// <summary>
		/// Make sure that overriding classes call this!
		/// Registers the Crestron device, connects up to the base events, starts communication monitor
		/// </summary>
		public override bool CustomActivate()
		{
            Debug.Console(0, this, "Activating");
            var response = Hardware.RegisterWithLogging(Key);
            if (response == eDeviceRegistrationUnRegistrationResponse.Success)
            {
                Hardware.OnlineStatusChange += new OnlineStatusChangeEventHandler(Hardware_OnlineStatusChange);
                CommunicationMonitor.Start();
            }

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
		/// Returns a list containing the Outputs that we want to expose.
		/// </summary>
		public virtual List<Feedback> Feedbacks
		{
			get
			{
				return new List<Feedback>
				{
					IsOnline,
					IsRegistered,
					IpConnectionsText
				};
			}
		}

		void Hardware_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
			IsOnline.FireUpdate();
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
			if (result != eDeviceRegistrationUnRegistrationResponse.Success)
			{
				Debug.Console(0, Debug.ErrorLogLevel.Error, "Cannot register device '{0}': {1}", key, result);
			}
			return result;
		}

	}
}