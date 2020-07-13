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
using PepperDash.Essentials.Core.Devices.VideoCodec;
using PepperDash.Essentials.Core.SmartObjects;
using PepperDash.Essentials.Core.PageManagers;
using PepperDash.Essentials.Core.Rooms.Config;
using PepperDash.Essentials.Core.Devices.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec;
using PepperDashEssentials.UIDrivers.EssentialsDualDisplay;


namespace PepperDash.Essentials
{
    /// <summary>
    /// 
    /// </summary>
    public class EssentialsHeaderDriver : PanelDriverBase
    {
        uint EnvironmentCaretVisible;
        uint CalendarCaretVisible;
        uint CallCaretVisible;

        JoinedSigInterlock CaretInterlock;

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
            CaretInterlock = new JoinedSigInterlock(TriList);
        }

        void SetUpGear(IAVDriver avDriver, EssentialsRoomBase currentRoom)
        {
            // Gear
            TriList.SetString(UIStringJoin.HeaderButtonIcon5, "Gear");
            TriList.SetSigHeldAction(UIBoolJoin.HeaderIcon5Press, 2000,
                avDriver.ShowTech,
                null,
                () =>
                {
                    if (currentRoom.OnFeedback.BoolValue)
                    {
                        avDriver.PopupInterlock.ShowInterlockedWithToggle(UIBoolJoin.VolumesPageVisible);
                        CaretInterlock.ShowInterlocked(UIBoolJoin.HeaderCaret5Visible);
                    }
                    else
                    {
                        avDriver.PopupInterlock.ShowInterlockedWithToggle(UIBoolJoin.VolumesPagePowerOffVisible);
                        CaretInterlock.ShowInterlocked(UIBoolJoin.HeaderCaret5Visible);
                    }
                });
            TriList.SetSigFalseAction(UIBoolJoin.TechExitButton, () =>
                avDriver.PopupInterlock.HideAndClear());
        }

