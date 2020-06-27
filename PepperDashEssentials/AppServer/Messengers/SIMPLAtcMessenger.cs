using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.EthernetCommunication;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Devices.Codec;

namespace PepperDash.Essentials.AppServer.Messengers
{
	public class SIMPLAtcMessenger : MessengerBase
	{
		BasicTriList EISC;

        public SIMPLAtcJoinMap JoinMap {get; private set;}

        ///// <summary>
        ///// 221
        ///// </summary>
        //const uint BDialHangupOnHook = 221;

        ///// <summary>
        ///// 251
        ///// </summary>
        //const uint BIncomingAnswer = 251;
        ///// <summary>
        ///// 252
        ///// </summary>
        //const uint BIncomingReject = 252;
        ///// <summary>
        ///// 241
        ///// </summary>
        //const uint BSpeedDial1 = 241;
        ///// <summary>
        ///// 242
        ///// </summary>
        //const uint BSpeedDial2 = 242;
        ///// <summary>
        ///// 243
        ///// </summary>
        //const uint BSpeedDial3 = 243;
        ///// <summary>
        ///// 244
        ///// </summary>
        //const uint BSpeedDial4 = 244;

        ///// <summary>
        ///// 201
        ///// </summary>
        //const uint SCurrentDialString = 201;
        ///// <summary>
        ///// 211
        ///// </summary>
        //const uint SCurrentCallNumber = 211;
        ///// <summary>
        ///// 212
        ///// </summary>
        //const uint SCurrentCallName = 212;
        ///// <summary>
        ///// 221
        ///// </summary>
        //const uint SHookState = 221;
        ///// <summary>
        ///// 222
        ///// </summary>
        //const uint SCallDirection = 222;

        ///// <summary>
        ///// 201-212 0-9*#
        ///// </summary>
        //Dictionary<string, uint> DTMFMap = new Dictionary<string, uint>
        //{
        //    { "1", 201 },
        //    { "2", 202 },
        //    { "3", 203 },
        //    { "4", 204 },
        //    { "5", 205 },
        //    { "6", 206 },
        //    { "7", 207 },
        //    { "8", 208 },
        //    { "9", 209 },
        //    { "0", 210 },
        //    { "*", 211 },
        //    { "#", 212 },
        //};

		/// <summary>
		/// 
		/// </summary>
		CodecActiveCallItem CurrentCallItem;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="eisc"></param>
		/// <param name="messagePath"></param>
		public SIMPLAtcMessenger(string key, BasicTriList eisc, string messagePath)
			: base(key, messagePath)
		{
            EISC = eisc;

            JoinMap = new SIMPLAtcJoinMap(201);

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
				currentCallString = EISC.GetString(JoinMap.CurrentCallName.JoinNumber),
				currentDialString = EISC.GetString(JoinMap.CurrentDialString.JoinNumber),
                isInCall = EISC.GetString(JoinMap.HookState.JoinNumber) == "Connected"
			});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="appServerController"></param>
		protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
		{
            //EISC.SetStringSigAction(SCurrentDialString, s => PostStatusMessage(new { currentDialString = s }));

            EISC.SetStringSigAction(JoinMap.HookState.JoinNumber, s => 
			{
				CurrentCallItem.Status = (eCodecCallStatus)Enum.Parse(typeof(eCodecCallStatus), s, true);
                //GetCurrentCallList();
				SendFullStatus();
			});

            EISC.SetStringSigAction(JoinMap.CurrentCallNumber.JoinNumber, s => 
			{
				CurrentCallItem.Number = s;
                SendCallsList();
			});

            EISC.SetStringSigAction(JoinMap.CurrentCallName.JoinNumber, s =>
            {
                CurrentCallItem.Name = s;
                SendCallsList();
            });

            EISC.SetStringSigAction(JoinMap.CallDirection.JoinNumber, s =>
            {
                CurrentCallItem.Direction = (eCodecCallDirection)Enum.Parse(typeof(eCodecCallDirection), s, true);
                SendCallsList();
            });

			// Add press and holds using helper
			Action<string, uint> addPHAction = (s, u) => 
				AppServerController.AddAction(MessagePath + s, new PressAndHoldAction(b => EISC.SetBool(u, b)));

			// Add straight pulse calls
			Action<string, uint> addAction = (s, u) =>
				AppServerController.AddAction(MessagePath + s, new Action(() => EISC.PulseBool(u, 100)));
            addAction("/endCallById", JoinMap.EndCall.JoinNumber);
            addAction("/endAllCalls", JoinMap.EndCall.JoinNumber);
            addAction("/acceptById", JoinMap.IncomingAnswer.JoinNumber);
            addAction("/rejectById", JoinMap.IncomingReject.JoinNumber);

            var speeddialStart = JoinMap.SpeedDialStart.JoinNumber;
            var speeddialEnd = JoinMap.SpeedDialStart.JoinNumber + JoinMap.SpeedDialStart.JoinSpan;

            var speedDialIndex = 1;
            for (uint i = speeddialStart; i < speeddialEnd; i++)
            {
                addAction(string.Format("/speedDial{0}", speedDialIndex), i);
                speedDialIndex++;
            }

			// Get status
			AppServerController.AddAction(MessagePath + "/fullStatus", new Action(SendFullStatus));
			// Dial on string
            AppServerController.AddAction(MessagePath + "/dial", new Action<string>(s => EISC.SetString(JoinMap.CurrentDialString.JoinNumber, s)));
			// Pulse DTMF
			AppServerController.AddAction(MessagePath + "/dtmf", new Action<string>(s =>
			{
                var join = JoinMap.Joins[s];           
                if (join != null)
                {
                    if (join.JoinNumber > 0)
                    {
                        EISC.PulseBool(join.JoinNumber, 100);
                    }
                }
			}));
		}

		/// <summary>
		/// 
		/// </summary>
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