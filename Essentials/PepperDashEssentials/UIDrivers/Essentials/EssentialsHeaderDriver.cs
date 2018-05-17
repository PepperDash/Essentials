using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;
using Crestron.SimplSharpPro.DeviceSupport;


using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;
using PepperDash.Essentials.Core.PageManagers;
using PepperDash.Essentials.Room.Config;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec;


namespace PepperDash.Essentials
{
    /// <summary>
    /// 
    /// </summary>
    public class EssentialsHeaderDriver : PanelDriverBase
    {
        CrestronTouchpanelPropertiesConfig Config;
        
        /// <summary>
        /// The parent driver for this
        /// </summary>
        EssentialsPanelMainInterfaceDriver Parent;

        /// <summary>
        /// Indicates that the SetHeaderButtons method has completed successfully
        /// </summary>
        public bool HeaderButtonsAreSetUp { get; private set; }

        StringInputSig HeaderCallButtonIconSig;

        public EssentialsHeaderDriver(EssentialsPanelMainInterfaceDriver parent, CrestronTouchpanelPropertiesConfig config)
            : base(parent.TriList)
        {
            Config = config;
            Parent = parent;
        }

        void SetUpGear(IAVDriver avDriver, EssentialsRoomBase currentRoom)
        {
            // Gear
            TriList.SetString(UIStringJoin.HeaderButtonIcon5, "Gear");
            TriList.SetSigHeldAction(UIBoolJoin.HeaderIcon5Press, 2000,
                Parent.AvDriver.ShowTech,
                null,
                () =>
                {
                    if (currentRoom.OnFeedback.BoolValue)
                        avDriver.PopupInterlock.ShowInterlockedWithToggle(UIBoolJoin.VolumesPageVisible);
                    else
                        avDriver.PopupInterlock.ShowInterlockedWithToggle(UIBoolJoin.VolumesPagePowerOffVisible);
                });
            TriList.SetSigFalseAction(UIBoolJoin.TechExitButton, () =>
                avDriver.PopupInterlock.HideAndClear());
        }

        void SetUpHelpButton(EssentialsRoomPropertiesConfig roomConf)
        {
            // Help roomConf and popup
            if (roomConf.Help != null)
            {
                TriList.SetString(UIStringJoin.HelpMessage, roomConf.Help.Message);
                TriList.SetBool(UIBoolJoin.HelpPageShowCallButtonVisible, roomConf.Help.ShowCallButton);
                TriList.SetString(UIStringJoin.HelpPageCallButtonText, roomConf.Help.CallButtonText);
                if (roomConf.Help.ShowCallButton)
                    TriList.SetSigFalseAction(UIBoolJoin.HelpPageShowCallButtonPress, () => { }); // ************ FILL IN
                else
                    TriList.ClearBoolSigAction(UIBoolJoin.HelpPageShowCallButtonPress);
            }
            else // older config
            {
                TriList.SetString(UIStringJoin.HelpMessage, roomConf.HelpMessage);
                TriList.SetBool(UIBoolJoin.HelpPageShowCallButtonVisible, false);
                TriList.SetString(UIStringJoin.HelpPageCallButtonText, null);
                TriList.ClearBoolSigAction(UIBoolJoin.HelpPageShowCallButtonPress);
            }
            TriList.SetString(UIStringJoin.HeaderButtonIcon4, "Help");
            TriList.SetSigFalseAction(UIBoolJoin.HeaderIcon4Press, () =>
            {
                string message = null;
                var room = DeviceManager.GetDeviceForKey(Config.DefaultRoomKey)
                    as EssentialsHuddleSpaceRoom;
                if (room != null)
                    message = room.Config.HelpMessage;
                else
                    message = "Sorry, no help message available. No room connected.";
                //TriList.StringInput[UIStringJoin.HelpMessage].StringValue = message;
                Parent.AvDriver.PopupInterlock.ShowInterlockedWithToggle(UIBoolJoin.HelpPageVisible);
            });
        }

        uint SetUpCalendarButton(EssentialsHuddleVtc1PanelAvFunctionsDriver avDriver, uint nextJoin)
        {
            // Calendar button
            if (avDriver.CurrentRoom.ScheduleSource != null)
            {
                TriList.SetString(nextJoin, "Calendar");
                TriList.SetSigFalseAction(nextJoin, avDriver.CalendarPress);

                return nextJoin--;
            }
            else
                return nextJoin;
        }

        uint SetUpCallButton(EssentialsHuddleVtc1PanelAvFunctionsDriver avDriver, uint nextJoin)
        {
            // Call button
            TriList.SetString(nextJoin, "DND");
            TriList.SetSigFalseAction(nextJoin, avDriver.ShowActiveCallsList);
            HeaderCallButtonIconSig = TriList.StringInput[nextJoin];

            return nextJoin--;
        }

