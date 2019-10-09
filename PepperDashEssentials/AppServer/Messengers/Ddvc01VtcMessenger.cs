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

namespace PepperDash.Essentials.AppServer.Messengers
{
	public class Ddvc01VtcMessenger : MessengerBase
	{
		BasicTriList EISC;

		/********* Bools *********/
		/// <summary>
		/// 724
		/// </summary>
		const uint BDialHangup = 724;
		/// <summary>
		/// 750
		/// </summary>
		const uint BCallIncoming = 750;
		/// <summary>
		/// 751
		/// </summary>
		const uint BIncomingAnswer = 751;
		/// <summary>
		/// 752
		/// </summary>
		const uint BIncomingReject = 752;
		/// <summary>
		/// 741
		/// </summary>
		const uint BSpeedDial1 = 741;
		/// <summary>
		/// 742
		/// </summary>
		const uint BSpeedDial2 = 742;
		/// <summary>
		/// 743
		/// </summary>
		const uint BSpeedDial3 = 743;
		/// <summary>
		/// 744
		/// </summary>
		const uint BSpeedDial4 = 744;
		/// <summary>
		/// 800
		/// </summary>
		const uint BDirectorySearchBusy = 800;
		/// <summary>
		/// 801 
		/// </summary>
		const uint BDirectoryLineSelected = 801;
		/// <summary>
		/// 801 when selected entry is a contact
		/// </summary>
		const uint BDirectoryEntryIsContact = 801;
		/// <summary>
		/// 802 To show/hide back button
		/// </summary>
		const uint BDirectoryIsRoot = 802;
		/// <summary>
		/// 803 Pulse from system to inform us when directory is ready
		/// </summary>
		const uint DDirectoryHasChanged = 803;
		/// <summary>
		/// 804
		/// </summary>
		const uint BDirectoryRoot = 804;
		/// <summary>
		/// 805
		/// </summary>
		const uint BDirectoryFolderBack = 805;
		/// <summary>
		/// 806
		/// </summary>
		const uint BDirectoryDialSelectedLine = 806;
		/// <summary>
		/// 811
		/// </summary>
		const uint BCameraControlUp = 811;
		/// <summary>
		/// 812
		/// </summary>
		const uint BCameraControlDown = 812;
		/// <summary>
		/// 813
		/// </summary>
		const uint BCameraControlLeft = 813;
		/// <summary>
		/// 814
		/// </summary>
		const uint BCameraControlRight = 814;
		/// <summary>
		/// 815
		/// </summary>
		const uint BCameraControlZoomIn = 815;
		/// <summary>
		/// 816
		/// </summary>
		const uint BCameraControlZoomOut = 816;
		/// <summary>
		/// 821 - 826
		/// </summary>
		const uint BCameraPresetStart = 821;

		/// <summary>
		/// 831
		/// </summary>
		const uint BCameraModeAuto = 831;
		/// <summary>
		/// 832
		/// </summary>
		const uint BCameraModeManual = 832;
		/// <summary>
		/// 833
		/// </summary>
		const uint BCameraModeOff = 833;

		/// <summary>
		/// 841
		/// </summary>
		const uint BCameraSelfView = 841;

		/// <summary>
		/// 842
		/// </summary>
		const uint BCameraLayout = 842;
		/// <summary>
		/// 843
		/// </summary>
		const uint BCameraSupportsAutoMode = 843;
		/// <summary>
		/// 844
		/// </summary>
		const uint BCameraSupportsOffMode = 844;


		/********* Ushorts *********/
		/// <summary>
		/// 760
		/// </summary>
		const uint UCameraNumberSelect = 760;
		/// <summary>
		/// 801
		/// </summary>
		const uint UDirectorySelectRow = 801;
		/// <summary>
		/// 801
		/// </summary>
		const uint UDirectoryRowCount = 801;



		/********* Strings *********/
		/// <summary>
		/// 701
		/// </summary>
		const uint SCurrentDialString = 701;
		/// <summary>
		/// 702
		/// </summary>
		const uint SCurrentCallName = 702;
        /// <summary>
		/// 703
        /// </summary>
		const uint SCurrentCallNumber = 703;
		/// <summary>
		/// 731
		/// </summary>
		const uint SHookState = 731;
        /// <summary>
		/// 722
        /// </summary>
        const uint SCallDirection = 722;
		/// <summary>
		/// 751
		/// </summary>
		const uint SIncomingCallName = 751;
		/// <summary>
		/// 752
		/// </summary>
		const uint SIncomingCallNumber = 752;

		/// <summary>
		/// 800
		/// </summary>
		const uint SDirectorySearchString = 800;
		/// <summary>
		/// 801-1055
		/// </summary>
		const uint SDirectoryEntriesStart = 801;
		/// <summary>
		/// 1056
		/// </summary>
		const uint SDirectoryEntrySelectedName = 1056;
		/// <summary>
		/// 1057
		/// </summary>
		const uint SDirectoryEntrySelectedNumber = 1057;
		/// <summary>
		/// 1058
		/// </summary>
		const uint SDirectorySelectedFolderName = 1058;


