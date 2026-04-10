using System;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Represents a C2nRthsControllerJoinMap
    /// </summary>
    public class C2nRthsControllerJoinMap : JoinMapBaseAdvanced
    {
        /// <summary>
        /// C2nRthsController Online status
        /// </summary>
        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Temp Sensor Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Temperature Format (C/F)
        /// </summary>
        [JoinName("TemperatureFormat")]
        public JoinDataComplete TemperatureFormat = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "Temp Sensor Unit Format", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Temperature Sensor Feedbacks
        /// </summary>
        [JoinName("Temperature")]
        public JoinDataComplete Temperature = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "Temp Sensor Temperature Feedback", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        /// <summary>
        /// Humidity Sensor Feedbacks
        /// </summary>
        [JoinName("Humidity")]
        public JoinDataComplete Humidity = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata { Description = "Temp Sensor Humidity Feedback", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        /// <summary>
        /// Temp Sensor Name
        /// </summary>
        [JoinName("Name")]
        public JoinDataComplete Name = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Temp Sensor Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public C2nRthsControllerJoinMap(uint joinStart)
            : this(joinStart, typeof(C2nRthsControllerJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected C2nRthsControllerJoinMap(uint joinStart, Type type) : base(joinStart, type)
        {
        }
    }
}