using System;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Represents a IBasicCommunicationJoinMap
    /// </summary>
    public class IBasicCommunicationJoinMap : JoinMapBaseAdvanced
    {
        /// <summary>
        /// Text Received From Remote Device
        /// </summary>
        [JoinName("TextReceived")]
        public JoinDataComplete TextReceived = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Text Received From Remote Device", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Text Sent To Remote Device
        /// </summary>
        [JoinName("SendText")]
        public JoinDataComplete SendText = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Text Sent To Remote Device", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Set Port Config
        /// </summary>
        [JoinName("SetPortConfig")]
        public JoinDataComplete SetPortConfig = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "Set Port Config", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Connect
        /// </summary>
        [JoinName("Connect")]
        public JoinDataComplete Connect = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Connect", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Disconnect
        /// </summary>
        [JoinName("Connected")]
        public JoinDataComplete Connected = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Connected", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Status
        /// </summary>
        [JoinName("Status")]
        public JoinDataComplete Status = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Status", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public IBasicCommunicationJoinMap(uint joinStart)
            : this(joinStart, typeof(IBasicCommunicationJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected IBasicCommunicationJoinMap(uint joinStart, Type type)
            : base(joinStart, type)
        {
        }
    }
}