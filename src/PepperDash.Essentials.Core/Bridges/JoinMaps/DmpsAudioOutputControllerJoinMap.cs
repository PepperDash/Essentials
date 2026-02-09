using System;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Represents a DmpsAudioOutputControllerJoinMap
    /// </summary>
    public class DmpsAudioOutputControllerJoinMap : JoinMapBaseAdvanced
    {

        /// <summary>
        /// Master Volume Level Signed dB Set / Get
        /// </summary>
        [JoinName("MasterVolumeLevel")]
        public JoinDataComplete MasterVolumeLevel = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Master Volume Signed dB Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// Master Volume 16bit Scaled Set / Get
        /// </summary>
        [JoinName("MasterVolumeLevelScaled")]
        public JoinDataComplete MasterVolumeLevelScaled = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "Master Volume 16bit Scaled Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// Mixer Preset Recall Set
        /// </summary>
        [JoinName("MixerPresetRecall")]
        public JoinDataComplete MixerPresetRecall = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata { Description = "Mixer Preset Recall Set", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// Mixer Eq Preset Recall Set
        /// </summary>
        [JoinName("MixerEqPresetRecall")]
        public JoinDataComplete MixerEqPresetRecall = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata { Description = "Mixer Eq Preset Recall Set", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// Master Volume Mute On Set / Get
        /// </summary>
        [JoinName("MasterVolumeMuteOn")]
        public JoinDataComplete MasterVolumeMuteOn = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Master Volume Mute On Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Master Volume Mute Off Set / Get
        /// </summary>
        [JoinName("MasterVolumeMuteOff")]
        public JoinDataComplete MasterVolumeMuteOff = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "Master Volume Mute Off Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Master Volume Level Up
        /// </summary>
        [JoinName("MasterVolumeUp")]
        public JoinDataComplete MasterVolumeUp = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata { Description = "Master Volume Level Up", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Master Volume Level Down
        /// </summary>
        [JoinName("MasterVolumeDown")]
        public JoinDataComplete MasterVolumeDown = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata { Description = "Master Volume Level Down", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Master Volume Scaled Send Enable/Disable
        /// </summary>
        [JoinName("MasterVolumeLevelScaledSend")]
        public JoinDataComplete MasterVolumeLevelScaledSend = new JoinDataComplete(new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata { Description = "Master Volume Scaled Send Enable/Disable", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Source Volume Signed dB Set / Get
        /// </summary>
        [JoinName("SourceVolumeLevel")]
        public JoinDataComplete SourceVolumeLevel = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata { Description = "Source Volume Signed dB Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// Source Volume 16bit Scaled Set / Get
        /// </summary>
        [JoinName("SourceVolumeLevelScaled")]
        public JoinDataComplete SourceVolumeLevelScaled = new JoinDataComplete(new JoinData { JoinNumber = 12, JoinSpan = 1 },
            new JoinMetadata { Description = "Source Volume 16bit Scaled Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// Source Volume Mute On Set / Get
        /// </summary>
        [JoinName("SourceVolumeMuteOn")]
        public JoinDataComplete SourceVolumeMuteOn = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata { Description = "Source Volume Mute On Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Source Volume Mute Off Set / Get
        /// </summary>
        [JoinName("SourceVolumeMuteOff")]
        public JoinDataComplete SourceVolumeMuteOff = new JoinDataComplete(new JoinData { JoinNumber = 12, JoinSpan = 1 },
            new JoinMetadata { Description = "Source Volume Mute Off Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Source Volume Level Up
        /// </summary>
        [JoinName("SourceVolumeUp")]
        public JoinDataComplete SourceVolumeUp = new JoinDataComplete(new JoinData { JoinNumber = 13, JoinSpan = 1 },
            new JoinMetadata { Description = "Source Volume Level Up", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Source Volume Level Down
        /// </summary>
        [JoinName("SourceVolumeDown")]
        public JoinDataComplete SourceVolumeDown = new JoinDataComplete(new JoinData { JoinNumber = 14, JoinSpan = 1 },
            new JoinMetadata { Description = "Source Volume Level Down", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Source Volume Scaled Send Enable/Disable
        /// </summary>
        [JoinName("SourceVolumeLevelScaledSend")]
        public JoinDataComplete SourceVolumeLevelScaledSend = new JoinDataComplete(new JoinData { JoinNumber = 15, JoinSpan = 1 },
            new JoinMetadata { Description = "Source Volume Scaled Send Enable/Disable", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Codec1 Volume Signed dB Set / Get
        /// </summary>
        [JoinName("Codec1VolumeLevel")]
        public JoinDataComplete Codec1VolumeLevel = new JoinDataComplete(new JoinData { JoinNumber = 21, JoinSpan = 1 },
            new JoinMetadata { Description = "Codec1 Volume Signed dB Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// Codec1 Volume 16bit Scaled Set / Get
        /// </summary>
        [JoinName("Codec1VolumeLevelScaled")]
        public JoinDataComplete Codec1VolumeLevelScaled = new JoinDataComplete(new JoinData { JoinNumber = 22, JoinSpan = 1 },
            new JoinMetadata { Description = "Codec1 Volume 16bit Scaled Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// Codec1 Volume Mute On Set / Get
        /// </summary>
        [JoinName("Codec1VolumeMuteOn")]
        public JoinDataComplete Codec1VolumeMuteOn = new JoinDataComplete(new JoinData { JoinNumber = 21, JoinSpan = 1 },
            new JoinMetadata { Description = "Codec1 Volume Mute On Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Codec1 Volume Mute Off Set / Get
        /// </summary>
        [JoinName("Codec1VolumeMuteOff")]
        public JoinDataComplete Codec1VolumeMuteOff = new JoinDataComplete(new JoinData { JoinNumber = 22, JoinSpan = 1 },
            new JoinMetadata { Description = "Codec1 Volume Mute Off Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Codec1 Volume Level Up
        /// </summary>
        [JoinName("Codec1VolumeUp")]
        public JoinDataComplete Codec1VolumeUp = new JoinDataComplete(new JoinData { JoinNumber = 23, JoinSpan = 1 },
            new JoinMetadata { Description = "Codec1 Volume Level Up", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Codec1 Volume Level Down
        /// </summary>
        [JoinName("Codec1VolumeDown")]
        public JoinDataComplete Codec1VolumeDown = new JoinDataComplete(new JoinData { JoinNumber = 24, JoinSpan = 1 },
            new JoinMetadata { Description = "Codec1 Volume Level Down", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Codec1 Volume Scaled Send Enable/Disable
        /// </summary>
        [JoinName("Codec1VolumeLevelScaledSend")]
        public JoinDataComplete Codec1VolumeLevelScaledSend = new JoinDataComplete(new JoinData { JoinNumber = 25, JoinSpan = 1 },
            new JoinMetadata { Description = "Codec1 Volume Scaled Send Enable/Disable", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Codec2 Volume Signed dB Set / Get
        /// </summary>
        [JoinName("Codec2VolumeLevel")]
        public JoinDataComplete Codec2VolumeLevel = new JoinDataComplete(new JoinData { JoinNumber = 31, JoinSpan = 1 },
            new JoinMetadata { Description = "Codec2 Volume Signed dB Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// Codec2 Volume 16bit Scaled Set / Get
        /// </summary>
        [JoinName("Codec2VolumeLevelScaled")]
        public JoinDataComplete Codec2VolumeLevelScaled = new JoinDataComplete(new JoinData { JoinNumber = 32, JoinSpan = 1 },
            new JoinMetadata { Description = "Codec2 Volume 16bit Scaled Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// Codec2 Volume Mute On Set / Get
        /// </summary>
        [JoinName("Codec2VolumeMuteOn")]
        public JoinDataComplete Codec2VolumeMuteOn = new JoinDataComplete(new JoinData { JoinNumber = 31, JoinSpan = 1 },
            new JoinMetadata { Description = "Codec2 Volume Mute On Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Codec2 Volume Mute Off Set / Get
        /// </summary>
        [JoinName("Codec2VolumeMuteOff")]
        public JoinDataComplete Codec2VolumeMuteOff = new JoinDataComplete(new JoinData { JoinNumber = 32, JoinSpan = 1 },
            new JoinMetadata { Description = "Codec2 Volume Mute Off Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Codec2 Volume Level Up
        /// </summary>
        [JoinName("Codec2VolumeUp")]
        public JoinDataComplete Codec2VolumeUp = new JoinDataComplete(new JoinData { JoinNumber = 33, JoinSpan = 1 },
            new JoinMetadata { Description = "Codec2 Volume Level Up", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Codec2 Volume Level Down
        /// </summary>
        [JoinName("Codec2VolumeDown")]
        public JoinDataComplete Codec2VolumeDown = new JoinDataComplete(new JoinData { JoinNumber = 34, JoinSpan = 1 },
            new JoinMetadata { Description = "Codec2 Volume Level Down", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Codec2 Volume Scaled Send Enable/Disable
        /// </summary>
        [JoinName("Codec2VolumeLevelScaledSend")]
        public JoinDataComplete Codec2VolumeLevelScaledSend = new JoinDataComplete(new JoinData { JoinNumber = 35, JoinSpan = 1 },
            new JoinMetadata { Description = "Codec2 Volume Scaled Send Enable/Disable", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// MicsMaster Volume Signed dB Set / Get
        /// </summary>
        [JoinName("MicsMasterVolumeLevel")]
        public JoinDataComplete MicsMasterVolumeLevel = new JoinDataComplete(new JoinData { JoinNumber = 41, JoinSpan = 1 },
            new JoinMetadata { Description = "MicsMaster Volume Signed dB Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// MicsMaster Volume 16bit Scaled Set / Get
        /// </summary>
        [JoinName("MicsMasterVolumeLevelScaled")]
        public JoinDataComplete MicsMasterVolumeLevelScaled = new JoinDataComplete(new JoinData { JoinNumber = 42, JoinSpan = 1 },
            new JoinMetadata { Description = "MicsMaster Volume 16bit Scaled Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// MicsMaster Volume Mute On Set / Get
        /// </summary>
        [JoinName("MicsMasterVolumeMuteOn")]
        public JoinDataComplete MicsMasterVolumeMuteOn = new JoinDataComplete(new JoinData { JoinNumber = 41, JoinSpan = 1 },
            new JoinMetadata { Description = "MicsMaster Volume Mute On Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// MicsMaster Volume Mute Off Set / Get
        /// </summary>
        [JoinName("MicsMasterVolumeMuteOff")]
        public JoinDataComplete MicsMasterVolumeMuteOff = new JoinDataComplete(new JoinData { JoinNumber = 42, JoinSpan = 1 },
            new JoinMetadata { Description = "MicsMaster Volume Mute Off Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// MicsMaster Volume Level Up
        /// </summary>
        [JoinName("MicsMasterVolumeUp")]
        public JoinDataComplete MicsMasterVolumeUp = new JoinDataComplete(new JoinData { JoinNumber = 43, JoinSpan = 1 },
            new JoinMetadata { Description = "MicsMaster Volume Level Up", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// MicsMaster Volume Level Down
        /// </summary>
        [JoinName("MicsMasterVolumeDown")]
        public JoinDataComplete MicsMasterVolumeDown = new JoinDataComplete(new JoinData { JoinNumber = 44, JoinSpan = 1 },
            new JoinMetadata { Description = "MicsMaster Volume Level Down", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// MicsMaster Volume Scaled Send Enable/Disable
        /// </summary>
        [JoinName("MicsMasterVolumeLevelScaledSend")]
        public JoinDataComplete MicsMasterVolumeLevelScaledSend = new JoinDataComplete(new JoinData { JoinNumber = 45, JoinSpan = 1 },
            new JoinMetadata { Description = "Mics Master Volume Scaled Send Enable/Disable", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public DmpsAudioOutputControllerJoinMap(uint joinStart)
            : this(joinStart, typeof(DmpsAudioOutputControllerJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected DmpsAudioOutputControllerJoinMap(uint joinStart, Type type) : base(joinStart, type)
        {
        }
    }
}