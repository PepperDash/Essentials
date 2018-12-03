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
	public static class DigitalLoggerApiExtensions
	{
		public static void LinkToApi(this DigitalLogger digitalLogger, BasicTriList trilist, uint joinStart, string joinMapKey)
		{
			var joinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as DigitalLoggerJoinMap;

			if (joinMap == null)
				joinMap = new DigitalLoggerJoinMap();

			joinMap.OffsetJoinNumbers(joinStart);
			Debug.Console(1, digitalLogger, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
			for (uint i = 1; i <= digitalLogger.CircuitCount; i++)
            {
                var circuit = i;
				digitalLogger.CircuitNameFeedbacks[circuit - 1].LinkInputSig(trilist.StringInput[joinMap.CircuitNames + circuit]);
			}
		}
	}
	public class DigitalLoggerJoinMap : JoinMapBase
	{
		public uint IsOnline { get; set; }
		public uint CircuitNames { get; set; }
		public DigitalLoggerJoinMap()
		{
			// Digital
			IsOnline = 1;

			// Serial
			CircuitNames = 1;
			// Analog
		}

		public override void OffsetJoinNumbers(uint joinStart)
		{
			var joinOffset = joinStart - 1;

			IsOnline = IsOnline + joinOffset;
			CircuitNames = CircuitNames + joinOffset;
		}
	}

}