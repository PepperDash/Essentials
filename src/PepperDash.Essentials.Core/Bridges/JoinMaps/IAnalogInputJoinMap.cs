using System;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Represents a IAnalogInputJoinMap
    /// </summary>
    public class IAnalogInputJoinMap : JoinMapBaseAdvanced
    {

        /// <summary>
        /// Input Value
        /// </summary>
        [JoinName("InputValue")]
        public JoinDataComplete InputValue = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Input Value", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// Minimum Change
        /// </summary>
        [JoinName("MinimumChange")]
        public JoinDataComplete MinimumChange = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "Minimum voltage change required to reflect a change", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public IAnalogInputJoinMap(uint joinStart)
            : this(joinStart, typeof(IAnalogInputJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected IAnalogInputJoinMap(uint joinStart, Type type)
            : base(joinStart, type)
        {
        }
    }
}