using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.DM
{
	/// <summary>
	/// These methods will get the funcs that return values from various video port types...
	/// </summary>
	public class VideoStatusHelper
	{
		public static VideoStatusFuncsWrapper GetHdmiInputStatusFuncs(HdmiInputPort port)
		{
			return new VideoStatusFuncsWrapper
			{
				HdcpActiveFeedbackFunc = () => port.VideoAttributes.HdcpActiveFeedback.BoolValue,
				HdcpStateFeedbackFunc = () => port.VideoAttributes.HdcpStateFeedback.ToString(),
				VideoResolutionFeedbackFunc = () => port.VideoAttributes.GetVideoResolutionString(),
				VideoSyncFeedbackFunc = () => port.SyncDetectedFeedback.BoolValue
			};
		}

		public static VideoStatusFuncsWrapper GetHdmiInputStatusFuncs(EndpointHdmiInput port)
		{
			return new VideoStatusFuncsWrapper
			{
				VideoResolutionFeedbackFunc = () => port.VideoAttributes.GetVideoResolutionString(),
				VideoSyncFeedbackFunc = () => port.SyncDetectedFeedback.BoolValue
			};
		}

		public static VideoStatusFuncsWrapper GetVgaInputStatusFuncs(EndpointVgaInput port)
		{
			return new VideoStatusFuncsWrapper
			{
				HdcpActiveFeedbackFunc = () => port.VideoAttributes.HdcpActiveFeedback.BoolValue,
				HdcpStateFeedbackFunc = () => port.VideoAttributes.HdcpStateFeedback.ToString(),
				VideoResolutionFeedbackFunc = () => port.VideoAttributes.GetVideoResolutionString(),
				VideoSyncFeedbackFunc = () => port.SyncDetectedFeedback.BoolValue
			};
		}

        public static VideoStatusFuncsWrapper GetVgaInputStatusFuncs(VgaDviInputPort port)
        {
            return new VideoStatusFuncsWrapper
            {
                HdcpActiveFeedbackFunc = () => port.VideoAttributes.HdcpActiveFeedback.BoolValue,
                HdcpStateFeedbackFunc = () => port.VideoAttributes.HdcpStateFeedback.ToString(),
                VideoResolutionFeedbackFunc = () => port.VideoAttributes.GetVideoResolutionString(),
                VideoSyncFeedbackFunc = () => port.SyncDetectedFeedback.BoolValue
            };
        }

        public static VideoStatusFuncsWrapper GetBncInputStatusFuncs(Component port)
        {
            return new VideoStatusFuncsWrapper
            {
                VideoResolutionFeedbackFunc = () => port.VideoAttributes.GetVideoResolutionString(),
                VideoSyncFeedbackFunc = () => port.VideoDetectedFeedback.BoolValue
            };
        }

		public static VideoStatusFuncsWrapper GetDmInputStatusFuncs(DMInputPort port)
		{
			return new VideoStatusFuncsWrapper
			{
				HdcpActiveFeedbackFunc = () => port.VideoAttributes.HdcpActiveFeedback.BoolValue,
				HdcpStateFeedbackFunc = () => port.VideoAttributes.HdcpStateFeedback.ToString(),
				VideoResolutionFeedbackFunc = () => port.VideoAttributes.GetVideoResolutionString(),
				VideoSyncFeedbackFunc = () => port.SyncDetectedFeedback.BoolValue
			};
		}

		public static VideoStatusFuncsWrapper GetDisplayPortInputStatusFuncs(EndpointDisplayPortInput port)
		{
			return new VideoStatusFuncsWrapper
			{
				HdcpActiveFeedbackFunc = () => port.VideoAttributes.HdcpActiveFeedback.BoolValue,
				HdcpStateFeedbackFunc = () => port.VideoAttributes.HdcpStateFeedback.ToString(),
				VideoResolutionFeedbackFunc = () => port.VideoAttributes.GetVideoResolutionString(),
				VideoSyncFeedbackFunc = () => port.SyncDetectedFeedback.BoolValue
			};
		}
	}
}