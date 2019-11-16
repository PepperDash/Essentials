using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
using PepperDash.Core;

namespace PepperDash.Essentials.Room.MobileControl
{
    public static class INumericExtensions
    {
        public static void LinkActions(this INumericKeypad dev, MobileControlSystemController controller)
        {
            var prefix = string.Format(@"/device/{0}/", (dev as IKeyed).Key);

            controller.AddAction(prefix + "num0", new PressAndHoldAction(dev.Digit0));
            controller.AddAction(prefix + "num1", new PressAndHoldAction(dev.Digit1));
            controller.AddAction(prefix + "num2", new PressAndHoldAction(dev.Digit2));
            controller.AddAction(prefix + "num3", new PressAndHoldAction(dev.Digit3));
            controller.AddAction(prefix + "num4", new PressAndHoldAction(dev.Digit4));
            controller.AddAction(prefix + "num5", new PressAndHoldAction(dev.Digit5));
            controller.AddAction(prefix + "num6", new PressAndHoldAction(dev.Digit6));
            controller.AddAction(prefix + "num7", new PressAndHoldAction(dev.Digit0));
            controller.AddAction(prefix + "num8", new PressAndHoldAction(dev.Digit0));
            controller.AddAction(prefix + "num9", new PressAndHoldAction(dev.Digit0));
            controller.AddAction(prefix + "numDash", new PressAndHoldAction(dev.KeypadAccessoryButton1));
            controller.AddAction(prefix + "numEnter", new PressAndHoldAction(dev.KeypadAccessoryButton2));
            // Deal with the Accessory functions on the numpad later
        }

        public static void UnlinkActions(this INumericKeypad dev, MobileControlSystemController controller)
        {
            var prefix = string.Format(@"/device/{0}/", (dev as IKeyed).Key);

            controller.RemoveAction(prefix + "num0");
            controller.RemoveAction(prefix + "num1");
            controller.RemoveAction(prefix + "num2");
            controller.RemoveAction(prefix + "num3");
            controller.RemoveAction(prefix + "num4");
            controller.RemoveAction(prefix + "num5");
            controller.RemoveAction(prefix + "num6");
            controller.RemoveAction(prefix + "num7");
            controller.RemoveAction(prefix + "num8");
            controller.RemoveAction(prefix + "num9");
            controller.RemoveAction(prefix + "numDash");
            controller.RemoveAction(prefix + "numEnter");
        }
    }
}