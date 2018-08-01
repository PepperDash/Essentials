using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Room.Cotija;

namespace PepperDash.Essentials
{
    public class CotijaEssentialsHuddleSpaceRoomBridge : CotijaBridgeBase
    {

        public EssentialsRoomBase Room { get; private set; }

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
			Parent.AddAction(string.Format(@"/room/{0}/status", Room.Key), new Action(() => Room_RoomFullStatus(Room)));

			var routeRoom = Room as IRunRouteAction;
			if(routeRoom != null)
				Parent.AddAction(string.Format(@"/room/{0}/source", Room.Key), new Action<SourceSelectMessageContent>(c => routeRoom.RunRouteAction(c.SourceListItem)));

			var defaultRoom = Room as IRunDefaultPresentRoute;
			if(defaultRoom != null)
				Parent.AddAction(string.Format(@"/room/{0}/defaultsource", Room.Key), new Action(() => defaultRoom.RunDefaultPresentRoute()));

			var vcRoom = Room as IHasCurrentVolumeControls;
			if (vcRoom != null)
			{
				Parent.AddAction(string.Format(@"/room/{0}/volumes/master/level", Room.Key), new Action<ushort>(u =>
					(vcRoom.CurrentVolumeControls as IBasicVolumeWithFeedback).SetVolume(u)));
				Parent.AddAction(string.Format(@"/room/{0}/volumes/master/mute", Room.Key), new Action(() => 
					vcRoom.CurrentVolumeControls.MuteToggle()));
				vcRoom.CurrentVolumeDeviceChange += new EventHandler<VolumeDeviceChangeEventArgs>(Room_CurrentVolumeDeviceChange);

				// Registers for initial volume events, if possible
				var currentVolumeDevice = vcRoom.CurrentVolumeControls as IBasicVolumeWithFeedback;
				if (currentVolumeDevice != null)
				{
					currentVolumeDevice.MuteFeedback.OutputChange += VolumeLevelFeedback_OutputChange;
					currentVolumeDevice.VolumeLevelFeedback.OutputChange += VolumeLevelFeedback_OutputChange;
				}
			}

			var sscRoom = Room as IHasCurrentSourceInfoChange;
			if(sscRoom != null)
				sscRoom.CurrentSingleSourceChange += new SourceInfoChangeHandler(Room_CurrentSingleSourceChange);

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
        void Room_RoomFullStatus(EssentialsRoomBase room)
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
				isOn = room.OnFeedback.BoolValue,
				selectedSourceKey = sourceKey,
				volumes = volumes
			});
        }
     
    }

    public class SourceSelectMessageContent
    {
		public string SourceListItem { get; set; }
    }

    public delegate void PressAndHoldAction(bool b);

}