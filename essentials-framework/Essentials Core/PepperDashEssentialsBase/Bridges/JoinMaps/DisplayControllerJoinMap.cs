using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Bridges
{
    public class DisplayControllerJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("Name")]
        public JoinDataComplete Name = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Description = "Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("PowerOff")]
        public JoinDataComplete PowerOff = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 }, 
            new JoinMetadata() { Description = "Power Off", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("PowerOn")]
        public JoinDataComplete PowerOn = new JoinDataComplete(new JoinData() { JoinNumber = 2, JoinSpan = 1 }, 
            new JoinMetadata() { Description = "Power On", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("IsTwoWayDisplay")]
        public JoinDataComplete IsTwoWayDisplay = new JoinDataComplete(new JoinData() { JoinNumber = 3, JoinSpan = 1 }, 
            new JoinMetadata() { Description = "Is Two Way Display", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("VolumeUp")]
        public JoinDataComplete VolumeUp = new JoinDataComplete(new JoinData() { JoinNumber = 5, JoinSpan = 1 }, 
            new JoinMetadata() { Description = "Volume Up", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("VolumeLevel")]
        public JoinDataComplete VolumeLevel = new JoinDataComplete(new JoinData() { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata() { Description = "Volume Level", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("VolumeDown")]
        public JoinDataComplete VolumeDown = new JoinDataComplete(new JoinData() { JoinNumber = 6, JoinSpan = 1 }, 
            new JoinMetadata() { Description = "Volume Down", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("VolumeMute")]
        public JoinDataComplete VolumeMute = new JoinDataComplete(new JoinData() { JoinNumber = 7, JoinSpan = 1 }, 
            new JoinMetadata() { Description = "Volume Mute", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("VolumeMuteOn")]
        public JoinDataComplete VolumeMuteOn = new JoinDataComplete(new JoinData() { JoinNumber = 8, JoinSpan = 1 }, 
            new JoinMetadata() { Description = "Volume Mute On", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("VolumeMuteOff")]
        public JoinDataComplete VolumeMuteOff = new JoinDataComplete(new JoinData() { JoinNumber = 9, JoinSpan = 1 }, 
            new JoinMetadata() { Description = "Volume Mute Off", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("InputSelectOffset")]
        public JoinDataComplete InputSelectOffset = new JoinDataComplete(new JoinData() { JoinNumber = 11, JoinSpan = 10 }, 
            new JoinMetadata() { Description = "Input Select", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("InputNamesOffset")]
        public JoinDataComplete InputNamesOffset = new JoinDataComplete(new JoinData() { JoinNumber = 11, JoinSpan = 10 }, 
            new JoinMetadata() { Description = "Input Names Offset", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("InputSelect")]
        public JoinDataComplete InputSelect = new JoinDataComplete(new JoinData() { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata() { Description = "Input Select", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("ButtonVisibilityOffset")]
        public JoinDataComplete ButtonVisibilityOffset = new JoinDataComplete(new JoinData() { JoinNumber = 41, JoinSpan = 10 }, 
            new JoinMetadata() { Description = "Button Visibility Offset", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.DigitalSerial });

        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData() { JoinNumber = 50, JoinSpan = 1 }, 
            new JoinMetadata() { Description = "Is Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        internal DisplayControllerJoinMap(uint joinStart)
            : base(joinStart, typeof(DisplayControllerJoinMap))
        {
        }

        public DisplayControllerJoinMap(uint joinStart, Type type) : base(joinStart, type)
        {
        }
    }
}