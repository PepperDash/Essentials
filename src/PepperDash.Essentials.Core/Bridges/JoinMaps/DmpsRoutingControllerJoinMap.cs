using System;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Represents a DmpsRoutingControllerJoinMap
    /// </summary>
    public class DmpsRoutingControllerJoinMap : JoinMapBaseAdvanced
    {
        /// <summary>
        /// DMPS Enable Audio and Video Routing
        /// </summary>
        [JoinName("EnableRouting")]
        public JoinDataComplete EnableRouting = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "DMPS Enable Audio and Video Routing", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DMPS Disable Audio and Video Routing
        /// </summary>
        [JoinName("SystemPowerOn")]
        public JoinDataComplete SystemPowerOn = new JoinDataComplete(new JoinData { JoinNumber = 12, JoinSpan = 1 },
            new JoinMetadata { Description = "DMPS System Power On Get/Set", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DMPS Disable Audio and Video Routing
        /// </summary>
        [JoinName("SystemPowerOff")]
        public JoinDataComplete SystemPowerOff = new JoinDataComplete(new JoinData { JoinNumber = 13, JoinSpan = 1 },
            new JoinMetadata { Description = "DMPS System Power Off Get/Set", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DMPS Front Panel Lock On Get/Set
        /// </summary>
        [JoinName("FrontPanelLockOn")]
        public JoinDataComplete FrontPanelLockOn = new JoinDataComplete(new JoinData { JoinNumber = 14, JoinSpan = 1 },
            new JoinMetadata { Description = "DMPS Front Panel Lock On Get/Set", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DMPS Front Panel Lock  Off Get/Set
        /// </summary>
        [JoinName("FrontPanelLockOff")]
        public JoinDataComplete FrontPanelLockOff = new JoinDataComplete(new JoinData { JoinNumber = 15, JoinSpan = 1 },
            new JoinMetadata { Description = "DMPS Front Panel Lock  Off Get/Set", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DM Input Video Sync
        /// </summary>
        [JoinName("VideoSyncStatus")]
        public JoinDataComplete VideoSyncStatus = new JoinDataComplete(new JoinData { JoinNumber = 101, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Input Video Sync", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DM Chassis Input Endpoint Online
        /// </summary>
        [JoinName("InputEndpointOnline")]
        public JoinDataComplete InputEndpointOnline = new JoinDataComplete(new JoinData { JoinNumber = 501, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Input Endpoint Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DM Chassis Output Endpoint Online
        /// </summary>
        [JoinName("OutputEndpointOnline")]
        public JoinDataComplete OutputEndpointOnline = new JoinDataComplete(new JoinData { JoinNumber = 701, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Output Endpoint Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DM Chassis Input Video Set / Get
        /// </summary>
        [JoinName("OutputVideo")]
        public JoinDataComplete OutputVideo = new JoinDataComplete(new JoinData { JoinNumber = 101, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Output Video Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM Chassis Input Audio Set / Get
        /// </summary>
        [JoinName("OutputAudio")]
        public JoinDataComplete OutputAudio = new JoinDataComplete(new JoinData { JoinNumber = 301, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Output Audio Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM Chassis Input Name
        /// </summary>
        [JoinName("InputNames")]
        public JoinDataComplete InputNames = new JoinDataComplete(new JoinData { JoinNumber = 101, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Input Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// DM Chassis Output Name
        /// </summary>
        [JoinName("OutputNames")]
        public JoinDataComplete OutputNames = new JoinDataComplete(new JoinData { JoinNumber = 301, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Output Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// DM Chassis Video Input Name
        /// </summary>
        [JoinName("InputVideoNames")]
        public JoinDataComplete InputVideoNames =
            new JoinDataComplete(new JoinData { JoinNumber = 501, JoinSpan = 32 },
            new JoinMetadata
            {
                Description = "DM Chassis Video Input Name",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });
       
        /// <summary>
        /// DM Chassis Audio Input Name
        /// </summary>
        [JoinName("InputAudioNames")]
        public JoinDataComplete InputAudioNames =
            new JoinDataComplete(new JoinData { JoinNumber = 701, JoinSpan = 32 },
            new JoinMetadata
            {
                Description = "Audio Input Name",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });
       
        /// <summary>
        /// DM Chassis Video Output Name
        /// </summary>
        [JoinName("OutputVideoNames")]
        public JoinDataComplete OutputVideoNames =
            new JoinDataComplete(new JoinData { JoinNumber = 901, JoinSpan = 32 },
            new JoinMetadata
            {
                Description = "Video Output Name",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });
       
        /// <summary>
        /// DM Chassis Audio Output Name
        /// </summary>    
        [JoinName("OutputAudioNames")]
        public JoinDataComplete OutputAudioNames =
            new JoinDataComplete(new JoinData { JoinNumber = 1101, JoinSpan = 32 },
            new JoinMetadata
            {
                Description = "Audio Output Name",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });
       
        /// <summary>
        /// DM Chassis Video Output Currently Routed Video Input Name
        /// </summary>
        [JoinName("OutputCurrentVideoInputNames")]
        public JoinDataComplete OutputCurrentVideoInputNames = new JoinDataComplete(new JoinData { JoinNumber = 2001, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Video Output Currently Routed Video Input Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
       
        /// <summary>
        /// DM Chassis Audio Output Currently Routed Video Input Name
        /// </summary>
        [JoinName("OutputCurrentAudioInputNames")]
        public JoinDataComplete OutputCurrentAudioInputNames = new JoinDataComplete(new JoinData { JoinNumber = 2201, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Audio Output Currently Routed Video Input Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
       
        /// <summary>
        /// DM Chassis Input Current Resolution
        /// </summary>
        [JoinName("InputCurrentResolution")]
        public JoinDataComplete InputCurrentResolution = new JoinDataComplete(new JoinData { JoinNumber = 2401, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Input Current Resolution", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public DmpsRoutingControllerJoinMap(uint joinStart)
            : this(joinStart, typeof(DmpsRoutingControllerJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected DmpsRoutingControllerJoinMap(uint joinStart, Type type) : base(joinStart, type)
        {
        }
    }
}