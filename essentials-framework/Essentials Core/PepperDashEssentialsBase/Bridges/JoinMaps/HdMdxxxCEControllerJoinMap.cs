using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Bridges
{
    public class HdMdxxxCEControllerJoinMap : JoinMapBaseAdvanced
    {

        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Device Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("RemoteEndDetected")]
        public JoinDataComplete RemoteEndDetected = new JoinDataComplete(new JoinData() { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata() { Label = "Device Remote End Detected", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("AutoRouteOn")]
        public JoinDataComplete AutoRouteOn = new JoinDataComplete(new JoinData() { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata() { Label = "Device Auto Route On", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("AutoRouteOff")]
        public JoinDataComplete AutoRouteOff = new JoinDataComplete(new JoinData() { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata() { Label = "Device Auto Route Off", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("PriorityRoutingOn")]
        public JoinDataComplete PriorityRoutingOn = new JoinDataComplete(new JoinData() { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata() { Label = "Device Priority Routing On", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("PriorityRoutingOff")]
        public JoinDataComplete PriorityRoutingOff = new JoinDataComplete(new JoinData() { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata() { Label = "Device Priority Routing Off", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("InputOnScreenDisplayEnabled")]
        public JoinDataComplete InputOnScreenDisplayEnabled = new JoinDataComplete(new JoinData() { JoinNumber = 7, JoinSpan = 1 },
            new JoinMetadata() { Label = "Device Input OSD Enabled", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("InputOnScreenDisplayDisabled")]
        public JoinDataComplete InputOnScreenDisplayDisabled = new JoinDataComplete(new JoinData() { JoinNumber = 8, JoinSpan = 1 },
            new JoinMetadata() { Label = "Device Input OSD Disabled", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("SyncDetected")]
        public JoinDataComplete SyncDetected = new JoinDataComplete(new JoinData() { JoinNumber = 11, JoinSpan = 5 },
            new JoinMetadata() { Label = "Device Sync Detected", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("VideoSource")]
        public JoinDataComplete VideoSource = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 5 },
            new JoinMetadata() { Label = "Device Video Source Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("SourceCount")]
        public JoinDataComplete SourceCount = new JoinDataComplete(new JoinData() { JoinNumber = 2, JoinSpan = 5 },
            new JoinMetadata() { Label = "Device Video Source Count", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        [JoinName("SourceNames")]
        public JoinDataComplete SourceNames = new JoinDataComplete(new JoinData() { JoinNumber = 11, JoinSpan = 5 },
            new JoinMetadata() { Label = "Device Video Source Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });



        public HdMdxxxCEControllerJoinMap(uint joinStart)
            : base(joinStart, typeof(HdMdxxxCEControllerJoinMap))
        {
        }
    }
}