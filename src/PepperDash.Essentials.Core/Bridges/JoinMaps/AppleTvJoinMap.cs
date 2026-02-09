using System;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Represents a AppleTvJoinMap
    /// </summary>
    public class AppleTvJoinMap : JoinMapBaseAdvanced
    {
        /// <summary>
        /// AppleTv Nav Up
        /// </summary>
        [JoinName("UpArrow")]
        public JoinDataComplete UpArrow = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "AppleTv Nav Up", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// AppleTv Nav Down
        /// </summary>
        [JoinName("DnArrow")]
        public JoinDataComplete DnArrow = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "AppleTv Nav Down", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// AppleTv Nav Left
        /// </summary>
        [JoinName("LeftArrow")]
        public JoinDataComplete LeftArrow = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata { Description = "AppleTv Nav Left", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// AppleTv Nav Right
        /// </summary>
        [JoinName("RightArrow")]
        public JoinDataComplete RightArrow = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata { Description = "AppleTv Nav Right", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// AppleTv Menu
        /// </summary>
        [JoinName("Menu")]
        public JoinDataComplete Menu = new JoinDataComplete(new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata { Description = "AppleTv Menu", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// AppleTv Select
        /// </summary>
        [JoinName("Select")]
        public JoinDataComplete Select = new JoinDataComplete(new JoinData { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata { Description = "AppleTv Select", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// AppleTv Play/Pause
        /// </summary>
        [JoinName("PlayPause")]
        public JoinDataComplete PlayPause = new JoinDataComplete(new JoinData { JoinNumber = 7, JoinSpan = 1 },
            new JoinMetadata { Description = "AppleTv Play/Pause", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public AppleTvJoinMap(uint joinStart)
            : base(joinStart, typeof(AppleTvJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        public AppleTvJoinMap(uint joinStart, Type type) : base(joinStart, type)
        {
        }
    }
}