using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.EthernetCommunication;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.Cameras;

namespace PepperDash.Essentials.AppServer.Messengers
{
	public class SIMPLVtcMessenger : MessengerBase
	{
		BasicTriList EISC;

        public SIMPLVtcJoinMap JoinMap { get; private set; }

        ///********* Bools *********/
        ///// <summary>
        ///// 724
        ///// </summary>
        //const uint BDialHangup = 724;
        ///// <summary>
        ///// 750
        ///// </summary>
        //const uint BCallIncoming = 750;
        ///// <summary>
        ///// 751
        ///// </summary>
        //const uint BIncomingAnswer = 751;
        ///// <summary>
        ///// 752
        ///// </summary>
        //const uint BIncomingReject = 752;
        ///// <summary>
        ///// 741
        ///// </summary>
        //const uint BSpeedDial1 = 741;
        ///// <summary>
        ///// 742
        ///// </summary>
        //const uint BSpeedDial2 = 742;
        ///// <summary>
        ///// 743
        ///// </summary>
        //const uint BSpeedDial3 = 743;
        ///// <summary>
        ///// 744
        ///// </summary>
        //const uint BSpeedDial4 = 744;
        ///// <summary>
        ///// 800
        ///// </summary>
        //const uint BDirectorySearchBusy = 800;
        ///// <summary>
        ///// 801 
        ///// </summary>
        //const uint BDirectoryLineSelected = 801;
        ///// <summary>
        ///// 801 when selected entry is a contact
        ///// </summary>
        //const uint BDirectoryEntryIsContact = 801;
        ///// <summary>
        ///// 802 To show/hide back button
        ///// </summary>
        //const uint BDirectoryIsRoot = 802;
        ///// <summary>
        ///// 803 Pulse from system to inform us when directory is ready
        ///// </summary>
        //const uint BDirectoryHasChanged = 803;
        ///// <summary>
        ///// 804
        ///// </summary>
        //const uint BDirectoryRoot = 804;
        ///// <summary>
        ///// 805
        ///// </summary>
        //const uint BDirectoryFolderBack = 805;
        ///// <summary>
        ///// 806
        ///// </summary>
        //const uint BDirectoryDialSelectedLine = 806;
        ///// <summary>
        ///// 811
        ///// </summary>
        //const uint BCameraControlUp = 811;
        ///// <summary>
        ///// 812
        ///// </summary>
        //const uint BCameraControlDown = 812;
        ///// <summary>
        ///// 813
        ///// </summary>
        //const uint BCameraControlLeft = 813;
        ///// <summary>
        ///// 814
        ///// </summary>
        //const uint BCameraControlRight = 814;
        ///// <summary>
        ///// 815
        ///// </summary>
        //const uint BCameraControlZoomIn = 815;
        ///// <summary>
        ///// 816
        ///// </summary>
        //const uint BCameraControlZoomOut = 816;
        ///// <summary>
        ///// 821 - 826
        ///// </summary>
        //const uint BCameraPresetStart = 821;

        ///// <summary>
        ///// 831
        ///// </summary>
        //const uint BCameraModeAuto = 831;
        ///// <summary>
        ///// 832
        ///// </summary>
        //const uint BCameraModeManual = 832;
        ///// <summary>
        ///// 833
        ///// </summary>
        //const uint BCameraModeOff = 833;

        ///// <summary>
        ///// 841
        ///// </summary>
        //const uint BCameraSelfView = 841;

        ///// <summary>
        ///// 842
        ///// </summary>
        //const uint BCameraLayout = 842;
        ///// <summary>
        ///// 843
        ///// </summary>
        //const uint BCameraSupportsAutoMode = 843;
        ///// <summary>
        ///// 844
        ///// </summary>
        //const uint BCameraSupportsOffMode = 844;


        ///********* Ushorts *********/
        ///// <summary>
        ///// 760
        ///// </summary>
        //const uint UCameraNumberSelect = 760;
        ///// <summary>
        ///// 801
        ///// </summary>
        //const uint UDirectorySelectRow = 801;
        ///// <summary>
        ///// 801
        ///// </summary>
        //const uint UDirectoryRowCount = 801;



        ///********* Strings *********/
        ///// <summary>
        ///// 701
        ///// </summary>
        //const uint SCurrentDialString = 701;
        ///// <summary>
        ///// 702
        ///// </summary>
        //const uint SCurrentCallName = 702;
        ///// <summary>
        ///// 703
        ///// </summary>
        //const uint SCurrentCallNumber = 703;
        ///// <summary>
        ///// 731
        ///// </summary>
        //const uint SHookState = 731;
        ///// <summary>
        ///// 722
        ///// </summary>
        //const uint SCallDirection = 722;
        ///// <summary>
        ///// 751
        ///// </summary>
        //const uint SIncomingCallName = 751;
        ///// <summary>
        ///// 752
        ///// </summary>
        //const uint SIncomingCallNumber = 752;

