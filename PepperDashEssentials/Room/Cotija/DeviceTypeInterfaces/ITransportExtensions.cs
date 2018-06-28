using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
using PepperDash.Core;

namespace PepperDash.Essentials.Room.Cotija
{
    public static class ITransportExtensions
    {
        public static void LinkActions(this ITransport dev, CotijaSystemController controller)
        {
            var prefix = string.Format(@"/device/{0}/", (dev as IKeyed).Key);

            controller.AddAction(prefix + "play", new PressAndHoldAction(dev.Play));
            controller.AddAction(prefix + "pause", new PressAndHoldAction(dev.Pause));
            controller.AddAction(prefix + "stop", new PressAndHoldAction(dev.Stop));
            controller.AddAction(prefix + "prevtrack", new PressAndHoldAction(dev.ChapPlus));
            controller.AddAction(prefix + "nexttrack", new PressAndHoldAction(dev.ChapMinus));
            controller.AddAction(prefix + "rewind", new PressAndHoldAction(dev.Rewind));
            controller.AddAction(prefix + "ffwd", new PressAndHoldAction(dev.FFwd));
            controller.AddAction(prefix + "record", new PressAndHoldAction(dev.Record));
        }

        public static void UnlinkActions(this ITransport dev, CotijaSystemController controller)
        {
            var prefix = string.Format(@"/device/{0}/", (dev as IKeyed).Key);

            controller.RemoveAction(prefix + "play");
            controller.RemoveAction(prefix + "pause");
            controller.RemoveAction(prefix + "stop");
            controller.RemoveAction(prefix + "prevtrack");
            controller.RemoveAction(prefix + "nexttrack");
            controller.RemoveAction(prefix + "rewind");
            controller.RemoveAction(prefix + "ffwd");
            controller.RemoveAction(prefix + "record");
        }
    }
}