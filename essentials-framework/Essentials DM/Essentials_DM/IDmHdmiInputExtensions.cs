using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DM;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.DM
{
	public static class IBasicDmInputExtensions
	{
		public static VideoStatusFuncsWrapper GetVideoStatusFuncsWrapper(this IBasicDMInput input)
		{
			var va = (input as IVideoAttributesEnhanced).VideoAttributes;
			return new VideoStatusFuncsWrapper 
			{
				HasVideoStatusFunc = () => true,
				HdcpActiveFeedbackFunc = () => va.HdcpActiveFeedback.BoolValue,
				HdcpStateFeedbackFunc = () => va.HdcpStateFeedback.ToString(),
				VideoResolutionFeedbackFunc = () => 
					{
                        //var h = va.HorizontalResolutionFeedback.UShortValue;
                        //var v = va.VerticalResolutionFeedback.UShortValue;
                        //if (h == 0 || v == 0)
                        //    return "---";
                        return va.GetVideoResolutionString(); // h + "x" + v;
					},
				VideoSyncFeedbackFunc = () => input.SyncDetectedFeedback.BoolValue
			};
		}
	}


	public static class IEndpointHdmiInputExtensions
	{
		public static VideoStatusFuncsWrapper GetVideoStatusFuncsWrapper(this Crestron.SimplSharpPro.DM.Endpoints.EndpointHdmiInput input)
		{
			var va = (input as IVideoAttributesEnhanced).VideoAttributes;
			return new VideoStatusFuncsWrapper
			{
				HasVideoStatusFunc = () => true,
				HdcpActiveFeedbackFunc = () => va.HdcpActiveFeedback.BoolValue,
				HdcpStateFeedbackFunc = () => va.HdcpStateFeedback.ToString(),
				VideoResolutionFeedbackFunc = () =>
				{
                    //var h = va.HorizontalResolutionFeedback.UShortValue;
                    //var v = va.VerticalResolutionFeedback.UShortValue;
                    //if (h == 0 || v == 0)
                    //    return "---";
                    return va.GetVideoResolutionString(); // h + "x" + v;
				},
				VideoSyncFeedbackFunc = () => input.SyncDetectedFeedback.BoolValue
			};
		}
	}


}