using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
using PepperDash.Core;

namespace PepperDash.Essentials.Room.MobileControl
{
    public static class IDPadExtensions
    {
        public static void LinkActions(this IDPad dev, MobileControlSystemController controller)
        {
            var prefix = string.Format(@"/device/{0}/", (dev as IKeyed).Key);

            controller.AddAction(prefix + "up", new PressAndHoldAction(dev.Up));
            controller.AddAction(prefix + "down", new PressAndHoldAction(dev.Down));
            controller.AddAction(prefix + "left", new PressAndHoldAction(dev.Left));
            controller.AddAction(prefix + "right", new PressAndHoldAction(dev.Right));
            controller.AddAction(prefix + "select", new PressAndHoldAction(dev.Select));
            controller.AddAction(prefix + "menu", new PressAndHoldAction(dev.Menu));
            controller.AddAction(prefix + "exit", new PressAndHoldAction(dev.Exit));
        }

        public static void UnlinkActions(this IDPad dev, MobileControlSystemController controller)
        {
            var prefix = string.Format(@"/device/{0}/", (dev as IKeyed).Key);

            controller.RemoveAction(prefix + "up");
            controller.RemoveAction(prefix + "down");
            controller.RemoveAction(prefix + "left");
            controller.RemoveAction(prefix + "right");
            controller.RemoveAction(prefix + "select");
            controller.RemoveAction(prefix + "menu");
            controller.RemoveAction(prefix + "exit"); 
        }
    }
}