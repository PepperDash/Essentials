using System;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Represents a PduJoinMapBase
    /// </summary>
    public class PduJoinMapBase : JoinMapBaseAdvanced
    {
        /// <summary>
        /// PDU Name
        /// </summary>
        [JoinName("Name")]
        public JoinDataComplete Name = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "PDU Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// PDU Online Status
        /// </summary>
        [JoinName("Online")]
        public JoinDataComplete Online = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Number of Controlled Outlets
        /// </summary>
        [JoinName("OutletCount")]
        public JoinDataComplete OutletCount = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Number of COntrolled Outlets", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        /// <summary>
        /// Outlet Name
        /// </summary>
        [JoinName("OutletName")]
        public JoinDataComplete OutletName = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata { Description = "Outlet Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Outlet Enabled Status
        /// </summary>
        [JoinName("OutletEnabled")]
        public JoinDataComplete OutletEnabled = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata { Description = "Outlet Enabled", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Outlet Power State
        /// </summary>
        [JoinName("OutletPowerCycle")]
        public JoinDataComplete OutletPowerCycle = new JoinDataComplete(new JoinData { JoinNumber = 12, JoinSpan = 1 },
            new JoinMetadata { Description = "Outlet Power Cycle", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Outlet Power On
        /// </summary>
        [JoinName("OutletPowerOn")]
        public JoinDataComplete OutletPowerOn = new JoinDataComplete(new JoinData { JoinNumber = 13, JoinSpan = 1 },
            new JoinMetadata { Description = "Outlet Power On", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Outlet Power Off
        /// </summary>
        [JoinName("OutletPowerOff")]
        public JoinDataComplete OutletPowerOff = new JoinDataComplete(new JoinData { JoinNumber = 14, JoinSpan = 1 },
            new JoinMetadata { Description = "Outlet Power Off", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        


        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public PduJoinMapBase(uint joinStart)
            :base(joinStart, typeof(PduJoinMapBase))
        {   
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        public PduJoinMapBase(uint joinStart, Type type)
            : base(joinStart, type)
        {            
        }
    }
}