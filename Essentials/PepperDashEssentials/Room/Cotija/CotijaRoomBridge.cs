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
    public class CotijaEssentialsHuddleSpaceRoomBridge
    {
        CotijaSystemController Parent;

        public EssentialsHuddleSpaceRoom Room { get; private set; }

        public CotijaEssentialsHuddleSpaceRoomBridge(CotijaSystemController parent, EssentialsHuddleSpaceRoom room)
        {
            Parent = parent;
            Room = room;

            // Source Changes and room off
            Parent.AddAction(string.Format(@"/room/{0}/status",Room.Key), new Action(() => Room_RoomFullStatus(Room)));
            Parent.AddAction(string.Format(@"/room/{0}/source", Room.Key), new Action<SourceSelectMessageContent>(c => room.RunRouteAction(c.SourceSelect)));
            Parent.AddAction(string.Format(@"/room/{0}/event/masterVolumeUpBtn", Room.Key), new PressAndHoldAction(b => room.CurrentVolumeControls.VolumeUp(b)));
            Parent.AddAction(string.Format(@"/room/{0}/event/masterVolumeDownBtn", Room.Key), new PressAndHoldAction(b => room.CurrentVolumeControls.VolumeDown(b)));
            Parent.AddAction(string.Format(@"/room/{0}/event/muteToggle", Room.Key), new Action(() => room.CurrentVolumeControls.MuteToggle()));

            Room.CurrentSingleSourceChange += new SourceInfoChangeHandler(Room_CurrentSingleSourceChange);

            Room.CurrentVolumeDeviceChange += new EventHandler<VolumeDeviceChangeEventArgs>(Room_CurrentVolumeDeviceChange);

            Room.OnFeedback.OutputChange += new EventHandler<EventArgs>(OnFeedback_OutputChange);

            // Registers for initial volume events, if possible
            var currentVolumeDevice = Room.CurrentVolumeControls;

            if (currentVolumeDevice != null)
            {
                if (currentVolumeDevice is IBasicVolumeWithFeedback)
                {
                    var newDev = currentVolumeDevice as IBasicVolumeWithFeedback;

                    newDev.MuteFeedback.OutputChange += new EventHandler<EventArgs>(VolumeLevelFeedback_OutputChange);
                    newDev.VolumeLevelFeedback.OutputChange += new EventHandler<EventArgs>(VolumeLevelFeedback_OutputChange);
                }
            }
            
        }

        void OnFeedback_OutputChange(object sender, EventArgs e)
        {
            /* Example message
            * {
                 "type":"/room/status",
                 "content": {
                   "isOn": false
                 }
               }
            */

            JObject roomStatus = new JObject();

            roomStatus.Add("isOn", (sender as BoolFeedback).BoolValue);

            JObject message = new JObject();

            message.Add("type", "/room/status/");
            message.Add("content", roomStatus);

            Parent.PostToServer(Room, message);
        }

        void Room_CurrentVolumeDeviceChange(object sender, VolumeDeviceChangeEventArgs e)
        {
            if (e.OldDev is IBasicVolumeWithFeedback)
            {
                var oldDev = e.OldDev as IBasicVolumeWithFeedback;

                oldDev.MuteFeedback.OutputChange -= VolumeLevelFeedback_OutputChange;
                oldDev.VolumeLevelFeedback.OutputChange -= VolumeLevelFeedback_OutputChange;
            }

            if (e.NewDev is IBasicVolumeWithFeedback)
            {
                var newDev = e.NewDev as IBasicVolumeWithFeedback;

                newDev.MuteFeedback.OutputChange += new EventHandler<EventArgs>(VolumeLevelFeedback_OutputChange);
                newDev.VolumeLevelFeedback.OutputChange += new EventHandler<EventArgs>(VolumeLevelFeedback_OutputChange);
            }
        }

        void VolumeLevelFeedback_OutputChange(object sender, EventArgs e)
        {
            /* Example message
             * {
                  "type":"/room/status",
                  "content": {
                    "masterVolumeLevel": 12345,
                    "masterVolumeMuteState": false
                  }
                }
             */

            var huddleRoom = Room as EssentialsHuddleSpaceRoom;

            if(huddleRoom.CurrentVolumeControls is IBasicVolumeWithFeedback)
            {
                JObject roomStatus = new JObject();

                if (huddleRoom.CurrentVolumeControls is IBasicVolumeWithFeedback)
                {
                    var currentVolumeConstrols = huddleRoom.CurrentVolumeControls as IBasicVolumeWithFeedback;
                    roomStatus.Add("masterVolumeLevel", currentVolumeConstrols.VolumeLevelFeedback.IntValue);
                    roomStatus.Add("masterVolumeMuteState", currentVolumeConstrols.MuteFeedback.BoolValue);
                }

                JObject message = new JObject();

                message.Add("type", "/room/status/");
                message.Add("content", roomStatus);

                Parent.PostToServer(Room, message);
            }
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

                JObject roomStatus = new JObject();

                var huddleRoom = room as EssentialsHuddleSpaceRoom;
                roomStatus.Add("selectedSourceKey", huddleRoom.CurrentSourceInfoKey);

                JObject message = new JObject();

                message.Add("type", "/room/status/");
                message.Add("content", roomStatus);

                Parent.PostToServer(Room, message);
            }
            else 
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
                }
            }
        }

        /// <summary>
        /// Posts the full status of the room to the server
        /// </summary>
        /// <param name="room"></param>
        void Room_RoomFullStatus(EssentialsRoomBase room)
        {
            /* Example message
            * {
                 "type":"/room/status",
                 "content": {
                   "selectedSourceKey": "off",
                   "isOn": false,
                   "masterVolumeLevel": 50,
                   "masterVolumeMuteState": false
                 }
               }
            */

            JObject roomStatus = new JObject();

            var huddleRoom = room as EssentialsHuddleSpaceRoom;
            roomStatus.Add("isOn", huddleRoom.OnFeedback.BoolValue);
            roomStatus.Add("selectedSourceKey", huddleRoom.CurrentSourceInfoKey);


            if(huddleRoom.CurrentVolumeControls is IBasicVolumeWithFeedback)
            {
                var currentVolumeConstrols = huddleRoom.CurrentVolumeControls as IBasicVolumeWithFeedback;
                roomStatus.Add("masterVolumeLevel", currentVolumeConstrols.VolumeLevelFeedback.IntValue);
                roomStatus.Add("masterVolumeMuteState", currentVolumeConstrols.MuteFeedback.BoolValue);
            }

            JObject message = new JObject();

            message.Add("type", "/room/status/");
            message.Add("content", roomStatus);

            Parent.PostToServer(Room, message);

        }
     
    }

    public class SourceSelectMessageContent
    {
        public string Destination { get; set; }
        public string SourceSelect { get; set; }
    }

    public delegate void PressAndHoldAction(bool b);

}