        ///// <summary>
        ///// 800
        ///// </summary>
        //const uint SDirectorySearchString = 800;
        ///// <summary>
        ///// 801-1055
        ///// </summary>
        //const uint SDirectoryEntriesStart = 801;
        ///// <summary>
        ///// 1056
        ///// </summary>
        //const uint SDirectoryEntrySelectedName = 1056;
        ///// <summary>
        ///// 1057
        ///// </summary>
        //const uint SDirectoryEntrySelectedNumber = 1057;
        ///// <summary>
        ///// 1058
        ///// </summary>
        //const uint SDirectorySelectedFolderName = 1058;


        ///// <summary>
        ///// 701-712 0-9*#
        ///// </summary>
        //Dictionary<string, uint> DTMFMap = new Dictionary<string, uint>
        //{
        //    { "1", 701 },
        //    { "2", 702 },
        //    { "3", 703 },
        //    { "4", 704 },
        //    { "5", 705 },
        //    { "6", 706 },
        //    { "7", 707 },
        //    { "8", 708 },
        //    { "9", 709 },
        //    { "0", 710 },
        //    { "*", 711 },
        //    { "#", 712 },
        //};

		CodecActiveCallItem CurrentCallItem;
		CodecActiveCallItem IncomingCallItem;

		ushort PreviousDirectoryLength = 701;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="eisc"></param>
		/// <param name="messagePath"></param>
		public SIMPLVtcMessenger(string key, BasicTriList eisc, string messagePath)
			: base(key, messagePath)
		{
			EISC = eisc;

            JoinMap = new SIMPLVtcJoinMap();

            // TODO: Take in JoinStart value from config
            JoinMap.OffsetJoinNumbers(701);


			CurrentCallItem = new CodecActiveCallItem();
			CurrentCallItem.Type = eCodecCallType.Video;
			CurrentCallItem.Id = "-video-";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="appServerController"></param>
		protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
		{
			var asc = appServerController;
			EISC.SetStringSigAction(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.HookState), s => 
			{
				CurrentCallItem.Status = (eCodecCallStatus)Enum.Parse(typeof(eCodecCallStatus), s, true);
				PostFullStatus(); // SendCallsList();
			});

			EISC.SetStringSigAction(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CurrentCallNumber), s => 
			{
				CurrentCallItem.Number = s;
                PostCallsList();
			});

            EISC.SetStringSigAction(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CurrentCallName), s =>
            {
                CurrentCallItem.Name = s;
                PostCallsList();
            });

