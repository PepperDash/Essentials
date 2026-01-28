using System;

namespace PepperDash.Essentials.Core.Bridges {
    /// <summary>
    /// Represents a DmBladeChassisControllerJoinMap
    /// </summary>
    public class DmBladeChassisControllerJoinMap : JoinMapBaseAdvanced {

        /// <summary>
        /// DM Blade Chassis Online status
        /// </summary>
        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata { Description = "DM Blade Chassis Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DM Blade Input Video Sync
        /// </summary>
        [JoinName("VideoSyncStatus")]
        public JoinDataComplete VideoSyncStatus = new JoinDataComplete(new JoinData { JoinNumber = 101, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Input Video Sync", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DM Blade Chassis Input Endpoint Online
        /// </summary>
        [JoinName("InputEndpointOnline")]
        public JoinDataComplete InputEndpointOnline = new JoinDataComplete(new JoinData { JoinNumber = 501, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Chassis Input Endpoint Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DM Blade Chassis Output Endpoint Online
        /// </summary>
        [JoinName("OutputEndpointOnline")]
        public JoinDataComplete OutputEndpointOnline = new JoinDataComplete(new JoinData { JoinNumber = 701, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Chassis Output Endpoint Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DM Blade Chassis Tx Advanced Is Present
        /// </summary>
        [JoinName("TxAdvancedIsPresent")]
        public JoinDataComplete TxAdvancedIsPresent = new JoinDataComplete(new JoinData { JoinNumber = 1001, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Chassis Tx Advanced Is Present", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DM Blade Chassis Rx Advanced Is Present
        /// </summary>
        [JoinName("OutputVideo")]
        public JoinDataComplete OutputVideo = new JoinDataComplete(new JoinData { JoinNumber = 101, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Chassis Output Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM Blade Chassis Input HDCP Support State
        /// </summary>
        [JoinName("HdcpSupportState")]
        public JoinDataComplete HdcpSupportState = new JoinDataComplete(new JoinData { JoinNumber = 1001, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Chassis Input HDCP Support State", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM Blade Chassis Input HDCP Support Capability
        /// </summary>
        [JoinName("HdcpSupportCapability")]
        public JoinDataComplete HdcpSupportCapability = new JoinDataComplete(new JoinData { JoinNumber = 1201, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Chassis Input HDCP Support Capability", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM Blade Chassis Input Names
        /// </summary>
        [JoinName("InputNames")]
        public JoinDataComplete InputNames = new JoinDataComplete(new JoinData { JoinNumber = 101, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Chassis Input Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// DM Blade Chassis Output Names
        /// </summary>
        [JoinName("OutputNames")]
        public JoinDataComplete OutputNames = new JoinDataComplete(new JoinData { JoinNumber = 301, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Chassis Output Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// DM Blade Chassis Video Output Currently Routed Video Input Name
        /// </summary>
        [JoinName("OutputCurrentVideoInputNames")]
        public JoinDataComplete OutputCurrentVideoInputNames = new JoinDataComplete(new JoinData { JoinNumber = 2001, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Chassis Video Output Currently Routed Video Input Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// DM Blade Chassis Input Current Resolution
        /// </summary>
        [JoinName("InputCurrentResolution")]
        public JoinDataComplete InputCurrentResolution = new JoinDataComplete(new JoinData { JoinNumber = 2401, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Chassis Input Current Resolution", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });


        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public DmBladeChassisControllerJoinMap(uint joinStart)
            : this(joinStart, typeof(DmBladeChassisControllerJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected DmBladeChassisControllerJoinMap(uint joinStart, Type type) : base(joinStart, type)
        {
        }

    }
}
