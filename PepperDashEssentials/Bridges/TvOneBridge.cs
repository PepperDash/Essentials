using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common;

namespace PepperDash.Essentials.Bridges
{
	public static class TvOneApiExtensions
	{
		public static void LinkToApi(this TVOneCorio TvOne, BasicTriList trilist, uint joinStart, string joinMapKey)
		{
			var joinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as TvOneJoinMap;

			if (joinMap == null)
				joinMap = new TvOneJoinMap();

			joinMap.OffsetJoinNumbers(joinStart);
			Debug.Console(1, TvOne, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

			TvOne.OnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
			trilist.SetUShortSigAction(joinMap.CallPreset, u => TvOne.CallPreset(u));
			TvOne.PresetFeedback.LinkInputSig(trilist.UShortInput[joinMap.PresetFeedback]);
		}
	}
	public class TvOneJoinMap : JoinMapBase
	{
		public uint CallPreset { get; set; }
		public uint PresetFeedback { get; set; }
		public uint IsOnline { get; set; }
		public TvOneJoinMap()
		{
			// Digital
			CallPreset = 1;
			PresetFeedback = 1;
			IsOnline = 1;

		}

		public override void OffsetJoinNumbers(uint joinStart)
		{
			var joinOffset = joinStart - 1;

			IsOnline = IsOnline + joinOffset;
			CallPreset = CallPreset + joinOffset;
			PresetFeedback = PresetFeedback + joinOffset;
		}
	}

}