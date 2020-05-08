using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.Core.Bridges
{
    public class GenericLightingJoinMap : JoinMapBaseAdvanced
    {

        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Lighting Controller Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("SelectScene")]
        public JoinDataComplete SelectScene = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Lighting Controller Select Scene By Index", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("SelectSceneDirect")]
        public JoinDataComplete SelectSceneDirect = new JoinDataComplete(new JoinData() { JoinNumber = 11, JoinSpan = 10 },
            new JoinMetadata() { Label = "Lighting Controller Select Scene", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.DigitalSerial });

        [JoinName("ButtonVisibility")]
        public JoinDataComplete ButtonVisibility = new JoinDataComplete(new JoinData() { JoinNumber = 41, JoinSpan = 10 },
            new JoinMetadata() { Label = "Lighting Controller Button Visibility", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("IntegrationIdSet")]
        public JoinDataComplete IntegrationIdSet = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Lighting Controller Set Integration Id", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });




        public GenericLightingJoinMap(uint joinStart)
            : base(joinStart, typeof(GenericLightingJoinMap))
        {
        }
    }
}