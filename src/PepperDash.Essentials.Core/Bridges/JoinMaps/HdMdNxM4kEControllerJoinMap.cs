using PepperDash.Essentials.Core.JoinMaps;
using System;

namespace PepperDash.Essentials.Core.Bridges.JoinMaps
{
    /// <summary>
    /// Represents a HdMdNxM4kEControllerJoinMap
    /// </summary>
    public class HdMdNxM4kEControllerJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("Name")]
        public JoinDataComplete Name = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Device Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("EnableAutoRoute")]
        public JoinDataComplete EnableAutoRoute = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Enable Automatic Routing on 4x1 Switchers", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("InputName")]
        public JoinDataComplete InputName = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 8 },
            new JoinMetadata { Description = "Device Input Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("InputSync")]
        public JoinDataComplete InputSync = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 8 },
            new JoinMetadata { Description = "Device Input Sync", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("OutputName")]
        public JoinDataComplete OutputName = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 2 },
            new JoinMetadata { Description = "Device Output Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("OutputRoute")]
        public JoinDataComplete OutputRoute = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 2 },
            new JoinMetadata { Description = "Device Output Route Set/Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("OutputRoutedName")]
        public JoinDataComplete OutputRoutedName = new JoinDataComplete(new JoinData { JoinNumber = 16, JoinSpan = 2 },
            new JoinMetadata { Description = "Device Output Route Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("EnableInputHdcp")]
        public JoinDataComplete EnableInputHdcp = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 8 },
            new JoinMetadata { Description = "Device Enable Input Hdcp", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DisableInputHdcp")]
        public JoinDataComplete DisableInputHdcp = new JoinDataComplete(new JoinData { JoinNumber = 21, JoinSpan = 8 },
            new JoinMetadata { Description = "Device Disnable Input Hdcp", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData { JoinNumber = 30, JoinSpan = 1 },
            new JoinMetadata { Description = "Device Onlne", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public HdMdNxM4kEControllerJoinMap(uint joinStart)
            : this(joinStart, typeof(HdMdNxM4kEControllerJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected HdMdNxM4kEControllerJoinMap(uint joinStart, Type type)
            : base(joinStart, type)
        {
        }
    }
}