            EISC.SetStringSigAction(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CallDirection), s =>
            {
                CurrentCallItem.Direction = (eCodecCallDirection)Enum.Parse(typeof(eCodecCallDirection), s, true);
                PostCallsList();
            });

			EISC.SetBoolSigAction(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.IncomingCall), b =>
			{
				if (b)
				{
					var ica = new CodecActiveCallItem()
					{
						Direction = eCodecCallDirection.Incoming,
						Id = "-video-incoming",
						Name = EISC.GetString(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.IncomingCallName)),
						Number = EISC.GetString(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.IncomingCallNumber)),
						Status = eCodecCallStatus.Ringing,
						Type = eCodecCallType.Video
					};
					IncomingCallItem = ica;
				}
				else
				{
					IncomingCallItem = null;
				}
				PostCallsList();
			});

			EISC.SetBoolSigAction(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraSupportsAutoMode), b =>
			{
				PostStatusMessage(new
				{	
					cameraSupportsAutoMode = b
				});
			});
			EISC.SetBoolSigAction(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraSupportsOffMode), b =>
			{
				PostStatusMessage(new
				{
					cameraSupportsOffMode = b
				});
			});

			// Directory insanity
			EISC.SetUShortSigAction(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectoryRowCount), u =>
			{
				// The length of the list comes in before the list does.
				// Splice the sig change operation onto the last string sig that will be changing
				// when the directory entries make it through.
				if (PreviousDirectoryLength > 0)
				{
					EISC.ClearStringSigAction(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectoryEntriesStart) + PreviousDirectoryLength - 1);
				}
				EISC.SetStringSigAction(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectoryEntriesStart) + u - 1, s => PostDirectory());
				PreviousDirectoryLength = u;
			});

			EISC.SetStringSigAction(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectoryEntrySelectedName), s =>
			{
				PostStatusMessage(new
				{
					directoryContactSelected = new
					{
						name = EISC.GetString(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectoryEntrySelectedName)),
					}
				});
			});

			EISC.SetStringSigAction(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectoryEntrySelectedNumber), s =>
			{
				PostStatusMessage(new
				{
					directoryContactSelected = new
					{
						number = EISC.GetString(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectoryEntrySelectedNumber)),
					}
				});
			});

			EISC.SetStringSigAction(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectorySelectedFolderName), s => PostStatusMessage(new
			{
				directorySelectedFolderName = EISC.GetString(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectorySelectedFolderName))
			}));

			EISC.SetSigTrueAction(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraModeAuto), () => PostCameraMode());
			EISC.SetSigTrueAction(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraModeManual), () => PostCameraMode());
			EISC.SetSigTrueAction(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraModeOff), () => PostCameraMode());

			EISC.SetBoolSigAction(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraSelfView), b => PostStatusMessage(new 
				{
					cameraSelfView = b
				}));

			EISC.SetUShortSigAction(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraNumberSelect), (u) => PostSelectedCamera());


			// Add press and holds using helper action
			Action<string, uint> addPHAction = (s, u) => 
				AppServerController.AddAction(MessagePath + s, new PressAndHoldAction(b => EISC.SetBool(u, b)));
			addPHAction("/cameraUp", JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraTiltUp));
			addPHAction("/cameraDown", JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraTiltDown));
			addPHAction("/cameraLeft", JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraPanLeft));
			addPHAction("/cameraRight", JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraPanRight));
			addPHAction("/cameraZoomIn", JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraZoomIn));
			addPHAction("/cameraZoomOut", JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraZoomOut));

			// Add straight pulse calls using helper action
			Action<string, uint> addAction = (s, u) =>
				AppServerController.AddAction(MessagePath + s, new Action(() => EISC.PulseBool(u, 100)));
			addAction("/endCallById", JoinMap.GetJoinForKey(SIMPLVtcJoinMap.EndCall));
			addAction("/endAllCalls", JoinMap.GetJoinForKey(SIMPLVtcJoinMap.EndCall));
            addAction("/acceptById", JoinMap.GetJoinForKey(SIMPLVtcJoinMap.IncomingAnswer));
            addAction("/rejectById", JoinMap.GetJoinForKey(SIMPLVtcJoinMap.IncomingReject));

            var speeddialStart = JoinMap.GetJoinForKey(SIMPLAtcJoinMap.SpeedDialStart);
            var speeddialEnd = JoinMap.GetJoinForKey(SIMPLAtcJoinMap.SpeedDialStart) + JoinMap.GetJoinSpanForKey(SIMPLAtcJoinMap.SpeedDialStart);

            var speedDialIndex = 1;
            for (uint i = speeddialStart; i < speeddialEnd; i++)
            {
                addAction(string.Format("/speedDial{0}", speedDialIndex), i);
                speedDialIndex++;
            }

			addAction("/cameraModeAuto", JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraModeAuto));
			addAction("/cameraModeManual", JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraModeManual));
			addAction("/cameraModeOff", JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraModeOff));
			addAction("/cameraSelfView", JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraSelfView));
			addAction("/cameraLayout", JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraLayout));

			asc.AddAction("/cameraSelect", new Action<string>(SelectCamera));

			// camera presets
			for(uint i = 0; i < 6; i++) 
			{
				addAction("/cameraPreset" + (i + 1), JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraPresetStart) + i);
			}

			asc.AddAction(MessagePath + "/isReady", new Action(PostIsReady));
			// Get status
			asc.AddAction(MessagePath + "/fullStatus", new Action(PostFullStatus));
			// Dial on string
			asc.AddAction(MessagePath + "/dial", new Action<string>(s => 
				EISC.SetString(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CurrentDialString), s)));
            // Pulse DTMF
            AppServerController.AddAction(MessagePath + "/dtmf", new Action<string>(s =>
            {
                var join = JoinMap.GetJoinForKey(s);
                if (join > 0)
                {
                    EISC.PulseBool(join, 100);
                }
            }));

			// Directory madness
			asc.AddAction(MessagePath + "/directoryRoot", new Action(() => EISC.PulseBool(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectoryRoot))));
			asc.AddAction(MessagePath + "/directoryBack", new Action(() => EISC.PulseBool(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectoryFolderBack))));
			asc.AddAction(MessagePath + "/directoryById", new Action<string>(s =>
			{
				// the id should contain the line number to forward to simpl
				try
				{
					var u = ushort.Parse(s);
					EISC.SetUshort(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectorySelectRow), u);
					EISC.PulseBool(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectoryLineSelected));
				}
				catch (Exception)
				{
					Debug.Console(1, this, Debug.ErrorLogLevel.Warning, 
						"/directoryById request contains non-numeric ID incompatible with DDVC bridge");
				}

			}));
			asc.AddAction(MessagePath + "/directorySelectContact", new Action<string>(s =>
			{
				try
				{
					var u = ushort.Parse(s);
					EISC.SetUshort(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectorySelectRow), u);
					EISC.PulseBool(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectoryLineSelected));
				}
				catch
				{
					
				}
			}));
			asc.AddAction(MessagePath + "/directoryDialContact", new Action(() => {
				EISC.PulseBool(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectoryDialSelectedLine));
			}));
			asc.AddAction(MessagePath + "/getDirectory", new Action(() =>
			{
				if (EISC.GetUshort(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectoryRowCount)) > 0)
				{
					PostDirectory();
				}
				else
				{
					EISC.PulseBool(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectoryRoot));
				}
			}));
		}

		/// <summary>
		/// 
		/// </summary>
		void PostFullStatus()
		{
			this.PostStatusMessage(new
			{
				calls = GetCurrentCallList(),
				cameraMode = GetCameraMode(),
				cameraSelfView = EISC.GetBool(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraSelfView)),
				cameraSupportsAutoMode = EISC.GetBool(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraSupportsAutoMode)),
				cameraSupportsOffMode = EISC.GetBool(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraSupportsOffMode)),
				currentCallString = EISC.GetString(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CurrentCallNumber)),
				currentDialString = EISC.GetString(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CurrentDialString)),
				directoryContactSelected = new
				{
					name = EISC.GetString(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectoryEntrySelectedName)),
					number = EISC.GetString(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectoryEntrySelectedNumber))
				},
				directorySelectedFolderName = EISC.GetString(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectorySelectedFolderName)),
				isInCall = EISC.GetString(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.HookState)) == "Connected",
				hasDirectory = true,
				hasDirectorySearch = false,
				hasRecents = !EISC.BooleanOutput[502].BoolValue,
				hasCameras = true,
				showCamerasWhenNotInCall = EISC.BooleanOutput[503].BoolValue,
				selectedCamera = GetSelectedCamera(),
			});
		}

		/// <summary>
		/// 
		/// </summary>
		void PostDirectory()
		{
			var u = EISC.GetUshort(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectoryRowCount));
			var items = new List<object>();
			for (uint i = 0; i < u; i++)
			{
				var name = EISC.GetString(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectoryEntriesStart) + i);
				var id = (i + 1).ToString();
				// is folder or contact?
				if (name.StartsWith("[+]"))
				{
					items.Add(new
					{
						folderId = id,
						name = name
					});
				}
				else
				{
					items.Add(new
					{
						contactId = id,
						name = name
					});
				}
			}

			var directoryMessage = new
			{
				currentDirectory = new
				{
					isRootDirectory = EISC.GetBool(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.DirectoryIsRoot)),
					directoryResults = items
				}
			};
			PostStatusMessage(directoryMessage);
		}

		/// <summary>
		/// 
		/// </summary>
		void PostCameraMode()
		{
			PostStatusMessage(new
			{
				cameraMode = GetCameraMode()
			});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mode"></param>
		string GetCameraMode()
		{
			string m;
            if (EISC.GetBool(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraModeAuto))) m = eCameraControlMode.Auto.ToString().ToLower();
            else if (EISC.GetBool(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraModeManual))) m = eCameraControlMode.Manual.ToString().ToLower();
            else m = eCameraControlMode.Off.ToString().ToLower();
			return m;
		}

		void PostSelectedCamera()
		{
			PostStatusMessage(new
			{
				selectedCamera = GetSelectedCamera()
			});
		}

		/// <summary>
		/// 
		/// </summary>
		string GetSelectedCamera()
		{
			var num = EISC.GetUshort(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraNumberSelect));
			string m;
			if (num == 100)
			{
				m = "cameraFar";
			}
			else
			{
				m = "camera" + num;
			}
			return m;
		}

		/// <summary>
		/// 
		/// </summary>
		void PostIsReady()
		{
			PostStatusMessage(new
			{
				isReady = true
			});
		}

		/// <summary>
		/// 
		/// </summary>
        void PostCallsList()
        {
            PostStatusMessage(new
            {
                calls = GetCurrentCallList(),
            });
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="s"></param>
		void SelectCamera(string s)
		{
			var cam = s.Substring(6);
			if (cam.ToLower() == "far")
			{
				EISC.SetUshort(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraNumberSelect), 100);
			}
			else
			{
				EISC.SetUshort(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.CameraNumberSelect), UInt16.Parse(cam));
			}
		}

		/// <summary>
		/// Turns the 
		/// </summary>
		/// <returns></returns>
		List<CodecActiveCallItem> GetCurrentCallList()
		{
			var list = new List<CodecActiveCallItem>();
			if (CurrentCallItem.Status != eCodecCallStatus.Disconnected)
			{
				list.Add(CurrentCallItem);
			}
			if (EISC.GetBool(JoinMap.GetJoinForKey(SIMPLVtcJoinMap.IncomingCall))) {

			}
			return list;
		}
	}
}