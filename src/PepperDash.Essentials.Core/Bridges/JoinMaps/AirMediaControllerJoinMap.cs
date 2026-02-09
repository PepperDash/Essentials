using System;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Represents a AirMediaControllerJoinMap
    /// </summary>
    public class AirMediaControllerJoinMap : JoinMapBaseAdvanced
    {
        /// <summary>
        /// Air Media Online status
        /// </summary>
        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Air Media Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Air Media In Sharing Session status
        /// </summary>
        [JoinName("IsInSession")]
        public JoinDataComplete IsInSession = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "Air Media In Sharing Session", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Air Media Has HDMI Video Sync status
        /// </summary>
        [JoinName("HdmiVideoSync")]
        public JoinDataComplete HdmiVideoSync = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata { Description = "Air Media Has HDMI Video Sync", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Air Media Automatic Input Routing Enable(d)
        /// </summary>
        [JoinName("AutomaticInputRoutingEnabled")]
        public JoinDataComplete AutomaticInputRoutingEnabled = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata { Description = "Air Media Automatic Input Routing Enable(d)", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Air Media Video Route Select / Feedback
        /// </summary>
        [JoinName("VideoOut")]
        public JoinDataComplete VideoOut = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Air Media Video Route Select / Feedback", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        /// <summary>
        /// Air Media Error Status Feedback
        /// </summary>
        [JoinName("ErrorFB")]
        public JoinDataComplete ErrorFB = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "Air Media Error Status", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        /// <summary>
        /// Air Media Number of Users Connected Feedback
        /// </summary>
        [JoinName("NumberOfUsersConnectedFB")]
        public JoinDataComplete NumberOfUsersConnectedFB = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata { Description = "Air Media Number of Users Connected", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        /// <summary>
        /// Air Media Login Code Set / Get
        /// </summary>
        [JoinName("LoginCode")]
        public JoinDataComplete LoginCode = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata { Description = "Air Media Login Code Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        /// <summary>
        /// Air Media Device Name
        /// </summary>
        [JoinName("Name")]
        public JoinDataComplete Name = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Air Media Device Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Air Media IP Address Feedback
        /// </summary>
        [JoinName("ConnectionAddressFB")]
        public JoinDataComplete ConnectionAddressFB = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "Air Media IP Address", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Air Media Hostname Feedback
        /// </summary>
        [JoinName("HostnameFB")]
        public JoinDataComplete HostnameFB = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata { Description = "Air Media Hostname", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Air Media Serial Number Feedback
        /// </summary>
        [JoinName("SerialNumberFeedback")]
        public JoinDataComplete SerialNumberFeedback = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata { Description = "Air Media Serial Number", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public AirMediaControllerJoinMap(uint joinStart)
            : this(joinStart, typeof(AirMediaControllerJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected AirMediaControllerJoinMap(uint joinStart, Type type) : base(joinStart, type){}
    }
}