		/// <summary>
		/// 701-712 0-9*#
		/// </summary>
		Dictionary<string, uint> DTMFMap = new Dictionary<string, uint>
		{
			{ "1", 701 },
			{ "2", 702 },
			{ "3", 703 },
			{ "4", 704 },
			{ "5", 705 },
			{ "6", 706 },
			{ "7", 707 },
			{ "8", 708 },
			{ "9", 709 },
			{ "0", 710 },
			{ "*", 711 },
			{ "#", 712 },
		};

		CodecActiveCallItem CurrentCallItem;
		CodecActiveCallItem IncomingCallItem;

		ushort PreviousDirectoryLength = 0;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="eisc"></param>
		/// <param name="messagePath"></param>
		public Ddvc01VtcMessenger(string key, BasicTriList eisc, string messagePath)
			: base(key, messagePath)
		{
			EISC = eisc;

			CurrentCallItem = new CodecActiveCallItem();
			CurrentCallItem.Type = eCodecCallType.Video;
			CurrentCallItem.Id = "-video-";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="appServerController"></param>
		protected override void CustomRegisterWithAppServer(CotijaSystemController appServerController)
		{
			var asc = appServerController;
			EISC.SetStringSigAction(SHookState, s => 
			{
				CurrentCallItem.Status = (eCodecCallStatus)Enum.Parse(typeof(eCodecCallStatus), s, true);
				PostFullStatus(); // SendCallsList();
			});

			EISC.SetStringSigAction(SCurrentCallNumber, s => 
			{
				CurrentCallItem.Number = s;
                PostCallsList();
			});

            EISC.SetStringSigAction(SCurrentCallName, s =>
            {
                CurrentCallItem.Name = s;
                PostCallsList();
            });

            EISC.SetStringSigAction(SCallDirection, s =>
            {
                CurrentCallItem.Direction = (eCodecCallDirection)Enum.Parse(typeof(eCodecCallDirection), s, true);
                PostCallsList();
            });

			EISC.SetBoolSigAction(BCallIncoming, b =>
			{
				if (b)
				{
					var ica = new CodecActiveCallItem()
					{
						Direction = eCodecCallDirection.Incoming,
						Id = "-video-incoming",
						Name = EISC.GetString(SIncomingCallName),
						Number = EISC.GetString(SIncomingCallNumber),
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

			EISC.SetBoolSigAction(BCameraSupportsAutoMode, b =>
			{
				PostStatusMessage(new
				{	
					cameraSupportsAutoMode = b
				});
			});
			EISC.SetBoolSigAction(BCameraSupportsOffMode, b =>
			{
				PostStatusMessage(new
				{
					cameraSupportsOffMode = b
				});
			});

			// Directory insanity
			EISC.SetUShortSigAction(UDirectoryRowCount, u =>
			{
				// The length of the list comes in before the list does.
				// Splice the sig change operation onto the last string sig that will be changing
				// when the directory entries make it through.
				if (PreviousDirectoryLength > 0)
				{
					EISC.ClearStringSigAction(SDirectoryEntriesStart + PreviousDirectoryLength - 1);
				}
				EISC.SetStringSigAction(SDirectoryEntriesStart + u - 1, s => PostDirectory());
				PreviousDirectoryLength = u;
			});

			EISC.SetStringSigAction(SDirectoryEntrySelectedName, s =>
			{
				PostStatusMessage(new
				{
					directoryContactSelected = new
					{
						name = EISC.GetString(SDirectoryEntrySelectedName),
					}
				});
			});

			EISC.SetStringSigAction(SDirectoryEntrySelectedNumber, s =>
			{
				PostStatusMessage(new
				{
					directoryContactSelected = new
					{
						number = EISC.GetString(SDirectoryEntrySelectedNumber),
					}
				});
			});

			EISC.SetStringSigAction(SDirectorySelectedFolderName, s => PostStatusMessage(new
			{
				directorySelectedFolderName = EISC.GetString(SDirectorySelectedFolderName)
			}));

			EISC.SetSigTrueAction(BCameraModeAuto, () => PostCameraMode());
			EISC.SetSigTrueAction(BCameraModeManual, () => PostCameraMode());
			EISC.SetSigTrueAction(BCameraModeOff, () => PostCameraMode());

			EISC.SetBoolSigAction(BCameraSelfView, b => PostStatusMessage(new 
				{
					cameraSelfView = b
				}));

			EISC.SetUShortSigAction(UCameraNumberSelect, (u) => PostSelectedCamera());


			// Add press and holds using helper action
			Action<string, uint> addPHAction = (s, u) => 
				AppServerController.AddAction(MessagePath + s, new PressAndHoldAction(b => EISC.SetBool(u, b)));
			addPHAction("/cameraUp", BCameraControlUp);
			addPHAction("/cameraDown", BCameraControlDown);
			addPHAction("/cameraLeft", BCameraControlLeft);
			addPHAction("/cameraRight", BCameraControlRight);
			addPHAction("/cameraZoomIn", BCameraControlZoomIn);
			addPHAction("/cameraZoomOut", BCameraControlZoomOut);

			// Add straight pulse calls using helper action
			Action<string, uint> addAction = (s, u) =>
				AppServerController.AddAction(MessagePath + s, new Action(() => EISC.PulseBool(u, 100)));
			addAction("/endCallById", BDialHangup);
			addAction("/endAllCalls", BDialHangup);
            addAction("/acceptById", BIncomingAnswer);
            addAction("/rejectById", BIncomingReject);
			addAction("/speedDial1", BSpeedDial1);
			addAction("/speedDial2", BSpeedDial2);
			addAction("/speedDial3", BSpeedDial3);
			addAction("/speedDial4", BSpeedDial4);
			addAction("/cameraModeAuto", BCameraModeAuto);
			addAction("/cameraModeManual", BCameraModeManual);
			addAction("/cameraModeOff", BCameraModeOff);
			addAction("/cameraSelfView", BCameraSelfView);
			addAction("/cameraLayout", BCameraLayout);

			asc.AddAction("/cameraSelect", new Action<string>(SelectCamera));

			// camera presets
			for(uint i = 0; i < 6; i++) 
			{
				addAction("/cameraPreset" + (i + 1), BCameraPresetStart + i);
			}

			asc.AddAction(MessagePath + "/isReady", new Action(PostIsReady));
			// Get status
			asc.AddAction(MessagePath + "/fullStatus", new Action(PostFullStatus));
			// Dial on string
			asc.AddAction(MessagePath + "/dial", new Action<string>(s => 
				EISC.SetString(SCurrentDialString, s)));
			// Pulse DTMF
			asc.AddAction(MessagePath + "/dtmf", new Action<string>(s =>
			{
				if (DTMFMap.ContainsKey(s))
				{
					EISC.PulseBool(DTMFMap[s], 100);
				}
			}));

			// Directory madness
			asc.AddAction(MessagePath + "/directoryRoot", new Action(() => EISC.PulseBool(BDirectoryRoot)));
			asc.AddAction(MessagePath + "/directoryBack", new Action(() => EISC.PulseBool(BDirectoryFolderBack)));
			asc.AddAction(MessagePath + "/directoryById", new Action<string>(s =>
			{
				// the id should contain the line number to forward to simpl
				try
				{
					var u = ushort.Parse(s);
					EISC.SetUshort(UDirectorySelectRow, u);
					EISC.PulseBool(BDirectoryLineSelected);
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
					EISC.SetUshort(UDirectorySelectRow, u);
					EISC.PulseBool(BDirectoryLineSelected);
				}
				catch
				{
					
				}
			}));
			asc.AddAction(MessagePath + "/directoryDialContact", new Action(() => {
				EISC.PulseBool(BDirectoryDialSelectedLine);
			}));
			asc.AddAction(MessagePath + "/getDirectory", new Action(() =>
			{
				if (EISC.GetUshort(UDirectoryRowCount) > 0)
				{
					PostDirectory();
				}
				else
				{
					EISC.PulseBool(BDirectoryRoot);
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
				cameraSelfView = EISC.GetBool(BCameraSelfView),
				cameraSupportsAutoMode = EISC.GetBool(BCameraSupportsAutoMode),
				cameraSupportsOffMode = EISC.GetBool(BCameraSupportsOffMode),
				currentCallString = EISC.GetString(SCurrentCallNumber),
				currentDialString = EISC.GetString(SCurrentDialString),
				directoryContactSelected = new
				{
					name = EISC.GetString(SDirectoryEntrySelectedName),
					number = EISC.GetString(SDirectoryEntrySelectedNumber)
				},
				directorySelectedFolderName = EISC.GetString(SDirectorySelectedFolderName),
				isInCall = EISC.GetString(SHookState) == "Connected",
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
			var u = EISC.GetUshort(UDirectoryRowCount);
			var items = new List<object>();
			for (uint i = 0; i < u; i++)
			{
				var name = EISC.GetString(SDirectoryEntriesStart + i);
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
					isRootDirectory = EISC.GetBool(BDirectoryIsRoot),
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
			if (EISC.GetBool(BCameraModeAuto)) m = "auto";
			else if (EISC.GetBool(BCameraModeManual)) m = "manual";
			else m = "off";
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
			var num = EISC.GetUshort(UCameraNumberSelect);
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
				EISC.SetUshort(UCameraNumberSelect, 100);
			}
			else
			{
				EISC.SetUshort(UCameraNumberSelect, UInt16.Parse(cam));
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
			if (EISC.GetBool(BCallIncoming)) {

			}
			return list;
		}
	}
}