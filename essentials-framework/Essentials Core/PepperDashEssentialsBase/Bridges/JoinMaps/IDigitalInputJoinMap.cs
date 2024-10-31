using System;

namespace PepperDash.Essentials.Core.Bridges
{
    public class IDigitalInputJoinMap : JoinMapBaseAdvanced
    {

        [JoinName("InputState")]
        public JoinDataComplete InputState = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Input State", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public IDigitalInputJoinMap(uint joinStart)
            : this(joinStart, typeof(IDigitalInputJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected IDigitalInputJoinMap(uint joinStart, Type type)
            : base(joinStart, type)
        {
        }
    }
}