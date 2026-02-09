using System;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Represents a DmChassisControllerJoinMap
    /// </summary>
    public class DmChassisControllerJoinMap : JoinMapBaseAdvanced
    {
        /// <summary>
        /// DM Chassis enable audio breakaway routing
        /// </summary>
        [JoinName("EnableAudioBreakaway")]
        public JoinDataComplete EnableAudioBreakaway = new JoinDataComplete(
            new JoinData {JoinNumber = 4, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "DM Chassis enable audio breakaway routing",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        
        /// <summary>
        /// DM Chassis enable USB breakaway routing
        /// </summary>
        [JoinName("EnableUsbBreakaway")]
        public JoinDataComplete EnableUsbBreakaway = new JoinDataComplete(
            new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "DM Chassis enable USB breakaway routing",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        
        /// <summary>
        /// DM Chassis Name
        /// </summary>
        [JoinName("Name")]
        public JoinDataComplete Name = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "DM Chassis Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// DM Chassis SystemId Get/Set/Trigger
        /// </summary>
        [JoinName("SystemId")]
        public JoinDataComplete SystemId = new JoinDataComplete(new JoinData { JoinNumber = 10, JoinSpan = 1 },
            new JoinMetadata { Description = "DM Chassis SystemId Get/Set/Trigger/", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.DigitalAnalog });
        
        /// <summary>
        /// DM Chassis Online status
        /// </summary>
        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata { Description = "DM Chassis Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
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
        /// DM Chassis Tx Advanced Is Present
        /// </summary>
        [JoinName("TxAdvancedIsPresent")]
        public JoinDataComplete TxAdvancedIsPresent = new JoinDataComplete(new JoinData { JoinNumber = 1001, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Tx Advanced Is Present", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DM Chassis Rx Advanced Is Present
        /// </summary>
        [JoinName("OutputDisabledByHdcp")]
        public JoinDataComplete OutputDisabledByHdcp = new JoinDataComplete(new JoinData { JoinNumber = 1201, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Output Disabled by HDCP", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// DM Chassis Output Set / Get
        /// </summary>
        [JoinName("OutputVideo")]
        public JoinDataComplete OutputVideo = new JoinDataComplete(new JoinData { JoinNumber = 101, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Output Video Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM Chassis Output Audio Set / Get
        /// </summary>
        [JoinName("OutputAudio")]
        public JoinDataComplete OutputAudio = new JoinDataComplete(new JoinData { JoinNumber = 301, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Output Audio Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM Chassis Input Set / Get
        /// </summary>
        [JoinName("OutputUsb")]
        public JoinDataComplete OutputUsb = new JoinDataComplete(new JoinData { JoinNumber = 501, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Output USB Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM Chassis Input Set / Get
        /// </summary>
        [JoinName("InputUsb")]
        public JoinDataComplete InputUsb = new JoinDataComplete(new JoinData { JoinNumber = 701, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Input Usb Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM Chassis Input HDCP Support State
        /// </summary>
        [JoinName("HdcpSupportState")]
        public JoinDataComplete HdcpSupportState = new JoinDataComplete(new JoinData { JoinNumber = 1001, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Input HDCP Support State", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM Chassis Input HDCP Support Capability
        /// </summary>
        [JoinName("HdcpSupportCapability")]
        public JoinDataComplete HdcpSupportCapability = new JoinDataComplete(new JoinData { JoinNumber = 1201, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Input HDCP Support Capability", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM Chassis Stream Input Start (1), Stop (2), Pause (3) with Feedback
        /// </summary>
        [JoinName("InputStreamCardState")]
        public JoinDataComplete InputStreamCardState = new JoinDataComplete(new JoinData { JoinNumber = 1501, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Stream Input Start (1), Stop (2), Pause (3) with Feedback", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM Chassis Stream Output Start (1), Stop (2), Pause (3) with Feedback
        /// </summary>
        [JoinName("OutputStreamCardState")]
        public JoinDataComplete OutputStreamCardState = new JoinDataComplete(new JoinData { JoinNumber = 1601, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Stream Output Start (1), Stop (2), Pause (3) with Feedback", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// DM Chassis No Route Name
        /// </summary>
        [JoinName("NoRouteName")]
        public JoinDataComplete NoRouteName = new JoinDataComplete(new JoinData { JoinNumber = 100, JoinSpan = 1 },
            new JoinMetadata { Description = "DM Chassis Input Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// DM Chassis Input Names
        /// </summary>
        [JoinName("InputNames")]
        public JoinDataComplete InputNames = new JoinDataComplete(new JoinData { JoinNumber = 101, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Input Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// DM Chassis Output Names
        /// </summary>
        [JoinName("OutputNames")]
        public JoinDataComplete OutputNames = new JoinDataComplete(new JoinData { JoinNumber = 301, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Output Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// DM Chassis Video Input Names
        /// </summary>
        [JoinName("InputVideoNames")] public JoinDataComplete InputVideoNames =
            new JoinDataComplete(new JoinData {JoinNumber = 501, JoinSpan = 200},
                new JoinMetadata
                {
                    Description = "DM Chassis Video Input Names",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Serial
                });
        
        /// <summary>
        /// DM Chassis Audio Input Names
        /// </summary>
        [JoinName("InputAudioNames")]
        public JoinDataComplete InputAudioNames =
            new JoinDataComplete(new JoinData { JoinNumber = 701, JoinSpan = 200 },
            new JoinMetadata
            {
                Description = "DM Chassis Audio Input Names",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Serial
            });
        
        /// <summary>
        /// DM Chassis Video Output Names
        /// </summary>
        [JoinName("OutputVideoNames")]
        public JoinDataComplete OutputVideoNames =
            new JoinDataComplete(new JoinData { JoinNumber = 901, JoinSpan = 200 },
            new JoinMetadata
            {
                Description = "DM Chassis Video Output Names",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Serial
            });
        
        /// <summary>
        /// DM Chassis Audio Output Names
        /// </summary>
        [JoinName("OutputAudioNames")]
        public JoinDataComplete OutputAudioNames =
            new JoinDataComplete(new JoinData { JoinNumber = 1101, JoinSpan = 200 },
            new JoinMetadata
            {
                Description = "DM Chassis Audio Output Names",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Serial
            });
        
        /// <summary>
        /// DM Chassis Video Output Currently Routed Video Input Name
        /// </summary>
        [JoinName("OutputCurrentVideoInputNames")]
        public JoinDataComplete OutputCurrentVideoInputNames = new JoinDataComplete(new JoinData { JoinNumber = 2001, JoinSpan = 32 },
            new JoinMetadata { Description = "DM Chassis Video Output Currently Routed Video Input Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// DM Chassis Audio Output Currently Routed Audio Input Name
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
        public DmChassisControllerJoinMap(uint joinStart)
            : this(joinStart, typeof(DmChassisControllerJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected DmChassisControllerJoinMap(uint joinStart, Type type) : base(joinStart, type)
        {
        }
    }
}
