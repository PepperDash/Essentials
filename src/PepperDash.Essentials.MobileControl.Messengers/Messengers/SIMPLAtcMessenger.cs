using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// SIMPL messenger for audio-only conference devices that provides audio calling functionality
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class SIMPLAtcMessenger : MessengerBase
    {
        /// <summary>
        /// The EISC (Ethernet Intersystem Communication) device
        /// </summary>
        private readonly BasicTriList _eisc;

        /// <summary>
        /// Gets the join map for SIMPL ATC operations
        /// </summary>
        public SIMPLAtcJoinMap JoinMap { get; private set; }

        /// <summary>
        /// The current active call item for audio calls
        /// </summary>
        private readonly CodecActiveCallItem _currentCallItem;

        /// <summary>
        /// Initializes a new instance of the SIMPLAtcMessenger class
        /// </summary>
        /// <param name="key">Unique identifier for the messenger</param>
        /// <param name="eisc">The EISC device for communication</param>
        /// <param name="messagePath">Path for message routing</param>
        public SIMPLAtcMessenger(string key, BasicTriList eisc, string messagePath)
            : base(key, messagePath)
        {
            _eisc = eisc;

            JoinMap = new SIMPLAtcJoinMap(201);

            _currentCallItem = new CodecActiveCallItem { Type = eCodecCallType.Audio, Id = "-audio-" };
        }

        /// <summary>
        /// Sends the current status of the audio conference device to connected clients
        /// </summary>
        private void SendFullStatus()
        {
            PostStatusMessage(JToken.FromObject(new
            {
                calls = GetCurrentCallList(),
                currentCallString = _eisc.GetString(JoinMap.CurrentCallName.JoinNumber),
                currentDialString = _eisc.GetString(JoinMap.CurrentDialString.JoinNumber),
                isInCall = _eisc.GetString(JoinMap.HookState.JoinNumber) == "Connected"
            })
            );
        }

        /// <summary>
        /// Registers actions and feedback handlers for the SIMPL ATC messenger
        /// </summary>
        protected override void RegisterActions()
        {
            //EISC.SetStringSigAction(SCurrentDialString, s => PostStatusMessage(new { currentDialString = s }));

            _eisc.SetStringSigAction(JoinMap.HookState.JoinNumber, s =>
            {
                _currentCallItem.Status = (eCodecCallStatus)Enum.Parse(typeof(eCodecCallStatus), s, true);
                //GetCurrentCallList();
                SendFullStatus();
            });

            _eisc.SetStringSigAction(JoinMap.CurrentCallNumber.JoinNumber, s =>
            {
                _currentCallItem.Number = s;
                SendCallsList();
            });

            _eisc.SetStringSigAction(JoinMap.CurrentCallName.JoinNumber, s =>
            {
                _currentCallItem.Name = s;
                SendCallsList();
            });

            _eisc.SetStringSigAction(JoinMap.CallDirection.JoinNumber, s =>
            {
                _currentCallItem.Direction = (eCodecCallDirection)Enum.Parse(typeof(eCodecCallDirection), s, true);
                SendCallsList();
            });

            // Add press and holds using helper
            //Action<string, uint> addPhAction = (s, u) => 
            //    AppServerController.AddAction(MessagePath + s, new PressAndHoldAction(b => _eisc.SetBool(u, b)));

            // Add straight pulse calls
            void addAction(string s, uint u) =>
                AddAction(s, (id, content) => _eisc.PulseBool(u, 100));
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
            AddAction("/fullStatus", (id, content) => SendFullStatus());
            // Dial on string
            AddAction("/dial",
                (id, content) =>
                {
                    var msg = content.ToObject<MobileControlSimpleContent<string>>();
                    _eisc.SetString(JoinMap.CurrentDialString.JoinNumber, msg.Value);
                });
            // Pulse DTMF
            AddAction("/dtmf", (id, content) =>
            {
                var s = content.ToObject<MobileControlSimpleContent<string>>();

                var join = JoinMap.Joins[s.Value];
                if (join != null)
                {
                    if (join.JoinNumber > 0)
                    {
                        _eisc.PulseBool(join.JoinNumber, 100);
                    }
                }
            });
        }

        /// <summary>
        /// Sends the current calls list to connected clients
        /// </summary>
        private void SendCallsList()
        {
            PostStatusMessage(JToken.FromObject(new
            {
                calls = GetCurrentCallList(),
            })
            );
        }

        /// <summary>
        /// Gets the current call list based on the call status
        /// </summary>
        /// <returns>A list containing the current call item if connected, or an empty list if disconnected</returns>
        private List<CodecActiveCallItem> GetCurrentCallList()
        {
            return _currentCallItem.Status == eCodecCallStatus.Disconnected
                ? new List<CodecActiveCallItem>()
                : new List<CodecActiveCallItem> { _currentCallItem };
        }
    }
}