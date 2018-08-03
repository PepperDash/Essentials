using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
using PepperDash.Core;

namespace PepperDash.Essentials.Room.Cotija
{
    public static class ISetTopBoxControlsExtensions
    {
        public static void LinkActions(this ISetTopBoxControls dev, CotijaSystemController controller)
        {
            var prefix = string.Format(@"/device/{0}/", (dev as IKeyed).Key);

            controller.AddAction(prefix + "dvrList", new PressAndHoldAction(dev.DvrList));
            controller.AddAction(prefix + "replay", new PressAndHoldAction(dev.Replay));
        }

        public static void UnlinkActions(this ISetTopBoxControls dev, CotijaSystemController controller)
        {
            var prefix = string.Format(@"/device/{0}/", (dev as IKeyed).Key);

            controller.RemoveAction(prefix + "dvrList");
            controller.RemoveAction(prefix + "replay");
        }
    }
}