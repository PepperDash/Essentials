using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Bridges
{
    public class IDigitalInputJoinMap : JoinMapBaseAdvanced
    {

        [JoinName("InputState")]
        public JoinDataComplete InputState = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Description = "Room Email Url", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });


        internal IDigitalInputJoinMap(uint joinStart)
            : base(joinStart, typeof(IDigitalInputJoinMap))
        {
        }

        public IDigitalInputJoinMap(uint joinStart, Type type)
            : base(joinStart, type)
        {
        }
    }
}