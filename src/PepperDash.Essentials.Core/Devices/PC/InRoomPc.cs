using System;
using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Feedbacks;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Core.VideoStatus;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Devices.PC
{
	[Obsolete("Please use PepperDash.Essentials.Devices.Common, this will be removed in 2.1")]
	public class InRoomPc : EssentialsDevice, IHasFeedback, IRoutingOutputs, IAttachVideoStatus, IUiDisplayInfo, IUsageTracking
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

		public InRoomPc(string key, string name)
			: base(key, name)
		{
			IconName = "PC";
			HasPowerOnFeedback = new BoolFeedback("HasPowerFeedback", 
				() => this.GetVideoStatuses() != VideoStatusOutputs.NoStatus);
			OutputPorts = new RoutingPortCollection<RoutingOutputPort>();
			OutputPorts.Add(AnyVideoOut = new RoutingOutputPort(RoutingPortNames.AnyVideoOut, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
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

    [Obsolete("Please use PepperDash.Essentials.Devices.Common, this will be removed in 2.1")]
    public class InRoomPcFactory : EssentialsDeviceFactory<InRoomPc>
    {
        public InRoomPcFactory()
        {
            TypeNames = new List<string>() { "inroompc" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new InRoomPc Device");
            return new InRoomPc(dc.Key, dc.Name);
        }
    }

}