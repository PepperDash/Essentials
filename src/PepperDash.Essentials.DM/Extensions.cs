using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Cards;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.DM
{
	public static class VideoAttributesBasicExtensions
	{
		public static string GetVideoResolutionString(this VideoAttributesBasic va)
		{
			ushort h = va.HorizontalResolutionFeedback.UShortValue;
			ushort v = va.VerticalResolutionFeedback.UShortValue;
            ushort r = va.FramesPerSecondFeedback.UShortValue;
			if (h == 0 || v == 0)
				return "n/a";
			else
				return string.Format("{0}x{1}@{2}Hz", h, v, r);
		}
	}

    public static class AdvEndpointHdmiOutputExtensions
    {
        public static string GetVideoResolutionString(this AdvEndpointHdmiOutput va)
        {
            ushort h = va.HorizontalResolutionFeedback.UShortValue;
            ushort v = va.VerticalResolutionFeedback.UShortValue;
            ushort r = va.FramesPerSecondFeedback.UShortValue;
            if (h == 0 || v == 0)
                return "n/a";
            else
                return string.Format("{0}x{1}@{2}Hz", h, v, r);
        }
    }
}