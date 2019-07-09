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

namespace PepperDash.Essentials.Bridges
{
	public static class AirMediaControllerApiExtensions
	{
        public static void LinkToApi(this AirMediaController airMedia, BasicTriList trilist, uint joinStart, string joinMapKey)
		{
			var joinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as AirMediaControllerJoinMap;

			if (joinMap == null)
			{
				joinMap = new AirMediaControllerJoinMap();
			}

			joinMap.OffsetJoinNumbers(joinStart);

			Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
			Debug.Console(0, "Linking to Bridge Type {0}", airMedia.GetType().Name.ToString());

			trilist.StringInput[joinMap.Name].StringValue = airMedia.GetType().Name.ToString();			

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
	public class AirMediaControllerJoinMap : JoinMapBase
	{
        // Digital
        public uint IsOnline { get; set; }
        public uint IsInSession { get; set; }
        public uint HdmiVideoSync { get; set; }
        public uint AutomaticInputRoutingEnabled { get; set; }

        // Analog
        public uint VideoOut { get; set; }
        public uint ErrorFB { get; set; }
        public uint NumberOfUsersConnectedFB { get; set; }
        public uint LoginCode { get; set; }

        // Serial
        public uint Name { get; set; }
        public uint ConnectionAddressFB { get; set; }
        public uint HostnameFB { get; set; }
        public uint SerialNumberFeedback { get; set; }


		public AirMediaControllerJoinMap()
		{
			// Digital
			IsOnline = 1;
            IsInSession = 2;
            HdmiVideoSync = 3;
            AutomaticInputRoutingEnabled = 4;

			// Analog
            VideoOut = 1;
            ErrorFB = 2;
            NumberOfUsersConnectedFB = 3;
            LoginCode = 4;

            // Serial
            Name = 1;
            ConnectionAddressFB = 2;
            HostnameFB = 3;
            SerialNumberFeedback = 4;
        }

		public override void OffsetJoinNumbers(uint joinStart)
		{
			var joinOffset = joinStart - 1;

			IsOnline = IsOnline + joinOffset;
            IsInSession = IsInSession + joinOffset;
            HdmiVideoSync = HdmiVideoSync + joinOffset;
            AutomaticInputRoutingEnabled = AutomaticInputRoutingEnabled + joinOffset;

            VideoOut = VideoOut + joinOffset;
            ErrorFB = ErrorFB + joinOffset;
            NumberOfUsersConnectedFB = NumberOfUsersConnectedFB + joinOffset;
            LoginCode = LoginCode + joinOffset;

			Name = Name + joinOffset;
            ConnectionAddressFB = ConnectionAddressFB + joinOffset;
            HostnameFB = HostnameFB + joinOffset;
            SerialNumberFeedback = SerialNumberFeedback + joinOffset;
		}
	}
}