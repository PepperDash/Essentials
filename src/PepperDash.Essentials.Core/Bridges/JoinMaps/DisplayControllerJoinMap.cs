using System;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Represents a DisplayControllerJoinMap
    /// </summary>
    public class DisplayControllerJoinMap : JoinMapBaseAdvanced
    {
        /// <summary>
        /// Name
        /// </summary>
        [JoinName("Name")]
        public JoinDataComplete Name = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// Power Off
        /// </summary>
        [JoinName("PowerOff")]
        public JoinDataComplete PowerOff = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 }, 
            new JoinMetadata { Description = "Power Off", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Power On
        /// </summary>
        [JoinName("PowerOn")]
        public JoinDataComplete PowerOn = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 }, 
            new JoinMetadata { Description = "Power On", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Is Two Way Display
        /// </summary>
        [JoinName("IsTwoWayDisplay")]
        public JoinDataComplete IsTwoWayDisplay = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 }, 
            new JoinMetadata { Description = "Is Two Way Display", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Volume Up
        /// </summary>
        [JoinName("VolumeUp")]
        public JoinDataComplete VolumeUp = new JoinDataComplete(new JoinData { JoinNumber = 5, JoinSpan = 1 }, 
            new JoinMetadata { Description = "Volume Up", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Volume Level
        /// </summary>
        [JoinName("VolumeLevel")]
        public JoinDataComplete VolumeLevel = new JoinDataComplete(new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata { Description = "Volume Level", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// Volume Down
        /// </summary>
        [JoinName("VolumeDown")]
        public JoinDataComplete VolumeDown = new JoinDataComplete(new JoinData { JoinNumber = 6, JoinSpan = 1 }, 
            new JoinMetadata { Description = "Volume Down", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Volume Mute
        /// </summary>
        [JoinName("VolumeMute")]
        public JoinDataComplete VolumeMute = new JoinDataComplete(new JoinData { JoinNumber = 7, JoinSpan = 1 }, 
            new JoinMetadata { Description = "Volume Mute", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Volume Mute On
        /// </summary>
        [JoinName("VolumeMuteOn")]
        public JoinDataComplete VolumeMuteOn = new JoinDataComplete(new JoinData { JoinNumber = 8, JoinSpan = 1 }, 
            new JoinMetadata { Description = "Volume Mute On", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Volume Mute Off
        /// </summary>
        [JoinName("VolumeMuteOff")]
        public JoinDataComplete VolumeMuteOff = new JoinDataComplete(new JoinData { JoinNumber = 9, JoinSpan = 1 }, 
            new JoinMetadata { Description = "Volume Mute Off", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Input Select Offset
        /// </summary>
        [JoinName("InputSelectOffset")]
        public JoinDataComplete InputSelectOffset = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 10 }, 
            new JoinMetadata { Description = "Input Select", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Input Names Offset
        /// </summary>
        [JoinName("InputNamesOffset")]
        public JoinDataComplete InputNamesOffset = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 10 }, 
            new JoinMetadata { Description = "Input Names Offset", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// Input Select
        /// </summary>
        [JoinName("InputSelect")]
        public JoinDataComplete InputSelect = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata { Description = "Input Select", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// Button Visibility Offset
        /// </summary>
        [JoinName("ButtonVisibilityOffset")]
        public JoinDataComplete ButtonVisibilityOffset = new JoinDataComplete(new JoinData { JoinNumber = 41, JoinSpan = 10 }, 
            new JoinMetadata { Description = "Button Visibility Offset", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.DigitalSerial });
        
        /// <summary>
        /// Is Online
        /// </summary>
        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData { JoinNumber = 50, JoinSpan = 1 }, 
            new JoinMetadata { Description = "Is Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public DisplayControllerJoinMap(uint joinStart)
            : this(joinStart, typeof(DisplayControllerJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected DisplayControllerJoinMap(uint joinStart, Type type) : base(joinStart, type)
        {
        }
    }
}