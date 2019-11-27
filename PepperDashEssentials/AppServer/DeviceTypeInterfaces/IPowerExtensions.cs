using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
using PepperDash.Core;

namespace PepperDash.Essentials.Room.MobileControl
{
    public static class IPowerExtensions
    {
        public static void LinkActions(this IPower dev, MobileControlSystemController controller)
        {
            var prefix = string.Format(@"/device/{0}/", (dev as IKeyed).Key);

            controller.AddAction(prefix + "powerOn", new Action(dev.PowerOn));
            controller.AddAction(prefix + "powerOff", new Action(dev.PowerOff));
            controller.AddAction(prefix + "powerToggle", new Action(dev.PowerToggle));
        }

        public static void UnlinkActions(this IPower dev, MobileControlSystemController controller)
        {
            var prefix = string.Format(@"/device/{0}/", (dev as IKeyed).Key);

            controller.RemoveAction(prefix + "powerOn");
            controller.RemoveAction(prefix + "powerOff");
            controller.RemoveAction(prefix + "powerToggle");
  
        }
    }
}