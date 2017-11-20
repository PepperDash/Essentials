using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;
using PepperDash.Essentials.Core.Touchpanels.Keyboards;
using PepperDash.Essentials.Devices.Displays;
using PepperDash.Essentials.Room.Config;

namespace PepperDash.Essentials.UIDrivers
{
    public class EssentialsHuddleTechPageDriver : PanelDriverBase
    {
        /// <summary>
        /// 
        /// </summary>
        SmartObjectDynamicList MenuList;
        /// <summary>
        /// 
        /// </summary>
        SubpageReferenceList StatusList;
        /// <summary>
        /// The list of display controls
        /// </summary>
        SubpageReferenceList DisplayList;
        /// <summary>
        /// References lines in the list against device instances
        /// </summary>
        Dictionary<ICommunicationMonitor, uint> StatusListDeviceIndexes;
        /// <summary>
        /// 
        /// </summary>
        JoinedSigInterlock PagesInterlock;

        /// <summary>
        /// 1
        /// </summary>
        public const uint JoinText = 1;

        CTimer PinAuthorizedTimer;

		EssentialsRoomTechConfig Config;

        StringBuilder PinEntryBuilder = new StringBuilder(4);

        bool IsAuthorized;

        SmartObjectNumeric PinKeypad;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trilist"></param>
        /// <param name="parent"></param>
        public EssentialsHuddleTechPageDriver(BasicTriListWithSmartObject trilist, EssentialsRoomTechConfig config)
            : base(trilist)
        {
			Config = config;

            PagesInterlock = new JoinedSigInterlock(trilist);
            PagesInterlock.SetButDontShow(UIBoolJoin.TechSystemStatusVisible);

            trilist.SetSigFalseAction(UIBoolJoin.TechExitButton, Hide);

            MenuList = new SmartObjectDynamicList(trilist.SmartObjects[UISmartObjectJoin.TechMenuList], 
                true, 3100);

            MenuList.SetFeedback(1, true); // initial fb

            MenuList.SetItemMainText(1, "System Status");
            MenuList.SetItemButtonAction(1, b => { 
                if (b) PagesInterlock.ShowInterlocked(UIBoolJoin.TechSystemStatusVisible); 
                MenuList.SetFeedback(1, true);
            });

            MenuList.SetItemMainText(2, "Display Controls");
            MenuList.SetItemButtonAction(2, b => { 
                if (b) PagesInterlock.ShowInterlocked(UIBoolJoin.TechDisplayControlsVisible); 
                MenuList.SetFeedback(2, true);
            });           
            
            MenuList.SetItemMainText(3, "Panel Setup");
            MenuList.SetItemButtonAction(3, b => { 
                if (b) PagesInterlock.ShowInterlocked(UIBoolJoin.TechPanelSetupVisible);
                MenuList.SetFeedback(3, true);
            });
            MenuList.Count = 3;

            BuildStatusList();

            BuildDisplayList();

            SetupPinModal();
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Show()
        {
            // divert to PIN if we need auth
            if (IsAuthorized)
            {
                // Cancel the auth timer so we don't deauth after coming back in
                if (PinAuthorizedTimer != null)
                    PinAuthorizedTimer.Stop();

                TriList.SetBool(UIBoolJoin.TechCommonItemsVisbible, true);
                PagesInterlock.Show();
                base.Show();
            }
            else
            {
                TriList.SetBool(UIBoolJoin.PinDialog4DigitVisible, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Hide()
        {
            // Leave it authorized for 60 seconds.
            if (IsAuthorized)
                PinAuthorizedTimer = new CTimer(o => { 
                    IsAuthorized = false;
                    PinAuthorizedTimer = null;
                }, 60000);
            TriList.SetBool(UIBoolJoin.TechCommonItemsVisbible, false);
            PagesInterlock.Hide();
            base.Hide();
        }

        /// <summary>
        /// Wire up the keypad and buttons
        /// </summary>
        void SetupPinModal()
        {
            TriList.SetSigFalseAction(UIBoolJoin.PinDialogCancelPress, CancelPinDialog);
            PinKeypad = new SmartObjectNumeric(TriList.SmartObjects[UISmartObjectJoin.TechPinDialogKeypad], true);
            PinKeypad.Digit0.UserObject = new Action<bool>(b => { if (b)DialPinDigit('0'); });
            PinKeypad.Digit1.UserObject = new Action<bool>(b => { if (b)DialPinDigit('1'); });
            PinKeypad.Digit2.UserObject = new Action<bool>(b => { if (b)DialPinDigit('2'); });
            PinKeypad.Digit3.UserObject = new Action<bool>(b => { if (b)DialPinDigit('3'); });
            PinKeypad.Digit4.UserObject = new Action<bool>(b => { if (b)DialPinDigit('4'); });
            PinKeypad.Digit5.UserObject = new Action<bool>(b => { if (b)DialPinDigit('5'); });
            PinKeypad.Digit6.UserObject = new Action<bool>(b => { if (b)DialPinDigit('6'); });
            PinKeypad.Digit7.UserObject = new Action<bool>(b => { if (b)DialPinDigit('7'); });
            PinKeypad.Digit8.UserObject = new Action<bool>(b => { if (b)DialPinDigit('8'); });
            PinKeypad.Digit9.UserObject = new Action<bool>(b => { if (b)DialPinDigit('9'); });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        void DialPinDigit(char d)
        {
            PinEntryBuilder.Append(d);
            var len = PinEntryBuilder.Length;
            SetPinDotsFeedback(len);

            // check it!
            if (len == 4)
            {
                if (Config.Password == PinEntryBuilder.ToString())
                {
                    IsAuthorized = true;
                    SetPinDotsFeedback(0);
                    TriList.SetBool(UIBoolJoin.PinDialog4DigitVisible, false);
                    Show();
                }
                else
                {
                    SetPinDotsFeedback(0);
                    TriList.SetBool(UIBoolJoin.PinDialogErrorVisible, true);
                    new CTimer(o => 
                        {
                            TriList.SetBool(UIBoolJoin.PinDialogErrorVisible, false);
                        }, 1500);
                }

                PinEntryBuilder.Remove(0, len); // clear it either way
            }
        }

        /// <summary>
        /// Draws the dots as pin is entered
        /// </summary>
        /// <param name="len"></param>
        void SetPinDotsFeedback(int len)
        {
            TriList.SetBool(UIBoolJoin.PinDialogDot1, len >= 1);
            TriList.SetBool(UIBoolJoin.PinDialogDot2, len >= 2);
            TriList.SetBool(UIBoolJoin.PinDialogDot3, len >= 3);
            TriList.SetBool(UIBoolJoin.PinDialogDot4, len == 4);

        }

        /// <summary>
        /// Does what it says
        /// </summary>
        void CancelPinDialog()
        {
            PinEntryBuilder.Remove(0, PinEntryBuilder.Length);
            TriList.SetBool(UIBoolJoin.PinDialog4DigitVisible, false);
        }


        /// <summary>
        /// 
        /// </summary>
        void BuildStatusList()
        {
            StatusList = new SubpageReferenceList(TriList, UISmartObjectJoin.TechStatusList, 3, 3, 3);
            StatusListDeviceIndexes = new Dictionary<ICommunicationMonitor, uint>();
            uint i = 0;
            foreach (var d in DeviceManager.AllDevices)
            {
                // make sure it is both ICommunicationMonitor and a Device
                var sd = d as ICommunicationMonitor;
                if (sd == null)
                    continue;
                var dd = sd as Device;
                if(dd == null)
                    continue;
                i++;
                StatusList.StringInputSig(i, 1).StringValue = dd.Name;
                StatusList.UShortInputSig(i, 1).UShortValue = (ushort)sd.CommunicationMonitor.Status;
                StatusListDeviceIndexes.Add(sd, i);
                sd.CommunicationMonitor.StatusChange += CommunicationMonitor_StatusChange ;
            }
            StatusList.Count = (ushort)i;
        }

        /// <summary>
        /// Builds the list of display controls
        /// </summary>
        void BuildDisplayList()
        {
            DisplayList = new SubpageReferenceList(TriList, UISmartObjectJoin.TechDisplayControlsList, 10, 3, 3);

            var devKeys = ConfigReader.ConfigObject.Devices.Where(d =>
                d.Group.Equals("display", StringComparison.OrdinalIgnoreCase)
                || d.Group.Equals("projector", StringComparison.OrdinalIgnoreCase))
                .Select(dd => dd.Key);
            var disps = DeviceManager.AllDevices.Where(d =>
                devKeys.Contains(d.Key));
            ushort i = 0;
            foreach (var disp in disps)
            {
                var display = disp as DisplayBase;
                if (display != null)
                {
                    i++;
                    DisplayList.StringInputSig(i, 1).StringValue = display.Name;
                    DisplayList.GetBoolFeedbackSig(i, 1).SetSigFalseAction(display.PowerOn);      
                    DisplayList.GetBoolFeedbackSig(i, 2).SetSigFalseAction(display.PowerOff);
                    if (display is TwoWayDisplayBase)
                    {
                        var powerOnSig = DisplayList.BoolInputSig(i, 1);
                        (display as TwoWayDisplayBase).PowerIsOnFeedback.LinkInputSig(powerOnSig);

                        var powerOffSig = DisplayList.BoolInputSig(1, 2);
                        (display as TwoWayDisplayBase).PowerIsOnFeedback.LinkComplementInputSig(powerOffSig);
                    } 
                    DisplayList.GetBoolFeedbackSig(i, 3).SetSigFalseAction(() =>
                        { if (display is IInputHdmi1) (display as IInputHdmi1).InputHdmi1(); });
                    DisplayList.GetBoolFeedbackSig(i, 4).SetSigFalseAction(() =>
                        { if (display is IInputHdmi2) (display as IInputHdmi2).InputHdmi2(); });
                    DisplayList.GetBoolFeedbackSig(i, 5).SetSigFalseAction(() =>
                        { if (display is IInputHdmi3) (display as IInputHdmi3).InputHdmi3(); });
                    //DisplayList.GetBoolFeedbackSig(i, 6).SetSigFalseAction(() =>
                    //{ if (display is IInputHdmi4) (display as IInputHdmi4).InputHdmi4(); });
                    DisplayList.GetBoolFeedbackSig(i, 6).SetSigFalseAction(() =>
                        { if (display is IInputDisplayPort1) (display as IInputDisplayPort1).InputDisplayPort1(); });

                
                    // Figure out some way to provide current input feedback
                    if (display is TwoWayDisplayBase)
                    {
                        (display as TwoWayDisplayBase).CurrentInputFeedback.OutputChange += new EventHandler<EventArgs>(CurrentInputFeedback_OutputChange);
                    }
                }

                
            }

            DisplayList.Count = i;
        }


        void CurrentInputFeedback_OutputChange(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        void CommunicationMonitor_StatusChange(object sender, MonitorStatusChangeEventArgs e)
        {
            var c = sender as ICommunicationMonitor;
            if (StatusListDeviceIndexes.ContainsKey(c))
            {
                var i = StatusListDeviceIndexes[c];
                StatusList.UShortInputSig(i, 1).UShortValue = (ushort)e.Status;
            }
        }
    }
}