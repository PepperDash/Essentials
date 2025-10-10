using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Core.Intersystem;
using PepperDash.Core.Intersystem.Tokens;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Bridges.JoinMaps;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Devices.Common.Cameras;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces;
using Serilog.Events;
using Feedback = PepperDash.Essentials.Core.Feedback;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
	/// <summary>
	/// Base class for video codec devices
	/// </summary>
	public abstract class VideoCodecBase : ReconfigurableDevice, IRoutingInputsOutputs,
		IUsageTracking, IHasDialer, IHasContentSharing, ICodecAudio, iVideoCodecInfo, IBridgeAdvanced, IHasStandbyMode
	{
		private const int XSigEncoding = 28591;

		/// <summary>
		/// Maximum number of participants
		/// </summary>
		protected const int MaxParticipants = 50;
		private readonly byte[] _clearBytes = XSigHelpers.ClearOutputs();

		/// <summary>
		/// Time format specifier
		/// </summary>
		protected string _timeFormatSpecifier;

		/// <summary>
		/// Date format specifier
		/// </summary>
		protected string _dateFormatSpecifier;

		/// <summary>
		/// Initializes a new instance of the <see cref="VideoCodecBase"/> class.
		/// </summary>
		protected VideoCodecBase(DeviceConfig config)
			: base(config)
		{

			StandbyIsOnFeedback = new BoolFeedback("standbyIsOn", StandbyIsOnFeedbackFunc);
			PrivacyModeIsOnFeedback = new BoolFeedback("privacyModeIsOn", PrivacyModeIsOnFeedbackFunc);
			VolumeLevelFeedback = new IntFeedback("volumeLevel", VolumeLevelFeedbackFunc);
			MuteFeedback = new BoolFeedback("mute", MuteFeedbackFunc);
			SharingSourceFeedback = new StringFeedback("sharingSource", SharingSourceFeedbackFunc);
			SharingContentIsOnFeedback = new BoolFeedback("sharingContentIsOn", SharingContentIsOnFeedbackFunc);

			// TODO [ ] hotfix/videocodecbase-max-meeting-xsig-set
			MeetingsToDisplayFeedback = new IntFeedback("meetingsToDisplay", () => MeetingsToDisplay);

			InputPorts = new RoutingPortCollection<RoutingInputPort>();
			OutputPorts = new RoutingPortCollection<RoutingOutputPort>();

			ActiveCalls = new List<CodecActiveCallItem>();
		}

		/// <summary>
		/// Gets or sets the Communication
		/// </summary>
		public IBasicCommunication Communication { get; protected set; }

		/// <summary>
		/// Gets or sets the OsdSource
		/// </summary>
		public DummyRoutingInputsDevice OsdSource { get; protected set; }

		/// <summary>
		/// Gets or sets the StandbyIsOnFeedback
		/// </summary>
		public BoolFeedback StandbyIsOnFeedback { get; private set; }

		/// <summary>
		/// Gets or sets the PrivacyModeIsOnFeedbackFunc
		/// </summary>
		protected abstract Func<bool> PrivacyModeIsOnFeedbackFunc { get; }

		/// <summary>
		/// Gets or sets the VolumeLevelFeedbackFunc
		/// </summary>
		protected abstract Func<int> VolumeLevelFeedbackFunc { get; }

		/// <summary>
		/// Gets or sets the MuteFeedbackFunc
		/// </summary>
		protected abstract Func<bool> MuteFeedbackFunc { get; }

		/// <summary>
		/// Gets or sets the StandbyIsOnFeedbackFunc
		/// </summary>
		protected abstract Func<bool> StandbyIsOnFeedbackFunc { get; }

		/// <summary>
		/// Gets or sets the ActiveCalls
		/// </summary>
		public List<CodecActiveCallItem> ActiveCalls { get; set; }

		/// <summary>
		/// Gets or sets the ShowSelfViewByDefault
		/// </summary>
		public bool ShowSelfViewByDefault { get; protected set; }

		/// <summary>
		/// Gets or sets the SupportsCameraOff
		/// </summary>
		public bool SupportsCameraOff { get; protected set; }
		/// <summary>
		/// Gets or sets the SupportsCameraAutoMode
		/// </summary>
		public bool SupportsCameraAutoMode { get; protected set; }

		/// <summary>
		/// Gets or sets the IsReady
		/// </summary>
		public bool IsReady { get; protected set; }

		/// <inheritdoc />
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

		/// <summary>
		/// 	Gets or sets the MeetingsToDisplay
		/// </summary>
		protected abstract Func<string> SharingSourceFeedbackFunc { get; }

		/// <summary>
		/// Gets or sets the SharingContentIsOnFeedbackFunc
		/// </summary>
		protected abstract Func<bool> SharingContentIsOnFeedbackFunc { get; }

		#region ICodecAudio Members

		/// <summary>
		/// Set Privacy Mode On
		/// </summary>
		public abstract void PrivacyModeOn();

		/// <summary>
		/// Set Privacy Mode Off
		/// </summary>
		public abstract void PrivacyModeOff();

		/// <summary>
		/// Toggles the Privacy Mode
		/// </summary>
		public abstract void PrivacyModeToggle();

		/// <summary>
		/// Gets or sets the PrivacyModeIsOnFeedback
		/// </summary>
		public BoolFeedback PrivacyModeIsOnFeedback { get; private set; }

		/// <summary>
		/// Gets or sets the MuteFeedback
		/// </summary>
		public BoolFeedback MuteFeedback { get; private set; }

		/// <summary>
		/// Sets the Mute state to Off
		/// </summary>
		public abstract void MuteOff();

		/// <summary>
		/// Sets the Mute state to On
		/// </summary>
		public abstract void MuteOn();

		/// <summary>
		/// Sets the Volume level
		/// </summary>
		public abstract void SetVolume(ushort level);

		/// <summary>
		/// Gets or sets the VolumeLevelFeedback
		/// </summary>
		public IntFeedback VolumeLevelFeedback { get; private set; }

		/// <summary>
		/// Toggles the Mute state
		/// </summary>
		public abstract void MuteToggle();

		/// <summary>
		/// Decreases the Volume level
		/// </summary>
		public abstract void VolumeDown(bool pressRelease);

		/// <summary>
		/// Increases the Volume level
		/// </summary>
		public abstract void VolumeUp(bool pressRelease);

		#endregion

		#region IHasContentSharing Members

		/// <summary>
		/// Starts content sharing
		/// </summary>
		public abstract void StartSharing();

		/// <summary>
		/// Stops content sharing
		/// </summary>
		public abstract void StopSharing();

		/// <summary>
		/// Gets or sets the AutoShareContentWhileInCall
		/// </summary>
		public bool AutoShareContentWhileInCall { get; protected set; }

		/// <summary>
		/// Gets or sets the SharingSourceFeedback
		/// </summary>
		public StringFeedback SharingSourceFeedback { get; private set; }

		/// <summary>
		/// Gets or sets the SharingContentIsOnFeedback
		/// </summary>
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
				var value = ActiveCalls != null && ActiveCalls.Any(c => c.IsActiveCall);
				return value;
			}
		}

		/// <summary>
		/// Dials the specified number
		/// </summary>
		public abstract void Dial(string number);

		/// <summary>
		/// Ends the specified call
		/// </summary>
		public abstract void EndCall(CodecActiveCallItem call);

		/// <summary>
		/// Ends all active calls
		/// </summary>
		public abstract void EndAllCalls();

		/// <summary>
		/// Accepts the specified call
		/// </summary>
		public abstract void AcceptCall(CodecActiveCallItem call);

		/// <summary>
		/// Rejects the specified call
		/// </summary>
		public abstract void RejectCall(CodecActiveCallItem call);

		/// <summary>
		/// Sends DTMF tones
		/// </summary>
		public abstract void SendDtmf(string s);

		/// <summary>
		/// SendDtmf method
		/// </summary>		
		public virtual void SendDtmf(string s, CodecActiveCallItem call) { }

		#endregion

		#region IRoutingInputsOutputs Members

		/// <summary>
		/// Gets or sets the InputPorts
		/// </summary>
		public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

		/// <summary>
		/// Gets or sets the OutputPorts
		/// </summary>
		public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

		#endregion

		#region IUsageTracking Members

		/// <summary>
		/// Gets or sets the UsageTracker
		/// </summary>
		public UsageTracking UsageTracker { get; set; }

		#endregion

		#region iVideoCodecInfo Members

		/// <summary>
		/// Gets the CodecInfo
		/// </summary>
		public VideoCodecInfo CodecInfo { get; protected set; }

		#endregion

		/// <summary>
		/// Fired when the Codec is ready to be used
		/// </summary>
		public event EventHandler<EventArgs> IsReadyChange;

		/// <summary>
		/// Dials the specified meeting
		/// </summary>
		/// <param name="meeting"></param>
		public abstract void Dial(Meeting meeting);

		/// <summary>
		/// Dial a contact
		/// </summary>
		/// <param name="contact">The contact to dial</param>
		public virtual void Dial(IInvitableContact contact)
		{
		}

		/// <inheritdoc />
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
		/// Sets the previous and new status for the call
		/// </summary>		
		/// <param name="item"></param>
		protected virtual void OnCallStatusChange(CodecActiveCallItem item)
		{
			CallStatusChange?.Invoke(this, new CodecCallStatusItemChangeEventArgs(item));

			PrivacyModeIsOnFeedback.FireUpdate();

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
			CrestronInvoke.BeginInvoke((o) =>
				{
					try
					{
						IsReady = true;
						IsReadyChange?.Invoke(this, new EventArgs());
					}
					catch (Exception e)
					{
						Debug.LogMessage(LogEventLevel.Verbose, this, "Error in SetIsReady() : {0}", e);
					}
				});
		}

		// **** DEBUGGING THINGS ****
		/// <summary>
		/// ListCalls method
		/// </summary>		
		public virtual void ListCalls()
		{
			Debug.LogMessage(LogEventLevel.Debug, this, "Active Calls:");

			var sb = new StringBuilder();
			foreach (var c in ActiveCalls)
			{
				sb.AppendFormat("id: {0} number: {1} -- name: {2} status: {3} onHold: {4}\r\n", c.Id, c.Number, c.Name, c.Status, c.IsOnHold);
			}
			Debug.LogMessage(LogEventLevel.Debug, this, "\n{0}\n", sb.ToString());
		}

		/// <summary>
		/// Activate standby
		/// </summary>
		public abstract void StandbyActivate();

		/// <summary>
		/// Deactivate standby
		/// </summary>
		public abstract void StandbyDeactivate();

		#region Implementation of IBridgeAdvanced

		/// <inheritdoc />
		public abstract void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge);

		/// <summary>
		/// Use this method when using a plain VideoCodecControllerJoinMap
		/// </summary>
		/// <param name="codec">codec to link</param>
		/// <param name="trilist">trilist to link</param>
		/// <param name="joinStart">join to start with</param>
		/// <param name="joinMapKey">key for custom join maps</param>
		/// <param name="bridge">bridge controller</param>
		protected void LinkVideoCodecToApi(VideoCodecBase codec, BasicTriList trilist, uint joinStart, string joinMapKey,
			EiscApiAdvanced bridge)
		{
			var joinMap = new VideoCodecControllerJoinMap(joinStart);

			var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);

			if (customJoins != null)
			{
				joinMap.SetCustomJoinData(customJoins);
			}

			bridge?.AddJoinMap(Key, joinMap);

			LinkVideoCodecToApi(codec, trilist, joinMap);

			trilist.OnlineStatusChange += (device, args) =>
			{
				if (!args.DeviceOnLine) return;
			};
		}

		/// <summary>
		/// Use this method when you need to pass in a join map that extends VideoCodecControllerJoinMap
		/// </summary>
		/// <param name="codec">codec to link</param>
		/// <param name="trilist">trilist to link</param>
		/// <param name="joinMap">join map to use</param>
		protected void LinkVideoCodecToApi(VideoCodecBase codec, BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
		{
			Debug.LogMessage(LogEventLevel.Debug, this, "Linking to Trilist {0}", trilist.ID.ToString("X"));

			LinkVideoCodecDtmfToApi(trilist, joinMap);

			LinkVideoCodecCallControlsToApi(trilist, joinMap);

			LinkVideoCodecContentSharingToApi(trilist, joinMap);

			LinkVideoCodecPrivacyToApi(trilist, joinMap);

			LinkVideoCodecVolumeToApi(trilist, joinMap);

			LinkVideoCodecInfoToApi(trilist, joinMap);

			// Register for this event to link any functions that require the codec to be ready first
			codec.IsReadyChange += (o, a) =>
					{
						if (codec is IHasCodecCameras)
						{
							LinkVideoCodecCameraToApi(codec as IHasCodecCameras, trilist, joinMap);
						}
					};

			if (codec is ICommunicationMonitor)
			{
				LinkVideoCodecCommMonitorToApi(codec as ICommunicationMonitor, trilist, joinMap);
			}


			if (codec is IHasCodecSelfView)
			{
				LinkVideoCodecSelfviewToApi(codec as IHasCodecSelfView, trilist, joinMap);
			}

			if (codec is IHasCameraAutoMode)
			{
				trilist.SetBool(joinMap.CameraSupportsAutoMode.JoinNumber, SupportsCameraAutoMode);
				LinkVideoCodecCameraModeToApi(codec as IHasCameraAutoMode, trilist, joinMap);
			}

			if (codec is IHasCameraOff)
			{
				trilist.SetBool(joinMap.CameraSupportsOffMode.JoinNumber, SupportsCameraOff);
				LinkVideoCodecCameraOffToApi(codec as IHasCameraOff, trilist, joinMap);
			}

			if (codec is IHasCodecLayouts)
			{
				LinkVideoCodecCameraLayoutsToApi(codec as IHasCodecLayouts, trilist, joinMap);
			}


			if (codec is IHasSelfviewPosition)
			{
				LinkVideoCodecSelfviewPositionToApi(codec as IHasSelfviewPosition, trilist, joinMap);
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

			if (codec is IHasFarEndContentStatus)
			{
				(codec as IHasFarEndContentStatus).ReceivingContent.LinkInputSig(trilist.BooleanInput[joinMap.RecievingContent.JoinNumber]);
			}

			if (codec is IHasPhoneDialing)
			{
				LinkVideoCodecPhoneToApi(codec as IHasPhoneDialing, trilist, joinMap);
			}

			if (codec is IHasCallHistory)
			{
				LinkVideoCodecCallHistoryToApi(codec as IHasCallHistory, trilist, joinMap);
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
					trilist.SetBool(joinMap.CameraSupportsAutoMode.JoinNumber, SupportsCameraAutoMode);

					(codec as IHasCameraAutoMode).CameraAutoModeIsOnFeedback.FireUpdate();
				}

				if (codec is IHasCodecSelfView)
				{
					(codec as IHasCodecSelfView).SelfviewIsOnFeedback.FireUpdate();
				}

				if (codec is IHasCameraAutoMode)
				{
					(codec as IHasCameraAutoMode).CameraAutoModeIsOnFeedback.FireUpdate();
				}

				if (codec is IHasCameraOff)
				{
					(codec as IHasCameraOff).CameraIsOffFeedback.FireUpdate();
				}

				if (codec is IHasPhoneDialing)
				{
					(codec as IHasPhoneDialing).PhoneOffHookFeedback.FireUpdate();
				}

				if (codec is IHasCallHistory)
				{
					UpdateCallHistory((codec as IHasCallHistory), trilist, joinMap);
				}

				SharingContentIsOnFeedback.FireUpdate();
			};
		}

		private void LinkVideoCodecInfoToApi(BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
		{
			trilist.SetBool(joinMap.MultiSiteOptionIsEnabled.JoinNumber, this.CodecInfo.MultiSiteOptionIsEnabled);
			trilist.SetBool(joinMap.AutoAnswerEnabled.JoinNumber, this.CodecInfo.AutoAnswerEnabled);
			trilist.SetString(joinMap.DeviceIpAddresss.JoinNumber, this.CodecInfo.IpAddress);
			trilist.SetString(joinMap.SipPhoneNumber.JoinNumber, this.CodecInfo.SipPhoneNumber);
			trilist.SetString(joinMap.E164Alias.JoinNumber, this.CodecInfo.E164Alias);
			trilist.SetString(joinMap.H323Id.JoinNumber, this.CodecInfo.H323Id);
			trilist.SetString(joinMap.SipUri.JoinNumber, this.CodecInfo.SipUri);

			trilist.OnlineStatusChange += (o, a) =>
			{
				if (a.DeviceOnLine)
				{
					trilist.SetBool(joinMap.MultiSiteOptionIsEnabled.JoinNumber, this.CodecInfo.MultiSiteOptionIsEnabled);
					trilist.SetBool(joinMap.AutoAnswerEnabled.JoinNumber, this.CodecInfo.AutoAnswerEnabled);
					trilist.SetString(joinMap.DeviceIpAddresss.JoinNumber, this.CodecInfo.IpAddress);
					trilist.SetString(joinMap.SipPhoneNumber.JoinNumber, this.CodecInfo.SipPhoneNumber);
					trilist.SetString(joinMap.E164Alias.JoinNumber, this.CodecInfo.E164Alias);
					trilist.SetString(joinMap.H323Id.JoinNumber, this.CodecInfo.H323Id);
					trilist.SetString(joinMap.SipUri.JoinNumber, this.CodecInfo.SipUri);
				}
			};
		}

		private void LinkVideoCodecPhoneToApi(IHasPhoneDialing codec, BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
		{
			codec.PhoneOffHookFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PhoneHookState.JoinNumber]);

			trilist.SetSigFalseAction(joinMap.DialPhone.JoinNumber,
				() => codec.DialPhoneCall(trilist.StringOutput[joinMap.PhoneDialString.JoinNumber].StringValue));

			trilist.SetSigFalseAction(joinMap.HangUpPhone.JoinNumber, codec.EndPhoneCall);
		}

		private void LinkVideoCodecSelfviewPositionToApi(IHasSelfviewPosition codec, BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
		{
			trilist.SetSigFalseAction(joinMap.SelfviewPosition.JoinNumber, codec.SelfviewPipPositionToggle);

			codec.SelfviewPipPositionFeedback.LinkInputSig(trilist.StringInput[joinMap.SelfviewPositionFb.JoinNumber]);
		}

		private void LinkVideoCodecCameraOffToApi(IHasCameraOff codec, BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
		{
			trilist.SetSigFalseAction(joinMap.CameraModeOff.JoinNumber, codec.CameraOff);

			codec.CameraIsOffFeedback.OutputChange += (o, a) =>
			{
				if (a.BoolValue)
				{
					trilist.SetBool(joinMap.CameraModeOff.JoinNumber, true);
					trilist.SetBool(joinMap.CameraModeManual.JoinNumber, false);
					trilist.SetBool(joinMap.CameraModeAuto.JoinNumber, false);
					return;
				}

				trilist.SetBool(joinMap.CameraModeOff.JoinNumber, false);


				if (!(codec is IHasCameraAutoMode autoCodec)) return;

				trilist.SetBool(joinMap.CameraModeAuto.JoinNumber, autoCodec.CameraAutoModeIsOnFeedback.BoolValue);
				trilist.SetBool(joinMap.CameraModeManual.JoinNumber, !autoCodec.CameraAutoModeIsOnFeedback.BoolValue);
			};

			if (codec.CameraIsOffFeedback.BoolValue)
			{
				trilist.SetBool(joinMap.CameraModeOff.JoinNumber, true);
				trilist.SetBool(joinMap.CameraModeManual.JoinNumber, false);
				trilist.SetBool(joinMap.CameraModeAuto.JoinNumber, false);
				return;
			}

			trilist.SetBool(joinMap.CameraModeOff.JoinNumber, false);


			if (!(codec is IHasCameraAutoMode autoModeCodec)) return;

			trilist.SetBool(joinMap.CameraModeAuto.JoinNumber, autoModeCodec.CameraAutoModeIsOnFeedback.BoolValue);
			trilist.SetBool(joinMap.CameraModeManual.JoinNumber, !autoModeCodec.CameraAutoModeIsOnFeedback.BoolValue);
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
			// make sure to update the values when the EISC comes online
			trilist.OnlineStatusChange += (sender, args) =>
					{
						if (sender.IsOnline)
						{
							UpdateParticipantsXSig(codec, trilist, joinMap);
						}
					};

			// set actions and update the values when the list changes
			codec.Participants.ParticipantsListHasChanged += (sender, args) =>
			{
				SetParticipantActions(trilist, joinMap, codec.Participants.CurrentParticipants);

				UpdateParticipantsXSig(codec, trilist, joinMap);
			};

			trilist.OnlineStatusChange += (device, args) =>
			{
				if (!args.DeviceOnLine) return;

				// TODO [ ] Issue #868
				trilist.SetString(joinMap.CurrentParticipants.JoinNumber, "\xFC");
				UpdateParticipantsXSig(codec, trilist, joinMap);
			};
		}

		private void UpdateParticipantsXSig(IHasParticipants codec, BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
		{
			string participantsXSig;

			if (codec.Participants.CurrentParticipants.Count == 0)
			{
				participantsXSig = Encoding.GetEncoding(XSigEncoding).GetString(_clearBytes, 0, _clearBytes.Length);
				trilist.SetString(joinMap.CurrentParticipants.JoinNumber, participantsXSig);
				trilist.SetUshort(joinMap.ParticipantCount.JoinNumber, (ushort)codec.Participants.CurrentParticipants.Count);
				return;
			}

			participantsXSig = UpdateParticipantsXSig(codec.Participants.CurrentParticipants);

			trilist.SetString(joinMap.CurrentParticipants.JoinNumber, participantsXSig);

			trilist.SetUshort(joinMap.ParticipantCount.JoinNumber, (ushort)codec.Participants.CurrentParticipants.Count);
		}

		/// <summary>
		/// Sets the actions for each participant in the list
		/// </summary>
		private void SetParticipantActions(BasicTriList trilist, VideoCodecControllerJoinMap joinMap, List<Participant> currentParticipants)
		{
			uint index = 0; // track the index of the participant in the 

			foreach (var participant in currentParticipants)
			{
				var p = participant;
				if (index > MaxParticipants) break;

				if (this is IHasParticipantAudioMute audioMuteCodec)
				{
					trilist.SetSigFalseAction(joinMap.ParticipantAudioMuteToggleStart.JoinNumber + index,
							() => audioMuteCodec.ToggleAudioForParticipant(p.UserId));

					trilist.SetSigFalseAction(joinMap.ParticipantVideoMuteToggleStart.JoinNumber + index,
							() => audioMuteCodec.ToggleVideoForParticipant(p.UserId));
				}

				if (this is IHasParticipantPinUnpin pinCodec)
				{
					trilist.SetSigFalseAction(joinMap.ParticipantPinToggleStart.JoinNumber + index,
							() => pinCodec.ToggleParticipantPinState(p.UserId, pinCodec.ScreenIndexToPinUserTo));
				}

				index++;
			}

			// Clear out any previously set actions
			while (index < MaxParticipants)
			{
				trilist.ClearBoolSigAction(joinMap.ParticipantAudioMuteToggleStart.JoinNumber + index);
				trilist.ClearBoolSigAction(joinMap.ParticipantVideoMuteToggleStart.JoinNumber + index);
				trilist.ClearBoolSigAction(joinMap.ParticipantPinToggleStart.JoinNumber + index);

				index++;
			}
		}

		private string UpdateParticipantsXSig(List<Participant> currentParticipants)
		{
			const int maxParticipants = MaxParticipants;
			const int maxDigitals = 7;
			const int maxStrings = 1;
			const int maxAnalogs = 1;
			const int offset = maxDigitals + maxStrings + maxAnalogs; // 9
			var digitalIndex = (maxStrings + maxAnalogs) * maxParticipants; // 100
			var stringIndex = 0;
			var analogIndex = stringIndex + maxParticipants;
			var meetingIndex = 0;

			var tokenArray = new XSigToken[maxParticipants * offset];

			foreach (var participant in currentParticipants)
			{
				if (meetingIndex >= maxParticipants * offset) break;

				//                Debug.LogMessage(LogEventLevel.Verbose, this,
				//@"Updating Participant on xsig:
				//Name: {0} (s{9})
				//AudioMute: {1} (d{10})
				//VideoMute: {2} (d{11})
				//CanMuteVideo: {3} (d{12})
				//CanUMuteVideo: {4} (d{13})
				//IsHost: {5} (d{14})
				//HandIsRaised: {6} (d{15})
				//IsPinned: {7} (d{16})
				//ScreenIndexIsPinnedTo: {8} (a{17})
				//",
				// participant.Name,
				// participant.AudioMuteFb,
				// participant.VideoMuteFb,
				// participant.CanMuteVideo,
				// participant.CanUnmuteVideo,
				// participant.IsHost,
				// participant.HandIsRaisedFb,
				// participant.IsPinnedFb,
				// participant.ScreenIndexIsPinnedToFb,
				// stringIndex + 1,
				// digitalIndex + 1,
				// digitalIndex + 2,
				// digitalIndex + 3,
				// digitalIndex + 4,
				// digitalIndex + 5,
				// digitalIndex + 6,
				// digitalIndex + 7,
				// analogIndex + 1
				// );


				//digitals
				tokenArray[digitalIndex] = new XSigDigitalToken(digitalIndex + 1, participant.AudioMuteFb);
				tokenArray[digitalIndex + 1] = new XSigDigitalToken(digitalIndex + 2, participant.VideoMuteFb);
				tokenArray[digitalIndex + 2] = new XSigDigitalToken(digitalIndex + 3, participant.CanMuteVideo);
				tokenArray[digitalIndex + 3] = new XSigDigitalToken(digitalIndex + 4, participant.CanUnmuteVideo);
				tokenArray[digitalIndex + 4] = new XSigDigitalToken(digitalIndex + 5, participant.IsHost);
				tokenArray[digitalIndex + 5] = new XSigDigitalToken(digitalIndex + 6, participant.HandIsRaisedFb);
				tokenArray[digitalIndex + 6] = new XSigDigitalToken(digitalIndex + 7, participant.IsPinnedFb);

				Debug.LogMessage(LogEventLevel.Verbose, this, "Index: {0} byte value: {1}", digitalIndex + 7, ComTextHelper.GetEscapedText(tokenArray[digitalIndex + 6].GetBytes()));

				//serials
				tokenArray[stringIndex] = new XSigSerialToken(stringIndex + 1, participant.Name);

				//analogs
				tokenArray[analogIndex] = new XSigAnalogToken(analogIndex + 1, (ushort)participant.ScreenIndexIsPinnedToFb);

				digitalIndex += maxDigitals;
				meetingIndex += offset;
				stringIndex += maxStrings;
				analogIndex += maxAnalogs;
			}

			while (meetingIndex < maxParticipants * offset)
			{
				//digitals
				tokenArray[digitalIndex] = new XSigDigitalToken(digitalIndex + 1, false);
				tokenArray[digitalIndex + 1] = new XSigDigitalToken(digitalIndex + 2, false);
				tokenArray[digitalIndex + 2] = new XSigDigitalToken(digitalIndex + 3, false);
				tokenArray[digitalIndex + 3] = new XSigDigitalToken(digitalIndex + 4, false);
				tokenArray[digitalIndex + 4] = new XSigDigitalToken(digitalIndex + 5, false);
				tokenArray[digitalIndex + 5] = new XSigDigitalToken(digitalIndex + 6, false);
				tokenArray[digitalIndex + 6] = new XSigDigitalToken(digitalIndex + 7, false);

				//serials
				tokenArray[stringIndex] = new XSigSerialToken(stringIndex + 1, String.Empty);

				//analogs
				tokenArray[analogIndex] = new XSigAnalogToken(analogIndex + 1, 0);

				digitalIndex += maxDigitals;
				meetingIndex += offset;
				stringIndex += maxStrings;
				analogIndex += maxAnalogs;
			}

			var returnString = GetXSigString(tokenArray);

			//Debug.LogMessage(LogEventLevel.Verbose, this, "{0}", ComTextHelper.GetEscapedText(Encoding.GetEncoding(28591).GetBytes(returnString)));


			return returnString;
		}

		private void LinkVideoCodecContentSharingToApi(BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
		{
			SharingContentIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.SourceShareStart.JoinNumber]);
			SharingContentIsOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.SourceShareEnd.JoinNumber]);

			SharingSourceFeedback.LinkInputSig(trilist.StringInput[joinMap.CurrentSource.JoinNumber]);

			trilist.SetSigFalseAction(joinMap.SourceShareStart.JoinNumber, StartSharing);
			trilist.SetSigFalseAction(joinMap.SourceShareEnd.JoinNumber, StopSharing);

			trilist.SetBoolSigAction(joinMap.SourceShareAutoStart.JoinNumber, b => AutoShareContentWhileInCall = b);
		}

		private List<Meeting> _currentMeetings = new List<Meeting>();

		private void LinkVideoCodecScheduleToApi(IHasScheduleAwareness codec, BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
		{
			trilist.SetSigFalseAction(joinMap.UpdateMeetings.JoinNumber, codec.GetSchedule);

			trilist.SetUShortSigAction(joinMap.MinutesBeforeMeetingStart.JoinNumber, (i) =>
			{
				codec.CodecSchedule.MeetingWarningMinutes = i;
			});


			for (uint i = 0; i < joinMap.DialMeetingStart.JoinSpan; i++)
			{
				Debug.LogMessage(LogEventLevel.Debug, this, "Setting action to Dial Meeting {0} to digital join {1}", i + 1, joinMap.DialMeetingStart.JoinNumber + i);
				var joinNumber = joinMap.DialMeetingStart.JoinNumber + i;
				var mtg = i + 1;
				var index = (int)i;

				trilist.SetSigFalseAction(joinNumber, () =>
				{
					Debug.LogMessage(LogEventLevel.Debug, this, "Meeting {0} Selected (EISC dig-o{1}) > _currentMeetings[{2}].Id: {3}, Title: {4}",
											mtg, joinMap.DialMeetingStart.JoinNumber + i, index, _currentMeetings[index].Id, _currentMeetings[index].Title);
					if (_currentMeetings[index] != null)
						Dial(_currentMeetings[index]);
				});
			}

			codec.CodecSchedule.MeetingsListHasChanged += (sender, args) => UpdateMeetingsList(codec, trilist, joinMap);
			codec.CodecSchedule.MeetingEventChange += (sender, args) =>
				{
					if (args.ChangeType == eMeetingEventChangeType.MeetingStartWarning)
					{
						UpdateMeetingsList(codec, trilist, joinMap);
					}
				};

			trilist.SetUShortSigAction(joinMap.MeetingsToDisplay.JoinNumber, m => MeetingsToDisplay = m);
			MeetingsToDisplayFeedback.LinkInputSig(trilist.UShortInput[joinMap.MeetingsToDisplay.JoinNumber]);

			trilist.OnlineStatusChange += (device, args) =>
			{
				if (!args.DeviceOnLine) return;

				// TODO [ ] Issue #868
				trilist.SetString(joinMap.Schedule.JoinNumber, "\xFC");
				UpdateMeetingsList(codec, trilist, joinMap);
				// TODO [ ] hotfix/videocodecbase-max-meeting-xsig-set
				MeetingsToDisplayFeedback.LinkInputSig(trilist.UShortInput[joinMap.MeetingsToDisplay.JoinNumber]);
			};
		}

		private void UpdateMeetingsList(IHasScheduleAwareness codec, BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
		{
			var currentTime = DateTime.Now;

			_currentMeetings = codec.CodecSchedule.Meetings.Where(m => m.StartTime >= currentTime || m.EndTime >= currentTime).ToList();

			if (_currentMeetings.Count == 0)
			{
				var emptyXSigByteArray = XSigHelpers.ClearOutputs();
				var emptyXSigString = Encoding.GetEncoding(XSigEncoding)
						.GetString(emptyXSigByteArray, 0, emptyXSigByteArray.Length);

				trilist.SetString(joinMap.Schedule.JoinNumber, emptyXSigString);
				return;
			}

			var meetingsData = UpdateMeetingsListXSig(_currentMeetings);
			trilist.SetString(joinMap.Schedule.JoinNumber, meetingsData);
			trilist.SetUshort(joinMap.MeetingCount.JoinNumber, (ushort)_currentMeetings.Count);

			trilist.OnlineStatusChange += (device, args) =>
			{
				if (!args.DeviceOnLine) return;

				// TODO [ ] Issue #868
				trilist.SetString(joinMap.Schedule.JoinNumber, "\xFC");
				UpdateMeetingsListXSig(_currentMeetings);
			};
		}



		private int _meetingsToDisplay = 3;

		/// <summary>
		/// Gets or sets the number of meetings to display on the XSIG
		/// </summary>
		protected int MeetingsToDisplay
		{
			get { return _meetingsToDisplay; }
			set
			{
				_meetingsToDisplay = (ushort)(value == 0 ? 3 : value);
				MeetingsToDisplayFeedback.FireUpdate();
			}
		}

		/// <summary>
		/// Gets or sets the MeetingsToDisplayFeedback
		/// </summary>
		public IntFeedback MeetingsToDisplayFeedback { get; set; }

		private string UpdateMeetingsListXSig(List<Meeting> meetings)
		{
			// TODO [ ] hotfix/videocodecbase-max-meeting-xsig-set
			//const int _meetingsToDisplay = 3;            
			const int maxDigitals = 2;
			const int maxStrings = 7;
			const int offset = maxDigitals + maxStrings;
			var digitalIndex = maxStrings * _meetingsToDisplay; //15
			var stringIndex = 0;
			var meetingIndex = 0;

			var tokenArray = new XSigToken[_meetingsToDisplay * offset];
			/* 
			 * Digitals
			 * IsJoinable - 1
			 * IsDialable - 2
			 * 
			 * Serials
			 * Organizer - 1
			 * Title - 2
			 * Start Date - 3
			 * Start Time - 4
			 * End Date - 5
			 * End Time - 6
			 * Id - 7
			*/


			foreach (var meeting in meetings)
			{
				var currentTime = DateTime.Now;

				if (meeting.StartTime < currentTime && meeting.EndTime < currentTime) continue;

				if (meetingIndex >= _meetingsToDisplay * offset)
				{
					Debug.LogMessage(LogEventLevel.Verbose, this, "Max Meetings reached");
					break;
				}

				//digitals
				tokenArray[digitalIndex] = new XSigDigitalToken(digitalIndex + 1, meeting.Joinable);
				tokenArray[digitalIndex + 1] = new XSigDigitalToken(digitalIndex + 2, meeting.Dialable);

				//serials
				tokenArray[stringIndex] = new XSigSerialToken(stringIndex + 1, meeting.Organizer);
				tokenArray[stringIndex + 1] = new XSigSerialToken(stringIndex + 2, meeting.Title);
				tokenArray[stringIndex + 2] = new XSigSerialToken(stringIndex + 3, meeting.StartTime.ToString(_dateFormatSpecifier.NullIfEmpty() ?? "d", Global.Culture));
				tokenArray[stringIndex + 3] = new XSigSerialToken(stringIndex + 4, meeting.StartTime.ToString(_timeFormatSpecifier.NullIfEmpty() ?? "t", Global.Culture));
				tokenArray[stringIndex + 4] = new XSigSerialToken(stringIndex + 5, meeting.EndTime.ToString(_dateFormatSpecifier.NullIfEmpty() ?? "d", Global.Culture));
				tokenArray[stringIndex + 5] = new XSigSerialToken(stringIndex + 6, meeting.EndTime.ToString(_timeFormatSpecifier.NullIfEmpty() ?? "t", Global.Culture));
				tokenArray[stringIndex + 6] = new XSigSerialToken(stringIndex + 7, meeting.Id);

				digitalIndex += maxDigitals;
				meetingIndex += offset;
				stringIndex += maxStrings;
			}

			while (meetingIndex < _meetingsToDisplay * offset)
			{
				Debug.LogMessage(LogEventLevel.Verbose, this, "Clearing unused data. Meeting Index: {0} MaxMeetings * Offset: {1}",
					meetingIndex, _meetingsToDisplay * offset);

				//digitals
				tokenArray[digitalIndex] = new XSigDigitalToken(digitalIndex + 1, false);
				tokenArray[digitalIndex + 1] = new XSigDigitalToken(digitalIndex + 2, false);

				//serials
				tokenArray[stringIndex] = new XSigSerialToken(stringIndex + 1, String.Empty);
				tokenArray[stringIndex + 1] = new XSigSerialToken(stringIndex + 2, String.Empty);
				tokenArray[stringIndex + 2] = new XSigSerialToken(stringIndex + 3, String.Empty);
				tokenArray[stringIndex + 3] = new XSigSerialToken(stringIndex + 4, String.Empty);
				tokenArray[stringIndex + 4] = new XSigSerialToken(stringIndex + 5, String.Empty);
				tokenArray[stringIndex + 5] = new XSigSerialToken(stringIndex + 6, String.Empty);
				tokenArray[stringIndex + 6] = new XSigSerialToken(stringIndex + 7, String.Empty);

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

			trilist.SetStringSigAction(joinMap.DirectorySearchString.JoinNumber, codec.SearchDirectory);

			trilist.SetUShortSigAction(joinMap.DirectorySelectRow.JoinNumber, (i) => SelectDirectoryEntry(codec, i, trilist, joinMap));

			//Special Change for protected directory clear

			trilist.SetBoolSigAction(joinMap.DirectoryClearSelected.JoinNumber, (b) => SelectDirectoryEntry(codec, 0, trilist, joinMap));

			// Report feedback for number of contact methods for selected contact

			trilist.SetSigFalseAction(joinMap.DirectoryRoot.JoinNumber, codec.SetCurrentDirectoryToRoot);

			trilist.SetSigFalseAction(joinMap.DirectoryFolderBack.JoinNumber, codec.GetDirectoryParentFolderContents);

			if (codec.DirectoryRoot != null)
			{
				var contactsCount = codec.DirectoryRoot.CurrentDirectoryResults.Where(c => c.ParentFolderId.Equals("root")).ToList().Count;
				trilist.SetUshort(joinMap.DirectoryRowCount.JoinNumber, (ushort)contactsCount);
				Debug.LogMessage(LogEventLevel.Verbose, this, ">>> contactsCount: {0}", contactsCount);

				var clearBytes = XSigHelpers.ClearOutputs();

				trilist.SetString(joinMap.DirectoryEntries.JoinNumber,
						Encoding.GetEncoding(XSigEncoding).GetString(clearBytes, 0, clearBytes.Length));
				var directoryXSig = UpdateDirectoryXSig(codec.DirectoryRoot,
						codec.CurrentDirectoryResultIsNotDirectoryRoot.BoolValue == false);

				Debug.LogMessage(LogEventLevel.Verbose, this, "Directory XSig Length: {0}", directoryXSig.Length);

				trilist.SetString(joinMap.DirectoryEntries.JoinNumber, directoryXSig);
			}

			codec.DirectoryResultReturned += (sender, args) =>
			{
				var isRoot = codec.CurrentDirectoryResultIsNotDirectoryRoot.BoolValue == false;
				var argsCount = isRoot
					? args.Directory.CurrentDirectoryResults.Where(a => a.ParentFolderId.Equals("root")).ToList().Count
					: args.Directory.CurrentDirectoryResults.Count;

				trilist.SetUshort(joinMap.DirectoryRowCount.JoinNumber, (ushort)argsCount);
				Debug.LogMessage(LogEventLevel.Verbose, this, ">>> argsCount: {0}", argsCount);

				var clearBytes = XSigHelpers.ClearOutputs();

				trilist.SetString(joinMap.DirectoryEntries.JoinNumber,
					Encoding.GetEncoding(XSigEncoding).GetString(clearBytes, 0, clearBytes.Length));
				var directoryXSig = UpdateDirectoryXSig(args.Directory,
						codec.CurrentDirectoryResultIsNotDirectoryRoot.BoolValue == false);
				Debug.LogMessage(LogEventLevel.Verbose, this, "Directory XSig Length: {0}", directoryXSig.Length);

				trilist.SetString(joinMap.DirectoryEntries.JoinNumber, directoryXSig);
			};

			trilist.OnlineStatusChange += (device, args) =>
			{
				if (!args.DeviceOnLine) return;

				var clearBytes = XSigHelpers.ClearOutputs();
				trilist.SetString(joinMap.DirectoryEntries.JoinNumber,
									Encoding.GetEncoding(XSigEncoding).GetString(clearBytes, 0, clearBytes.Length));
				var directoryXSig = UpdateDirectoryXSig(codec.DirectoryRoot, codec.CurrentDirectoryResultIsNotDirectoryRoot.BoolValue == false);
				trilist.SetString(joinMap.DirectoryEntries.JoinNumber, directoryXSig);
			};
		}

		private void SelectDirectoryEntry(IHasDirectory codec, ushort i, BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
		{
			if (i > codec.CurrentDirectoryResult.CurrentDirectoryResults.Count) return;
			_selectedDirectoryItem = i == 0 ? null : codec.CurrentDirectoryResult.CurrentDirectoryResults[i - 1];
			trilist.SetUshort(joinMap.DirectorySelectRowFeedback.JoinNumber, i);

			if (_selectedDirectoryItem == null) trilist.SetBool(joinMap.DirectoryEntryIsContact.JoinNumber, false);


			if (_selectedDirectoryItem is DirectoryFolder)
			{
				codec.GetDirectoryFolderContents(_selectedDirectoryItem.FolderId);
				trilist.SetUshort(joinMap.SelectedContactMethodCount.JoinNumber, 0);
				trilist.SetString(joinMap.DirectorySelectedFolderName.JoinNumber, _selectedDirectoryItem.Name);
				trilist.SetString(joinMap.DirectoryEntrySelectedName.JoinNumber, string.Empty);
				trilist.ClearUShortSigAction(joinMap.SelectContactMethod.JoinNumber);
				trilist.ClearBoolSigAction(joinMap.DirectoryDialSelectedLine.JoinNumber);
				trilist.ClearBoolSigAction(joinMap.DirectoryDialSelectedContactMethod.JoinNumber);
				trilist.SetBool(joinMap.DirectoryEntryIsContact.JoinNumber, false);
				return;
			}

			// not a folder.  Clear this value
			trilist.SetString(joinMap.DirectorySelectedFolderName.JoinNumber, string.Empty);

			var selectedContact = _selectedDirectoryItem as DirectoryContact;

			if (selectedContact != null && selectedContact.ContactMethods.Count >= 1)
			{
				trilist.SetBool(joinMap.DirectoryEntryIsContact.JoinNumber, true);
			}

			trilist.SetString(joinMap.DirectoryEntrySelectedName.JoinNumber,
					selectedContact != null ? selectedContact.Name : string.Empty);

			// Allow auto dial of selected line.  Always dials first contact method
			if (!trilist.GetBool(joinMap.DirectoryDisableAutoDialSelectedLine.JoinNumber))
			{
				if (_selectedDirectoryItem is IInvitableContact invitableEntry)
				{
					Dial(invitableEntry);
					return;
				}


				trilist.SetString(joinMap.DirectoryEntrySelectedNumber.JoinNumber,
						selectedContact != null ? selectedContact.ContactMethods[0].Number : string.Empty);

				if (!(_selectedDirectoryItem is DirectoryContact entryToDial)) return;

				Dial(entryToDial.ContactMethods[0].Number);
			}
			else
			{
				// If auto dial is disabled...

				if (!(_selectedDirectoryItem is DirectoryContact entryToDial))
				{
					// Clear out values and actions from last selected item
					trilist.SetUshort(joinMap.SelectedContactMethodCount.JoinNumber, 0);
					trilist.SetString(joinMap.DirectoryEntrySelectedName.JoinNumber, string.Empty);
					trilist.ClearUShortSigAction(joinMap.SelectContactMethod.JoinNumber);
					trilist.ClearBoolSigAction(joinMap.DirectoryDialSelectedLine.JoinNumber);
					trilist.ClearBoolSigAction(joinMap.DirectoryDialSelectedContactMethod.JoinNumber);
					return;
				}

				trilist.SetUshort(joinMap.SelectedContactMethodCount.JoinNumber, (ushort)entryToDial.ContactMethods.Count);

				// Update the action to dial the selected contact method
				trilist.SetUShortSigAction(joinMap.SelectContactMethod.JoinNumber, (u) =>
				{
					if (u < 1 || u > entryToDial.ContactMethods.Count) return;

					trilist.SetSigFalseAction(joinMap.DirectoryDialSelectedContactMethod.JoinNumber, () => Dial(entryToDial.ContactMethods[u - 1].Number));
				});

				// Sets DirectoryDialSelectedLine join action to dial first contact method
				trilist.SetSigFalseAction(joinMap.DirectoryDialSelectedLine.JoinNumber, () => Dial(entryToDial.ContactMethods[0].Number));

				var clearBytes = XSigHelpers.ClearOutputs();

				trilist.SetString(joinMap.ContactMethods.JoinNumber,
						Encoding.GetEncoding(XSigEncoding).GetString(clearBytes, 0, clearBytes.Length));
				var contactMethodsXSig = UpdateContactMethodsXSig(entryToDial);

				trilist.SetString(joinMap.ContactMethods.JoinNumber, contactMethodsXSig);
			}
		}

		/// <summary>
		/// Generates the XSig data representing the available contact methods for the selected DirectoryContact
		/// </summary>
		/// <param name="contact"></param>
		/// <returns></returns>
		private string UpdateContactMethodsXSig(DirectoryContact contact)
		{
			const int maxMethods = 10;
			const int maxStrings = 3;
			const int offset = maxStrings;
			var stringIndex = 0;
			var arrayIndex = 0;
			// Create a new token array and set the size to the number of methods times the total number of signals
			var tokenArray = new XSigToken[maxMethods * offset];

			Debug.LogMessage(LogEventLevel.Verbose, this, "Creating XSIG token array with size {0}", maxMethods * offset);

			// TODO: Add code to generate XSig data
			foreach (var method in contact.ContactMethods)
			{
				if (arrayIndex >= maxMethods * offset)
					break;

				//serials
				tokenArray[arrayIndex + 1] = new XSigSerialToken(stringIndex + 1, method.Number);
				tokenArray[arrayIndex + 2] = new XSigSerialToken(stringIndex + 2, method.ContactMethodId.ToString());
				tokenArray[arrayIndex + 3] = new XSigSerialToken(stringIndex + 3, method.Device.ToString());

				arrayIndex += offset;
				stringIndex += maxStrings;
			}

			while (arrayIndex < maxMethods)
			{
				tokenArray[arrayIndex + 1] = new XSigSerialToken(stringIndex + 1, String.Empty);
				tokenArray[arrayIndex + 2] = new XSigSerialToken(stringIndex + 2, String.Empty);
				tokenArray[arrayIndex + 3] = new XSigSerialToken(stringIndex + 3, String.Empty);

				arrayIndex += offset;
				stringIndex += maxStrings;
			}

			return GetXSigString(tokenArray);
		}

		private string UpdateDirectoryXSig(CodecDirectory directory, bool isRoot)
		{
			var xSigMaxIndex = 1023;
			var tokenArray = new XSigToken[directory.CurrentDirectoryResults.Count > xSigMaxIndex
				? xSigMaxIndex
				: directory.CurrentDirectoryResults.Count];

			Debug.LogMessage(LogEventLevel.Verbose, this, "IsRoot: {0}, Directory Count: {1}, TokenArray.Length: {2}", isRoot, directory.CurrentDirectoryResults.Count, tokenArray.Length);

			var contacts = directory.CurrentDirectoryResults.Count > xSigMaxIndex
				? directory.CurrentDirectoryResults.Take(xSigMaxIndex)
				: directory.CurrentDirectoryResults;

			var contactsToDisplay = isRoot
				? contacts.Where(c => c.ParentFolderId == "root")
				: contacts.Where(c => c.ParentFolderId != "root");

			var counterIndex = 1;
			foreach (var entry in contactsToDisplay)
			{
				var arrayIndex = counterIndex - 1;
				var entryIndex = counterIndex;

				Debug.LogMessage(LogEventLevel.Verbose, this, "Entry{2:0000} Name: {0}, Folder ID: {1}, Type: {3}, ParentFolderId: {4}",
					entry.Name, entry.FolderId, entryIndex, entry.GetType().FullName, entry.ParentFolderId);

				if (entry is DirectoryFolder)
				{
					tokenArray[arrayIndex] = new XSigSerialToken(entryIndex, String.Format("[+] {0}", entry.Name));

					counterIndex++;

					continue;
				}

				tokenArray[arrayIndex] = new XSigSerialToken(entryIndex, entry.Name);

				counterIndex++;
			}

			return GetXSigString(tokenArray);
		}

		private void LinkVideoCodecCallControlsToApi(BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
		{
			trilist.SetSigFalseAction(joinMap.ManualDial.JoinNumber,
				() => Dial(trilist.StringOutput[joinMap.CurrentDialString.JoinNumber].StringValue));

			//End All calls
			trilist.SetSigFalseAction(joinMap.EndAllCalls.JoinNumber, EndAllCalls);

			//End a specific call, specified by index. Maximum 8 calls supported
			for (int i = 0; i < joinMap.EndCallStart.JoinSpan; i++)
			{
				var callIndex = i;

				trilist.SetSigFalseAction((uint)(joinMap.EndCallStart.JoinNumber + i), () =>
						{

							if (callIndex < 0 || callIndex >= ActiveCalls.Count)
							{
								Debug.LogMessage(LogEventLevel.Verbose, this, "Cannot end call. No call found at index: {0}", callIndex);
								return;
							}

							var call = ActiveCalls[callIndex];
							if (call != null)
							{
								EndCall(call);
							}
							else
							{
								Debug.LogMessage(LogEventLevel.Information, this, "[End Call] Unable to find call at index '{0}'", i);
							}
						});
			}

			trilist.SetBool(joinMap.HookState.JoinNumber, IsInCall);

			CallStatusChange += (sender, args) =>
			{
				trilist.SetBool(joinMap.HookState.JoinNumber, IsInCall);

				Debug.LogMessage(LogEventLevel.Debug, this, "Call Direction: {0}", args.CallItem.Direction);
				Debug.LogMessage(LogEventLevel.Debug, this, "Call is incoming: {0}", args.CallItem.Direction == eCodecCallDirection.Incoming);
				trilist.SetBool(joinMap.IncomingCall.JoinNumber, args.CallItem.Direction == eCodecCallDirection.Incoming && args.CallItem.Status == eCodecCallStatus.Ringing);

				if (args.CallItem.Direction == eCodecCallDirection.Incoming)
				{
					trilist.SetSigFalseAction(joinMap.IncomingAnswer.JoinNumber, () => AcceptCall(args.CallItem));
					trilist.SetSigFalseAction(joinMap.IncomingReject.JoinNumber, () => RejectCall(args.CallItem));
					trilist.SetString(joinMap.IncomingCallName.JoinNumber, args.CallItem.Name);
					trilist.SetString(joinMap.IncomingCallNumber.JoinNumber, args.CallItem.Number);
				}
				else
				{
					trilist.SetString(joinMap.IncomingCallName.JoinNumber, string.Empty);
					trilist.SetString(joinMap.IncomingCallNumber.JoinNumber, string.Empty);
				}


				trilist.SetString(joinMap.CurrentCallData.JoinNumber, UpdateCallStatusXSig());

				trilist.SetUshort(joinMap.ConnectedCallCount.JoinNumber, (ushort)ActiveCalls.Count);
			};

			if (this is IJoinCalls joinCodec)
			{
				trilist.SetSigFalseAction(joinMap.JoinAllCalls.JoinNumber, () => joinCodec.JoinAllCalls());

				for (int i = 0; i < joinMap.JoinCallStart.JoinSpan; i++)
				{
					trilist.SetSigFalseAction((uint)(joinMap.JoinCallStart.JoinNumber + i), () =>
									{
										var call = ActiveCalls[i];
										if (call != null)
										{
											joinCodec.JoinCall(call);
										}
										else
										{
											Debug.LogMessage(LogEventLevel.Information, this, "[Join Call] Unable to find call at index '{0}'", i);
										}
									});
				}
			}

			if (this is IHasCallHold holdCodec)
			{
				trilist.SetSigFalseAction(joinMap.HoldAllCalls.JoinNumber, () =>
				{
					foreach (var call in ActiveCalls)
					{
						holdCodec.HoldCall(call);
					}
				});

				for (int i = 0; i < joinMap.HoldCallsStart.JoinSpan; i++)
				{
					var index = i;

					trilist.SetSigFalseAction((uint)(joinMap.HoldCallsStart.JoinNumber + index), () =>
									{
										if (index < 0 || index >= ActiveCalls.Count) return;

										var call = ActiveCalls[index];
										if (call != null)
										{
											holdCodec.HoldCall(call);
										}
										else
										{
											Debug.LogMessage(LogEventLevel.Information, this, "[Hold Call] Unable to find call at index '{0}'", i);
										}
									});

					trilist.SetSigFalseAction((uint)(joinMap.ResumeCallsStart.JoinNumber + index), () =>
									{
										if (index < 0 || index >= ActiveCalls.Count) return;

										var call = ActiveCalls[index];
										if (call != null)
										{
											holdCodec.ResumeCall(call);
										}
										else
										{
											Debug.LogMessage(LogEventLevel.Information, this, "[Resume Call] Unable to find call at index '{0}'", i);
										}
									});
				}
			}



			trilist.OnlineStatusChange += (device, args) =>
{
	if (!args.DeviceOnLine) return;

	// TODO [ ] #983
	Debug.LogMessage(LogEventLevel.Information, this, "LinkVideoCodecCallControlsToApi: device is {0}, IsInCall {1}", args.DeviceOnLine ? "online" : "offline", IsInCall);
	trilist.SetBool(joinMap.HookState.JoinNumber, IsInCall);
	trilist.SetString(joinMap.CurrentCallData.JoinNumber, "\xFC");
	trilist.SetString(joinMap.CurrentCallData.JoinNumber, UpdateCallStatusXSig());
};
		}

		private string UpdateCallStatusXSig()
		{
			const int maxCalls = 8;
			const int maxStrings = 6;
			const int maxDigitals = 2;
			const int offset = maxStrings + maxDigitals;
			var stringIndex = 0;
			var digitalIndex = maxStrings * maxCalls;
			var arrayIndex = 0;

			var tokenArray = new XSigToken[maxCalls * offset]; //set array size for number of calls * pieces of info

			foreach (var call in ActiveCalls)
			{
				if (arrayIndex >= maxCalls * offset)
					break;
				//digitals
				tokenArray[digitalIndex] = new XSigDigitalToken(digitalIndex + 1, call.IsActiveCall);
				tokenArray[digitalIndex + 1] = new XSigDigitalToken(digitalIndex + 2, call.IsOnHold);

				//serials
				tokenArray[stringIndex] = new XSigSerialToken(stringIndex + 1, call.Name ?? String.Empty);
				tokenArray[stringIndex + 1] = new XSigSerialToken(stringIndex + 2, call.Number ?? String.Empty);
				tokenArray[stringIndex + 2] = new XSigSerialToken(stringIndex + 3, call.Direction.ToString());
				tokenArray[stringIndex + 3] = new XSigSerialToken(stringIndex + 4, call.Type.ToString());
				tokenArray[stringIndex + 4] = new XSigSerialToken(stringIndex + 5, call.Status.ToString());
				if (call.Duration != null)
				{
					// May need to verify correct string format here
					var dur = string.Format("{0:c}", call.Duration);
					tokenArray[arrayIndex + 6] = new XSigSerialToken(stringIndex + 6, dur);
				}

				arrayIndex += offset;
				stringIndex += maxStrings;
				digitalIndex += maxDigitals;
			}
			while (arrayIndex < maxCalls * offset)
			{
				//digitals
				tokenArray[digitalIndex] = new XSigDigitalToken(digitalIndex + 1, false);
				tokenArray[digitalIndex + 1] = new XSigDigitalToken(digitalIndex + 2, false);


				//serials
				tokenArray[stringIndex] = new XSigSerialToken(stringIndex + 1, String.Empty);
				tokenArray[stringIndex + 1] = new XSigSerialToken(stringIndex + 2, String.Empty);
				tokenArray[stringIndex + 2] = new XSigSerialToken(stringIndex + 3, String.Empty);
				tokenArray[stringIndex + 3] = new XSigSerialToken(stringIndex + 4, String.Empty);
				tokenArray[stringIndex + 4] = new XSigSerialToken(stringIndex + 5, String.Empty);
				tokenArray[stringIndex + 5] = new XSigSerialToken(stringIndex + 6, String.Empty);

				arrayIndex += offset;
				stringIndex += maxStrings;
				digitalIndex += maxDigitals;
			}

			return GetXSigString(tokenArray);
		}

		private void LinkVideoCodecDtmfToApi(BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
		{
			trilist.SetSigFalseAction(joinMap.Dtmf0.JoinNumber, () => SendDtmfAction("0", trilist, joinMap));
			trilist.SetSigFalseAction(joinMap.Dtmf1.JoinNumber, () => SendDtmfAction("1", trilist, joinMap));
			trilist.SetSigFalseAction(joinMap.Dtmf2.JoinNumber, () => SendDtmfAction("2", trilist, joinMap));
			trilist.SetSigFalseAction(joinMap.Dtmf3.JoinNumber, () => SendDtmfAction("3", trilist, joinMap));
			trilist.SetSigFalseAction(joinMap.Dtmf4.JoinNumber, () => SendDtmfAction("4", trilist, joinMap));
			trilist.SetSigFalseAction(joinMap.Dtmf5.JoinNumber, () => SendDtmfAction("5", trilist, joinMap));
			trilist.SetSigFalseAction(joinMap.Dtmf6.JoinNumber, () => SendDtmfAction("6", trilist, joinMap));
			trilist.SetSigFalseAction(joinMap.Dtmf7.JoinNumber, () => SendDtmfAction("7", trilist, joinMap));
			trilist.SetSigFalseAction(joinMap.Dtmf8.JoinNumber, () => SendDtmfAction("8", trilist, joinMap));
			trilist.SetSigFalseAction(joinMap.Dtmf9.JoinNumber, () => SendDtmfAction("9", trilist, joinMap));
			trilist.SetSigFalseAction(joinMap.DtmfStar.JoinNumber, () => SendDtmfAction("*", trilist, joinMap));
			trilist.SetSigFalseAction(joinMap.DtmfPound.JoinNumber, () => SendDtmfAction("#", trilist, joinMap));
		}

		/// <summary>
		/// Sends the specified string as a DTMF command.
		/// Reads the value of the SendDtmfToSpecificCallInstance digital join and SelectCall analog join to determine
		/// Whther to send to a specific call index or to the last connected call
		/// </summary>
		/// <param name="s"></param>
		/// <param name="trilist"></param>
		/// <param name="joinMap"></param>
		private void SendDtmfAction(string s, BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
		{
			if (!trilist.GetBool(joinMap.SendDtmfToSpecificCallIndex.JoinNumber))
			{
				SendDtmf(s);
			}
			else
			{
				var callIndex = trilist.GetUshort(joinMap.SelectCall.JoinNumber);
				if (callIndex > 0 && callIndex <= 8)
				{
					var call = ActiveCalls[callIndex - 1];
					if (call != null && call.IsActiveCall)
					{
						SendDtmf(s, call);
					}
					else
					{
						Debug.LogMessage(LogEventLevel.Information, this, "Warning: No call found at index {0} or call is not active.", callIndex);
					}
				}
				else
				{
					Debug.LogMessage(LogEventLevel.Information, this, "Warning: Invalid call index specified.  Please use a value of 1-8.");
				}
			}
		}

		private void LinkVideoCodecCameraLayoutsToApi(IHasCodecLayouts codec, BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
		{
			trilist.SetSigFalseAction(joinMap.CameraLayout.JoinNumber, codec.LocalLayoutToggle);

			codec.LocalLayoutFeedback.LinkInputSig(trilist.StringInput[joinMap.CurrentLayoutStringFb.JoinNumber]);
		}

		private void LinkVideoCodecCameraModeToApi(IHasCameraAutoMode codec, BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
		{
			trilist.SetSigFalseAction(joinMap.CameraModeAuto.JoinNumber, codec.CameraAutoModeOn);
			trilist.SetSigFalseAction(joinMap.CameraModeManual.JoinNumber, codec.CameraAutoModeOff);

			codec.CameraAutoModeIsOnFeedback.OutputChange += (o, a) =>
			{
				if (codec is IHasCameraOff offCodec)
				{
					if (offCodec.CameraIsOffFeedback.BoolValue)
					{
						trilist.SetBool(joinMap.CameraModeAuto.JoinNumber, false);
						trilist.SetBool(joinMap.CameraModeManual.JoinNumber, false);
						trilist.SetBool(joinMap.CameraModeOff.JoinNumber, true);
						return;
					}

					trilist.SetBool(joinMap.CameraModeAuto.JoinNumber, a.BoolValue);
					trilist.SetBool(joinMap.CameraModeManual.JoinNumber, !a.BoolValue);
					trilist.SetBool(joinMap.CameraModeOff.JoinNumber, false);
					return;
				}

				trilist.SetBool(joinMap.CameraModeAuto.JoinNumber, a.BoolValue);
				trilist.SetBool(joinMap.CameraModeManual.JoinNumber, !a.BoolValue);
				trilist.SetBool(joinMap.CameraModeOff.JoinNumber, false);
			};


			if (codec is IHasCameraOff offModeCodec)
			{
				if (offModeCodec.CameraIsOffFeedback.BoolValue)
				{
					trilist.SetBool(joinMap.CameraModeAuto.JoinNumber, false);
					trilist.SetBool(joinMap.CameraModeManual.JoinNumber, false);
					trilist.SetBool(joinMap.CameraModeOff.JoinNumber, true);
					return;
				}

				trilist.SetBool(joinMap.CameraModeAuto.JoinNumber, codec.CameraAutoModeIsOnFeedback.BoolValue);
				trilist.SetBool(joinMap.CameraModeManual.JoinNumber, !codec.CameraAutoModeIsOnFeedback.BoolValue);
				trilist.SetBool(joinMap.CameraModeOff.JoinNumber, false);
				return;
			}

			trilist.SetBool(joinMap.CameraModeAuto.JoinNumber, codec.CameraAutoModeIsOnFeedback.BoolValue);
			trilist.SetBool(joinMap.CameraModeManual.JoinNumber, !codec.CameraAutoModeIsOnFeedback.BoolValue);
			trilist.SetBool(joinMap.CameraModeOff.JoinNumber, false);
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

				if (!(codec.SelectedCamera is IHasCameraPtzControl camera)) return;

				if (b) camera.TiltUp();
				else camera.TiltStop();
			});

			trilist.SetBoolSigAction(joinMap.CameraTiltDown.JoinNumber, (b) =>
			{
				if (codec.SelectedCamera == null) return;

				if (!(codec.SelectedCamera is IHasCameraPtzControl camera)) return;

				if (b) camera.TiltDown();
				else camera.TiltStop();
			});
			trilist.SetBoolSigAction(joinMap.CameraPanLeft.JoinNumber, (b) =>
			{
				if (codec.SelectedCamera == null) return;

				if (!(codec.SelectedCamera is IHasCameraPtzControl camera)) return;

				if (b) camera.PanLeft();
				else camera.PanStop();
			});
			trilist.SetBoolSigAction(joinMap.CameraPanRight.JoinNumber, (b) =>
			{
				if (codec.SelectedCamera == null) return;

				if (!(codec.SelectedCamera is IHasCameraPtzControl camera)) return;

				if (b) camera.PanRight();
				else camera.PanStop();
			});

			trilist.SetBoolSigAction(joinMap.CameraZoomIn.JoinNumber, (b) =>
			{
				if (codec.SelectedCamera == null) return;

				if (!(codec.SelectedCamera is IHasCameraPtzControl camera)) return;

				if (b) camera.ZoomIn();
				else camera.ZoomStop();
			});

			trilist.SetBoolSigAction(joinMap.CameraZoomOut.JoinNumber, (b) =>
			{
				if (codec.SelectedCamera == null) return;

				if (!(codec.SelectedCamera is IHasCameraPtzControl camera)) return;

				if (b) camera.ZoomOut();
				else camera.ZoomStop();
			});


			trilist.SetBoolSigAction(joinMap.CameraFocusNear.JoinNumber, (b) =>
			{
				if (codec.SelectedCamera == null) return;

				if (!(codec.SelectedCamera is IHasCameraFocusControl camera)) return;

				if (b) camera.FocusNear();
				else camera.FocusStop();
			});

			trilist.SetBoolSigAction(joinMap.CameraFocusFar.JoinNumber, (b) =>
			{
				if (codec.SelectedCamera == null) return;

				if (!(codec.SelectedCamera is IHasCameraFocusControl camera)) return;

				if (b) camera.FocusFar();
				else camera.FocusStop();
			});

			trilist.SetSigFalseAction(joinMap.CameraFocusAuto.JoinNumber, () =>
			{
				if (codec.SelectedCamera == null) return;

				if (!(codec.SelectedCamera is IHasCameraFocusControl camera)) return;

				camera.TriggerAutoFocus();
			});

			// Camera count
			trilist.SetUshort(joinMap.CameraCount.JoinNumber, (ushort)codec.Cameras.Count);

			// Camera names
			for (uint i = 0; i < joinMap.CameraNamesFb.JoinSpan; i++)
			{
				//Check the count first
				if (i < codec.Cameras.Count && codec.Cameras[(int)i] != null)
				{
					trilist.SetString(joinMap.CameraNamesFb.JoinNumber + i, codec.Cameras[(int)i].Name);
				}
				else
				{
					trilist.SetString(joinMap.CameraNamesFb.JoinNumber + i, "");
				}
			}

			//Camera Select
			trilist.SetUShortSigAction(joinMap.CameraNumberSelect.JoinNumber, (i) =>
			{
				if (i > 0 && i <= codec.Cameras.Count)
				{
					codec.SelectCamera(codec.Cameras[i - 1].Key);
				}
				else
				{
					Debug.LogMessage(LogEventLevel.Information, this, "Unable to select.  No camera found at index {0}", i);
				}
			});

			// Set initial selected camera feedback
			if (codec.SelectedCamera != null)
			{
				trilist.SetUshort(joinMap.CameraNumberSelect.JoinNumber, (ushort)codec.Cameras.FindIndex((c) => c.Key == codec.SelectedCamera.Key));
			}

			codec.CameraSelected += (sender, args) =>
			{
				var i = (ushort)codec.Cameras.FindIndex((c) => c.Key == args.SelectedCamera.Key);

				trilist.SetUshort(joinMap.CameraNumberSelect.JoinNumber, (ushort)(i + 1));

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

				trilist.SetUShortSigAction(joinMap.CameraPresetSelect.JoinNumber,
						(a) =>
						{
							cam.PresetSelect(a);
							trilist.SetUshort(joinMap.CameraPresetSelect.JoinNumber, a);
						});

				trilist.SetSigFalseAction(joinMap.CameraPresetSave.JoinNumber,
					() =>
					{
						cam.PresetStore(trilist.UShortOutput[joinMap.CameraPresetSelect.JoinNumber].UShortValue,
							String.Empty);
						trilist.PulseBool(joinMap.CameraPresetSave.JoinNumber, 3000);
					});
			};

			if (!(codec is IHasCodecRoomPresets)) return;

			var presetCodec = codec as IHasCodecRoomPresets;

			presetCodec.CodecRoomPresetsListHasChanged +=
				(sender, args) => SetCameraPresetNames(presetCodec.NearEndPresets);

			//Camera Presets
			trilist.SetUShortSigAction(joinMap.CameraPresetSelect.JoinNumber, (i) =>
			{
				presetCodec.CodecRoomPresetSelect(i);
			});


			// Far End Presets
			trilist.SetUShortSigAction(joinMap.FarEndPresetSelect.JoinNumber, (i) =>
			{
				presetCodec.SelectFarEndPreset(i);
			});


			trilist.SetSigFalseAction(joinMap.CameraPresetSave.JoinNumber,
					() =>
					{
						presetCodec.CodecRoomPresetStore(
							trilist.UShortOutput[joinMap.CameraPresetSelect.JoinNumber].UShortValue, String.Empty);
						trilist.PulseBool(joinMap.CameraPresetSave.JoinNumber, 3000);
					});

			trilist.OnlineStatusChange += (device, args) =>
			{
				if (!args.DeviceOnLine) return;

				// TODO [ ] Issue #868
				trilist.SetString(joinMap.CameraPresetNames.JoinNumber, "\xFC");
				SetCameraPresetNames(presetCodec.NearEndPresets);
			};
		}

		// Following fields only used for Bridging
		private int _selectedRecentCallItemIndex;
		private DirectoryItem _selectedDirectoryItem;

		private void LinkVideoCodecCallHistoryToApi(IHasCallHistory codec, BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
		{
			codec.CallHistory.RecentCallsListHasChanged += (o, a) =>
					{
						UpdateCallHistory(codec, trilist, joinMap);
					};

			// Selected item action and feedback
			trilist.SetUShortSigAction(joinMap.SelectRecentCallItem.JoinNumber, (u) =>
					{
						if (u == 0 || u > codec.CallHistory.RecentCalls.Count)
						{
							Debug.LogMessage(LogEventLevel.Verbose, this, "Recent Call History index out of range");
							return;
						}

						_selectedRecentCallItemIndex = (int)(u - 1);
						trilist.SetUshort(joinMap.SelectRecentCallItem.JoinNumber, u);

						var _selectedRecentCallItem = codec.CallHistory.RecentCalls[_selectedRecentCallItemIndex];

						if (_selectedRecentCallItem != null)
						{
							trilist.SetString(joinMap.SelectedRecentCallName.JoinNumber, _selectedRecentCallItem.Name);
							trilist.SetString(joinMap.SelectedRecentCallNumber.JoinNumber, _selectedRecentCallItem.Number);
							trilist.SetSigFalseAction(joinMap.RemoveSelectedRecentCallItem.JoinNumber, () => codec.RemoveCallHistoryEntry(_selectedRecentCallItem));
							trilist.SetSigFalseAction(joinMap.DialSelectedRecentCallItem.JoinNumber, () => this.Dial(_selectedRecentCallItem.Number));
						}
						else
						{
							trilist.SetString(joinMap.SelectedRecentCallName.JoinNumber, string.Empty);
							trilist.SetString(joinMap.SelectedRecentCallNumber.JoinNumber, string.Empty);
							trilist.ClearBoolSigAction(joinMap.RemoveSelectedRecentCallItem.JoinNumber);
							trilist.ClearBoolSigAction(joinMap.DialSelectedRecentCallItem.JoinNumber);
						}
					});
		}



		private void UpdateCallHistory(IHasCallHistory codec, BasicTriList trilist, VideoCodecControllerJoinMap joinMap)
		{
			// Clear out selected item
			_selectedRecentCallItemIndex = 0;

			trilist.SetUshort(joinMap.SelectRecentCallItem.JoinNumber, 0);
			trilist.SetString(joinMap.SelectedRecentCallName.JoinNumber, string.Empty);
			trilist.SetString(joinMap.SelectedRecentCallNumber.JoinNumber, string.Empty);
			trilist.ClearBoolSigAction(joinMap.RemoveSelectedRecentCallItem.JoinNumber);
			//

			trilist.SetUshort(joinMap.RecentCallCount.JoinNumber, (ushort)codec.CallHistory.RecentCalls.Count);

			// Update the call history joins
			var maxItems = joinMap.RecentCallNamesStart.JoinSpan;

			// Create history
			uint index = 0;
			for (uint i = 0; i < maxItems && i < codec.CallHistory.RecentCalls.Count; i++)
			{
				trilist.SetString(joinMap.RecentCallNamesStart.JoinNumber + i, codec.CallHistory.RecentCalls[(int)i].Name);
				trilist.SetString(joinMap.RecentCallTimesStart.JoinNumber + i, codec.CallHistory.RecentCalls[(int)i].StartTime.ToShortTimeString());
				trilist.SetUshort(joinMap.RecentCallOccurrenceType.JoinNumber + i, (ushort)codec.CallHistory.RecentCalls[(int)i].OccurrenceType);
				//i++;
				index = i;
			}

			//foreach(var item in codec.CallHistory.RecentCalls)
			//{
			//    trilist.SetString(joinMap.RecentCallNamesStart.JoinNumber + i, item.Name);
			//    trilist.SetString(joinMap.RecentCallTimesStart.JoinNumber + i, item.StartTime.ToShortTimeString());
			//    trilist.SetUshort(joinMap.RecentCallOccurrenceType.JoinNumber + i, (ushort)item.OccurrenceType);
			//    i++;
			//}

			// Clears existing items 
			for (uint j = index; j < maxItems; j++)
			{
				trilist.SetString(joinMap.RecentCallNamesStart.JoinNumber + j, string.Empty);
				trilist.SetString(joinMap.RecentCallTimesStart.JoinNumber + j, string.Empty);
				trilist.SetUshort(joinMap.RecentCallOccurrenceType.JoinNumber + j, 0);
			}
		}

		private string SetCameraPresetNames(IEnumerable<CodecRoomPreset> presets)
		{
			return SetCameraPresetNames(presets.Select(p => p.Description).ToList());
		}

		private string SetCameraPresetNames(IEnumerable<CameraPreset> presets)
		{
			return SetCameraPresetNames(presets.Select(p => p.Description).ToList());
		}

		private string SetCameraPresetNames(ICollection<string> presets)
		{
			var i = 1; //start index for xsig;

			var tokenArray = new XSigToken[presets.Count];

			foreach (var preset in presets)
			{
				var cameraPreset = new XSigSerialToken(i, preset);
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
}