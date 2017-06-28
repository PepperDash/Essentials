using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
using PepperDash.Core;

namespace PepperDash.Essentials
{
    public static class IDPadExtensions
    {
        public static void LinkActions(this IDPad dev, CotijaSystemController controller)
        {
            var prefix = string.Format(@"/device/{0}/", (dev as IKeyed).Key);

            controller.AddAction(prefix + "up", new Action<bool>(dev.Up));
            controller.AddAction(prefix + "down", new Action<bool>(dev.Down));
            controller.AddAction(prefix + "left", new Action<bool>(dev.Left));
            controller.AddAction(prefix + "right", new Action<bool>(dev.Right));
            controller.AddAction(prefix + "select", new Action<bool>(dev.Select));
            controller.AddAction(prefix + "menu", new Action<bool>(dev.Menu));
            controller.AddAction(prefix + "exit", new Action<bool>(dev.Exit));
        }

        public static void UnlinkActions(this IDPad dev, CotijaSystemController controller)
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