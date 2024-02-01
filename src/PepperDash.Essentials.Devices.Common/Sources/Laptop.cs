using System;
using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials.Devices.Common.Sources
{
    public class Laptop : EssentialsDevice, IHasFeedback, IRoutingOutputs, IAttachVideoStatus, IUiDisplayInfo, IUsageTracking
	{
		public uint DisplayUiType { get { return DisplayUiConstants.TypeLaptop; } }
		public string IconName { get; set; }
		public BoolFeedback HasPowerOnFeedback { get; private set; }

		public RoutingOutputPort AnyVideoOut { get; private set; }

		#region IRoutingOutputs Members

		/// <summary>
		/// Options: hdmi
		/// </summary>
		public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

		#endregion

		public Laptop(string key, string name)
			: base(key, name)
		{
			IconName = "Laptop";
			HasPowerOnFeedback = new BoolFeedback("HasPowerFeedback", 
				() => this.GetVideoStatuses() != VideoStatusOutputs.NoStatus);
			OutputPorts = new RoutingPortCollection<RoutingOutputPort>();
			OutputPorts.Add(AnyVideoOut = new RoutingOutputPort(RoutingPortNames.AnyOut, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
				eRoutingPortConnectionType.None, 0, this));
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

        public UsageTracking UsageTracker { get; set; }

        #endregion
	}

    public class LaptopFactory : EssentialsDeviceFactory<Laptop>
    {
        public LaptopFactory()
        {
            TypeNames = new List<string>() { "laptop" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Laptop Device");
            return new Laptop(dc.Key, dc.Name);
        }
    }
}