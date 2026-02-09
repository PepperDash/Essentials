using System;


namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Represents a GenericLightingJoinMap
    /// </summary>
    public class GenericLightingJoinMap : JoinMapBaseAdvanced
    {

        /// <summary>
        /// Lighting Controller Online
        /// </summary>
        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Lighting Controller Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Select Scene By Index
        /// </summary>
        [JoinName("SelectScene")]
        public JoinDataComplete SelectScene = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Lighting Controller Select Scene By Index", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Select Scene Direct
        /// </summary>
        [JoinName("SelectSceneDirect")]
        public JoinDataComplete SelectSceneDirect = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 10 },
            new JoinMetadata { Description = "Lighting Controller Select Scene", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.DigitalSerial });
        
        /// <summary>
        /// Button Visibility
        /// </summary>
        [JoinName("ButtonVisibility")]
        public JoinDataComplete ButtonVisibility = new JoinDataComplete(new JoinData { JoinNumber = 41, JoinSpan = 10 },
            new JoinMetadata { Description = "Lighting Controller Button Visibility", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Set Integration Id
        /// </summary>
        [JoinName("IntegrationIdSet")]
        public JoinDataComplete IntegrationIdSet = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Lighting Controller Set Integration Id", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });



        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public GenericLightingJoinMap(uint joinStart)
            : this(joinStart, typeof(GenericLightingJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected GenericLightingJoinMap(uint joinStart, Type type) : base(joinStart, type)
        {
        }
    }
}