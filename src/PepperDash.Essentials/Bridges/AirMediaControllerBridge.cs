using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common;
using PepperDash.Essentials.DM.AirMedia;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Bridges
{
	public static class AirMediaControllerApiExtensions
	{
        public static void LinkToApi(this AirMediaController airMedia, BasicTriList trilist, uint joinStart, string joinMapKey)
		{
            AirMediaControllerJoinMap joinMap = new AirMediaControllerJoinMap();

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<AirMediaControllerJoinMap>(joinMapSerialized);

			joinMap.OffsetJoinNumbers(joinStart);

			Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
			Debug.Console(0, "Linking to Airmedia: {0}", airMedia.Name);

			trilist.StringInput[joinMap.Name].StringValue = airMedia.Name;			

			var commMonitor = airMedia as ICommunicationMonitor;
            if (commMonitor != null)
            {
                commMonitor.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
            }

            airMedia.IsInSessionFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsInSession]);
            airMedia.HdmiVideoSyncDetectedFeedback.LinkInputSig(trilist.BooleanInput[joinMap.HdmiVideoSync]);

            trilist.SetSigTrueAction(joinMap.AutomaticInputRoutingEnabled, new Action( airMedia.AirMedia.DisplayControl.EnableAutomaticRouting));
            trilist.SetSigFalseAction(joinMap.AutomaticInputRoutingEnabled, new Action( airMedia.AirMedia.DisplayControl.DisableAutomaticRouting));
            airMedia.AutomaticInputRoutingEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.AutomaticInputRoutingEnabled]);

            trilist.SetUShortSigAction(joinMap.VideoOut, new Action<ushort>((u) => airMedia.SelectVideoOut(u)));

            airMedia.VideoOutFeedback.LinkInputSig(trilist.UShortInput[joinMap.VideoOut]);
            airMedia.ErrorFeedback.LinkInputSig(trilist.UShortInput[joinMap.ErrorFB]);
            airMedia.NumberOfUsersConnectedFeedback.LinkInputSig(trilist.UShortInput[joinMap.NumberOfUsersConnectedFB]);

            trilist.SetUShortSigAction(joinMap.LoginCode, new Action<ushort>((u) => airMedia.AirMedia.AirMedia.LoginCode.UShortValue = u));
            airMedia.LoginCodeFeedback.LinkInputSig(trilist.UShortInput[joinMap.LoginCode]);

            airMedia.ConnectionAddressFeedback.LinkInputSig(trilist.StringInput[joinMap.ConnectionAddressFB]);
            airMedia.HostnameFeedback.LinkInputSig(trilist.StringInput[joinMap.HostnameFB]);
            airMedia.SerialNumberFeedback.LinkInputSig(trilist.StringInput[joinMap.SerialNumberFeedback]);
		}
	}	
}