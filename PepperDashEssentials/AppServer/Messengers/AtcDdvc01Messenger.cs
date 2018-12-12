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
	public class Ddvc01AtcMessenger : MessengerBase
	{
		BasicTriList EISC;

		const uint BDialHangup = 221;
		const uint BIncomingAnswer = 251;
		const uint BIncomingReject = 252;
		const uint BSpeedDial1 = 241;
		const uint BSpeedDial2 = 242;
		const uint BSpeedDial3 = 243;
		const uint BSpeedDial4 = 244;

		/// <summary>
		/// 201
		/// </summary>
		const uint SCurrentDialString = 201;
		/// <summary>
		/// 211
		/// </summary>
		const uint SCurrentCallString = 211;
		/// <summary>
		/// 221
		/// </summary>
		const uint SHookState = 221;

		/// <summary>
		/// 
		/// </summary>
		Dictionary<string, uint> DTMFMap = new Dictionary<string, uint>
		{
			{ "1", 201 },
			{ "2", 202 },
			{ "3", 203 },
			{ "4", 204 },
			{ "5", 205 },
			{ "6", 206 },
			{ "7", 207 },
			{ "8", 208 },
			{ "9", 209 },
			{ "0", 210 },
			{ "*", 211 },
			{ "#", 212 },
		};

		CodecActiveCallItem CurrentCallItem;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="eisc"></param>
		/// <param name="messagePath"></param>
		public Ddvc01AtcMessenger(string key, BasicTriList eisc, string messagePath)
			: base(key, messagePath)
		{
			EISC = eisc;

			CurrentCallItem = new CodecActiveCallItem();
			CurrentCallItem.Type = eCodecCallType.Audio;
			CurrentCallItem.Id = "-audio-";
		}

		/// <summary>
		/// 
		/// </summary>
		void SendFullStatus()
		{
			this.PostStatusMessage(new
			{				
				calls = GetCurrentCallList(),
				callStatus = EISC.GetString(SHookState),
				currentCallString = EISC.GetString(SCurrentCallString),
				currentDialString = EISC.GetString(SCurrentDialString),
			});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="appServerController"></param>
		protected override void CustomRegisterWithAppServer(CotijaSystemController appServerController)
		{
			Action<object> send = this.PostStatusMessage;
			EISC.SetStringSigAction(SCurrentDialString, s => send(new { currentDialString = s }));

			EISC.SetStringSigAction(SHookState, s => 
			{
				CurrentCallItem.Status = (eCodecCallStatus)Enum.Parse(typeof(eCodecCallStatus), s, true);
				GetCurrentCallList();
				send(new 
				{ 
					calls = GetCurrentCallList(),
					callStatus = s 
				});
			});

			EISC.SetStringSigAction(SCurrentCallString, s => 
			{
				CurrentCallItem.Name = s;
				CurrentCallItem.Number = s;
				send(new 
				{
					calls = GetCurrentCallList(),
					currentCallString = s 			
				});
			});

			// Add press and holds using helper
			Action<string, uint> addPHAction = (s, u) => 
				AppServerController.AddAction(MessagePath + s, new PressAndHoldAction(b => EISC.SetBool(u, b)));

			// Add straight pulse calls
			Action<string, uint> addAction = (s, u) =>
				AppServerController.AddAction(MessagePath + s, new Action(() => EISC.PulseBool(u, 100)));
			addAction("/endCall", BDialHangup);
			addAction("/incomingAnswer", BIncomingAnswer);
			addAction("/incomingReject", BIncomingReject);
			addAction("/speedDial1", BSpeedDial1);
			addAction("/speedDial2", BSpeedDial2);
			addAction("/speedDial3", BSpeedDial3);
			addAction("/speedDial4", BSpeedDial4);

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
		}

		/// <summary>
		/// Turns the 
		/// </summary>
		/// <returns></returns>
		List<CodecActiveCallItem> GetCurrentCallList()
		{
			if (CurrentCallItem.Status == eCodecCallStatus.Disconnected)
			{
				return new List<CodecActiveCallItem>();
			}
			else
			{
				return new List<CodecActiveCallItem>() { CurrentCallItem };
			}
		}
	}
}