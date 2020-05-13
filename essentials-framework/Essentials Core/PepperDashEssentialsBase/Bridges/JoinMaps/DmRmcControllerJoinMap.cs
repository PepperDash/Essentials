using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Bridges
{
    public class DmRmcControllerJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM RMC Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("CurrentOutputResolution")]
        public JoinDataComplete CurrentOutputResolution = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM RMC Current Output Resolution", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("EdidManufacturer")]
        public JoinDataComplete EdidManufacturer = new JoinDataComplete(new JoinData() { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM RMC EDID Manufacturer", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("EdidName")]
        public JoinDataComplete EdidName = new JoinDataComplete(new JoinData() { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM RMC EDID Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("EdidPrefferedTiming")]
        public JoinDataComplete EdidPrefferedTiming = new JoinDataComplete(new JoinData() { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM RMC EDID Preferred Timing", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("EdidSerialNumber")]
        public JoinDataComplete EdidSerialNumber = new JoinDataComplete(new JoinData() { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM RMC EDID Serial Number", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("AudioVideoSource")]
        public JoinDataComplete AudioVideoSource = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM RMC Audio Video Source Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });


        public DmRmcControllerJoinMap(uint joinStart)
            : base(joinStart, typeof(DmRmcControllerJoinMap))
        {
        }
    }
}