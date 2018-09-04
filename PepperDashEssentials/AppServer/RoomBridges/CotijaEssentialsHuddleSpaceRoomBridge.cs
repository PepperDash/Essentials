using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Room.Cotija;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials
{
    public class CotijaEssentialsHuddleSpaceRoomBridge : CotijaBridgeBase
    {

        public EssentialsRoomBase Room { get; private set; }

		public VideoCodecBaseMessenger VCMessenger { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public override string RoomName
		{
			get
			{
				return Room.Name;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="room"></param>
        public CotijaEssentialsHuddleSpaceRoomBridge(EssentialsRoomBase room):
			base("mobileControlBridge-essentialsHuddle", "Essentials Mobile Control Bridge-Huddle")
        {
            Room = room;   
        }

		/// <summary>
		/// Override of base: calls base to add parent and then registers actions and events.
		/// </summary>
		/// <param name="parent"></param>
		public override void AddParent(CotijaSystemController parent)
		{
			base.AddParent(parent);

			// we add actions to the messaging system with a path, and a related action. Custom action
			// content objects can be handled in the controller's LineReceived method - and perhaps other
			// sub-controller parsing could be attached to these classes, so that the systemController
			// doesn't need to know about everything.

			// Source Changes and room off
			Parent.AddAction(string.Format(@"/room/{0}/status", Room.Key), new Action(() => SendFullStatus(Room)));

			var routeRoom = Room as IRunRouteAction;
			if(routeRoom != null)
				Parent.AddAction(string.Format(@"/room/{0}/source", Room.Key), new Action<SourceSelectMessageContent>(c => 
					routeRoom.RunRouteAction(c.SourceListItem)));

			var defaultRoom = Room as IRunDefaultPresentRoute;
			if(defaultRoom != null)
				Parent.AddAction(string.Format(@"/room/{0}/defaultsource", Room.Key), new Action(() => defaultRoom.RunDefaultPresentRoute()));

			var volumeRoom = Room as IHasCurrentVolumeControls;
			if (volumeRoom != null)
			{
				Parent.AddAction(string.Format(@"/room/{0}/volumes/master/level", Room.Key), new Action<ushort>(u =>
					(volumeRoom.CurrentVolumeControls as IBasicVolumeWithFeedback).SetVolume(u)));
				Parent.AddAction(string.Format(@"/room/{0}/volumes/master/muteToggle", Room.Key), new Action(() => 
					volumeRoom.CurrentVolumeControls.MuteToggle()));
				volumeRoom.CurrentVolumeDeviceChange += new EventHandler<VolumeDeviceChangeEventArgs>(Room_CurrentVolumeDeviceChange);

				// Registers for initial volume events, if possible
				var currentVolumeDevice = volumeRoom.CurrentVolumeControls as IBasicVolumeWithFeedback;
				if (currentVolumeDevice != null)
				{
					currentVolumeDevice.MuteFeedback.OutputChange += VolumeLevelFeedback_OutputChange;
					currentVolumeDevice.VolumeLevelFeedback.OutputChange += VolumeLevelFeedback_OutputChange;
				}
			}

			var sscRoom = Room as IHasCurrentSourceInfoChange;
			if(sscRoom != null)
				sscRoom.CurrentSingleSourceChange += new SourceInfoChangeHandler(Room_CurrentSingleSourceChange);

			var vcRoom = Room as IHasVideoCodec;
			if (vcRoom != null)
			{
				var codec = vcRoom.VideoCodec;
				VCMessenger = new VideoCodecBaseMessenger(vcRoom.VideoCodec, "/device/videoCodec");
				VCMessenger.RegisterWithAppServer(Parent);

				// May need to move this or remove this 
				codec.CallStatusChange += new EventHandler<CodecCallStatusItemChangeEventArgs>(codec_CallStatusChange);

				vcRoom.IsSharingFeedback.OutputChange += new EventHandler<FeedbackEventArgs>(IsSharingFeedback_OutputChange);

				//Parent.AddAction("/device/videoCodec/dial", new Action<string>(s => codec.Dial(s)));
				//Parent.AddAction("/device/videoCodec/endCall", new Action<string>(s =>
				//{
				//    var call = codec.ActiveCalls.FirstOrDefault(c => c.Id == s);
				//    if (call != null)
				//    {
				//        codec.EndCall(call);
				//    }
				//}));
				//Parent.AddAction("/device/videoCodec/endAllCalls", new Action(() => codec.EndAllCalls()));
			}

			var defCallRm = Room as IRunDefaultCallRoute;
			if (defCallRm != null)
			{
				Parent.AddAction(string.Format(@"/room/{0}/activityVideo", Room.Key), new Action(()=>defCallRm.RunDefaultCallRoute()));
			}

			Parent.AddAction(string.Format(@"/room/{0}/shutdownStart", Room.Key), new Action(() => Room.StartShutdown(eShutdownType.Manual)));
			Parent.AddAction(string.Format(@"/room/{0}/shutdownEnd", Room.Key), new Action(() => Room.ShutdownPromptTimer.Finish()));
			Parent.AddAction(string.Format(@"/room/{0}/shutdownCancel", Room.Key), new Action(() => Room.ShutdownPromptTimer.Cancel()));

			Room.OnFeedback.OutputChange += OnFeedback_OutputChange;
			Room.IsCoolingDownFeedback.OutputChange += IsCoolingDownFeedback_OutputChange;
			Room.IsWarmingUpFeedback.OutputChange += IsWarmingUpFeedback_OutputChange;

			Room.ShutdownPromptTimer.HasStarted += ShutdownPromptTimer_HasStarted;
			Room.ShutdownPromptTimer.HasFinished += ShutdownPromptTimer_HasFinished;
			Room.ShutdownPromptTimer.WasCancelled += ShutdownPromptTimer_WasCancelled;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void IsSharingFeedback_OutputChange(object sender, FeedbackEventArgs e)
		{
			// sharing source 
			string shareText;
			bool isSharing;
#warning This share update needs to happen on source change as well!
			var vcRoom = Room as IHasVideoCodec;
			var srcInfoRoom = Room as IHasCurrentSourceInfoChange;
			if (vcRoom.VideoCodec.SharingContentIsOnFeedback.BoolValue && srcInfoRoom.CurrentSourceInfo != null)
			{

				shareText = srcInfoRoom.CurrentSourceInfo.PreferredName;
				isSharing = true;
			}
			else
			{
				shareText = "None";
				isSharing = false;
			}

			PostStatusMessage(new
			{
				share = new
				{
					currentShareText = shareText,
					isSharing = isSharing
				}
			});
		}

		/// <summary>
		/// Handler for codec changes
		/// </summary>
		void codec_CallStatusChange(object sender, CodecCallStatusItemChangeEventArgs e)
		{
			PostStatusMessage(new
			{
				calls = GetCallsMessageObject(),
				//vtc = GetVtcCallsMessageObject()
			});

		}

		/// <summary>
		/// Helper for posting status message
		/// </summary>
		/// <param name="contentObject">The contents of the content object</param>
		void PostStatusMessage(object contentObject)
		{
			Parent.SendMessageToServer(JObject.FromObject(new
			{
				type = "/room/status/",
				content = contentObject
			}));
		}

		/// <summary>
		/// Handler for cancelled shutdown
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void ShutdownPromptTimer_WasCancelled(object sender, EventArgs e)
		{
			JObject roomStatus = new JObject();
			roomStatus.Add("state", "wasCancelled");
			JObject message = new JObject();
			message.Add("type", "/room/shutdown/");
			message.Add("content", roomStatus);
			Parent.SendMessageToServer(message);
		}

		/// <summary>
		/// Handler for when shutdown finishes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void ShutdownPromptTimer_HasFinished(object sender, EventArgs e)
		{
			JObject roomStatus = new JObject();
			roomStatus.Add("state", "hasFinished");
			JObject message = new JObject();
			message.Add("type", "/room/shutdown/");
			message.Add("content", roomStatus);
			Parent.SendMessageToServer(message);
		}

		/// <summary>
		/// Handler for when shutdown starts
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void ShutdownPromptTimer_HasStarted(object sender, EventArgs e)
		{
			JObject roomStatus = new JObject();
			roomStatus.Add("state", "hasStarted");
			roomStatus.Add("duration", Room.ShutdownPromptTimer.SecondsToCount);
			JObject message = new JObject();
			message.Add("type", "/room/shutdown/");
			message.Add("content", roomStatus);
			Parent.SendMessageToServer(message);
			// equivalent JS message:
			//	Post( { type: '/room/status/', content: { shutdown: 'hasStarted', duration: Room.ShutdownPromptTimer.SecondsToCount })
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void IsWarmingUpFeedback_OutputChange(object sender, FeedbackEventArgs e)
		{
			PostStatusMessage(new
			{
				isWarmingUp = e.BoolValue
			});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void IsCoolingDownFeedback_OutputChange(object sender, FeedbackEventArgs e)
		{
			PostStatusMessage(new
			{
				isCoolingDown = e.BoolValue
			});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        void OnFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            PostStatusMessage(new
				{
					isOn = e.BoolValue
				});
        }

        void Room_CurrentVolumeDeviceChange(object sender, VolumeDeviceChangeEventArgs e)
        {
            if (e.OldDev is IBasicVolumeWithFeedback)
            {
                var oldDev = e.OldDev as IBasicVolumeWithFeedback;
                oldDev.MuteFeedback.OutputChange -= MuteFeedback_OutputChange;
                oldDev.VolumeLevelFeedback.OutputChange -= VolumeLevelFeedback_OutputChange;
            }

            if (e.NewDev is IBasicVolumeWithFeedback)
            {
                var newDev = e.NewDev as IBasicVolumeWithFeedback;
				newDev.MuteFeedback.OutputChange += MuteFeedback_OutputChange;
                newDev.VolumeLevelFeedback.OutputChange += VolumeLevelFeedback_OutputChange;
            }
        }

		/// <summary>
		/// Event handler for mute changes
		/// </summary>
		void MuteFeedback_OutputChange(object sender, FeedbackEventArgs e)
		{
			PostStatusMessage(new
			{
				volumes = new
				{
					master = new
					{
						muted = e.BoolValue
					}
				}
			});
		}

		/// <summary>
		/// Handles Volume changes on room
		/// </summary>
        void VolumeLevelFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
			PostStatusMessage(new
			{
				volumes = new
				{
					master = new
					{
						level = e.IntValue
					}
				}
			});
        }


        void Room_CurrentSingleSourceChange(EssentialsRoomBase room, PepperDash.Essentials.Core.SourceListItem info, ChangeType type)
        {
            /* Example message
             * {
                  "type":"/room/status",
                  "content": {
                    "selectedSourceKey": "off",
                  }
                }
             */
            if (type == ChangeType.WillChange)
            {
                // Disconnect from previous source

                if (info != null)
                {
                    var previousDev = info.SourceDevice;

                    // device type interfaces
                    if (previousDev is ISetTopBoxControls)
                        (previousDev as ISetTopBoxControls).UnlinkActions(Parent);
                    // common interfaces
                    if (previousDev is IChannel)
                        (previousDev as IChannel).UnlinkActions(Parent);
                    if (previousDev is IColor)
                        (previousDev as IColor).UnlinkActions(Parent);
                    if (previousDev is IDPad)
                        (previousDev as IDPad).UnlinkActions(Parent);
                    if (previousDev is IDvr)
                        (previousDev as IDvr).UnlinkActions(Parent);
                    if (previousDev is INumericKeypad)
                        (previousDev as INumericKeypad).UnlinkActions(Parent);
                    if (previousDev is IPower)
                        (previousDev as IPower).UnlinkActions(Parent);
                    if (previousDev is ITransport)
                        (previousDev as ITransport).UnlinkActions(Parent);
                }
            }
            else // did change
            {
                if (info != null)
                {
                    var dev = info.SourceDevice;

                    if (dev is ISetTopBoxControls)
                        (dev as ISetTopBoxControls).LinkActions(Parent);
                    if (dev is IChannel)
                        (dev as IChannel).LinkActions(Parent);
                    if (dev is IColor)
                        (dev as IColor).LinkActions(Parent);
                    if (dev is IDPad)
                        (dev as IDPad).LinkActions(Parent);
                    if (dev is IDvr)
                        (dev as IDvr).LinkActions(Parent);
                    if (dev is INumericKeypad)
                        (dev as INumericKeypad).LinkActions(Parent);
                    if (dev is IPower)
                        (dev as IPower).LinkActions(Parent);
                    if (dev is ITransport)
                        (dev as ITransport).LinkActions(Parent);

					var srcRm = room as IHasCurrentSourceInfoChange;
					PostStatusMessage(new
					{
						selectedSourceKey = srcRm.CurrentSourceInfoKey
					});
                }
            }
        }

        /// <summary>
        /// Posts the full status of the room to the server
        /// </summary>
        /// <param name="room"></param>
        void SendFullStatus(EssentialsRoomBase room)
        {
			var sourceKey = room is IHasCurrentSourceInfoChange ? (room as IHasCurrentSourceInfoChange).CurrentSourceInfoKey : null;
			
			var rmVc = room as IHasCurrentVolumeControls;
			var volumes = new Volumes();
			if (rmVc != null)
			{
				var vc = rmVc.CurrentVolumeControls as IBasicVolumeWithFeedback;
				if (rmVc != null)
				{
					volumes.Master = new Volume("master", vc.VolumeLevelFeedback.UShortValue, vc.MuteFeedback.BoolValue, "Volume", true, "");
				}
			}

			PostStatusMessage(new
			{
				calls = GetCallsMessageObject(),
				isOn = room.OnFeedback.BoolValue,
				selectedSourceKey = sourceKey,
				vtc = GetVtcCallsMessageObject(),
				volumes = volumes
			});
        }

		/// <summary>
		/// Helper to return a anonymous object with the call data for JSON message
		/// </summary>
		/// <returns></returns>
		object GetCallsMessageObject()
		{
			var callRm = Room as IHasVideoCodec;
			if (callRm == null)
				return null;
			return new
			{
				activeCalls = callRm.VideoCodec.ActiveCalls,
				callType = callRm.CallTypeFeedback.IntValue,
				inCall = callRm.InCallFeedback.BoolValue,
				isSharing = callRm.IsSharingFeedback.BoolValue,
				privacyModeIsOn = callRm.PrivacyModeIsOnFeedback.BoolValue
			};
		}

		/// <summary>
		/// Helper method to build call status for vtc
		/// </summary>
		/// <returns></returns>
		object GetVtcCallsMessageObject()
		{
			var callRm = Room as IHasVideoCodec;
			object vtc = null;
			if (callRm != null)
			{
				var codec = callRm.VideoCodec;
				vtc = new
				{
					isInCall = codec.IsInCall,
					calls = codec.ActiveCalls
				};
			}
			return vtc;
		}
     
    }

	/// <summary>
	/// 
	/// </summary>
    public class SourceSelectMessageContent
    {
		public string SourceListItem { get; set; }
    }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="b"></param>
    public delegate void PressAndHoldAction(bool b);

}