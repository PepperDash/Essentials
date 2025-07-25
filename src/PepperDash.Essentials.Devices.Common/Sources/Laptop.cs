using System;
using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Sources
{
    /// <summary>
    /// Represents a Laptop
    /// </summary>
    public class Laptop : EssentialsDevice, IHasFeedback, IRoutingSource, IRoutingOutputs, IAttachVideoStatus, IUiDisplayInfo, IUsageTracking
	{
  /// <summary>
  /// Gets or sets the DisplayUiType
  /// </summary>
		public uint DisplayUiType { get { return DisplayUiConstants.TypeLaptop; } }
  /// <summary>
  /// Gets or sets the IconName
  /// </summary>
		public string IconName { get; set; }
  /// <summary>
  /// Gets or sets the HasPowerOnFeedback
  /// </summary>
		public BoolFeedback HasPowerOnFeedback { get; private set; }

  /// <summary>
  /// Gets or sets the AnyVideoOut
  /// </summary>
		public RoutingOutputPort AnyVideoOut { get; private set; }

		#region IRoutingOutputs Members

  /// <summary>
  /// Gets or sets the OutputPorts
  /// </summary>
		public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

		#endregion

		public Laptop(string key, string name)
			: base(key, name)
		{
			IconName = "Laptop";

			HasPowerOnFeedback = new BoolFeedback("HasPowerFeedback", 
				() => this.GetVideoStatuses() != VideoStatusOutputs.NoStatus);

			OutputPorts = new RoutingPortCollection<RoutingOutputPort>
            {
                (AnyVideoOut = new RoutingOutputPort(RoutingPortNames.AnyOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.None, 0, this))
            };
		}

		#region IHasFeedback Members

		/// <summary>
		/// Passes through the VideoStatuses list
		/// </summary>
        public FeedbackCollection<Feedback> Feedbacks
        {
            get
            {
                var newList = new FeedbackCollection<Feedback>();
                newList.AddRange(this.GetVideoStatuses().ToList());
                return newList;
            }
        }

		#endregion

        #region IUsageTracking Members

        /// <summary>
        /// Gets or sets the UsageTracker
        /// </summary>
        public UsageTracking UsageTracker { get; set; }

        #endregion
	}

    /// <summary>
    /// Represents a LaptopFactory
    /// </summary>
    public class LaptopFactory : EssentialsDeviceFactory<Laptop>
    {
        public LaptopFactory()
        {
            TypeNames = new List<string>() { "laptop" };
        }

        /// <summary>
        /// BuildDevice method
        /// </summary>
        /// <inheritdoc />
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new Laptop Device");
            return new Laptop(dc.Key, dc.Name);
        }
    }
}