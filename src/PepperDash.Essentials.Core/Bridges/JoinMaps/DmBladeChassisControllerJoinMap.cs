using PepperDash.Essentials.Core.JoinMaps;
using System;

namespace PepperDash.Essentials.Core.Bridges.JoinMaps
{
    /// <summary>
    /// Represents a DmBladeChassisControllerJoinMap
    /// </summary>
    public class DmBladeChassisControllerJoinMap : JoinMapBaseAdvanced {

        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata { Description = "DM Blade Chassis Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("VideoSyncStatus")]
        public JoinDataComplete VideoSyncStatus = new JoinDataComplete(new JoinData { JoinNumber = 101, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Input Video Sync", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("InputEndpointOnline")]
        public JoinDataComplete InputEndpointOnline = new JoinDataComplete(new JoinData { JoinNumber = 501, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Chassis Input Endpoint Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("OutputEndpointOnline")]
        public JoinDataComplete OutputEndpointOnline = new JoinDataComplete(new JoinData { JoinNumber = 701, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Chassis Output Endpoint Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("TxAdvancedIsPresent")]
        public JoinDataComplete TxAdvancedIsPresent = new JoinDataComplete(new JoinData { JoinNumber = 1001, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Chassis Tx Advanced Is Present", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("OutputVideo")]
        public JoinDataComplete OutputVideo = new JoinDataComplete(new JoinData { JoinNumber = 101, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Chassis Output Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("HdcpSupportState")]
        public JoinDataComplete HdcpSupportState = new JoinDataComplete(new JoinData { JoinNumber = 1001, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Chassis Input HDCP Support State", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        [JoinName("HdcpSupportCapability")]
        public JoinDataComplete HdcpSupportCapability = new JoinDataComplete(new JoinData { JoinNumber = 1201, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Chassis Input HDCP Support Capability", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("InputNames")]
        public JoinDataComplete InputNames = new JoinDataComplete(new JoinData { JoinNumber = 101, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Chassis Input Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("OutputNames")]
        public JoinDataComplete OutputNames = new JoinDataComplete(new JoinData { JoinNumber = 301, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Chassis Output Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("OutputCurrentVideoInputNames")]
        public JoinDataComplete OutputCurrentVideoInputNames = new JoinDataComplete(new JoinData { JoinNumber = 2001, JoinSpan = 128 },
            new JoinMetadata { Description = "DM Blade Chassis Video Output Currently Routed Video Input Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

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
