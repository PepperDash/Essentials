using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.GeneralIO;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Occupancy
{
    public class EssentialsGlsOccupancySensorBaseController : CrestronGenericBaseDevice, IOccupancyStatusProvider
    {
        public GlsOccupancySensorBase OccSensor { get; private set; }

        public BoolFeedback RoomIsOccupiedFeedback { get; private set; }

        public EssentialsGlsOccupancySensorBaseController(string key, string name, GlsOccupancySensorBase sensor, GlsOccupancySensorConfigurationProperties props)
            : base(key, name, sensor)
        {

        }
    }

    public class GlsOccupancySensorConfigurationProperties
    {
        public string CresnetId { get; set; }
        public string Model { get; set; }
    }
}