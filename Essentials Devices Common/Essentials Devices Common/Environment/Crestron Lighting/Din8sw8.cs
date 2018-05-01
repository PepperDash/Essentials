using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Lighting;

using PepperDash.Core;
using PepperDash.Essentials.Core.CrestronIO;

namespace PepperDash.Essentials.Devices.Common.Environment.Lighting
{
    public class Din8sw8Controller : Device
    {
        // Need to figure out some sort of interface to make these switched outputs behave like processor relays so they can be used interchangably

        public Din8Sw8 SwitchModule { get; private set; }

        public Din8sw8Controller(string key, string cresnetId)
            : base(key)
        {
            
        }

    }
}