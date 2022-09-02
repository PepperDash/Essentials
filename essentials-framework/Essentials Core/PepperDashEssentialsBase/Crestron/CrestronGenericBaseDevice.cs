using System;
using System.Linq;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Core.JsonStandardObjects;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// A bridge class to cover the basic features of GenericBase hardware
	/// </summary>
	public abstract class CrestronGenericBaseDevice : EssentialsDevice, IOnline, IHasFeedback, ICommunicationMonitor, IUsageTracking
	{
	    protected GenericBase Hardware;

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

	    protected CrestronGenericBaseDevice(string key, string name, GenericBase hardware)
            : base(key, name)
        {
            Feedbacks = new FeedbackCollection<Feedback>();

            Hardware = hardware;
            IsOnline = new BoolFeedback("IsOnlineFeedback", () => Hardware.IsOnline);
            IsRegistered = new BoolFeedback("IsRegistered", () => Hardware.Registered);
            IpConnectionsText = new StringFeedback("IpConnectionsText", () => Hardware.ConnectedIpList != null ? string.Join(",", Hardware.ConnectedIpList.Select(cip => cip.DeviceIpAddress).ToArray()) : string.Empty);
            AddToFeedbackList(IsOnline, IpConnectionsText);

            CommunicationMonitor = new CrestronGenericBaseCommunicationMonitor(this, hardware, 120000, 300000);
        }

        protected CrestronGenericBaseDevice(string key, string name)
            : base(key, name)
        {
            Feedbacks = new FeedbackCollection<Feedback>();

        }

	    protected void RegisterCrestronGenericBase(GenericBase hardware)
	    {
            Hardware = hardware;
            IsOnline = new BoolFeedback("IsOnlineFeedback", () => Hardware.IsOnline);
            IsRegistered = new BoolFeedback("IsRegistered", () => Hardware.Registered);
            IpConnectionsText = new StringFeedback("IpConnectionsText", () => Hardware.ConnectedIpList != null ? string.Join(",", Hardware.ConnectedIpList.Select(cip => cip.DeviceIpAddress).ToArray()) : string.Empty);
            AddToFeedbackList(IsOnline, IpConnectionsText);

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

                if (Hardware.Registerable && !Hardware.Registered)
                {
                    var response = Hardware.RegisterWithLogging(Key);
                    if (response != eDeviceRegistrationUnRegistrationResponse.Success)
                    {
                        //Debug.Console(0, this, "ERROR: Cannot register Crestron device: {0}", response);
                        return false;
                    }
                }

                IsRegistered.FireUpdate();
            }
            else
            {
                AddPostActivationAction(() =>
                    {
                        if (Hardware.Registerable && !Hardware.Registered)
                        {
                            var response = Hardware.RegisterWithLogging(Key);
                        }

                        IsRegistered.FireUpdate();
                    });
            }

            foreach (var f in Feedbacks)
            {
                f.FireUpdate();
            }

            Hardware.OnlineStatusChange += Hardware_OnlineStatusChange;
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

			var success = Hardware.UnRegister() == eDeviceRegistrationUnRegistrationResponse.Success;

            IsRegistered.FireUpdate();

            return success;
		}

	    /// <summary>
        /// Adds feedback(s) to the list
        /// </summary>
        /// <param name="newFbs"></param>
        public void AddToFeedbackList(params Feedback[] newFbs)
        {
            foreach (var f in newFbs)
            {
                if (f == null) continue;

                if (!Feedbacks.Contains(f))
                {
                    Feedbacks.Add(f);
                }
            }
        }

		void Hardware_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
            Debug.Console(2, this, "OnlineStatusChange Event.  Online = {0}", args.DeviceOnLine);

            if (!Hardware.Registered)
            {
                return;  // protects in cases where device has been unregistered and feedbacks would attempt to access null sigs.
            }

            foreach (var feedback in Feedbacks)
            {
                if (feedback != null)
                    feedback.FireUpdate();
            }         
		}

		#region IStatusMonitor Members

		public StatusMonitorBase CommunicationMonitor { get; private set; }
		#endregion

        #region IUsageTracking Members

        public UsageTracking UsageTracker { get; set; }

        #endregion
	}

    public abstract class CrestronGenericBridgeableBaseDevice : CrestronGenericBaseDevice, IBridgeAdvanced
    {
        protected CrestronGenericBridgeableBaseDevice(string key, string name, GenericBase hardware) : base(key, name, hardware)
        {
        }

        protected CrestronGenericBridgeableBaseDevice(string key, string name)
            : base(key, name)
        {
        }


        public abstract void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge);
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