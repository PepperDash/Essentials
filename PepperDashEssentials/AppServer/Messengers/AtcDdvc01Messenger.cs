using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.EthernetCommunication;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
	public class Ddvc01AtcMessenger : MessengerBase
	{
		BasicTriList EISC;

		const uint BKeypad1 = 201;
		const uint BKeypad2 = 202;
		const uint BKeypad3 = 203;
		const uint BKeypad4 = 204;
		const uint BKeypad5 = 205;
		const uint BKeypad6 = 206;
		const uint BKeypad7 = 207;
		const uint BKeypad8 = 208;
		const uint BKeypad9 = 209;
		const uint BKeypad0 = 210;
		const uint BKeypadStar = 211;
		const uint BKeypadPound = 212;
		const uint BDialHangup = 221;
		const uint BIncomingAnswer = 251;
		const uint BIncomingReject = 252;
		const uint BSpeedDial1 = 241;
		const uint BSpeedDial2 = 242;
		const uint BSpeedDial3 = 243;
		const uint BSpeedDial4 = 244;

		const uint BIsOnHook = 222;
		const uint BIsOffHook = 224;
		const uint BDialHangupIsVisible = 251;
		const uint BCallIsIncoming = 254;
		const uint BSpeedDialIsVisible1 = 261;
		const uint BSpeedDialIsVisible2 = 262;
		const uint BSpeedDialIsVisible3 = 263;
		const uint BSpeedDialIsVisible4 = 264;


		const uint SCurrentDialString = 201;
		const uint SSpeedDialName1 = 241;
		const uint SSpeedDialName2 = 242;
		const uint SSpeedDialName3 = 243;
		const uint SSpeedDialName4 = 244;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="eisc"></param>
		/// <param name="messagePath"></param>
		public Ddvc01AtcMessenger(BasicTriList eisc, string messagePath)
			: base(messagePath)
		{
			EISC = eisc;

		}

		/// <summary>
		/// 
		/// </summary>
		void SendFullStatus()
		{
			this.PostStatusMessage(new
			{
				atc = new
				{
					callIsIncoming = EISC.GetBool(BCallIsIncoming),
					isOnHook = EISC.GetBool(BIsOnHook),
					isOffHook = EISC.GetBool(BIsOffHook),
					dialHangupIsVisible = EISC.GetBool(BDialHangupIsVisible),
					currentDialString = EISC.GetString(SCurrentDialString),
				}
			});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="appServerController"></param>
		protected override void CustomRegisterWithAppServer(CotijaSystemController appServerController)
		{
			Action<object> send = this.PostStatusMessage;
			EISC.SetBoolSigAction(BIsOffHook, b => send(new { isOffHook = b }));
			EISC.SetBoolSigAction(BIsOnHook, b => send(new { isOnHook = b }));
			EISC.SetBoolSigAction(BDialHangupIsVisible, b => send(new { dialHangupIsVisible = b }));
			EISC.SetBoolSigAction(BCallIsIncoming, b => send(new { callIsIncoming = b }));
			EISC.SetStringSigAction(SCurrentDialString, s => send(new { currentDialString = s }));

			// Add press and holds using helper
			Action<string, uint> addPHAction = (s, u) => 
				AppServerController.AddAction(MessagePath + s, new PressAndHoldAction(b => EISC.SetBool(u, b)));
			addPHAction("/dial1", BKeypad1);
			addPHAction("/dial2", BKeypad2);
			addPHAction("/dial3", BKeypad3);
			addPHAction("/dial4", BKeypad4);
			addPHAction("/dial5", BKeypad5);
			addPHAction("/dial6", BKeypad6);
			addPHAction("/dial7", BKeypad7);
			addPHAction("/dial8", BKeypad8);
			addPHAction("/dial9", BKeypad9);
			addPHAction("/dial0", BKeypad0);
			addPHAction("/dialStar", BKeypadStar);
			addPHAction("/dialPound", BKeypadPound);

			// Add straight calls
			Action<string, uint> addAction = (s, u) =>
				AppServerController.AddAction(MessagePath + s, new Action(() => EISC.PulseBool(u, 100)));
			addAction("/dialHangup", BDialHangup);
			addAction("/incomingAnswer", BIncomingAnswer);
			addAction("/incomingReject", BIncomingReject);
			addAction("/speedDial1", BSpeedDial1);
			addAction("/speedDial2", BSpeedDial2);
			addAction("/speedDial3", BSpeedDial3);
			addAction("/speedDial4", BSpeedDial4);

			AppServerController.AddAction(MessagePath + "/fullStatus", new Action(SendFullStatus));
		}
	}
}