using System.Linq;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Feedbacks;
using PepperDash.Essentials.Core.Monitoring;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Crestron
{
	public abstract class CrestronGenericBaseDevice : EssentialsDevice, IOnline, IHasFeedback, ICommunicationMonitor, IUsageTracking
	{
	    protected GenericBase Hardware;

        /// <summary>
        /// Gets or sets the Feedbacks
        /// </summary>
        public FeedbackCollection<Feedbacks.Feedback> Feedbacks { get; private set; }

  /// <summary>
  /// Gets or sets the IsOnline
  /// </summary>
		public BoolFeedback IsOnline { get; private set; }
  /// <summary>
  /// Gets or sets the IsRegistered
  /// </summary>
		public BoolFeedback IsRegistered { get; private set; }
  /// <summary>
  /// Gets or sets the IpConnectionsText
  /// </summary>
		public StringFeedback IpConnectionsText { get; private set; }

  /// <summary>
  /// Gets or sets the PreventRegistration
  /// </summary>
		public bool PreventRegistration { get; protected set; }

	    protected CrestronGenericBaseDevice(string key, string name, GenericBase hardware)
            : base(key, name)
        {
            Feedbacks = new FeedbackCollection<Feedbacks.Feedback>();

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
            Feedbacks = new FeedbackCollection<Feedbacks.Feedback>();

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
  /// CustomActivate method
  /// </summary>
  /// <inheritdoc />
		public override bool CustomActivate()
		{
            Debug.LogMessage(LogEventLevel.Information, this, "Activating");
            if (!PreventRegistration)
            {
                //Debug.LogMessage(LogEventLevel.Debug, this, "  Does not require registration. Skipping");

                if (Hardware.Registerable && !Hardware.Registered)
                {
                    var response = Hardware.RegisterWithLogging(Key);
                    if (response != eDeviceRegistrationUnRegistrationResponse.Success)
                    {
                        //Debug.LogMessage(LogEventLevel.Information, this, "ERROR: Cannot register Crestron device: {0}", response);
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

			return base.CustomActivate();
		}

		/// <summary>
		/// This disconnects events and unregisters the base hardware device.
		/// </summary>
		/// <returns></returns>
  /// <summary>
  /// Deactivate method
  /// </summary>
  /// <inheritdoc />
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
        /// <summary>
        /// AddToFeedbackList method
        /// </summary>
        public void AddToFeedbackList(params Feedbacks.Feedback[] newFbs)
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
            Debug.LogMessage(LogEventLevel.Verbose, this, "OnlineStatusChange Event.  Online = {0}", args.DeviceOnLine);

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

  /// <summary>
  /// Gets or sets the CommunicationMonitor
  /// </summary>
		public StatusMonitorBase CommunicationMonitor { get; private set; }
		#endregion

        #region IUsageTracking Members

        /// <summary>
        /// Gets or sets the UsageTracker
        /// </summary>
        public UsageTracking UsageTracker { get; set; }

        FeedbackCollection<Feedbacks.Feedback> IHasFeedback.Feedbacks => throw new System.NotImplementedException();

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
 /// <summary>
 /// Represents a CrestronGenericBaseDeviceEventIds
 /// </summary>
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
  /// <summary>
  /// RegisterWithLogging method
  /// </summary>
		public static eDeviceRegistrationUnRegistrationResponse RegisterWithLogging(this GenericBase device, string key)
		{
		    var result = device.Register();
			
			Debug.LogMessage(LogEventLevel.Information, "Register device result: '{0}', type '{1}', result {2}", key, device, result);
			//if (result != eDeviceRegistrationUnRegistrationResponse.Success)
			//{
			//    Debug.LogMessage(LogEventLevel.Information, "Cannot register device '{0}': {1}", key, result);
			//}
			return result;
		}

	}
}