using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Bridges
{
    public class DmpsAudioOutputControllerJoinMap : JoinMapBaseAdvanced
    {

        [JoinName("MasterVolumeLevel")]
        public JoinDataComplete MasterVolumeLevel = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Master Volume Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("MasterVolumeMuteOn")]
        public JoinDataComplete MasterVolumeMuteOn = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Master Volume Mute On Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("MasterVolumeMuteOff")]
        public JoinDataComplete MasterVolumeMuteOff = new JoinDataComplete(new JoinData() { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata() { Label = "Master Volume Mute Off Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("MasterVolumeUp")]
        public JoinDataComplete MasterVolumeUp = new JoinDataComplete(new JoinData() { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata() { Label = "Master Volume Level Up", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("MasterVolumeDown")]
        public JoinDataComplete MasterVolumeDown = new JoinDataComplete(new JoinData() { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata() { Label = "Master Volume Mute Level Down", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("SourceVolumeLevel")]
        public JoinDataComplete SourceVolumeLevel = new JoinDataComplete(new JoinData() { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata() { Label = "Source Volume Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("SourceVolumeMuteOn")]
        public JoinDataComplete SourceVolumeMuteOn = new JoinDataComplete(new JoinData() { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata() { Label = "Source Volume Mute On Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("SourceVolumeMuteOff")]
        public JoinDataComplete SourceVolumeMuteOff = new JoinDataComplete(new JoinData() { JoinNumber = 12, JoinSpan = 1 },
            new JoinMetadata() { Label = "Source Volume Mute Off Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("SourceVolumeUp")]
        public JoinDataComplete SourceVolumeUp = new JoinDataComplete(new JoinData() { JoinNumber = 13, JoinSpan = 1 },
            new JoinMetadata() { Label = "Source Volume Level Up", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("SourceVolumeDown")]
        public JoinDataComplete SourceVolumeDown = new JoinDataComplete(new JoinData() { JoinNumber = 14, JoinSpan = 1 },
            new JoinMetadata() { Label = "Source Volume Mute Level Down", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Codec1VolumeLevel")]
        public JoinDataComplete Codec1VolumeLevel = new JoinDataComplete(new JoinData() { JoinNumber = 21, JoinSpan = 1 },
            new JoinMetadata() { Label = "Codec1 Volume Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("Codec1VolumeMuteOn")]
        public JoinDataComplete Codec1VolumeMuteOn = new JoinDataComplete(new JoinData() { JoinNumber = 21, JoinSpan = 1 },
            new JoinMetadata() { Label = "Codec1 Volume Mute On Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Codec1VolumeMuteOff")]
        public JoinDataComplete Codec1VolumeMuteOff = new JoinDataComplete(new JoinData() { JoinNumber = 22, JoinSpan = 1 },
            new JoinMetadata() { Label = "Codec1 Volume Mute Off Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Codec1VolumeUp")]
        public JoinDataComplete Codec1VolumeUp = new JoinDataComplete(new JoinData() { JoinNumber = 23, JoinSpan = 1 },
            new JoinMetadata() { Label = "Codec1 Volume Level Up", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Codec1VolumeDown")]
        public JoinDataComplete Codec1VolumeDown = new JoinDataComplete(new JoinData() { JoinNumber = 24, JoinSpan = 1 },
            new JoinMetadata() { Label = "Codec1 Volume Mute Level Down", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Codec2VolumeLevel")]
        public JoinDataComplete Codec2VolumeLevel = new JoinDataComplete(new JoinData() { JoinNumber = 31, JoinSpan = 1 },
            new JoinMetadata() { Label = "Codec2 Volume Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("Codec2VolumeMuteOn")]
        public JoinDataComplete Codec2VolumeMuteOn = new JoinDataComplete(new JoinData() { JoinNumber = 31, JoinSpan = 1 },
            new JoinMetadata() { Label = "Codec2 Volume Mute On Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Codec2VolumeMuteOff")]
        public JoinDataComplete Codec2VolumeMuteOff = new JoinDataComplete(new JoinData() { JoinNumber = 32, JoinSpan = 1 },
            new JoinMetadata() { Label = "Codec2 Volume Mute Off Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Codec2VolumeUp")]
        public JoinDataComplete Codec2VolumeUp = new JoinDataComplete(new JoinData() { JoinNumber = 33, JoinSpan = 1 },
            new JoinMetadata() { Label = "Codec2 Volume Level Up", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Codec2VolumeDown")]
        public JoinDataComplete Codec2VolumeDown = new JoinDataComplete(new JoinData() { JoinNumber = 34, JoinSpan = 1 },
            new JoinMetadata() { Label = "Codec2 Volume Mute Level Down", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });



        public DmpsAudioOutputControllerJoinMap(uint joinStart)
            : base(joinStart, typeof(DmpsAudioOutputControllerJoinMap))
        {
        }
    }
}