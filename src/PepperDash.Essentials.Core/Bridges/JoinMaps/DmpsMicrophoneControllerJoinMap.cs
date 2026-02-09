using System;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Represents a DmpsMicrophoneControllerJoinMap
    /// </summary>
    public class DmpsMicrophoneControllerJoinMap : JoinMapBaseAdvanced
    {

        /// <summary>
        /// Mic Gain dB Set / Get
        /// </summary>
        [JoinName("MicGain")]
        public JoinDataComplete MicGain = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Mic Gain dB Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// Mic Gain 16bit Scaled Set / Get
        /// </summary>
        [JoinName("MicGainScaled")]
        public JoinDataComplete MicGainScaled = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "Mic Gain 16bit Scaled Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// Mic Mute On Set / Get
        /// </summary>
        [JoinName("MicMuteOn")]
        public JoinDataComplete MicMuteOn = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Mic Mute On Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Mic Mute Off Set / Get
        /// </summary>
        [JoinName("MicMuteOff")]
        public JoinDataComplete MicMuteOff = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "Mic Mute Off Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Mic Gain Scaled Send Enable/Disable
        /// </summary>
        [JoinName("MicGainScaledSend")]
        public JoinDataComplete MicGainScaledSend = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata { Description = "Mic Gain Scaled Send Enable/Disable", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Mic Name Get
        /// </summary>
        [JoinName("MicName")]
        public JoinDataComplete MicName = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Mic Name Get", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public DmpsMicrophoneControllerJoinMap(uint joinStart)
            : this(joinStart, typeof(DmpsMicrophoneControllerJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected DmpsMicrophoneControllerJoinMap(uint joinStart, Type type)
            : base(joinStart, type)
        {
        }
    }
}