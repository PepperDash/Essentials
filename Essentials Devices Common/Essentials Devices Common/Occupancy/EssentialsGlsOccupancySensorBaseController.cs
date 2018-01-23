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

        // Debug properties
        public bool InTestMode { get; private set; }

        public bool TestRoomIsOccupiedFeedback { get; private set; }

        public Func<bool> RoomIsOccupiedFeedbackFunc
        {
            get
            {
                return () => InTestMode ? TestRoomIsOccupiedFeedback : OccSensor.OccupancyDetectedFeedback.BoolValue;
            }
        }

        public EssentialsGlsOccupancySensorBaseController(string key, string name, GlsOccupancySensorBase sensor)
            : base(key, name, sensor)
        {
            OccSensor = sensor;
			RoomIsOccupiedFeedback = new BoolFeedback(RoomIsOccupiedFeedbackFunc);

            OccSensor.GlsOccupancySensorChange += new GlsOccupancySensorChangeEventHandler(sensor_GlsOccupancySensorChange);
        }

        void sensor_GlsOccupancySensorChange(GlsOccupancySensorBase device, GlsOccupancySensorChangeEventArgs args)
        {
            RoomIsOccupiedFeedback.FireUpdate();
        }

        public void SetTestMode(bool mode)
        {
            InTestMode = mode;

            Debug.Console(1, this, "In Mock Mode: '{0}'", InTestMode);
        }

        public void SetTestOccupiedState(bool state)
        {
            if (!InTestMode)
                Debug.Console(1, "Mock mode not enabled");
            else
            {
                TestRoomIsOccupiedFeedback = state;

                RoomIsOccupiedFeedback.FireUpdate();
            }
        }
    }
}