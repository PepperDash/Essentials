using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash_Essentials_Core.Bridges.JoinMaps
{
	public class GlsPartitionSensorJoinMap : JoinMapBaseAdvanced
	{
		[JoinName("IsOnline")]
		public JoinDataComplete IsOnline = new JoinDataComplete(
			new JoinData()
			{
				JoinNumber = 1,
				JoinSpan = 1
			},
			new JoinMetadata()
			{
				Description = "Sensor Is Online",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		[JoinName("Name")]
		public JoinDataComplete Name = new JoinDataComplete(
			new JoinData()
			{
				JoinNumber = 1,
				JoinSpan = 1
			},
			new JoinMetadata()
			{
				Description = "Sensor Name",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		[JoinName("Enable")]
		public JoinDataComplete Enable = new JoinDataComplete(
			new JoinData()
			{
				JoinNumber = 2,
				JoinSpan = 1
			},
			new JoinMetadata()
			{
				Description = "Sensor Enable",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		[JoinName("PartitionSensed")]
		public JoinDataComplete PartitionSensed = new JoinDataComplete(
			new JoinData()
			{
				JoinNumber = 3,
				JoinSpan = 1
			},
			new JoinMetadata()
			{
				Description = "Sensor Partition Sensed",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		[JoinName("PartitionNotSensed")]
		public JoinDataComplete PartitionNotSensed = new JoinDataComplete(
			new JoinData()
			{
				JoinNumber = 4,
				JoinSpan = 1
			},
			new JoinMetadata()
			{
				Description = "Sensor Partition Not Sensed",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		[JoinName("IncreaseSensitivity")]
		public JoinDataComplete IncreaseSensitivity = new JoinDataComplete(
			new JoinData()
			{
				JoinNumber = 6,
				JoinSpan = 1
			},
			new JoinMetadata()
			{
				Description = "Sensor Increase Sensitivity",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		[JoinName("DecreaseSensitivity")]
		public JoinDataComplete DecreaseSensitivity = new JoinDataComplete(
			new JoinData()
			{
				JoinNumber = 7,
				JoinSpan = 1
			},
			new JoinMetadata()
			{
				Description = "Sensor Decrease Sensitivity",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		[JoinName("Sensitivity")]
		public JoinDataComplete Sensitivity = new JoinDataComplete(
			new JoinData()
			{
				JoinNumber = 2,
				JoinSpan = 1
			},
			new JoinMetadata()
			{
				Description = "Sensor Sensitivity",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Analog
			});

		internal GlsPartitionSensorJoinMap(uint joinStart)
			: base(joinStart, typeof (GlsPartitionSensorJoinMap))
		{

		}

        public GlsPartitionSensorJoinMap(uint joinStart, Type type)
            : base(joinStart, type)
        {

        }
	}
}