        /// <summary>
        /// Evaluates the call status and sets the icon mode and text label 
        /// </summary>
        public void ComputeHeaderCallStatus(VideoCodecBase codec)
        {
            if (codec == null)
            {
                Debug.Console(1, "ComputeHeaderCallStatus() cannot execute.  codec is null");
                return;
            }

            if (HeaderCallButtonIconSig == null)
            {
                Debug.Console(1, "ComputeHeaderCallStatus() cannot execute.  HeaderCallButtonIconSig is null");
                return;
            }

            // Set mode of header button
            if (!codec.IsInCall)
            {
                HeaderCallButtonIconSig.StringValue = "DND";
                //HeaderCallButton.SetIcon(HeaderListButton.OnHook);
            }
            else if (codec.ActiveCalls.Any(c => c.Type == eCodecCallType.Video))
                HeaderCallButtonIconSig.StringValue = "Misc-06_Dark";
            //HeaderCallButton.SetIcon(HeaderListButton.Camera);
            //TriList.SetUshort(UIUshortJoin.CallHeaderButtonMode, 2);
            else
                HeaderCallButtonIconSig.StringValue = "Misc-09_Dark";
            //HeaderCallButton.SetIcon(HeaderListButton.Phone);
            //TriList.SetUshort(UIUshortJoin.CallHeaderButtonMode, 1);

            // Set the call status text
            if (codec.ActiveCalls.Count > 0)
            {
                if (codec.ActiveCalls.Count == 1)
                    TriList.SetString(UIStringJoin.HeaderCallStatusLabel, "1 Active Call");
                else if (codec.ActiveCalls.Count > 1)
                    TriList.SetString(UIStringJoin.HeaderCallStatusLabel, string.Format("{0} Active Calls", codec.ActiveCalls.Count));
            }
            else
                TriList.SetString(UIStringJoin.HeaderCallStatusLabel, "No Active Calls");
        }

        /// <summary>
        /// Sets up Header Buttons for the EssentialsHuddleVtc1Room type
        /// </summary>
        public void SetupHeaderButtons(EssentialsHuddleVtc1Room currentRoom)
        {
            var avDriver = Parent.AvDriver as EssentialsHuddleVtc1PanelAvFunctionsDriver;

            HeaderButtonsAreSetUp = false;

            TriList.SetBool(UIBoolJoin.TopBarHabaneroDynamicVisible, true);

            var roomConf = currentRoom.Config;

            SetUpGear(avDriver, currentRoom);

            SetUpHelpButton(roomConf);
            
            uint nextJoin = 3953;

            nextJoin = SetUpCalendarButton(avDriver, nextJoin);

            nextJoin = SetUpCallButton(avDriver, nextJoin);
 
            // blank any that remain
            for (var i = nextJoin; i > 3950; i--)
            {
                TriList.SetString(i, "Blank");
                TriList.SetSigFalseAction(i, () => { });
            }

            TriList.SetSigFalseAction(UIBoolJoin.HeaderCallStatusLabelPress, avDriver.ShowActiveCallsList);

            // Set Call Status Subpage Position

            if (nextJoin == 3951)
            {
                // Set to right position
                TriList.SetBool(UIBoolJoin.HeaderCallStatusLeftPositionVisible, false);
                TriList.SetBool(UIBoolJoin.HeaderCallStatusRightPositionVisible, true);
            }
            else if (nextJoin == 3950)
            {
                // Set to left position
                TriList.SetBool(UIBoolJoin.HeaderCallStatusLeftPositionVisible, true);
                TriList.SetBool(UIBoolJoin.HeaderCallStatusRightPositionVisible, false);
            }

            HeaderButtonsAreSetUp = true;

            ComputeHeaderCallStatus(currentRoom.VideoCodec);
        }

        public void SetupHeaderButtons(EssentialsHuddleSpaceRoom currentRoom)
        {
            var avDriver = Parent.AvDriver as EssentialsHuddlePanelAvFunctionsDriver;

            HeaderButtonsAreSetUp = false;

            TriList.SetBool(UIBoolJoin.TopBarHabaneroDynamicVisible, true);

            var roomConf = currentRoom.Config;


            //SetUpGear(avDriver, currentRoom);

            SetUpHelpButton(roomConf);
            
            uint nextJoin = 3953;

            //// Calendar button
            //if (_CurrentRoom.ScheduleSource != null)
            //{
            //    TriList.SetString(nextJoin, "Calendar");
            //    TriList.SetSigFalseAction(nextJoin, CalendarPress);

            //    nextJoin--;
            //}

            //nextJoin--;

            // blank any that remain
            for (var i = nextJoin; i > 3950; i--)
            {
                TriList.SetString(i, "Blank");
                TriList.SetSigFalseAction(i, () => { });
            }

            HeaderButtonsAreSetUp = true;
        }

    }
}