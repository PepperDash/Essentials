using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.GeneralIO;

namespace PepperDash.Essentials.Devices.Common.Occupancy
{
    public class EssentialsGlsOirCsmExBatt : GlsOccupancySensorBase, IOccupancyStatusProvider
    {
        public GlsOirCsmExBatt OccSensor { get; set; }

        
    }
}