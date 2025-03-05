using System;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Bridges
{
	public class HdPsXxxControllerJoinMap : JoinMapBaseAdvanced
	{

		#region Digital

		[JoinName("EnableAutoRoute")]
		public JoinDataComplete EnableAutoRoute = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 1,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Enable Automatic Routing on Xx1 Switchers",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});


		[JoinName("InputSync")]
		public JoinDataComplete InputSync = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 2,
				JoinSpan = 8
			},
			new JoinMetadata
			{
				Description = "Device Input Sync",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});


		[JoinName("EnableInputHdcp")]
		public JoinDataComplete EnableInputHdcp = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 11,
				JoinSpan = 8
			},
			new JoinMetadata
			{
				Description = "Device Enable Input Hdcp",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});


		[JoinName("DisableInputHdcp")]
		public JoinDataComplete DisableInputHdcp = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 21,
				JoinSpan = 8
			},
			new JoinMetadata
			{
				Description = "Device Disnable Input Hdcp",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});


		[JoinName("IsOnline")]
		public JoinDataComplete IsOnline = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 30,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Device Onlne",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		#endregion


		#region Analog

		[JoinName("OutputRoute")]
		public JoinDataComplete OutputRoute = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 11,
				JoinSpan = 2
			},
			new JoinMetadata
			{
				Description = "Device Output Route Set/Get",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Analog
			});

		#endregion


		#region Serial

		[JoinName("Name")]
		public JoinDataComplete Name = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 1,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Device Name",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});


		[JoinName("InputName")]
		public JoinDataComplete InputName = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 2,
				JoinSpan = 8
			},
			new JoinMetadata
			{
				Description = "Device Input Name",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});


		[JoinName("OutputName")]
		public JoinDataComplete OutputName = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 11,
				JoinSpan = 2
			},
			new JoinMetadata
			{
				Description = "Device Output Name",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});


		[JoinName("OutputRoutedName")]
		public JoinDataComplete OutputRoutedName = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 16,
				JoinSpan = 2
			},
			new JoinMetadata
			{
				Description = "Device Output Route Name",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});


		#endregion

		/// <summary>
		/// Constructor to use when instantiating this join map without inheriting from it
		/// </summary>
		/// <param name="joinStart">Join this join map will start at</param>
		public HdPsXxxControllerJoinMap(uint joinStart)
			: this(joinStart, typeof(HdPsXxxControllerJoinMap))
		{
		}

		/// <summary>
		/// Constructor to use when extending this Join map
		/// </summary>
		/// <param name="joinStart">Join this join map will start at</param>
		/// <param name="type">Type of the child join map</param>
		protected HdPsXxxControllerJoinMap(uint joinStart, Type type)
			: base(joinStart, type)
		{
		}
	}
}