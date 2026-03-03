using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Bridges.JoinMaps
{
		/// <summary>
		/// Represents a GenericIrControllerJoinMap
		/// </summary>
		public sealed class GenericIrControllerJoinMap : JoinMapBaseAdvanced
		{
			/// <summary>
			/// PLAY
			/// </summary>
			[JoinName("PLAY")]
			public JoinDataComplete Play = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 1,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "PLAY",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// STOP
			/// </summary>
			[JoinName("STOP")]
			public JoinDataComplete Stop = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 2,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "STOP",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// PAUSE
			/// </summary>
			[JoinName("PAUSE")]
			public JoinDataComplete Pause = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 3,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "PAUSE",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			/// <summary>
			/// FSCAN
			/// </summary>
			[JoinName("FSCAN")]
			public JoinDataComplete ForwardScan = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 4,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "FSCAN",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// RSCAN
			/// </summary>
			[JoinName("RSCAN")]
			public JoinDataComplete ReverseScan = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 5,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "RSCAN",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// F_SKIP
			/// </summary>
			[JoinName("F_SKIP")]
			public JoinDataComplete ForwardSkip = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 6,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "F_SKIP",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// R_SKIP
			/// </summary>
			[JoinName("R_SKIP")]
			public JoinDataComplete ReverseSkip = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 7,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "R_SKIP",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// RECORD
			/// </summary>
			[JoinName("RECORD")]
			public JoinDataComplete Record = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 8,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "RECORD",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// POWER
			/// </summary>
			[JoinName("POWER")]
			public JoinDataComplete Power = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 9,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "POWER",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// 0
			/// </summary>
			[JoinName("0")]
			public JoinDataComplete Kp0 = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 10,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "0",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// 1
			/// </summary>
			[JoinName("1")]
			public JoinDataComplete Kp1 = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 11,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "1",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// 2
			/// </summary>
			[JoinName("2")]
			public JoinDataComplete Kp2 = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 12,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "2",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// 3
			/// </summary>
			[JoinName("3")]
			public JoinDataComplete Kp3 = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 13,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "3",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// 4
			/// </summary>
			[JoinName("4")]
			public JoinDataComplete Kp4 = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 14,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "4",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// 5
			/// </summary>
			[JoinName("5")]
			public JoinDataComplete Kp5 = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 15,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "5",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// 6
			/// </summary>
			[JoinName("6")]
			public JoinDataComplete Kp6 = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 16,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "6",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// 7
			/// </summary>
			[JoinName("7")]
			public JoinDataComplete Kp7 = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 17,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "7",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// 8
			/// </summary>
			[JoinName("8")]
			public JoinDataComplete Kp8 = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 18,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "8",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// 9
			/// </summary>
			[JoinName("9")]
			public JoinDataComplete Kp9 = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 19,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "9",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			// [JoinName("+10")]
			// public JoinDataComplete Kp9 = new JoinDataComplete(
			// 	new JoinData
			// 	{
			// 		JoinNumber = 20,
			// 		JoinSpan = 1
			// 	},
			// 	new JoinMetadata
			// 	{
			// 		Description = "+10",
			// 		JoinCapabilities = eJoinCapabilities.FromSIMPL,
			// 		JoinType = eJoinType.Digital
			// 	});

			/// <summary>
			/// ENTER
			/// </summary>
			[JoinName("ENTER")]
			public JoinDataComplete Enter = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 21,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "ENTER",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// CH+
			/// </summary>
			[JoinName("CH+")]
			public JoinDataComplete ChannelUp = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 22,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "CH+",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// CH-
			/// </summary>
			[JoinName("CH-")]
			public JoinDataComplete ChannelDown = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 23,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "CH-",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			/// <summary>
			/// *
			/// </summary>
			[JoinName("*")]
			public JoinDataComplete KpStar = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 24,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "*",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// #
			/// </summary>
			[JoinName("#")]
			public JoinDataComplete KpPound = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 25,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "#",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			// [JoinName(".")]
			// public JoinDataComplete KpPound = new JoinDataComplete(
			// 	new JoinData
			// 	{
			// 		JoinNumber = 26,
			// 		JoinSpan = 1
			// 	},
			// 	new JoinMetadata
			// 	{
			// 		Description = ".",
			// 		JoinCapabilities = eJoinCapabilities.FromSIMPL,
			// 		JoinType = eJoinType.Digital
			// 	});

			/// <summary>
			/// POWER_ON
			/// </summary>
			[JoinName("POWER_ON")]
			public JoinDataComplete PowerOn = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 27,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "POWER_ON",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// POWER_OFF
			/// </summary>
			[JoinName("POWER_OFF")]
			public JoinDataComplete PowerOff = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 28,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "POWER_OFF",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// PLAY_PAUSE
			/// </summary>
			[JoinName("PLAY_PAUSE")]
			public JoinDataComplete PlayPause = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 29,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "PLAY_PAUSE",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// LAST
			/// </summary>
			[JoinName("LAST")]
			public JoinDataComplete Last = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 30,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "LAST",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// HOME
			/// </summary>
			[JoinName("HOME")]
			public JoinDataComplete Home = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 40,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "HOME",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// BACK
			/// </summary>
			[JoinName("BACK")]
			public JoinDataComplete Back = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 41,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "BACK",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			/// <summary>
			/// GUIDE
			/// </summary>
			[JoinName("GUIDE")]
			public JoinDataComplete Guide = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 42,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "GUIDE",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// INFO
			/// </summary>
			[JoinName("INFO")]
			public JoinDataComplete Info = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 43,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "INFO",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// MENU
			/// </summary>
			[JoinName("MENU")]
			public JoinDataComplete Menu = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 44,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "MENU",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// UP_ARROW
			/// </summary>
			[JoinName("UP_ARROW")]
			public JoinDataComplete DpadUp = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 45,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "UP_ARROW",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// DN_ARROW
			/// </summary>
			[JoinName("DN_ARROW")]
			public JoinDataComplete DpadDown = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 46,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "DN_ARROW",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// LEFT_ARROW
			/// </summary>
			[JoinName("LEFT_ARROW")]
			public JoinDataComplete DpadLeft = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 47,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "LEFT_ARROW",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// RIGHT_ARROW
			/// </summary>
			[JoinName("RIGHT_ARROW")]
			public JoinDataComplete DpadRight = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 48,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "RIGHT_ARROW",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// SELECT
			/// </summary>
			[JoinName("SELECT")]
			public JoinDataComplete DpadSelect = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 49,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "SELECT",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// OPTIONS
			/// </summary>
			[JoinName("OPTIONS")]
			public JoinDataComplete Options = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 50,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "OPTIONS",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// RETURN
			/// </summary>
			[JoinName("RETURN")]
			public JoinDataComplete Return = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 51,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "RETURN",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// DVR
			/// </summary>
			[JoinName("DVR")]
			public JoinDataComplete Dvr = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 52,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "DVR",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// ON_DEMAND
			/// </summary>
			[JoinName("ON_DEMAND")]
			public JoinDataComplete OnDemand = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 53,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "ON_DEMAND",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// PAGE_UP
			/// </summary>
			[JoinName("PAGE_UP")]
			public JoinDataComplete PageUp = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 54,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "PAGE_UP",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// PAGE_DOWN
			/// </summary>
			[JoinName("PAGE_DOWN")]
			public JoinDataComplete PageDown = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 55,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "PAGE_DOWN",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// F_SRCH
			/// </summary>
			[JoinName("F_SRCH")]
			public JoinDataComplete ForwardSearch = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 56,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "F_SRCH",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// R_SRCH
			/// </summary>
			[JoinName("R_SRCH")]
			public JoinDataComplete ReverseSearch = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 57,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "R_SRCH",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// TRACK+
			/// </summary>
			[JoinName("TRACK+")]
			public JoinDataComplete TrackPlus = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 58,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "TRACK+",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// TRACK-
			/// </summary>
			[JoinName("TRACK-")]
			public JoinDataComplete TrackMinus = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 59,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "TRACK-",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// A
			/// </summary>
			[JoinName("A")]
			public JoinDataComplete KpA = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 61,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "A",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// B
			/// </summary>
			[JoinName("B")]
			public JoinDataComplete KpB = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 62,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "B",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// C
			/// </summary>
			[JoinName("C")]
			public JoinDataComplete KpC = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 63,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "C",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// D
			/// </summary>
			[JoinName("D")]
			public JoinDataComplete KpD = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 64,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "D",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// RED
			/// </summary>
			[JoinName("RED")]
			public JoinDataComplete KpRed = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 65,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "RED",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// GREEN
			/// </summary>
			[JoinName("GREEN")]
			public JoinDataComplete KpGreen = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 66,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "GREEN",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// YELLOW
			/// </summary>
			[JoinName("YELLOW")]
			public JoinDataComplete KpYellow = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 67,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "YELLOW",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			
			/// <summary>
			/// BLUE
			/// </summary>
			[JoinName("BLUE")]
			public JoinDataComplete KpBlue = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 68,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "BLUE",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="joinStart">Join this join map will start at</param>
			public GenericIrControllerJoinMap(uint joinStart)
				: base(joinStart, typeof(GenericIrControllerJoinMap))
			{
			}
	}
}