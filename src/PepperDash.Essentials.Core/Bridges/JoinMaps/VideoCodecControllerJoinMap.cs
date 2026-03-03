using System;
using PepperDash.Essentials.Core;
namespace PepperDash.Essentials.Core.Bridges.JoinMaps
{
 /// <summary>
 /// Represents a VideoCodecControllerJoinMap
 /// </summary>
	public class VideoCodecControllerJoinMap : JoinMapBaseAdvanced
	{
		#region Digital

		/// <summary>
		/// Device is Online
		/// </summary>
		[JoinName("IsOnline")]
		public JoinDataComplete IsOnline = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 1,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Device is Online",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// If High, will send DTMF tones to the call set by SelectCall analog.  If low sends DTMF tones to last connected call.
		/// </summary>
        [JoinName("SendDtmfToSpecificCallIndex")]
        public JoinDataComplete SendDtmfToSpecificCallIndex = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 10,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "If High, will send DTMF tones to the call set by SelectCall analog.  If low sends DTMF tones to last connected call.",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

		/// <summary>
		/// DTMF 1
		/// </summary>
        [JoinName("Dtmf1")]
		public JoinDataComplete Dtmf1 = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 11,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "DTMF 1",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// DTMF 2
		/// </summary>
        [JoinName("Dtmf2")]
		public JoinDataComplete Dtmf2 = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 12,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "DTMF 2",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// DTMF 3
		/// </summary>
        [JoinName("Dtmf3")]
		public JoinDataComplete Dtmf3 = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 13,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "DTMF 3",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// DTMF 4
		/// </summary>
        [JoinName("Dtmf4")]
		public JoinDataComplete Dtmf4 = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 14,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "DTMF 4",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// DTMF 5
		/// </summary>
        [JoinName("Dtmf5")]
		public JoinDataComplete Dtmf5 = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 15,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "DTMF 5",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// DTMF 6
		/// </summary>
        [JoinName("Dtmf6")]
		public JoinDataComplete Dtmf6 = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 16,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "DTMF 6",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// DTMF 7
		/// </summary>
        [JoinName("Dtmf7")]
		public JoinDataComplete Dtmf7 = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 17,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "DTMF 7",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// DTMF 8
		/// </summary>
        [JoinName("Dtmf8")]
		public JoinDataComplete Dtmf8 = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 18,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "DTMF 8",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// DTMF 9
		/// </summary>
        [JoinName("Dtmf9")]
		public JoinDataComplete Dtmf9 = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 19,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "DTMF 9",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// DTMF 0
		/// </summary>
        [JoinName("Dtmf0")]
		public JoinDataComplete Dtmf0 = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 20,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "DTMF 0",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// DTMF *
		/// </summary>
        [JoinName("DtmfStar")]
		public JoinDataComplete DtmfStar = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 21,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "DTMF *",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// DTMF #
		/// </summary>
        [JoinName("DtmfPound")]
		public JoinDataComplete DtmfPound = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 22,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "DTMF #",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// End All Calls
		/// </summary>
        [JoinName("EndAllCalls")]
		public JoinDataComplete EndAllCalls = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 24,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "End All Calls",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Current Hook State
		/// </summary>
		[JoinName("HookState")]
		public JoinDataComplete HookState = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 31,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Current Hook State",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Speed Dial
		/// </summary>
		[JoinName("SpeedDialStart")]
		public JoinDataComplete SpeedDialStart = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 41,
				JoinSpan = 4
			},
			new JoinMetadata
			{
				Description = "Speed Dial",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Incoming Call
		/// </summary>
		[JoinName("IncomingCall")]
		public JoinDataComplete IncomingCall = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 50,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Incoming Call",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Answer Incoming Call
		/// </summary>
		[JoinName("IncomingAnswer")]
		public JoinDataComplete IncomingAnswer = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 51,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Answer Incoming Call",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Reject Incoming Call
		/// </summary>
		[JoinName("IncomingReject")]
		public JoinDataComplete IncomingReject = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 52,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Reject Incoming Call",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Manual Dial
		/// </summary>
		[JoinName("ManualDial")]
		public JoinDataComplete ManualDial = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 71,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Dial manual string specified by CurrentDialString serial join",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Dial Phone
		/// </summary>
        [JoinName("DialPhone")]
		public JoinDataComplete DialPhone = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 72,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Dial Phone",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Phone Hook State
		/// </summary>
		[JoinName("PhoneHookState")]
		public JoinDataComplete PhoneHookState = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 72,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Dial Phone",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Hang Up Phone
		/// </summary>
        [JoinName("HangUpPhone")]
		public JoinDataComplete HangUpPhone = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 73,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Hang Up Phone",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// End Call
		/// </summary>
        [JoinName("EndCallStart")]
        public JoinDataComplete EndCallStart = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 81,
                JoinSpan = 8
            },
            new JoinMetadata
            {
                Description = "End a specific call by call index. ",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

		/// <summary>
		/// Join All Calls
		/// </summary>
        [JoinName("JoinAllCalls")]
        public JoinDataComplete JoinAllCalls = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 90,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Join all calls",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

		/// <summary>
		/// Join Call
		/// </summary>
        [JoinName("JoinCallStart")]
        public JoinDataComplete JoinCallStart = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 91,
                JoinSpan = 8
            },
            new JoinMetadata
            {
                Description = "Join a specific call by call index. ",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

		/// <summary>
		/// Directory Search Busy
		/// </summary>
		[JoinName("DirectorySearchBusy")]
		public JoinDataComplete DirectorySearchBusy = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 100,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Directory Search Busy FB",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Directory Selected Entry Is Contact
		/// </summary>
		[JoinName("DirectoryEntryIsContact")]
		public JoinDataComplete DirectoryEntryIsContact = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 101,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Directory Selected Entry Is Contact FB",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Directory Line Selected
		/// </summary>
		[JoinName("DirectoryLineSelected")]
		public JoinDataComplete DirectoryLineSelected = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 101,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Directory Line Selected FB",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Directory Is Root
		/// </summary>
		[JoinName("DirectoryIsRoot")]
		public JoinDataComplete DirectoryIsRoot = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 102,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Directory is on Root FB",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Directory Has Changed
		/// </summary>
		[JoinName("DirectoryHasChanged")]
		public JoinDataComplete DirectoryHasChanged = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 103,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Directory has changed FB",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Directory Go to Root
		/// </summary>
		[JoinName("DirectoryRoot")]
		public JoinDataComplete DirectoryRoot = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 104,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Go to Directory Root",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Directory Go Back One Level
		/// </summary>
		[JoinName("DirectoryFolderBack")]
		public JoinDataComplete DirectoryFolderBack = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 105,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Go back one directory level",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Directory Dial Selected Line
		/// </summary>
		[JoinName("DirectoryDialSelectedLine")]
		public JoinDataComplete DirectoryDialSelectedLine = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 106,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Dial selected directory line",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Directory Disable Auto Dial Selected Line
		/// </summary>
        [JoinName("DirectoryDisableAutoDialSelectedLine")]
        public JoinDataComplete DirectoryDisableAutoDialSelectedLine = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 107,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Set high to disable automatic dialing of a contact when selected",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

		/// <summary>
		/// Directory Dial Selected Contact Method
		/// </summary>
        [JoinName("DirectoryDialSelectedContactMethod")]
        public JoinDataComplete DirectoryDialSelectedContactMethod = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 108,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Pulse to dial the selected contact method",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

		/// <summary>
		/// Directory Clear Selected
		/// </summary>
        [JoinName("DirectoryClearSelected")]
        public JoinDataComplete DirectoryClearSelected = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 110,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Clear Selected Entry and String from Search",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

		/// <summary>
		/// Camera Tilt Up
		/// </summary>
		[JoinName("CameraTiltUp")]
		public JoinDataComplete CameraTiltUp = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 111,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Camera Tilt Up",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Camera Tilt Down
		/// </summary>
		[JoinName("CameraTiltDown")]
		public JoinDataComplete CameraTiltDown = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 112,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Camera Tilt Down",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Camera Pan Left
		/// </summary>
		[JoinName("CameraPanLeft")]
		public JoinDataComplete CameraPanLeft = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 113,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Camera Pan Left",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Camera Pan Right
		/// </summary>
		[JoinName("CameraPanRight")]
		public JoinDataComplete CameraPanRight = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 114,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Camera Pan Right",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Camera Zoom In
		/// </summary>
		[JoinName("CameraZoomIn")]
		public JoinDataComplete CameraZoomIn = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 115,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Camera Zoom In",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Camera Zoom Out
		/// </summary>
		[JoinName("CameraZoomOut")]
		public JoinDataComplete CameraZoomOut = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 116,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Camera Zoom Out",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Camera Focus Near
		/// </summary>
        [JoinName("CameraFocusNear")]
        public JoinDataComplete CameraFocusNear = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 117,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Camera Focus Near",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

		/// <summary>
		/// Camera Focus Far
		/// </summary>
        [JoinName("CameraFocusFar")]
        public JoinDataComplete CameraFocusFar = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 118,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Camera Focus Far",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

		/// <summary>
		/// Camera Auto Focus
		/// </summary>
        [JoinName("CameraFocusAuto")]
        public JoinDataComplete CameraFocusAuto = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 119,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Camera Auto Focus Trigger",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

		/// <summary>
		/// Camera Preset Save
		/// </summary>
		[JoinName("CameraPresetSave")]
		public JoinDataComplete CameraPresetSave = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 121,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Pulse to save selected preset spcified by CameraPresetSelect analog join.  FB will pulse for 3s when preset saved.",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Camera Preset Recall
		/// </summary>
		[JoinName("CameraModeAuto")]
		public JoinDataComplete CameraModeAuto = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 131,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Camera Mode Auto.  Enables camera auto tracking mode, with feedback",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Camera Mode Manual
		/// </summary>
		[JoinName("CameraModeManual")]
		public JoinDataComplete CameraModeManual = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 132,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Camera Mode Manual.  Disables camera auto tracking mode, with feedback",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Camera Mode Off
		/// </summary>
		[JoinName("CameraModeOff")]
		public JoinDataComplete CameraModeOff = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 133,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Camera Mode Off.  Disables camera video, with feedback. Works like video mute.",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Camera Self View
		/// </summary>
		[JoinName("CameraSelfView")]
		public JoinDataComplete CameraSelfView = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 141,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Camera Self View Toggle/FB",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Camera Layout
		/// </summary>
		[JoinName("CameraLayout")]
		public JoinDataComplete CameraLayout = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 142,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Camera Layout Toggle",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Camera Supports Auto Mode
		/// </summary>
		[JoinName("CameraSupportsAutoMode")]
		public JoinDataComplete CameraSupportsAutoMode = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 143,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Camera Supports Auto Mode FB",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Camera Supports Off Mode
		/// </summary>
		[JoinName("CameraSupportsOffMode")]
		public JoinDataComplete CameraSupportsOffMode = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 144,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Camera Supports Off Mode FB",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Update Meetings
		/// </summary>
		[JoinName("UpdateMeetings")]
		public JoinDataComplete UpdateMeetings = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 160,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Update Meetings",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Dial Meeting Start
		/// </summary>
        [JoinName("DialMeetingStart")]
		public JoinDataComplete DialMeetingStart = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 161,
				JoinSpan = 10
			},
			new JoinMetadata
			{
				Description = "Join meeting",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Mic Mute On
		/// </summary>
		[JoinName("MicMuteOn")]
		public JoinDataComplete MicMuteOn = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 171,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Mic Mute On",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Mic Mute Off
		/// </summary>
		[JoinName("MicMuteOff")]
		public JoinDataComplete MicMuteOff = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 172,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Mic Mute Off",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Mic Mute Toggle
		/// </summary>
		[JoinName("MicMuteToggle")]
		public JoinDataComplete MicMuteToggle = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 173,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Mic Mute Toggle",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Volume Up
		/// </summary>
		[JoinName("VolumeUp")]
		public JoinDataComplete VolumeUp = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 174,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Volume Up",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Volume Down
		/// </summary>
		[JoinName("VolumeDown")]
		public JoinDataComplete VolumeDown = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 175,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Volume Down",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Volume Mute On
		/// </summary>
		[JoinName("VolumeMuteOn")]
		public JoinDataComplete VolumeMuteOn = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 176,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Volume Mute On",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Volume Mute Off
		/// </summary>
		[JoinName("VolumeMuteOff")]
		public JoinDataComplete VolumeMuteOff = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 177,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Volume Mute Off",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Volume Mute Toggle
		/// </summary>
		[JoinName("VolumeMuteToggle")]
		public JoinDataComplete VolumeMuteToggle = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 178,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Volume Mute Toggle",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Remove Selected Recent Call Item
		/// </summary>
        [JoinName("RemoveSelectedRecentCallItem")]
        public JoinDataComplete RemoveSelectedRecentCallItem = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 181,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Pulse to remove the selected recent call item specified by the SelectRecentCallItem analog join",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

		/// <summary>
		/// Dial Selected Recent Call Item
		/// </summary>
        [JoinName("DialSelectedRecentCallItem")]
        public JoinDataComplete DialSelectedRecentCallItem = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 182,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Pulse to dial the selected recent call item specified by the SelectRecentCallItem analog join",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

		/// <summary>
		/// Source Share Start
		/// </summary>
		[JoinName("SourceShareStart")]
		public JoinDataComplete SourceShareStart = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 201,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Start Sharing & Feedback",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Source Share End
		/// </summary>
		[JoinName("SourceShareEnd")]
		public JoinDataComplete SourceShareEnd = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 202,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Stop Sharing & Feedback",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Source Share Auto Start
		/// </summary>
		[JoinName("AutoShareWhileInCall")]
		public JoinDataComplete SourceShareAutoStart = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 203,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "When high, will autostart sharing when a call is joined",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Recieving Content
		/// </summary>
		[JoinName("RecievingContent")]
		public JoinDataComplete RecievingContent = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 204,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Recieving content from the far end",
				JoinType = eJoinType.Digital,
				JoinCapabilities = eJoinCapabilities.ToSIMPL
			});

		/// <summary>
		/// Selfview Position
		/// </summary>
		[JoinName("SelfviewPosition")]
		public JoinDataComplete SelfviewPosition = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 211,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Toggles selfview position",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Hold All Calls
		/// </summary>
        [JoinName("HoldAllCalls")]
        public JoinDataComplete HoldAllCalls = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 220,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Holds all calls",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

		/// <summary>
		/// Hold Call at Index
		/// </summary>
        [JoinName("HoldCallsStart")]
        public JoinDataComplete HoldCallsStart = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 221,
                JoinSpan = 8
            },
            new JoinMetadata
            {
                Description = "Holds Call at specified index. FB reported on Call Status XSIG",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

		/// <summary>
		/// Resume All Calls
		/// </summary>
        [JoinName("ResumeCallsStart")]
        public JoinDataComplete ResumeCallsStart = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 231,
                JoinSpan = 8
            },
            new JoinMetadata
            {
                Description = "Resume Call at specified index",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

		/// <summary>
		/// Multi Site Option Is Enabled
		/// </summary>
        [JoinName("MultiSiteOptionIsEnabled")]
        public JoinDataComplete MultiSiteOptionIsEnabled = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 301,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Multi site option is enabled FB",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

		/// <summary>
		/// Auto Answer Enabled
		/// </summary>
        [JoinName("AutoAnswerEnabled")]
        public JoinDataComplete AutoAnswerEnabled = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 302,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Auto Answer is enabled FB",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

		/// <summary>
		/// Participant Audio Mute Toggle
		/// </summary>
        [JoinName("ParticipantAudioMuteToggleStart")]
        public JoinDataComplete ParticipantAudioMuteToggleStart = new JoinDataComplete(
			new JoinData 
			{ 
				JoinNumber = 501,
				JoinSpan = 50 
			},
            new JoinMetadata
            {
                Description = "Toggles the participant's audio mute status",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

		/// <summary>
		/// Participant Video Mute Toggle
		/// </summary>
        [JoinName("ParticipantVideoMuteToggleStart")]
        public JoinDataComplete ParticipantVideoMuteToggleStart = new JoinDataComplete(
			new JoinData 
			{ 
				JoinNumber = 801, 
				JoinSpan = 50 
			},
            new JoinMetadata
            {
                Description = "Toggles the participant's video mute status",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

		/// <summary>
		/// Participant Pin Toggle
		/// </summary>
        [JoinName("ParticipantPinToggleStart")]
        public JoinDataComplete ParticipantPinToggleStart = new JoinDataComplete(
			new JoinData 
			{ 
				JoinNumber = 1101, 
				JoinSpan = 50 
			},
            new JoinMetadata
            {
                Description = "Toggles the participant's pin status",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });


		#endregion



		#region Analog

        // TODO [ ] hotfix/videocodecbase-max-meeting-xsig-set

		/// <summary>
		/// Meetings To Display
		/// </summary>		
        [JoinName("MeetingsToDisplay")]
        public JoinDataComplete MeetingsToDisplay = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 40,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Set/FB the number of meetings to display via the bridge xsig; default: 3 meetings.",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Analog
            });

		/// <summary>
		/// Select Call
		/// </summary>
        [JoinName("SelectCall")]
        public JoinDataComplete SelectCall = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 24,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Sets the selected Call for DTMF commands. Valid values 1-8",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Analog
            });

		/// <summary>
		/// Connected Call Count
		/// </summary>
        [JoinName("ConnectedCallCount")]
        public JoinDataComplete ConnectedCallCount = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 25,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Reports the number of currently connected calls",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Analog
            });

		/// <summary>
		/// Minutes Before Meeting Start
		/// </summary>
		[JoinName("MinutesBeforeMeetingStart")]
		public JoinDataComplete MinutesBeforeMeetingStart = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 41,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Minutes before meeting start that a meeting is joinable",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Analog
			});

		/// <summary>
		/// Camera Number Select
		/// </summary>
		[JoinName("CameraNumberSelect")]
		public JoinDataComplete CameraNumberSelect = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 60,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Camera Number Select/FB.  1 based index.  Valid range is 1 to the value reported by CameraCount.",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Analog
			});

		/// <summary>
		/// Camera Count
		/// </summary>
        [JoinName("CameraCount")]
        public JoinDataComplete CameraCount = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 61,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Reports the number of cameras",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Analog
            });

		/// <summary>
		/// Directory Row Count
		/// </summary>
		[JoinName("DirectoryRowCount")]
		public JoinDataComplete DirectoryRowCount = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 101,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Directory Row Count FB",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Analog
			});

		/// <summary>
		/// Directory Select Row
		/// </summary>
		[JoinName("DirectorySelectRow")]
		public JoinDataComplete DirectorySelectRow = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 101,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Directory Select Row and Feedback",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Analog
			});

		/// <summary>
		/// Selected Contact Method Count
		/// </summary>
        [JoinName("SelectedContactMethodCount")]
        public JoinDataComplete SelectedContactMethodCount = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 102,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Reports the number of contact methods for the selected contact",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Analog
            });

		/// <summary>
		/// Select Contact Method
		/// </summary>
        [JoinName("SelectContactMethod")]
        public JoinDataComplete SelectContactMethod = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 103,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Selects a contact method by index",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Analog
            });

		/// <summary>
		/// Directory Select Row Feedback
		/// </summary>
        [JoinName("DirectorySelectRowFeedback")]
        public JoinDataComplete DirectorySelectRowFeedback = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 104,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Directory Select Row and Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Analog
            });


		/// <summary>
		/// Camera Preset Select
		/// </summary>
		[JoinName("CameraPresetSelect")]
		public JoinDataComplete CameraPresetSelect = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 121,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Camera Preset Select",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Analog
			});

		/// <summary>
		/// Far End Preset Select
		/// </summary>
        [JoinName("FarEndPresetSelect")]
        public JoinDataComplete FarEndPresetSelect = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 122,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Far End Preset Preset Select",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Analog
            });

		/// <summary>
		/// Participant Count
		/// </summary>
		[JoinName("ParticipantCount")]
		public JoinDataComplete ParticipantCount = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 151,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Current Participant Count",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Analog
			});

		/// <summary>
		/// Meeting Count
		/// </summary>
		[JoinName("Meeting Count Fb")]
		public JoinDataComplete MeetingCount = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 161,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Meeting Count",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Analog
			});

		/// <summary>
		/// Volume Level
		/// </summary>
		[JoinName("VolumeLevel")]
		public JoinDataComplete VolumeLevel = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 174,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Volume Level",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Analog
			});

		/// <summary>
		/// Select Recent Call Item
		/// </summary>
        [JoinName("SelectRecentCallItem")]
        public JoinDataComplete SelectRecentCallItem = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 180,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Select/FB for Recent Call Item.  Valid values 1 - 10",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Analog
            });

		/// <summary>
		/// Recent Call Occurrence Type
		/// </summary>
        [JoinName("RecentCallOccurrenceType")]
        public JoinDataComplete RecentCallOccurrenceType = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 181,
                JoinSpan = 10
            },
            new JoinMetadata
            {
                Description = "Recent Call Occurrence Type. [0-3] 0 = Unknown, 1 = Placed, 2 = Received, 3 = NoAnswer",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Analog
            });

		/// <summary>
		/// Recent Call Count
		/// </summary>
        [JoinName("RecentCallCount")]
        public JoinDataComplete RecentCallCount = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 191,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Recent Call Count",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Analog
            });

		#endregion



		#region Serials

		/// <summary>
		/// Current Dial String
		/// </summary>
		[JoinName("CurrentDialString")]
		public JoinDataComplete CurrentDialString = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 1,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Value to dial when ManualDial digital join is pulsed",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Phone Dial String
		/// </summary>
        [JoinName("PhoneDialString")]
		public JoinDataComplete PhoneDialString = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 2,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Phone Dial String",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Current Call Data
		/// </summary>
        [JoinName("CurrentCallData")]
		public JoinDataComplete CurrentCallData = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 2,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Current Call Data - XSIG",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Call Direction
		/// </summary>
		[JoinName("CallDirection")]
		public JoinDataComplete CallDirection = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 22,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Current Call Direction",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Incoming Call Name
		/// </summary>
		[JoinName("IncomingCallName")]
		public JoinDataComplete IncomingCallName = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 51,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Incoming Call Name",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Incoming Call Number
		/// </summary>
		[JoinName("IncomingCallNumber")]
		public JoinDataComplete IncomingCallNumber = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 52,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Incoming Call Number",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Directory Search String
		/// </summary>
		[JoinName("DirectorySearchString")]
		public JoinDataComplete DirectorySearchString = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 100,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Directory Search String",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Directory Entries
		/// </summary>
		[JoinName("DirectoryEntries")]
		public JoinDataComplete DirectoryEntries = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 101,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Directory Entries - XSig, 255 entries",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Schedule Data
		/// </summary>
		[JoinName("Schedule")]
		public JoinDataComplete Schedule = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 102,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Schedule Data - XSIG",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Contact Methods
		/// </summary>
        [JoinName("ContactMethods")]
        public JoinDataComplete ContactMethods = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 103,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Contact Methods - XSig, 10 entries",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

		/// <summary>
		/// Camera Preset Names
		/// </summary>
		[JoinName("CameraPresetNames")]
		public JoinDataComplete CameraPresetNames = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 121,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Camera Preset Names - XSIG, max of 15",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Current Layout String
		/// </summary>
        [JoinName("CurrentLayoutStringFb")]
		public JoinDataComplete CurrentLayoutStringFb = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 141,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Current Layout Fb",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Available Layouts XSig
		/// </summary>
        [JoinName("AvailableLayoutsFb")]
        public JoinDataComplete AvailableLayoutsFb = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 142,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "xSig of all available layouts",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

		/// <summary>
		/// Select Layout
		/// </summary>
        [JoinName("SelectLayout")]
        public JoinDataComplete SelectLayout = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 142,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Select Layout by string",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });


		/// <summary>
		/// Current Participants XSig
		/// </summary>
		[JoinName("CurrentParticipants")]
		public JoinDataComplete CurrentParticipants = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 151,
				JoinSpan = 1
			},
			new JoinMetadata()
			{
				Description = "Current Participants XSig",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Camera Names XSig
		/// </summary>
        [JoinName("CameraNamesFb")]
        public JoinDataComplete CameraNamesFb = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 161,
                JoinSpan = 10
            },
            new JoinMetadata
            {
                Description = "Camera Name Fb",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

		/// <summary>
		/// Selected Recent Call Name
		/// </summary>
        [JoinName("SelectedRecentCallName")]
        public JoinDataComplete SelectedRecentCallName = new JoinDataComplete(
         new JoinData
         {
             JoinNumber = 171,
             JoinSpan = 1
         },
         new JoinMetadata
         {
             Description = "Selected Recent Call Name",
             JoinCapabilities = eJoinCapabilities.ToSIMPL,
             JoinType = eJoinType.Serial
         });

		/// <summary>
		/// Selected Recent Call Number
		/// </summary>
        [JoinName("SelectedRecentCallNumber")]
        public JoinDataComplete SelectedRecentCallNumber = new JoinDataComplete(
         new JoinData
         {
             JoinNumber = 172,
             JoinSpan = 1
         },
         new JoinMetadata
         {
             Description = "Selected Recent Call Number",
             JoinCapabilities = eJoinCapabilities.ToSIMPL,
             JoinType = eJoinType.Serial
         });

		/// <summary>
		/// Recent Call Names
		/// </summary>
        [JoinName("RecentCallNamesStart")]
        public JoinDataComplete RecentCallNamesStart = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 181,
                JoinSpan = 10
            },
            new JoinMetadata
            {
                Description = "Recent Call Names",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

		/// <summary>
		/// Recent Call Numbers
		/// </summary>
        [JoinName("RecentCallTimesStart")]
        public JoinDataComplete RecentCallTimesStart = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 191,
                JoinSpan = 10
            },
            new JoinMetadata
            {
                Description = "Recent Calls Times",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

		/// <summary>
		/// Current Source
		/// </summary>
		[JoinName("CurrentSource")]
		public JoinDataComplete CurrentSource = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 201,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Current Source",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Selfview Position Feedback
		/// </summary>
		[JoinName("SelfviewPositionFb")]
		public JoinDataComplete SelfviewPositionFb = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 211,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "advance selfview position",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Device IP Address
		/// </summary>
        [JoinName("DeviceIpAddresss")]
        public JoinDataComplete DeviceIpAddresss = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 301,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "IP Address of device",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

		/// <summary>
		/// SIP Phone Number
		/// </summary>
        [JoinName("SipPhoneNumber")]
        public JoinDataComplete SipPhoneNumber = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 302,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "SIP phone number of device",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

		/// <summary>
		/// E164 Alias
		/// </summary>
        [JoinName("E164Alias")]
        public JoinDataComplete E164Alias = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 303,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "E164 alias of device",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

		/// <summary>
		/// H323 ID
		/// </summary>
        [JoinName("H323Id")]
        public JoinDataComplete H323Id = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 304,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "H323 ID of device",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

		/// <summary>
		/// SIP URI
		/// </summary>
        [JoinName("SipUri")]
        public JoinDataComplete SipUri = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 305,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "SIP URI of device",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

		/// <summary>
		/// Selected Directory Entry Name
		/// </summary>
		[JoinName("DirectoryEntrySelectedName")]
		public JoinDataComplete DirectoryEntrySelectedName = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 356,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Selected Directory Entry Name",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Selected Directory Entry Number
		/// </summary>
		[JoinName("DirectoryEntrySelectedNumber")]
		public JoinDataComplete DirectoryEntrySelectedNumber = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 357,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Selected Directory Entry Number",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Selected Directory Folder Name
		/// </summary>
		[JoinName("DirectorySelectedFolderName")]
		public JoinDataComplete DirectorySelectedFolderName = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 358,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Selected Directory Folder Name",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		#endregion


		/// <summary>
		/// Constructor for the VideoCodecControllerJoinMap class.
		/// </summary>
		/// <param name="joinStart">Join this join map will start at</param>
		public VideoCodecControllerJoinMap(uint joinStart)
			: base(joinStart, typeof(VideoCodecControllerJoinMap))
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="joinStart">Join this join map will start at</param>
		/// <param name="type">Type of the child join map</param>
		public VideoCodecControllerJoinMap(uint joinStart, Type type)
			: base(joinStart, type)
		{
		}
	}
}
