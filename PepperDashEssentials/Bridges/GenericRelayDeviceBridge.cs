using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;


using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.CrestronIO;

namespace PepperDash.Essentials.Bridges
{
    public static class GenericRelayDeviceApiExtensions
    {
        public static void LinkToApi(this GenericRelayDevice relay, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            var joinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as GenericRelayControllerJoinMap;

            if (joinMap == null)
                joinMap = new GenericRelayControllerJoinMap();

            joinMap.OffsetJoinNumbers(joinStart);

            if (relay.RelayOutput == null)
            {
                Debug.Console(1, relay, "Unable to link device '{0}'.  Relay is null", relay.Key);
                return;
            }

            Debug.Console(1, relay, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            trilist.SetBoolSigAction(joinMap.Relay, new Action<bool>(b =>
                {
                    if (b)
                        relay.CloseRelay();
                    else
                        relay.OpenRelay();
                }));

            // feedback for relay state

            relay.OutputIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Relay]);
        }

    }
}