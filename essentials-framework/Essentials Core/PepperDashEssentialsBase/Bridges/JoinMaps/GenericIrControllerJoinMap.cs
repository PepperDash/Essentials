using PepperDash.Essentials.Core;

namespace PepperDash_Essentials_Core.Bridges.JoinMaps
{
		public sealed class GenericIrControllerJoinMap : JoinMapBaseAdvanced
		{
			[JoinName("POWER")]
			public JoinDataComplete Power = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 1,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Power Toggle",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("POWER_ON")]
			public JoinDataComplete PowerOn = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 2,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Discrete Power On",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("POWER_OFF")]
			public JoinDataComplete PowerOff = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 3,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Discrete Power Off",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("PLAY")]
			public JoinDataComplete Play = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 4,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Transport Play",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("PLAY_PAUSE")]
			public JoinDataComplete PlayPause = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 5,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Transport Play/Pause",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("STOP")]
			public JoinDataComplete Stop = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 6,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Transport Stop",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("PAUSE")]
			public JoinDataComplete Pause = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 7,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Transport Pause",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("FSCAN")]
			public JoinDataComplete ForwardScan = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 9,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Transport Forward Scan",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("F_SRCH")]
			public JoinDataComplete ForwardSearch = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 10,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Transport Forward Search",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("F_SKIP")]
			public JoinDataComplete ForwardSkip = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 11,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Transport Forward Skip/Next",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("RSCAN")]
			public JoinDataComplete ReverseScan = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 12,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Transport Reverse Scan",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("R_SRCH")]
			public JoinDataComplete ReverseSearch = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 13,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Transport Reverse Search",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("R_SKIP")]
			public JoinDataComplete ReverseSkip = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 14,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Transport Reverse Skip/Previous",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("TRACK+")]
			public JoinDataComplete TrackPlus = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 15,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Transport Track +",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("TRACK-")]
			public JoinDataComplete TrackMinus = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 16,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Transport Track -",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});
			

			[JoinName("0")]
			public JoinDataComplete Kp0 = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 20,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Keypad 0",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("1")]
			public JoinDataComplete Kp1 = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 21,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Keypad 1",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("2")]
			public JoinDataComplete Kp2 = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 22,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Keypad 2",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("3")]
			public JoinDataComplete Kp3 = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 23,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Keypad 3",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("4")]
			public JoinDataComplete Kp4 = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 24,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Keypad 4",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("5")]
			public JoinDataComplete Kp5 = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 25,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Keypad 5",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("6")]
			public JoinDataComplete Kp6 = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 26,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Keypad 6",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("7")]
			public JoinDataComplete Kp7 = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 27,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Keypad 7",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("8")]
			public JoinDataComplete Kp8 = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 28,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Keypad 8",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("9")]
			public JoinDataComplete Kp9 = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 29,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Keypad 9",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("#")]
			public JoinDataComplete KpPound = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 30,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Keypad #",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("*")]
			public JoinDataComplete KpStar = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 31,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Keypad Star",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("A")]
			public JoinDataComplete KpA = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 32,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Keypad A",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("B")]
			public JoinDataComplete KpB = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 33,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Keypad B",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("C")]
			public JoinDataComplete KpC = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 34,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Keypad C",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("D")]
			public JoinDataComplete KpD = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 35,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Keypad D",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("RED")]
			public JoinDataComplete KpRed = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 36,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Keypad Red",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("GREEN")]
			public JoinDataComplete KpGreen = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 37,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Keypad Green",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("YELLOW")]
			public JoinDataComplete KpYellow = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 38,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Keypad Yellow",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("BLUE")]
			public JoinDataComplete KpBlue = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 39,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Keypad Blue",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});


			[JoinName("MENU")]
			public JoinDataComplete Menu = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 41,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Menu",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("GUIDE")]
			public JoinDataComplete Guide = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 42,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Guide",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("DVR")]
			public JoinDataComplete Dvr = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 43,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Dvr",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("OPTIONS")]
			public JoinDataComplete Options = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 44,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Options",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("ON_DEMAND")]
			public JoinDataComplete OnDemand = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 45,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "On Demand",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});


			[JoinName("UP_ARROW")]
			public JoinDataComplete DpadUp = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 46,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Dpad Up",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("DN_ARROW")]
			public JoinDataComplete DpadDown = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 47,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Dpad Down",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("LEFT_ARROW")]
			public JoinDataComplete DpadLeft = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 48,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Dpad Left",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("RIGHT_ARROW")]
			public JoinDataComplete DpadRight = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 49,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Dpad Right",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("SELECT")]
			public JoinDataComplete DpadSelect = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 50,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Dpad Select",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("RETURN")]
			public JoinDataComplete Return = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 51,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Return",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("BACK")]
			public JoinDataComplete Back = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 52,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Back",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("HOME")]
			public JoinDataComplete Home = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 53,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Home",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("CH+")]
			public JoinDataComplete ChannelUp = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 54,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Channel Up",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("CH-")]
			public JoinDataComplete ChannelDown = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 55,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Channel Down",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("LAST")]
			public JoinDataComplete Last = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 56,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Last",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("PAGE_UP")]
			public JoinDataComplete PageUp = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 57,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Page Up",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			[JoinName("PAGE_DOWN")]
			public JoinDataComplete PageDown = new JoinDataComplete(
				new JoinData
				{
					JoinNumber = 58,
					JoinSpan = 1
				},
				new JoinMetadata
				{
					Description = "Page Down",
					JoinCapabilities = eJoinCapabilities.FromSIMPL,
					JoinType = eJoinType.Digital
				});

			public GenericIrControllerJoinMap(uint joinStart)
				: base(joinStart, typeof(GenericIrControllerJoinMap))
			{
			}
	}
}