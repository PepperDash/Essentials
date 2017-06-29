using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
using PepperDash.Core;

namespace PepperDash.Essentials.Room.Cotija
{
    public static class IChannelExtensions
    {
        public static void LinkActions(this IChannel dev, CotijaSystemController controller)
        {
            var prefix = string.Format(@"/device/{0}/", (dev as IKeyed).Key);

            controller.AddAction(prefix + "chanup", new PressAndHoldAction(dev.ChannelUp));
            controller.AddAction(prefix + "chandown", new PressAndHoldAction(dev.ChannelDown));
            controller.AddAction(prefix + "lastchan", new PressAndHoldAction(dev.LastChannel));
            controller.AddAction(prefix + "guide", new PressAndHoldAction(dev.Guide));
            controller.AddAction(prefix + "info", new PressAndHoldAction(dev.Info));
            controller.AddAction(prefix + "exit", new PressAndHoldAction(dev.Exit));
        }

        public static void UnlinkActions(this IChannel dev, CotijaSystemController controller)
        {
            var prefix = string.Format(@"/device/{0}/", (dev as IKeyed).Key);

            controller.RemoveAction(prefix + "chanup");
            controller.RemoveAction(prefix + "chandown");
            controller.RemoveAction(prefix + "lastchan");
            controller.RemoveAction(prefix + "guide");
            controller.RemoveAction(prefix + "info");
            controller.RemoveAction(prefix + "exit");
        }
    }
}