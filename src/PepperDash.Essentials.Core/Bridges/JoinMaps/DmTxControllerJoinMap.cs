using System;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Represents a DmTxControllerJoinMap
    /// </summary>
    public class DmTxControllerJoinMap : JoinMapBaseAdvanced
    {
        /// <summary>
        /// DM TX Online
        /// </summary>
        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "DM TX Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DM TX Video Sync
        /// </summary>
        [JoinName("VideoSyncStatus")]
        public JoinDataComplete VideoSyncStatus = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "DM TX Video Sync", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DM TX Enable Free Run Set / Get
        /// </summary>
        [JoinName("FreeRunEnabled")]
        public JoinDataComplete FreeRunEnabled = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata { Description = "DM TX Enable Free Run Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Input 1 Video Sync Status
        /// </summary>
        [JoinName("Input1VideoSyncStatus")]
        public JoinDataComplete Input1VideoSyncStatus = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata { Description = "Input 1 Video Sync Status", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Input 2 Video Sync Status
        /// </summary>
        [JoinName("Input2VideoSyncStatus")]
        public JoinDataComplete Input2VideoSyncStatus = new JoinDataComplete(new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata { Description = "Input 2 Video Sync Status", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Input 3 Video Sync Status
        /// </summary>
        [JoinName("Input3VideoSyncStatus")]
        public JoinDataComplete Input3VideoSyncStatus = new JoinDataComplete(new JoinData { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata { Description = "Input 3 Video Sync Status", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DM TX Current Input Resolution
        /// </summary>
        [JoinName("CurrentInputResolution")]
        public JoinDataComplete CurrentInputResolution = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "DM TX Current Input Resolution", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// DM TX Name
        /// </summary>
        [JoinName("Name")]
        public JoinDataComplete Name = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "DM TX Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// DM TX Video Input Set / Get
        /// </summary>
        [JoinName("VideoInput")]
        public JoinDataComplete VideoInput = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "DM TX Video Input Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM TX Audio Input Set / Get
        /// </summary>
        [JoinName("AudioInput")]
        public JoinDataComplete AudioInput = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "DM TX Audio Input Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM TX HDCP Support Capability
        /// </summary>
        [JoinName("HdcpSupportCapability")]
        public JoinDataComplete HdcpSupportCapability = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata { Description = "DM TX HDCP Support Capability", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM TX Port 1 HDCP State Set / Get
        /// </summary>
        [JoinName("Port1HdcpState")]
        public JoinDataComplete Port1HdcpState = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata { Description = "DM TX Port 1 HDCP State Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM TX Port 2 HDCP State Set / Get
        /// </summary>
        [JoinName("Port2HdcpState")]
        public JoinDataComplete Port2HdcpState = new JoinDataComplete(new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata { Description = "DM TX Port 2 HDCP State Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM TX VGA Brightness
        /// </summary>
        [JoinName("VgaBrightness")]
        public JoinDataComplete VgaBrightness = new JoinDataComplete(new JoinData { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata { Description = "DM TX VGA Brightness", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM TX VGA Contrast
        /// </summary>
        [JoinName("VgaContrast")]
        public JoinDataComplete VgaContrast = new JoinDataComplete(new JoinData { JoinNumber = 7, JoinSpan = 1 },
            new JoinMetadata { Description = "DM TX Online", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM TX Port 3 HDCP State Set / Get
        /// </summary>
        [JoinName("Port3HdcpState")]
        public JoinDataComplete Port3HdcpState = new JoinDataComplete(new JoinData { JoinNumber = 8, JoinSpan = 1 },
            new JoinMetadata { Description = "DM TX Port 3 HDCP State Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM TX Number of Input Ports that support HDCP
        /// </summary>
        [JoinName("HdcpInputPortCount")]
        public JoinDataComplete HdcpInputPortCount = new JoinDataComplete(new JoinData { JoinNumber = 9, JoinSpan = 1 },
            new JoinMetadata { Description = "Number of Input Ports that support HDCP", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });



        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public DmTxControllerJoinMap(uint joinStart)
            : this(joinStart, typeof(DmTxControllerJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected DmTxControllerJoinMap(uint joinStart, Type type)
            : base(joinStart, type)
        {
        }
    }
}