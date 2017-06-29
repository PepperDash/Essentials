using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
using PepperDash.Core;

namespace PepperDash.Essentials.Room.Cotija
{
    public static class IPowerExtensions
    {
        public static void LinkActions(this IPower dev, CotijaSystemController controller)
        {
            var prefix = string.Format(@"/device/{0}/", (dev as IKeyed).Key);

            controller.AddAction(prefix + "poweron", new Action(dev.PowerOn));
            controller.AddAction(prefix + "poweroff", new Action(dev.PowerOff));
            controller.AddAction(prefix + "powertoggle", new Action(dev.PowerToggle));
        }

        public static void UnlinkActions(this IPower dev, CotijaSystemController controller)
        {
            var prefix = string.Format(@"/device/{0}/", (dev as IKeyed).Key);

            controller.RemoveAction(prefix + "poweron");
            controller.RemoveAction(prefix + "poweroff");
            controller.RemoveAction(prefix + "powertoggle");
  
        }
    }
}