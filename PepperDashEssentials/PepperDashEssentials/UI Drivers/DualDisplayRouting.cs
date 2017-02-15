using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;
using PepperDash.Essentials.Core.PageManagers;

namespace PepperDash.Essentials
{
    public class DualDisplayRouting : PanelDriverBase
    {
        CTimer SourceSelectedTimer;

        public DualDisplayRouting(BasicTriListWithSmartObject trilist) : base(trilist)
        {
            TriList.SetSigFalseAction(UIBoolJoin.Display1AudioButtonPressAndFb, Display1AudioPress);
            TriList.SetSigFalseAction(UIBoolJoin.Display1ControlButtonPress, Display1ControlPress);
            TriList.SetSigTrueAction(UIBoolJoin.Display1SelectPress, Display1Press);

            TriList.SetSigFalseAction(UIBoolJoin.Display2AudioButtonPressAndFb, Display2AudioPress);
            TriList.SetSigFalseAction(UIBoolJoin.Display2ControlButtonPress, Display2ControlPress);
            TriList.SetSigTrueAction(UIBoolJoin.Display2SelectPress, Display2Press);
        }

        public void Enable()
        {
            // attach to the source list SRL
        }

        public override void Show()
        {
            TriList.BooleanInput[UIBoolJoin.DualDisplayPageVisible].BoolValue = true;
            base.Show();
        }

        public override void Hide()
        {
            TriList.BooleanInput[UIBoolJoin.DualDisplayPageVisible].BoolValue = false;
            base.Hide();
        }


        public void SourceListButtonPress(SourceListItem item)
        {
            // start the timer
            // show FB on potential source
            TriList.BooleanInput[UIBoolJoin.Display1AudioButtonEnable].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.Display1ControlButtonEnable].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.Display2AudioButtonEnable].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.Display2ControlButtonEnable].BoolValue = false;
        }

        void EnableAppropriateDisplayButtons()
        {
            TriList.BooleanInput[UIBoolJoin.Display1AudioButtonEnable].BoolValue = true;
            TriList.BooleanInput[UIBoolJoin.Display1ControlButtonEnable].BoolValue = true;
            TriList.BooleanInput[UIBoolJoin.Display2AudioButtonEnable].BoolValue = true;
            TriList.BooleanInput[UIBoolJoin.Display2ControlButtonEnable].BoolValue = true;
        }

        public void Display1Press()
        {
            EnableAppropriateDisplayButtons();
        }

        public void Display1AudioPress()
        {

        }

        public void Display1ControlPress()
        {

        }

        public void Display2Press()
        {
            EnableAppropriateDisplayButtons();
        }

        public void Display2AudioPress()
        {

        }

        public void Display2ControlPress()
        {

        }
    }
}