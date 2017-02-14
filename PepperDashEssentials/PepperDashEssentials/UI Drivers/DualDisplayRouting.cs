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
        //public BoolFeedback Display1AudioButtonEnable { get; private set; }
        //bool _Display1AudioButtonEnable;
        //public BoolFeedback Display1AudioButtonFeedback { get; private set; }
        //bool _Display1AudioButtonFeedback;
        //public BoolFeedback Display1ControlButtonEnable { get; private set; }
        //bool _Display1ControlButtonEnable;
        //public BoolFeedback Display2AudioButtonEnable { get; private set; }
        //bool _Display2AudioButtonEnable;
        //public BoolFeedback Display2AudioButtonFeedback { get; private set; }
        //bool _Display2AudioButtonFeedback;
        //public BoolFeedback Display2ControlButtonEnable { get; private set; }
        //bool _Display2ControlButtonEnable;
        //public BoolFeedback DualDisplayRoutingVisible { get; private set; }
        //bool _DualDisplayRoutingVisible;

        CTimer SourceSelectedTimer;

        public DualDisplayRouting(BasicTriListWithSmartObject trilist) : base(trilist)
        {
            //Display1AudioButtonEnable = new BoolFeedback(() => _Display1AudioButtonEnable);
            //Display1AudioButtonFeedback = new BoolFeedback(() => _Display1AudioButtonFeedback);
            TriList.SetSigFalseAction(UIBoolJoin.Display1AudioButtonPressAndFb, Display1AudioPress);

            //Display1ControlButtonEnable = new BoolFeedback(() => _Display1ControlButtonEnable);
            TriList.SetSigFalseAction(UIBoolJoin.Display1ControlButtonPress, Display1ControlPress);

            TriList.SetSigFalseAction(UIBoolJoin.Display1SelectPress, Display1Press);

            //Display2AudioButtonEnable = new BoolFeedback(() => _Display2AudioButtonEnable);
            //Display2AudioButtonFeedback = new BoolFeedback(() => _Display2AudioButtonFeedback);
            TriList.SetSigFalseAction(UIBoolJoin.Display2AudioButtonPressAndFb, Display2AudioPress);

            //Display2ControlButtonEnable = new BoolFeedback(() => _Display2ControlButtonEnable);
            TriList.SetSigFalseAction(UIBoolJoin.Display2ControlButtonPress, Display2ControlPress);

            TriList.SetSigFalseAction(UIBoolJoin.Display2SelectPress, Display2Press);

            //DualDisplayRoutingVisible = new BoolFeedback(() => _DualDisplayRoutingVisible);
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