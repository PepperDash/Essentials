using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    public static class ButtonExtensions
    {
        public static Button SetButtonAction(this Button button, Action<bool> a)
        {
            button.UserObject = a;
            return button;
        }

        public static Button SetButtonAction(this CrestronCollection<Button> bc, uint sigNum, Action<bool> a)
        {
            return bc[sigNum].SetButtonAction(a);
        }

        public static bool GetBool(this CrestronCollection<Button> bc, uint sigNum)
        {
            return bc[sigNum].State == eButtonState.Pressed ? true : false;
        }

        public static Button SetButtonPressedAction(this CrestronCollection<Button> bc, uint sigNum, Action a)
        {
            return bc[sigNum].SetButtonAction(b => { if (b) a(); });
        }

        public static Button SetButtonReleasedAction(this CrestronCollection<Button> bc, uint sigNum, Action a)
        {
            return bc[sigNum].SetButtonAction(b => { if (!b) a(); });
        }

        public static Button SetButtonFalseAction(this Button button, Action a)
        {
            return button.SetButtonAction(b => { if (!b) a(); });
        }
    }
}