        public void SetUpHelpButton(EssentialsRoomPropertiesConfig roomConf)
        {
            // Help roomConf and popup
            if (roomConf.Help != null)
            {
                TriList.SetString(UIStringJoin.HelpMessage, roomConf.Help.Message);
                TriList.SetBool(UIBoolJoin.HelpPageShowCallButtonVisible, roomConf.Help.ShowCallButton);
                TriList.SetString(UIStringJoin.HelpPageCallButtonText, roomConf.Help.CallButtonText);
                if (roomConf.Help.ShowCallButton)
                {
                    TriList.SetSigFalseAction(UIBoolJoin.HelpPageShowCallButtonPress, () => { }); // ************ FILL IN
                }
                else
                {
                    TriList.ClearBoolSigAction(UIBoolJoin.HelpPageShowCallButtonPress);
                }
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
                    message = room.PropertiesConfig.HelpMessage;
                else
                    message = "Sorry, no help message available. No room connected.";
                //TriList.StringInput[UIStringJoin.HelpMessage].StringValue = message;
                Parent.AvDriver.PopupInterlock.ShowInterlockedWithToggle(UIBoolJoin.HelpPageVisible);
                CaretInterlock.ShowInterlocked(UIBoolJoin.HeaderCaret4Visible);
            });
        }

        uint SetUpEnvironmentButton(EssentialsEnvironmentDriver environmentDriver, uint nextJoin)
        {
            if (environmentDriver != null)
            {
                var tempJoin = nextJoin;
                TriList.SetString(tempJoin, "Lights");
                EnvironmentCaretVisible = tempJoin + 10;
                TriList.SetSigFalseAction(tempJoin, () =>
                    {
                        environmentDriver.Toggle();
                        CaretInterlock.ShowInterlocked(EnvironmentCaretVisible);
                    });
                nextJoin--;
                return nextJoin;
            }
            else
                return nextJoin;
        }

        uint SetUpCalendarButton(IHasCalendarButton avDriver, uint nextJoin)
        {
            // Calendar button
            var room = avDriver.CurrentRoom as EssentialsHuddleVtc1Room;
            if (room != null && room.ScheduleSource == null)
            {
                return nextJoin;
            }

            var tempJoin = nextJoin;
            TriList.SetString(tempJoin, "Calendar");
            CalendarCaretVisible = tempJoin + 10;
            TriList.SetSigFalseAction(tempJoin, () =>
            {
                avDriver.CalendarPress();
                CaretInterlock.ShowInterlocked(CalendarCaretVisible);
            });

            nextJoin--;
            return nextJoin;
        }

        uint SetUpCallButton(IHasCallButton avDriver, uint nextJoin)
        {
            // Call button
            var room = avDriver.CurrentRoom as EssentialsHuddleVtc1Room;
            var tempJoin = nextJoin;
            TriList.SetString(tempJoin, "DND");
            CallCaretVisible = tempJoin + 10;
            TriList.SetSigFalseAction(tempJoin, () =>
                {
                    avDriver.ShowActiveCallsList();
                    if(room != null && room.InCallFeedback.BoolValue)
                        CaretInterlock.ShowInterlocked(CallCaretVisible);
                });
            HeaderCallButtonIconSig = TriList.StringInput[tempJoin];

            nextJoin--;
            return nextJoin;
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
        public void SetupHeaderButtons(EssentialsHuddleVtc1PanelAvFunctionsDriver avDriver, EssentialsHuddleVtc1Room currentRoom)
        {
            HeaderButtonsAreSetUp = false;
            var room = avDriver.CurrentRoom as EssentialsHuddleVtc1Room;

            TriList.SetBool(UIBoolJoin.TopBarHabaneroDynamicVisible, true);

            var roomConf = currentRoom.PropertiesConfig;

            // Register for the PopupInterlock IsShowsFeedback event to tie the header carets subpage visiblity to it
            Parent.AvDriver.PopupInterlock.StatusChanged -= PopupInterlock_StatusChanged;
            Parent.AvDriver.PopupInterlock.StatusChanged += PopupInterlock_StatusChanged; 

            SetUpGear(avDriver, currentRoom);

            SetUpHelpButton(roomConf);
            
            uint nextJoin = 3953;

            nextJoin = SetUpEnvironmentButton(Parent.EnvironmentDriver, nextJoin);

            nextJoin = SetUpCalendarButton(avDriver, nextJoin);

            nextJoin = SetUpCallButton(avDriver, nextJoin);
 
            // blank any that remain
            for (var i = nextJoin; i > 3950; i--)
            {
                TriList.SetString(i, "Blank");
                TriList.SetSigFalseAction(i, () => { });
            }

            TriList.SetSigFalseAction(UIBoolJoin.HeaderCallStatusLabelPress,
                () =>
                {
                    avDriver.ShowActiveCallsList();
                    if (room != null && room.InCallFeedback.BoolValue)
                        CaretInterlock.ShowInterlocked(CallCaretVisible);
                });

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

        public void SetupHeaderButtons(EssentialsDualDisplayPanelAvFunctionsDriver avDriver, EssentialsHuddleVtc1Room currentRoom)
        {
            HeaderButtonsAreSetUp = false;
            var room = avDriver.CurrentRoom as EssentialsHuddleVtc1Room;

            TriList.SetBool(UIBoolJoin.TopBarHabaneroDynamicVisible, true);

            var roomConf = currentRoom.PropertiesConfig;

            // Register for the PopupInterlock IsShowsFeedback event to tie the header carets subpage visiblity to it
            Parent.AvDriver.PopupInterlock.StatusChanged -= PopupInterlock_StatusChanged;
            Parent.AvDriver.PopupInterlock.StatusChanged += PopupInterlock_StatusChanged;

            SetUpGear(avDriver, currentRoom);

            SetUpHelpButton(roomConf);

            uint nextJoin = 3953;

            nextJoin = SetUpEnvironmentButton(Parent.EnvironmentDriver, nextJoin);

            nextJoin = SetUpCalendarButton(avDriver, nextJoin);

            nextJoin = SetUpCallButton(avDriver, nextJoin);

            // blank any that remain
            for (var i = nextJoin; i > 3950; i--)
            {
                TriList.SetString(i, "Blank");
                TriList.SetSigFalseAction(i, () => { });
            }

            TriList.SetSigFalseAction(UIBoolJoin.HeaderCallStatusLabelPress,
                () =>
                {
                    avDriver.ShowActiveCallsList();
                    if (room != null && room.InCallFeedback.BoolValue)
                        CaretInterlock.ShowInterlocked(CallCaretVisible);
                });

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

        /// <summary>
        /// Sets up Header Buttons for the EssentialsHuddleSpaceRoom type
        /// </summary>
        public void SetupHeaderButtons(EssentialsHuddlePanelAvFunctionsDriver avDriver, EssentialsHuddleSpaceRoom currentRoom)
        {
            HeaderButtonsAreSetUp = false;

            TriList.SetBool(UIBoolJoin.TopBarHabaneroDynamicVisible, true);

            var roomConf = currentRoom.PropertiesConfig;

            // Register for the PopupInterlock IsShowsFeedback event to tie the header carets subpage visiblity to it
            Parent.AvDriver.PopupInterlock.StatusChanged -= PopupInterlock_StatusChanged;
            Parent.AvDriver.PopupInterlock.StatusChanged += PopupInterlock_StatusChanged; 

            SetUpGear(avDriver, currentRoom);

            SetUpHelpButton(roomConf);
            
            uint nextJoin = 3953;

            nextJoin = SetUpEnvironmentButton(Parent.EnvironmentDriver, nextJoin);

            // blank any that remain
            for (var i = nextJoin; i > 3950; i--)
            {
                TriList.SetString(i, "Blank");
                TriList.SetSigFalseAction(i, () => { });
            }

            HeaderButtonsAreSetUp = true;
        }

        ///// <summary>
        ///// Whenever a popup is shown/hidden, show/hide the header carets subpage and set the visibility of the correct caret
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //void IsShownFeedback_OutputChange(object sender, EventArgs e)
        //{
        //    var popupInterlockIsShown = Parent.AvDriver.PopupInterlock.IsShown;
        //    // Set the visible state for the HeaderPopupCaretsSubpage to match that of the PopupInterlock state
        //    TriList.SetBool(UIBoolJoin.HeaderPopupCaretsSubpageVisibile, popupInterlockIsShown);

        //    // Clear all caret visibility
        //    for (uint i = UIBoolJoin.HeaderCaret5Visible; i >= UIBoolJoin.HeaderCaret1Visible; i--)
        //    {
        //        TriList.SetBool(i, false);
        //    }

        //    // Set the current caret visible if the popup is still shown
        //    if (popupInterlockIsShown)
        //        TriList.SetBool(NextCaretVisible, true);
        //}

        /// <summary>
        /// Whenever a popup is shown/hidden, show/hide the header carets subpage and set the visibility of the correct caret
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PopupInterlock_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            // Set the visible state for the HeaderPopupCaretsSubpage to match that of the PopupInterlock state

            bool headerPopupShown = false;

            // Check if the popup interlock is shown, and if one of the header popups is current, then show the carets subpage
            if (e.IsShown)
            {
                if (e.NewJoin == Parent.EnvironmentDriver.BackgroundSubpageJoin)
                    headerPopupShown = true;
                else if (e.NewJoin == UIBoolJoin.HeaderActiveCallsListVisible)
                    headerPopupShown = true;
                else if (e.NewJoin == UIBoolJoin.HelpPageVisible)
                    headerPopupShown = true;
                else if (e.NewJoin == UIBoolJoin.MeetingsOrContacMethodsListVisible)
                    headerPopupShown = true;
                else if (e.NewJoin == UIBoolJoin.VolumesPagePowerOffVisible || e.NewJoin == UIBoolJoin.VolumesPageVisible)
                    headerPopupShown = true;
            }

            // Set the carets subpage visibility
            TriList.SetBool(UIBoolJoin.HeaderPopupCaretsSubpageVisibile, headerPopupShown);

            if (!e.IsShown)
                CaretInterlock.HideAndClear();
        }
    }
}