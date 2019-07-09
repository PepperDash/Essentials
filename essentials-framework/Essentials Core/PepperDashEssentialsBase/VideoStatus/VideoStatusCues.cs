using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Core
{
	public static class VideoStatusCues
	{
		public static readonly Cue HasVideoStatusFeedback = Cue.BoolCue("HasVideoStatusFeedback", 1);
		public static readonly Cue VideoSyncFeedback = Cue.BoolCue("VideoSyncFeedback", 2);
		public static readonly Cue HdcpActiveFeedback = Cue.BoolCue("HdcpActiveFeedback", 3);
		public static readonly Cue HdcpStateFeedback = Cue.StringCue("HdcpStateFeedback", 3);
		public static readonly Cue VideoResolutionFeedback = Cue.StringCue("VideoResolutionFeedback", 2);
		public static readonly Cue VideoStatusDeviceKey = Cue.StringCue("VideoStatusDeviceKey", 0);
		public static readonly Cue VideoStatusDeviceName = Cue.StringCue("VideoStatusDeviceName", 4);
	}
}