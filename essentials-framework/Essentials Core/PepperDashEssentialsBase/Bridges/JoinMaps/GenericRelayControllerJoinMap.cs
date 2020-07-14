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
            new JoinMetadata() { Description = "Device Relay State Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });


        internal GenericRelayControllerJoinMap(uint joinStart)
            : base(joinStart, typeof(GenericRelayControllerJoinMap))
        {
        }

        public GenericRelayControllerJoinMap(uint joinStart, Type type) : base(joinStart, type)
        {
            
        }
    }
}