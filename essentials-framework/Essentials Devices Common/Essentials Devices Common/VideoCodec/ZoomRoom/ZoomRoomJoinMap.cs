using System;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges.JoinMaps;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
	public class ZoomRoomJoinMap : VideoCodecControllerJoinMap
	{
		#region Digital

        [JoinName("CancelJoinAttempt")]
        public JoinDataComplete CancelJoinAttempt = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 5,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Pulse to hide the password prompt",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("MeetingPasswordRequired")]
        public JoinDataComplete MeetingPasswordRequired = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 6,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "FB Indicates to show the password prompt",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("PasswordIncorrect")]
        public JoinDataComplete PasswordIncorrect = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 7,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "FB Indicates the password entered is incorrect",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("PasswordLoginFailed")]
        public JoinDataComplete PasswordLoginFailed = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 8,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "FB Indicates the password entered is incorrect",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("WaitingForHost")]
        public JoinDataComplete WaitingForHost = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 9,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "FB Indicates system is waiting for host",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("IsHost")]
        public JoinDataComplete IsHost = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 10,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "FB Indicates system is the host",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("StartMeetingNow")]
        public JoinDataComplete StartMeetingNow = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 25,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "FB Indicates the password prompt is active",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("ShareOnlyMeeting")]
        public JoinDataComplete ShareOnlyMeeting = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 26,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Triggers a share only meeting, feedback indicates the current meeting is share only",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("StartNormalMeetingFromSharingOnlyMeeting")]
        public JoinDataComplete StartNormalMeetingFromSharingOnlyMeeting = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 27,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Starts a normal meeting from a share only meeting",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital
            });

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

		// TODO: #714 [ ] JoinMap >> SelfivewPipSizeToggle
		[JoinName("SelfviewPipSizeToggle")]
		public JoinDataComplete SelfviewPipSizeToggle = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 231,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Toggles the selfview pip size, (aka layout size)",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

        [JoinName("StartRecording")]
        public JoinDataComplete StartRecording = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 241,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Pulse to start the Meeting Recording.  FB high if meeting is currently recording",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("StopRecording")]
        public JoinDataComplete StopRecording = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 242,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Pulse to stop the Meeting Recording.  FB high if meeting is currently NOT recording",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("RecordConsentPromptIsVisible")]
        public JoinDataComplete RecordConsentPromptIsVisible = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 243,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "When high, indicates that the recording consent prompt is visible on the ZoomRoom UI",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("RecordingPromptAgree")]
        public JoinDataComplete RecordingPromptAgree = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 244,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Pulse to agree to consent for meeting recording",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("RecordingPromptDisagree")]
        public JoinDataComplete RecordingPromptDisagree = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 245,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Pulse to disagree to consent for meeting recording",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });


        #region Sharing Status

        [JoinName("IsSharingAirplay")]
        public JoinDataComplete IsSharingAirplay = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 250,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Indicates an Airplay source is sharing",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("IsSharingHdmi")]
        public JoinDataComplete IsSharingHdmi = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 251,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Indicates an HDMI source is sharing",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });



        #endregion
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

        // TODO [ ] Issue #868
        [JoinName("SubmitPassword")]
        public JoinDataComplete SubmitPassword = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 6,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Submit password text",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        // TODO [ ] Issue #868
        [JoinName("PasswordPromptMessage")]
        public JoinDataComplete PasswordPromptMessage = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 6,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Password prompt message",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        // TODO [ ] Issue #868
        [JoinName("MeetingInfoId")]
        public JoinDataComplete MeetingInfoId = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 11,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Meeting info ID text feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        // TODO [ ] Issue #868
        [JoinName("MeetingInfoHostt")]
        public JoinDataComplete MeetingInfoHost = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 12,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Meeting info Host text feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        // TODO [ ] Issue #868
        [JoinName("MeetingInfoPassword")]
        public JoinDataComplete MeetingInfoPassword = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 13,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Meeting info Password text feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

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

		// TODO: #714 [ ] JoinMap >> GetSetSelfviewPipSize
		[JoinName("GetSetSelfviewPipSize")]
		public JoinDataComplete GetSetSelfviewPipSize = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 230,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Sets and reports the selfview pip size, (aka layout size).",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.DigitalSerial
			});

        [JoinName("DisplayState")]
        public JoinDataComplete DisplayState = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 250,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Reports the instructions the ZoomRoom is displaying on the monitor. <None | Laptop | IOS>",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("AirplayShareCode")]
        public JoinDataComplete AirplayShareCode = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 251,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Reports the current code for Airplay Sharing.",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("LaptopShareKey")]
        public JoinDataComplete LaptopShareKey = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 252,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "The alpha-only sharing key that users type into a laptop client to share with the Zoom Room.",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("LaptopSharePairingCode")]
        public JoinDataComplete LaptopSharePairingCode = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 253,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "This is the paring code that is broadcast via an ultrasonic signal from the ZRC. It is different than the user-supplied paring code. The ZRC uses a Zoom-proprietary method of advertizing the ultrasonic pairing code, so it\'s not possible to advertize it using commonly available libraries.",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("WifiName")]
        public JoinDataComplete WifiName = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 254,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Reports the Wifi SSID used by the ZoomRoom.",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("ServerName")]
        public JoinDataComplete ServerName = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 255,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Reports the namne of the the ZoomRoom.",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
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