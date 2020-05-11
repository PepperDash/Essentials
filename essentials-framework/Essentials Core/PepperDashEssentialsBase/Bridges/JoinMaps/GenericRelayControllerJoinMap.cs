using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Bridges
{
    public class GenericRelayControllerJoinMap : JoinMapBaseAdvanced
    {

        [JoinName("Relay")]
        public JoinDataComplete Relay = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Device Relay State Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });


        public GenericRelayControllerJoinMap(uint joinStart)
            : base(joinStart, typeof(GenericRelayControllerJoinMap))
        {
        }
    }
}