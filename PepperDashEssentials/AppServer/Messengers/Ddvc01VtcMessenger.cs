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

		/********* Ushorts *********/
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
		const uint SCurrentCallNumber = 702;
        /// <summary>
		/// 703
        /// </summary>
		const uint SCurrentCallName = 703;
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
		void SendFullStatus()
		{
			this.PostStatusMessage(new
			{				
				calls = GetCurrentCallList(),
				currentCallString = EISC.GetString(SCurrentCallNumber),
				currentDialString = EISC.GetString(SCurrentDialString),
                isInCall = EISC.GetString(SHookState) == "Connected",

			});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="appServerController"></param>
		protected override void CustomRegisterWithAppServer(CotijaSystemController appServerController)
		{
			EISC.SetStringSigAction(SHookState, s => 
			{
				CurrentCallItem.Status = (eCodecCallStatus)Enum.Parse(typeof(eCodecCallStatus), s, true);
				SendCallsList();
			});

			EISC.SetStringSigAction(SCurrentCallNumber, s => 
			{
				CurrentCallItem.Number = s;
                SendCallsList();
			});

            EISC.SetStringSigAction(SCurrentCallName, s =>
            {
                CurrentCallItem.Name = s;
                SendCallsList();
            });

            EISC.SetStringSigAction(SCallDirection, s =>
            {
                CurrentCallItem.Direction = (eCodecCallDirection)Enum.Parse(typeof(eCodecCallDirection), s, true);
                SendCallsList();
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
				SendCallsList();
			});

			// Directory insanity
			EISC.SetUShortSigAction(UDirectoryRowCount, u =>
			{
				var items = new List<object>();
				for (uint i = 0; i < u; i++)
				{
					var newItem = new
					{
						name = EISC.GetString(SDirectoryEntriesStart + i),
					};
					items.Add(newItem);
				}

				var directoryMessage = new { 
					content = new {
						currentDirectory = new {
							isRootDirectory = EISC.GetBool(BDirectoryIsRoot),
							directoryResults = items
						}
					}
				};
				PostStatusMessage(directoryMessage);
			});

			EISC.SetStringSigAction(SDirectoryEntrySelectedName, s =>
			{
				PostStatusMessage(new { content = new { directorySelectedEntryName = EISC.GetString(SDirectoryEntrySelectedName) } });
			});

			EISC.SetStringSigAction(SDirectoryEntrySelectedNumber, s =>
			{
				PostStatusMessage(new { content = new { directorySelectedEntryNumber = EISC.GetString(SDirectoryEntrySelectedNumber) } });
			});

			

			// Add press and holds using helper action
			Action<string, uint> addPHAction = (s, u) => 
				AppServerController.AddAction(MessagePath + s, new PressAndHoldAction(b => EISC.SetBool(u, b)));
			addPHAction("/cameraUp", BCameraControlUp);
			addPHAction("/cameraDown", BCameraControlDown);
			addPHAction("/cameraLeft", BCameraControlLeft);
			addPHAction("/cameraRight", BCameraControlRight);
			addPHAction("/cameraZoomIn", BCameraControlZoomIn);
			addPHAction("/cameraZoonOut", BCameraControlZoomOut);

			// Add straight pulse calls using helper action
			Action<string, uint> addAction = (s, u) =>
				AppServerController.AddAction(MessagePath + s, new Action(() => EISC.PulseBool(u, 100)));
            addAction("/endCallById", BDialHangup);
            addAction("/acceptById", BIncomingAnswer);
            addAction("/rejectById", BIncomingReject);
			addAction("/speedDial1", BSpeedDial1);
			addAction("/speedDial2", BSpeedDial2);
			addAction("/speedDial3", BSpeedDial3);
			addAction("/speedDial4", BSpeedDial4);
			for(uint i = 0; i < 6; i++) 
			{
				addAction("/cameraPreset" + (i + 1), BCameraPresetStart + i);
			}

			// Get status
			AppServerController.AddAction(MessagePath + "/fullStatus", new Action(SendFullStatus));
			// Dial on string
			AppServerController.AddAction(MessagePath + "/dial", new Action<string>(s => EISC.SetString(SCurrentDialString, s)));
			// Pulse DTMF
			AppServerController.AddAction(MessagePath + "/dtmf", new Action<string>(s =>
			{
				if (DTMFMap.ContainsKey(s))
				{
					EISC.PulseBool(DTMFMap[s], 100);
				}
			}));

			// Directory madness
			AppServerController.AddAction(MessagePath + "/directorySelectLine", new Action<ushort>(u => 
			{
				EISC.SetUshort(UDirectorySelectRow, u);
				EISC.PulseBool(BDirectoryLineSelected);
			}));

		}

        void SendCallsList()
        {
            PostStatusMessage(new
            {
                calls = GetCurrentCallList(),
            });
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