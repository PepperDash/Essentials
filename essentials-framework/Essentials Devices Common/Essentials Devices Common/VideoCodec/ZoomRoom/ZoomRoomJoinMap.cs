using System;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges.JoinMaps;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
	public class ZoomRoomJoinMap : VideoCodecControllerJoinMap
	{
		// TODO: #697 [X] Set join numbers

		#region Digital

		[JoinName("CanSwapContentWithThumbnail")]
		public JoinDataComplete CanSwapContentWithThumbnail = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 206,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "FB Indicates if content can be swapped with thumbnail",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		[JoinName("SwapContentWithThumbnail")]
		public JoinDataComplete SwapContentWithThumbnail = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 206,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Pulse to swap content with thumbnail.  FB reports current state",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		[JoinName("GetAvailableLayouts")]
		public JoinDataComplete GetAvailableLayouts = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 215,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Gets the available layouts.  Will update the LayoutXXXXXIsAvailbale signals.",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		[JoinName("LayoutIsOnFirstPage")]
		public JoinDataComplete LayoutIsOnFirstPage = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 216,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Indicates if layout is on first page",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		[JoinName("LayoutIsOnLastPage")]
		public JoinDataComplete LayoutIsOnLastPage = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 217,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Indicates if layout is on first page",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		[JoinName("LayoutTurnToNextPage")]
		public JoinDataComplete LayoutTurnToNextPage = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 216,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Turns layout view to next page",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		[JoinName("LayoutTurnToPreviousPage")]
		public JoinDataComplete LayoutTurnToPreviousPage = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 217,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Turns layout view to previous page",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});	

		[JoinName("LayoutGalleryIsAvailable")]
		public JoinDataComplete LayoutGalleryIsAvailable = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 221,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "FB Indicates if layout 'Gallery' is available",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.DigitalSerial
			});

		[JoinName("LayoutSpeakerIsAvailable")]
		public JoinDataComplete LayoutSpeakerIsAvailable = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 222,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "FB Indicates if layout 'Speaker' is available",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.DigitalSerial
			});

		[JoinName("LayoutStripIsAvailable")]
		public JoinDataComplete LayoutStripIsAvailable = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 223,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "FB Indicates if layout 'Strip' is available",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.DigitalSerial
			});

		[JoinName("LayoutShareAllIsAvailable")]
		public JoinDataComplete LayoutShareAllIsAvailable = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 224,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "FB Indicates if layout 'ShareAll' is available",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.DigitalSerial
			});		

		// TODO: #714 [ ] Feature Layout Size
		[JoinName("SetLayoutSizeOff")]
		public JoinDataComplete SetLayoutSizeOff = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 231,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Sets layout size off",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		// TODO: #714 [ ] Feature Layout Size
		[JoinName("SetLayoutSize1")]
		public JoinDataComplete SetLayoutSize1 = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 232,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Sets layout size 1",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		// TODO: #714 [ ] Feature Layout Size
		[JoinName("SetLayoutSize2")]
		public JoinDataComplete SetLayoutSize2 = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 233,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Sets layout size 2",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		// TODO: #714 [ ] Feature Layout Size
		[JoinName("SetLayoutSize3")]
		public JoinDataComplete SetLayoutSize3 = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 234,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Sets layout size 3",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		// TODO: #714 [ ] Feature Layout Size
		[JoinName("SetLayoutSizeStrip")]
		public JoinDataComplete SetLayoutSizeStrip = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 235,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Sets layout size strip",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});		

		// TODO: #714 [ ] Feature Layout Size
		[JoinName("SetLayoutPositionCenter")]
		public JoinDataComplete SetLayoutPositionCenter = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 241,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Sets layout position to center",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		// TODO: #714 [ ] Feature Layout Size
		[JoinName("SetLayoutPositionUp")]
		public JoinDataComplete SetLayoutPositionUp = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 242,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Sets layout position to up",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		// TODO: #714 [ ] Feature Layout Size
		[JoinName("SetLayoutPositionRight")]
		public JoinDataComplete SetLayoutPositionRight = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 243,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Sets layout position to right",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		// TODO: #714 [ ] Feature Layout Size
		[JoinName("SetLayoutPositionUpRight")]
		public JoinDataComplete SetLayoutPositionUpRight = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 244,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Sets layout position to up right",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		// TODO: #714 [ ] Feature Layout Size
		[JoinName("SetLayoutPositionDown")]
		public JoinDataComplete SetLayoutPositionDown = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 245,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Sets layout position to down",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		// TODO: #714 [ ] Feature Layout Size
		[JoinName("SetLayoutPositionDownRight")]
		public JoinDataComplete SetLayoutPositionDownRight = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 246,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Sets layout position to down right",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		// TODO: #714 [ ] Feature Layout Size
		[JoinName("SetLayoutPositionLeft")]
		public JoinDataComplete SetLayoutPositionLeft = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 247,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Sets layout position todown left",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		// TODO: #714 [ ] Feature Layout Size
		[JoinName("SetLayoutPositionUpLeft")]
		public JoinDataComplete SetLayoutPositionUpLeft = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 248,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Sets layout position to up left",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		// TODO: #714 [ ] Feature Layout Size
		[JoinName("SetLayoutPositionDownLeft")]
		public JoinDataComplete SetLayoutPositionDownLeft = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 249,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Sets layout position to down left",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

        //[JoinName("ParticipantAudioMuteToggleStart")]
        //public JoinDataComplete ParticipantAudioMuteToggleStart = new JoinDataComplete(
        //    new JoinData
        //    {
        //        JoinNumber = 500,
        //        JoinSpan = 100
        //    },
        //    new JoinMetadata
        //    {
        //        Description = "Toggles the participant's audio mute status",
        //        JoinCapabilities = eJoinCapabilities.ToSIMPL,
        //        JoinType = eJoinType.Digital
        //    });

        //[JoinName("ParticipantVideoMuteToggleStart")]
        //public JoinDataComplete ParticipantVideoMuteToggleStart = new JoinDataComplete(
        //    new JoinData
        //    {
        //        JoinNumber = 800,
        //        JoinSpan = 100
        //    },
        //    new JoinMetadata
        //    {
        //        Description = "Toggles the participant's video mute status",
        //        JoinCapabilities = eJoinCapabilities.ToSIMPL,
        //        JoinType = eJoinType.Digital
        //    });

        //[JoinName("ParticipantPinToggleStart")]
        //public JoinDataComplete ParticipantPinToggleStart = new JoinDataComplete(
        //    new JoinData
        //    {
        //        JoinNumber = 1100,
        //        JoinSpan = 100
        //    },
        //    new JoinMetadata
        //    {
        //        Description = "Toggles the participant's pin status",
        //        JoinCapabilities = eJoinCapabilities.ToSIMPL,
        //        JoinType = eJoinType.Digital
        //    });

		#endregion


		#region Analog

		[JoinName("NumberOfScreens")]
		public JoinDataComplete NumberOfScreens = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 11,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Reports the number of screens connected",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Analog
			});

		[JoinName("ScreenIndexToPinUserTo")]
		public JoinDataComplete ScreenIndexToPinUserTo = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 11,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Specifies the screen index a participant should be pinned to",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Analog
			});

		#endregion


		#region Serials

		[JoinName("GetSetCurrentLayout")]
		public JoinDataComplete GetSetCurrentLayout = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 215,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Sets and reports the current layout.  Use the LayoutXXXXIsAvailable signals to determine valid layouts",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Serial
			});

		// TODO: #714 [ ] Feature Layout Size
		[JoinName("GetSetCurrentLayoutSize")]
		public JoinDataComplete GetSetCurrentLayoutSize = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 230,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Sets and reports the current layout size.",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.DigitalSerial
			});

		// TODO: #714 [ ] Feature Layout Size
		[JoinName("GetSetCurrentLayoutPosition")]
		public JoinDataComplete GetSetCurrentLayoutPosition = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 240,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Sets and reports the current layout position.",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.DigitalSerial
			});


		#endregion

		public ZoomRoomJoinMap(uint joinStart)
			: base(joinStart, typeof(ZoomRoomJoinMap))
		{
		}

		public ZoomRoomJoinMap(uint joinStart, Type type)
			: base(joinStart, type)
		{
		}
	}
}