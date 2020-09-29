using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Ssh;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Core.Intersystem;
using PepperDash.Core.Intersystem.Tokens;
using PepperDash.Core.WebApi.Presets;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Devices.Common.Cameras;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces;
using PepperDash_Essentials_Core.Bridges.JoinMaps;
using Feedback = PepperDash.Essentials.Core.Feedback;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    public abstract class VideoCodecBase : ReconfigurableDevice, IRoutingInputsOutputs,
        IUsageTracking, IHasDialer, IHasContentSharing, ICodecAudio, iVideoCodecInfo, IBridgeAdvanced
    {
        private const int XSigEncoding = 28591;
        private readonly byte[] _clearBytes = XSigHelpers.ClearOutputs();
        protected VideoCodecBase(DeviceConfig config)
            : base(config)
        {
            
            StandbyIsOnFeedback = new BoolFeedback(StandbyIsOnFeedbackFunc);
            PrivacyModeIsOnFeedback = new BoolFeedback(PrivacyModeIsOnFeedbackFunc);
            VolumeLevelFeedback = new IntFeedback(VolumeLevelFeedbackFunc);
            MuteFeedback = new BoolFeedback(MuteFeedbackFunc);
            SharingSourceFeedback = new StringFeedback(SharingSourceFeedbackFunc);
            SharingContentIsOnFeedback = new BoolFeedback(SharingContentIsOnFeedbackFunc);

            InputPorts = new RoutingPortCollection<RoutingInputPort>();
            OutputPorts = new RoutingPortCollection<RoutingOutputPort>();

            ActiveCalls = new List<CodecActiveCallItem>();
        }

        public IBasicCommunication Communication { get; protected set; }

        /// <summary>
        /// An internal pseudo-source that is routable and connected to the osd input
        /// </summary>
        public DummyRoutingInputsDevice OsdSource { get; protected set; }

        public BoolFeedback StandbyIsOnFeedback { get; private set; }

        protected abstract Func<bool> PrivacyModeIsOnFeedbackFunc { get; }
        protected abstract Func<int> VolumeLevelFeedbackFunc { get; }
        protected abstract Func<bool> MuteFeedbackFunc { get; }
        protected abstract Func<bool> StandbyIsOnFeedbackFunc { get; }

        public List<CodecActiveCallItem> ActiveCalls { get; set; }

        public bool ShowSelfViewByDefault { get; protected set; }


        public bool IsReady { get; protected set; }

        public virtual List<Feedback> Feedbacks
        {
            get
            {
                return new List<Feedback>
                {
                    PrivacyModeIsOnFeedback,
                    SharingSourceFeedback
                };
            }
        }

        protected abstract Func<string> SharingSourceFeedbackFunc { get; }
        protected abstract Func<bool> SharingContentIsOnFeedbackFunc { get; }

        #region ICodecAudio Members

        public abstract void PrivacyModeOn();
        public abstract void PrivacyModeOff();
        public abstract void PrivacyModeToggle();
        public BoolFeedback PrivacyModeIsOnFeedback { get; private set; }


        public BoolFeedback MuteFeedback { get; private set; }

        public abstract void MuteOff();

        public abstract void MuteOn();

        public abstract void SetVolume(ushort level);

        public IntFeedback VolumeLevelFeedback { get; private set; }

        public abstract void MuteToggle();

        public abstract void VolumeDown(bool pressRelease);


        public abstract void VolumeUp(bool pressRelease);

        #endregion

        #region IHasContentSharing Members

        public abstract void StartSharing();
        public abstract void StopSharing();

        public bool AutoShareContentWhileInCall { get; protected set; }

        public StringFeedback SharingSourceFeedback { get; private set; }
        public BoolFeedback SharingContentIsOnFeedback { get; private set; }

        #endregion

        #region IHasDialer Members

        /// <summary>
        /// Fires when the status of any active, dialing, or incoming call changes or is new
        /// </summary>
        public event EventHandler<CodecCallStatusItemChangeEventArgs> CallStatusChange;

        /// <summary>
        /// Returns true when any call is not in state Unknown, Disconnecting, Disconnected
        /// </summary>
        public bool IsInCall
        {
            get
            {
                bool value;

                if (ActiveCalls != null)
                {
                    value = ActiveCalls.Any(c => c.IsActiveCall);
                }
                else
                {
                    value = false;
                }
                return value;
            }
        }

        public abstract void Dial(string number);
        public abstract void EndCall(CodecActiveCallItem call);
        public abstract void EndAllCalls();
        public abstract void AcceptCall(CodecActiveCallItem call);
        public abstract void RejectCall(CodecActiveCallItem call);
        public abstract void SendDtmf(string s);

        #endregion

        #region IRoutingInputsOutputs Members

        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

        #endregion

        #region IUsageTracking Members

        /// <summary>
        /// This object can be added by outside users of this class to provide usage tracking
        /// for various services
        /// </summary>
        public UsageTracking UsageTracker { get; set; }

        #endregion

        #region iVideoCodecInfo Members

        public VideoCodecInfo CodecInfo { get; protected set; }

        #endregion

        public event EventHandler<EventArgs> IsReadyChange;
        public abstract void Dial(Meeting meeting);

        public virtual void Dial(IInvitableContact contact)
        {
        }

        public abstract void ExecuteSwitch(object selector);

        /// <summary>
        /// Helper method to fire CallStatusChange event with old and new status
        /// </summary>
        protected void SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus newStatus, CodecActiveCallItem call)
        {
            call.Status = newStatus;

            OnCallStatusChange(call);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="previousStatus"></param>
        /// <param name="newStatus"></param>
        /// <param name="item"></param>
        protected void OnCallStatusChange(CodecActiveCallItem item)
        {
            var handler = CallStatusChange;
            if (handler != null)
            {
                handler(this, new CodecCallStatusItemChangeEventArgs(item));
            }

            if (AutoShareContentWhileInCall)
            {
                StartSharing();
            }

            if (UsageTracker != null)
            {
                if (IsInCall && !UsageTracker.UsageTrackingStarted)
                {
                    UsageTracker.StartDeviceUsage();
                }
                else if (UsageTracker.UsageTrackingStarted && !IsInCall)
                {
                    UsageTracker.EndDeviceUsage();
                }
            }
        }

        /// <summary>
        /// Sets IsReady property and fires the event. Used for dependent classes to sync up their data.
        /// </summary>
        protected void SetIsReady()
        {
            IsReady = true;
            var h = IsReadyChange;
            if (h != null)
            {
                h(this, new EventArgs());
            }
        }

        // **** DEBUGGING THINGS ****
        /// <summary>
        /// 
        /// </summary>
        public virtual void ListCalls()
        {
            var sb = new StringBuilder();
            foreach (var c in ActiveCalls)
            {
                sb.AppendFormat("{0} {1} -- {2} {3}\n", c.Id, c.Number, c.Name, c.Status);
            }
            Debug.Console(1, this, "\n{0}\n", sb.ToString());
        }

        public abstract void StandbyActivate();

        public abstract void StandbyDeactivate();

        #region Implementation of IBridgeAdvanced

        public abstract void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge);

        protected void LinkVideoCodecToApi(VideoCodecBase codec, BasicTriList trilist, uint joinStart, string joinMapKey,
            EiscApiAdvanced bridge)
        {
            var joinMap = new VideoCodecControllerJoinMap(joinStart);

            var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);

            if (customJoins != null)
            {
                joinMap.SetCustomJoinData(customJoins);
            }

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }

            Debug.Console(1, this, "Linking to Trilist {0}", trilist.ID.ToString("X"));

            

            LinkVideoCodecDtmfToApi(trilist, joinMap);

            LinkVideoCodecCallControlsToApi(trilist, joinMap);

            LinkVideoCodecContentSharingToApi(trilist, joinMap);

            LinkVideoCodecPrivacyToApi(trilist, joinMap);

            LinkVideoCodecVolumeToApi(trilist, joinMap);

            if (codec is ICommunicationMonitor)
            {
                LinkVideoCodecCommMonitorToApi(codec as ICommunicationMonitor, trilist, joinMap);
            }

            if (codec is IHasCodecCameras)
            {
                LinkVideoCodecCameraToApi(codec as IHasCodecCameras, trilist, joinMap);
            }

            if (codec is IHasCodecSelfView)
            {
                LinkVideoCodecSelfviewToApi(codec as IHasCodecSelfView, trilist, joinMap);
            }

            if (codec is IHasCameraAutoMode)
            {
                trilist.SetBool(joinMap.CameraSupportsAutoMode.JoinNumber, true);
                LinkVideoCodecCameraModeToApi(codec as IHasCameraAutoMode, trilist, joinMap);
            }

            if (codec is IHasCodecLayouts)
            {
                LinkVideoCodecCameraLayoutsToApi(codec as IHasCodecLayouts, trilist, joinMap);
            }

            if (codec is IHasDirectory)
            {
                LinkVideoCodecDirectoryToApi(codec as IHasDirectory, trilist, joinMap);
            }

            if (codec is IHasScheduleAwareness)
            {
                LinkVideoCodecScheduleToApi(codec as IHasScheduleAwareness, trilist, joinMap);
            }

            if (codec is IHasParticipants)
            {
                LinkVideoCodecParticipantsToApi(codec as IHasParticipants, trilist, joinMap);
            }

            trilist.OnlineStatusChange += (device, args) =>
            {
                if (!args.DeviceOnLine) return;

                if (codec is IHasDirectory)
                {
                    (codec as IHasDirectory).SetCurrentDirectoryToRoot();
                }

                if (codec is IHasScheduleAwareness)
                {
                    (codec as IHasScheduleAwareness).GetSchedule();
                }

                if (codec is IHasParticipants)
                {
                    UpdateParticipantsXSig((codec as IHasParticipants).Participants.CurrentParticipants);
                }

                if (codec is IHasCameraAutoMode)
                {
                    trilist.SetBool(joinMap.CameraSupportsAutoMode.JoinNumber, true);

                    (codec as IHasCameraAutoMode).CameraAutoModeIsOnFeedback.InvokeFireUpdate();
                }

                if (codec is IHasCodecSelfView)
                {
                    (codec as IHasCodecSelfView).SelfviewIsOnFeedback.InvokeFireUpdate();
                }

                SharingContentIsOnFeedback.InvokeFireUpdate();

                UpdateCallStatusXSig();
            };
        }

        private void LinkVideoCodecVolumeToApi(BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
        {
            MuteFeedback.LinkInputSig(trilist.BooleanInput[joinMap.VolumeMuteOn.JoinNumber]);
            MuteFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.VolumeMuteOff.JoinNumber]);

            trilist.SetSigFalseAction(joinMap.VolumeMuteOn.JoinNumber, MuteOn);
            trilist.SetSigFalseAction(joinMap.VolumeMuteOff.JoinNumber, MuteOff);
            trilist.SetSigFalseAction(joinMap.VolumeMuteToggle.JoinNumber, MuteToggle);

            VolumeLevelFeedback.LinkInputSig(trilist.UShortInput[joinMap.VolumeLevel.JoinNumber]);

            trilist.SetBoolSigAction(joinMap.VolumeUp.JoinNumber, VolumeUp);
            trilist.SetBoolSigAction(joinMap.VolumeDown.JoinNumber, VolumeDown);

            trilist.SetUShortSigAction(joinMap.VolumeLevel.JoinNumber, SetVolume);

        }

        private void LinkVideoCodecPrivacyToApi(BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
        {
            PrivacyModeIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.MicMuteOn.JoinNumber]);
            PrivacyModeIsOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.MicMuteOff.JoinNumber]);

            trilist.SetSigFalseAction(joinMap.MicMuteOn.JoinNumber, PrivacyModeOn);
            trilist.SetSigFalseAction(joinMap.MicMuteOff.JoinNumber, PrivacyModeOff);
            trilist.SetSigFalseAction(joinMap.MicMuteToggle.JoinNumber, PrivacyModeToggle);
        }

        private void LinkVideoCodecCommMonitorToApi(ICommunicationMonitor codec, BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
        {
            codec.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
        }

        private void LinkVideoCodecParticipantsToApi(IHasParticipants codec, BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
        {
            codec.Participants.ParticipantsListHasChanged += (sender, args) =>
            {
                string participantsXSig;
                if (codec.Participants.CurrentParticipants.Count == 0)
                {
                    participantsXSig = Encoding.GetEncoding(XSigEncoding).GetString(_clearBytes, 0, _clearBytes.Length);
                    trilist.SetString(joinMap.CurrentParticipants.JoinNumber, participantsXSig);
                    trilist.SetUshort(joinMap.ParticipantCount.JoinNumber, (ushort)codec.Participants.CurrentParticipants.Count);
                    return;
                }

                participantsXSig = Encoding.GetEncoding(XSigEncoding).GetString(_clearBytes, 0, _clearBytes.Length);

                trilist.SetString(joinMap.CurrentParticipants.JoinNumber, participantsXSig);

                participantsXSig = UpdateParticipantsXSig(codec.Participants.CurrentParticipants);

                trilist.SetString(joinMap.CurrentParticipants.JoinNumber, participantsXSig);

                trilist.SetUshort(joinMap.ParticipantCount.JoinNumber, (ushort) codec.Participants.CurrentParticipants.Count);
            };
        }

        private string UpdateParticipantsXSig(List<Participant> currentParticipants)
        {
            const int maxParticipants = 255;
            const int maxDigitals = 5;
            const int maxStrings = 1;
            const int offset = maxDigitals + maxStrings;
            var digitalIndex = maxStrings * maxParticipants; //15
            var stringIndex = 0;
            var meetingIndex = 0;

            var tokenArray = new XSigToken[maxParticipants * offset];

            foreach (var participant in currentParticipants)
            {
                if (meetingIndex > maxParticipants * offset) break;

                //digitals
                tokenArray[digitalIndex] = new XSigDigitalToken(digitalIndex + 1, participant.AudioMuteFb);
                tokenArray[digitalIndex + 1] = new XSigDigitalToken(digitalIndex + 2, participant.VideoMuteFb);
                tokenArray[digitalIndex + 2] = new XSigDigitalToken(digitalIndex + 3, participant.CanMuteVideo);
                tokenArray[digitalIndex + 3] = new XSigDigitalToken(digitalIndex + 4, participant.CanUnmuteVideo);
                tokenArray[digitalIndex + 4] = new XSigDigitalToken(digitalIndex + 5, participant.IsHost);

                //serials
                tokenArray[stringIndex] = new XSigSerialToken(stringIndex + 1, participant.Name);

                digitalIndex += maxDigitals;
                meetingIndex += offset;
                stringIndex += maxStrings;
            }
            
            return GetXSigString(tokenArray);
        }

        private void LinkVideoCodecContentSharingToApi(BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
        {
            SharingContentIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.SourceShareStart.JoinNumber]);
            SharingContentIsOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.SourceShareEnd.JoinNumber]);

            SharingSourceFeedback.LinkInputSig(trilist.StringInput[joinMap.CurrentSource.JoinNumber]);

            trilist.SetSigFalseAction(joinMap.SourceShareStart.JoinNumber, StartSharing);
            trilist.SetSigFalseAction(joinMap.SourceShareEnd.JoinNumber, StopSharing);

            trilist.SetBoolSigAction(joinMap.SourceShareAutoStart.JoinNumber, (b) => AutoShareContentWhileInCall = b);
        }

        private void LinkVideoCodecScheduleToApi(IHasScheduleAwareness codec, BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
        {
            trilist.SetSigFalseAction(joinMap.UpdateMeetings.JoinNumber, codec.GetSchedule);

            trilist.SetUShortSigAction(joinMap.MinutesBeforeMeetingStart.JoinNumber, (i) =>
            {
                codec.CodecSchedule.MeetingWarningMinutes = i;
            });

            codec.CodecSchedule.MeetingsListHasChanged += (sender, args) =>
            {
                var clearBytes = XSigHelpers.ClearOutputs();

                trilist.SetString(joinMap.Schedule.JoinNumber,
                    Encoding.GetEncoding(XSigEncoding).GetString(clearBytes, 0, clearBytes.Length));

                var meetingsData = UpdateMeetingsListXSig(codec.CodecSchedule.Meetings);

                trilist.SetString(joinMap.Schedule.JoinNumber, meetingsData);

                trilist.SetSigFalseAction(joinMap.DialMeeting.JoinNumber, () =>
                {
                    if (codec.CodecSchedule.Meetings[0].Joinable)
                    {
                        Dial(codec.CodecSchedule.Meetings[0]);
                    }
                });

                trilist.SetUshort(joinMap.MeetingCount.JoinNumber, (ushort) codec.CodecSchedule.Meetings.Count);
            };
        }

        private string UpdateMeetingsListXSig(List<Meeting> meetings)
        {
            /*const int maxCalls = 8;
            const int maxStrings = 5;
            const int offset = 6;
            var callIndex = 0;
            var digitalIndex = maxStrings*maxCalls;
             */

            const int maxMeetings = 3;
            const int maxDigitals = 1;
            const int maxStrings = 4;
            const int offset = maxDigitals + maxStrings;
            var digitalIndex = maxStrings*maxMeetings; //15
            var stringIndex = 0;
            var meetingIndex = 0;

            var tokenArray = new XSigToken[maxMeetings*offset];
            /* 
             * Digitals
             * IsJoinable - 1
             * 
             * Serials
             * Organizer - 1
             * Title - 2
             * Start Time - 3
             * End Time - 4
            */
            
            foreach(var meeting in meetings)
            {
                var currentTime = DateTime.Now;
                
                if(meeting.StartTime < currentTime && meeting.EndTime < currentTime) continue;
                
                if (meetingIndex > maxMeetings*offset) break;
                
                //digitals
                tokenArray[digitalIndex] = new XSigDigitalToken(digitalIndex + 1, meeting.Joinable);

                //serials
                tokenArray[stringIndex] = new XSigSerialToken(stringIndex + 1, meeting.Organizer);
                tokenArray[stringIndex + 1] = new XSigSerialToken(stringIndex + 2, meeting.Title);
                tokenArray[stringIndex + 2] = new XSigSerialToken(stringIndex + 3, meeting.StartTime.ToString("MM/dd/yyyy h:mm"));
                tokenArray[stringIndex + 3] = new XSigSerialToken(stringIndex + 4, meeting.EndTime.ToString("MM/dd/yyyy h:mm"));

                digitalIndex += maxDigitals;
                meetingIndex += offset;
                stringIndex += maxStrings;
            }

            return GetXSigString(tokenArray);
        }

        private void LinkVideoCodecDirectoryToApi(IHasDirectory codec, BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
        {
            codec.CurrentDirectoryResultIsNotDirectoryRoot.LinkComplementInputSig(
                trilist.BooleanInput[joinMap.DirectoryIsRoot.JoinNumber]);

            trilist.SetSigFalseAction(joinMap.DirectoryRoot.JoinNumber, codec.SetCurrentDirectoryToRoot);

            trilist.SetStringSigAction(joinMap.DirectorySearchString.JoinNumber, codec.SearchDirectory);

            trilist.SetUShortSigAction(joinMap.DirectorySelectRow.JoinNumber, (i) => SelectDirectoryEntry(codec, i));

            trilist.SetSigFalseAction(joinMap.DirectoryRoot.JoinNumber, codec.SetCurrentDirectoryToRoot);

            trilist.SetSigFalseAction(joinMap.DirectoryFolderBack.JoinNumber, codec.GetDirectoryParentFolderContents);

            codec.DirectoryResultReturned += (sender, args) =>
            {
                trilist.SetUshort(joinMap.DirectoryRowCount.JoinNumber, (ushort) args.Directory.CurrentDirectoryResults.Count);

                var clearBytes = XSigHelpers.ClearOutputs();

                trilist.SetString(joinMap.DirectoryEntries.JoinNumber,
                    Encoding.GetEncoding(XSigEncoding).GetString(clearBytes, 0, clearBytes.Length));
                var directoryXSig = UpdateDirectoryXSig(args.Directory, !codec.CurrentDirectoryResultIsNotDirectoryRoot.BoolValue);
                
                trilist.SetString(joinMap.DirectoryEntries.JoinNumber, directoryXSig);
            };
        }

        private void SelectDirectoryEntry(IHasDirectory codec, ushort i)
        {
            var entry = codec.CurrentDirectoryResult.CurrentDirectoryResults[i - 1];

            if (entry is DirectoryFolder)
            {
                codec.GetDirectoryFolderContents(entry.FolderId);
                return;
            }

            var dialableEntry = entry as IInvitableContact;

            if (dialableEntry != null)
            {
                Dial(dialableEntry);
                return;
            }

            var entryToDial = entry as DirectoryContact;

            if (entryToDial == null) return;

            Dial(entryToDial.ContactMethods[0].Number);
        }

        private string UpdateDirectoryXSig(CodecDirectory directory, bool isRoot)
        {
            var contactIndex = 1;
            var tokenArray = new XSigToken[directory.CurrentDirectoryResults.Count];

            foreach(var entry in directory.CurrentDirectoryResults)
            {
                var arrayIndex = contactIndex - 1;

                if (entry is DirectoryFolder && entry.ParentFolderId == "root")
                {
                    tokenArray[arrayIndex] = new XSigSerialToken(contactIndex, String.Format("[+] {0}", entry.Name));

                    contactIndex++;

                    continue;
                }

                if(isRoot && String.IsNullOrEmpty(entry.FolderId)) continue;
               
                tokenArray[arrayIndex] = new XSigSerialToken(contactIndex, entry.Name);

                contactIndex++;
            }

            return GetXSigString(tokenArray);
        }

        private void LinkVideoCodecCallControlsToApi(BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
        {
            trilist.SetSigFalseAction(joinMap.ManualDial.JoinNumber,
                () => Dial(trilist.StringOutput[joinMap.CurrentDialString.JoinNumber].StringValue));

            //End All calls for now
            trilist.SetSigFalseAction(joinMap.EndCall.JoinNumber, EndAllCalls);

            CallStatusChange += (sender, args) =>
            {
                trilist.SetBool(joinMap.HookState.JoinNumber, IsInCall);

                trilist.SetBool(joinMap.IncomingCall.JoinNumber, args.CallItem.Direction == eCodecCallDirection.Incoming);

                if (args.CallItem.Direction == eCodecCallDirection.Incoming)
                {
                    trilist.SetSigFalseAction(joinMap.IncomingAnswer.JoinNumber, () => AcceptCall(args.CallItem));
                    trilist.SetSigFalseAction(joinMap.IncomingReject.JoinNumber, () => RejectCall(args.CallItem));
                }

                var callStatusXsig = UpdateCallStatusXSig();

                trilist.SetString(joinMap.CurrentCallData.JoinNumber, callStatusXsig);
            };
        }

        private string UpdateCallStatusXSig()
        {
            const int maxCalls = 8;
            const int maxStrings = 5;
            const int offset = 6;
            var callIndex = 0;
            var digitalIndex = maxStrings*maxCalls;
            

            var tokenArray = new XSigToken[ActiveCalls.Count*offset]; //set array size for number of calls * pieces of info

            foreach (var call in ActiveCalls)
            {
                var arrayIndex = callIndex;
                //digitals
                tokenArray[arrayIndex] = new XSigDigitalToken(digitalIndex + 1, call.IsActiveCall);

                //serials
                tokenArray[arrayIndex + 1] = new XSigSerialToken(callIndex + 1, call.Name ?? String.Empty);
                tokenArray[arrayIndex + 2] = new XSigSerialToken(callIndex + 2, call.Number ?? String.Empty);
                tokenArray[arrayIndex + 3] = new XSigSerialToken(callIndex + 3, call.Direction.ToString());
                tokenArray[arrayIndex + 4] = new XSigSerialToken(callIndex + 4, call.Type.ToString());
                tokenArray[arrayIndex + 5] = new XSigSerialToken(callIndex + 5, call.Status.ToString());

                callIndex += offset;
                digitalIndex++;
            }

            return GetXSigString(tokenArray);
        }

        private void LinkVideoCodecDtmfToApi(BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
        {
            trilist.SetSigFalseAction(joinMap.Dtmf0.JoinNumber, () => SendDtmf("0"));
            trilist.SetSigFalseAction(joinMap.Dtmf1.JoinNumber, () => SendDtmf("1"));
            trilist.SetSigFalseAction(joinMap.Dtmf2.JoinNumber, () => SendDtmf("2"));
            trilist.SetSigFalseAction(joinMap.Dtmf3.JoinNumber, () => SendDtmf("3"));
            trilist.SetSigFalseAction(joinMap.Dtmf4.JoinNumber, () => SendDtmf("4"));
            trilist.SetSigFalseAction(joinMap.Dtmf5.JoinNumber, () => SendDtmf("5"));
            trilist.SetSigFalseAction(joinMap.Dtmf6.JoinNumber, () => SendDtmf("6"));
            trilist.SetSigFalseAction(joinMap.Dtmf7.JoinNumber, () => SendDtmf("7"));
            trilist.SetSigFalseAction(joinMap.Dtmf8.JoinNumber, () => SendDtmf("8"));
            trilist.SetSigFalseAction(joinMap.Dtmf9.JoinNumber, () => SendDtmf("9"));
            trilist.SetSigFalseAction(joinMap.DtmfStar.JoinNumber, () => SendDtmf("*"));
            trilist.SetSigFalseAction(joinMap.DtmfPound.JoinNumber, () => SendDtmf("#"));
        }

        private void LinkVideoCodecCameraLayoutsToApi(IHasCodecLayouts codec, BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
        {
            trilist.SetSigFalseAction(joinMap.CameraLayout.JoinNumber, codec.LocalLayoutToggle);

            codec.LocalLayoutFeedback.LinkInputSig(trilist.StringInput[joinMap.CameraLayoutStringFb.JoinNumber]);
        }

        private void LinkVideoCodecCameraModeToApi(IHasCameraAutoMode codec, BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
        {
            trilist.SetSigFalseAction(joinMap.CameraModeAuto.JoinNumber, codec.CameraAutoModeOn);
            trilist.SetSigFalseAction(joinMap.CameraModeManual.JoinNumber, codec.CameraAutoModeOff);
            
            codec.CameraAutoModeIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.CameraModeAuto.JoinNumber]);
            codec.CameraAutoModeIsOnFeedback.LinkComplementInputSig(
                trilist.BooleanInput[joinMap.CameraModeManual.JoinNumber]);
        }

        private void LinkVideoCodecSelfviewToApi(IHasCodecSelfView codec, BasicTriList trilist,
            VideoCodecControllerJoinMap joinMap)
        {
            trilist.SetSigFalseAction(joinMap.CameraSelfView.JoinNumber, codec.SelfViewModeToggle);

            codec.SelfviewIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.CameraSelfView.JoinNumber]);
        }

        private void LinkVideoCodecCameraToApi(IHasCodecCameras codec, BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
        {
            //Camera PTZ
            trilist.SetBoolSigAction(joinMap.CameraTiltUp.JoinNumber, (b) =>
            {
                if (codec.SelectedCamera == null) return;
                var camera = codec.SelectedCamera as IHasCameraPtzControl;

                if (camera == null) return;

                if (b) camera.TiltUp();
                else camera.TiltStop();
            });

            trilist.SetBoolSigAction(joinMap.CameraTiltDown.JoinNumber, (b) =>
            {
                if (codec.SelectedCamera == null) return;
                var camera = codec.SelectedCamera as IHasCameraPtzControl;

                if (camera == null) return;

                if (b) camera.TiltDown();
                else camera.TiltStop();
            });
            trilist.SetBoolSigAction(joinMap.CameraPanLeft.JoinNumber, (b) =>
            {
                if (codec.SelectedCamera == null) return;
                var camera = codec.SelectedCamera as IHasCameraPtzControl;

                if (camera == null) return;

                if (b) camera.PanLeft();
                else camera.PanStop();
            });
            trilist.SetBoolSigAction(joinMap.CameraPanRight.JoinNumber, (b) =>
            {
                if (codec.SelectedCamera == null) return;
                var camera = codec.SelectedCamera as IHasCameraPtzControl;

                if (camera == null) return;

                if (b) camera.PanRight();
                else camera.PanStop();
            });

            trilist.SetBoolSigAction(joinMap.CameraZoomIn.JoinNumber, (b) =>
            {
                if (codec.SelectedCamera == null) return;
                var camera = codec.SelectedCamera as IHasCameraPtzControl;

                if (camera == null) return;

                if (b) camera.ZoomIn();
                else camera.ZoomStop();
            });

            trilist.SetBoolSigAction(joinMap.CameraZoomOut.JoinNumber, (b) =>
            {
                if (codec.SelectedCamera == null) return;
                var camera = codec.SelectedCamera as IHasCameraPtzControl;

                if (camera == null) return;

                if (b) camera.ZoomOut();
                else camera.ZoomStop();
            });

            //Camera Select
            trilist.SetUShortSigAction(joinMap.CameraNumberSelect.JoinNumber, (i) =>
            {
                if (codec.SelectedCamera == null) return;

                codec.SelectCamera(codec.Cameras[i].Key);
            });

            codec.CameraSelected += (sender, args) =>
            {
                var i = (ushort) codec.Cameras.FindIndex((c) => c.Key == args.SelectedCamera.Key);

                trilist.SetUshort(joinMap.CameraPresetSelect.JoinNumber, i);

                if (codec is IHasCodecRoomPresets)
                {
                    return;
                }

                if (!(args.SelectedCamera is IHasCameraPresets))
                {
                    return;
                }

                var cam = args.SelectedCamera as IHasCameraPresets;
                SetCameraPresetNames(cam.Presets);

                (args.SelectedCamera as IHasCameraPresets).PresetsListHasChanged += (o, eventArgs) => SetCameraPresetNames(cam.Presets);
            };

            //Camera Presets
            trilist.SetUShortSigAction(joinMap.CameraPresetSelect.JoinNumber, (i) =>
            {
                if (codec.SelectedCamera == null) return;

                var cam = codec.SelectedCamera as IHasCameraPresets;

                if (cam == null) return;

                cam.PresetSelect(i);

                trilist.SetUshort(joinMap.CameraPresetSelect.JoinNumber, i);
            });
        }

        private string SetCameraPresetNames(List<CameraPreset> presets)
        {
            var i = 1; //start index for xsig;

            var tokenArray = new XSigToken[presets.Count];

            foreach (var preset in presets)
            {
                var cameraPreset = new XSigSerialToken(i, preset.Description);
                tokenArray[i - 1] = cameraPreset;
                i++;
            }
            
            return GetXSigString(tokenArray);
        }

        private string GetXSigString(XSigToken[] tokenArray)
        {
            string returnString;
            using (var s = new MemoryStream())
            {
                using (var tw = new XSigTokenStreamWriter(s, true))
                {
                    tw.WriteXSigData(tokenArray);
                }

                var xSig = s.ToArray();

                returnString = Encoding.GetEncoding(XSigEncoding).GetString(xSig, 0, xSig.Length);
            }

            return returnString;
        }

        #endregion
    }


    /// <summary>
    /// Used to track the status of syncronizing the phonebook values when connecting to a codec or refreshing the phonebook info
    /// </summary>
    public class CodecPhonebookSyncState : IKeyed
    {
        private bool _InitialSyncComplete;

        public CodecPhonebookSyncState(string key)
        {
            Key = key;

            CodecDisconnected();
        }

        public bool InitialSyncComplete
        {
            get { return _InitialSyncComplete; }
            private set
            {
                if (value == true)
                {
                    var handler = InitialSyncCompleted;
                    if (handler != null)
                    {
                        handler(this, new EventArgs());
                    }
                }
                _InitialSyncComplete = value;
            }
        }

        public bool InitialPhonebookFoldersWasReceived { get; private set; }

        public bool NumberOfContactsWasReceived { get; private set; }

        public bool PhonebookRootEntriesWasRecieved { get; private set; }

        public bool PhonebookHasFolders { get; private set; }

        public int NumberOfContacts { get; private set; }

        #region IKeyed Members

        public string Key { get; private set; }

        #endregion

        public event EventHandler<EventArgs> InitialSyncCompleted;

        public void InitialPhonebookFoldersReceived()
        {
            InitialPhonebookFoldersWasReceived = true;

            CheckSyncStatus();
        }

        public void PhonebookRootEntriesReceived()
        {
            PhonebookRootEntriesWasRecieved = true;

            CheckSyncStatus();
        }

        public void SetPhonebookHasFolders(bool value)
        {
            PhonebookHasFolders = value;

            Debug.Console(1, this, "Phonebook has folders: {0}", PhonebookHasFolders);
        }

        public void SetNumberOfContacts(int contacts)
        {
            NumberOfContacts = contacts;
            NumberOfContactsWasReceived = true;

            Debug.Console(1, this, "Phonebook contains {0} contacts.", NumberOfContacts);

            CheckSyncStatus();
        }

        public void CodecDisconnected()
        {
            InitialPhonebookFoldersWasReceived = false;
            PhonebookHasFolders = false;
            NumberOfContacts = 0;
            NumberOfContactsWasReceived = false;
        }

        private void CheckSyncStatus()
        {
            if (InitialPhonebookFoldersWasReceived && NumberOfContactsWasReceived && PhonebookRootEntriesWasRecieved)
            {
                InitialSyncComplete = true;
                Debug.Console(1, this, "Initial Phonebook Sync Complete!");
            }
            else
            {
                InitialSyncComplete = false;
            }
        }
    }
}