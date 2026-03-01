using System;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Represents a DmRmcControllerJoinMap
    /// </summary>
    public class DmRmcControllerJoinMap : JoinMapBaseAdvanced
    {
        /// <summary>
        /// DM RMC Online
        /// </summary>
        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "DM RMC Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DM RMC Mute Video
        /// </summary>
        [JoinName("VideoMuteOn")]
        public JoinDataComplete VideoMuteOn = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata { Description = "DM RMC Mute Video", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DM RMC UnMute Video
        /// </summary>
        [JoinName("VideoMuteOff")]
        public JoinDataComplete VideoMuteOff = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata { Description = "DM RMC UnMute Video", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DM RMC Mute Video Toggle
        /// </summary>
        [JoinName("VideoMuteToggle")]
        public JoinDataComplete VideoMuteToggle = new JoinDataComplete(new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata { Description = "DM RMC Mute Video Toggle", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DM RMC Current Output Resolution
        /// </summary>
        [JoinName("CurrentOutputResolution")]
        public JoinDataComplete CurrentOutputResolution = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "DM RMC Current Output Resolution", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// DM RMC EDID Manufacturer
        /// </summary>
        [JoinName("EdidManufacturer")]
        public JoinDataComplete EdidManufacturer = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "DM RMC EDID Manufacturer", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// DM RMC EDID Name
        /// </summary>
        [JoinName("EdidName")]
        public JoinDataComplete EdidName = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata { Description = "DM RMC EDID Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// DM RMC EDID Preferred Timing
        /// </summary>
        [JoinName("EdidPrefferedTiming")]
        public JoinDataComplete EdidPrefferedTiming = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata { Description = "DM RMC EDID Preferred Timing", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// DM RMC EDID Serial Number
        /// </summary>
        [JoinName("EdidSerialNumber")]
        public JoinDataComplete EdidSerialNumber = new JoinDataComplete(new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata { Description = "DM RMC EDID Serial Number", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// DM RMC Name
        /// </summary>
        [JoinName("Name")]
        public JoinDataComplete Name = new JoinDataComplete(new JoinData { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata { Description = "DM RMC Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// DM RMC Audio Video Source Set / Get
        /// </summary>
        [JoinName("AudioVideoSource")]
        public JoinDataComplete AudioVideoSource = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "DM RMC Audio Video Source Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM RMC HDCP Support Capability
        /// </summary>
        [JoinName("HdcpSupportCapability")]
        public JoinDataComplete HdcpSupportCapability = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "DM RMC HDCP Support Capability", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM RMC Port 1 (DM) HDCP State Set / Get
        /// </summary>
        [JoinName("Port1HdcpState")]
        public JoinDataComplete Port1HdcpState = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata { Description = "DM RMC Port 1 (DM) HDCP State Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM RMC Port 2 (HDMI) HDCP State Set / Get
        /// </summary>
        [JoinName("Port2HdcpState")]
        public JoinDataComplete Port2HdcpState = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata { Description = "DM RMC Port 2 (HDMI) HDCP State Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM RMC HDMI Input Sync
        /// </summary>
        [JoinName("HdmiInputSync")]
        public JoinDataComplete HdmiInputSync = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "DM RMC HDMI Input Sync", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DM RMC Number of Input Ports that support HDCP
        /// </summary>
        [JoinName("HdcpInputPortCount")]
        public JoinDataComplete HdcpInputPortCount = new JoinDataComplete(new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata { Description = "Number of Input Ports that support HDCP", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });



        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public DmRmcControllerJoinMap(uint joinStart)
            : this(joinStart, typeof(DmRmcControllerJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected DmRmcControllerJoinMap(uint joinStart, Type type)
            : base(joinStart, type)
        {
        }
    }
}