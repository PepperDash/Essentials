using System;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Bridges.JoinMaps
{
    public class GlsPartitionSensorJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 1,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Sensor Is Online",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("Name")]
        public JoinDataComplete Name = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 1,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Sensor Name",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("Enable")]
        public JoinDataComplete Enable = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 2,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Sensor Enable",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("PartitionSensed")]
        public JoinDataComplete PartitionSensed = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 3,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Sensor Partition Sensed",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("PartitionNotSensed")]
        public JoinDataComplete PartitionNotSensed = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 4,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Sensor Partition Not Sensed",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("IncreaseSensitivity")]
        public JoinDataComplete IncreaseSensitivity = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 6,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Sensor Increase Sensitivity",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("DecreaseSensitivity")]
        public JoinDataComplete DecreaseSensitivity = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 7,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Sensor Decrease Sensitivity",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("Sensitivity")]
        public JoinDataComplete Sensitivity = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 2,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Sensor Sensitivity",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Analog
            });

        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public GlsPartitionSensorJoinMap(uint joinStart)
            : this(joinStart, typeof(GlsPartitionSensorJoinMap))
        {

        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected GlsPartitionSensorJoinMap(uint joinStart, Type type)
            : base(joinStart, type)
        {

        }
    }
}

namespace PepperDash_Essentials_Core.Bridges.JoinMaps
{
    /// <summary>
    /// 
    /// </summary>
    [Obsolete("use PepperDash.Essentials.Core.Bridges.JoinMaps version")]
	public class GlsPartitionSensorJoinMap : PepperDash.Essentials.Core.Bridges.JoinMaps.GlsPartitionSensorJoinMap
	{
        public GlsPartitionSensorJoinMap(uint joinStart)
            : this(joinStart, typeof(GlsPartitionSensorJoinMap))
        {

        }

        protected GlsPartitionSensorJoinMap(uint joinStart, Type type) : base(joinStart, type)
        {
        }
	}
}