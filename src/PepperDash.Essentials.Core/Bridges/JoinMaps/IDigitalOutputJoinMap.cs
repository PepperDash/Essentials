using System;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Represents a IDigitalOutputJoinMap
    /// </summary>
    public class IDigitalOutputJoinMap : JoinMapBaseAdvanced
    {
        /// <summary>
        /// Output State
        /// </summary>
        [JoinName("OutputState")]
        public JoinDataComplete OutputState = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Get / Set state of Digital Input", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public IDigitalOutputJoinMap(uint joinStart)
            : this(joinStart, typeof(IDigitalOutputJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected IDigitalOutputJoinMap(uint joinStart, Type type)
            : base(joinStart, type)
        {
        }
    }
}