using System;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Represents a GenericRelayControllerJoinMap
    /// </summary>
    public class GenericRelayControllerJoinMap : JoinMapBaseAdvanced
    {

        /// <summary>
        /// Device Relay State Set / Get
        /// </summary>
        [JoinName("Relay")]
        public JoinDataComplete Relay = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Device Relay State Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public GenericRelayControllerJoinMap(uint joinStart)
            : this(joinStart, typeof(GenericRelayControllerJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected GenericRelayControllerJoinMap(uint joinStart, Type type) : base(joinStart, type)
        {
            
        }
    }
}