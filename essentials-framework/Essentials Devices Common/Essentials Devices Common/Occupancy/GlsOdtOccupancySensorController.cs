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
    public class GlsOdtOccupancySensorController : GlsOccupancySensorBaseController
    {
        public new GlsOdtCCn OccSensor { get; private set; }

        public BoolFeedback OrWhenVacatedFeedback { get; private set; }

        public BoolFeedback AndWhenVacatedFeedback { get; private set; }

        public BoolFeedback UltrasonicAEnabledFeedback { get; private set; }

        public BoolFeedback UltrasonicBEnabledFeedback { get; private set; }

        public IntFeedback UltrasonicSensitivityInVacantStateFeedback { get; private set; }

        public IntFeedback UltrasonicSensitivityInOccupiedStateFeedback { get; private set; }


        public GlsOdtOccupancySensorController(string key, string name, GlsOdtCCn sensor)
            : base(key, name, sensor)
        {
            AndWhenVacatedFeedback = new BoolFeedback(() => OccSensor.AndWhenVacatedFeedback.BoolValue);

            OrWhenVacatedFeedback = new BoolFeedback(() => OccSensor.OrWhenVacatedFeedback.BoolValue);

            UltrasonicAEnabledFeedback = new BoolFeedback(() => OccSensor.UsAEnabledFeedback.BoolValue);

            UltrasonicBEnabledFeedback = new BoolFeedback(() => OccSensor.UsBEnabledFeedback.BoolValue);

            UltrasonicSensitivityInVacantStateFeedback = new IntFeedback(() => OccSensor.UsSensitivityInVacantStateFeedback.UShortValue);

            UltrasonicSensitivityInOccupiedStateFeedback = new IntFeedback(() => OccSensor.UsSensitivityInOccupiedStateFeedback.UShortValue);
        }

        /// <summary>
        /// Overrides the base class event delegate to fire feedbacks for event IDs that pertain to this extended class.
        /// Then calls the base delegate method to ensure any common event IDs are captured.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="args"></param>
        protected override void OccSensor_GlsOccupancySensorChange(GlsOccupancySensorBase device, GlsOccupancySensorChangeEventArgs args)
        {
            if (args.EventId == GlsOccupancySensorBase.AndWhenVacatedFeedbackEventId)
                AndWhenVacatedFeedback.FireUpdate();
            else if (args.EventId == GlsOccupancySensorBase.OrWhenVacatedFeedbackEventId)
                OrWhenVacatedFeedback.FireUpdate();
            else if (args.EventId == GlsOccupancySensorBase.UsAEnabledFeedbackEventId)
                UltrasonicAEnabledFeedback.FireUpdate();
            else if (args.EventId == GlsOccupancySensorBase.UsBEnabledFeedbackEventId)
                UltrasonicBEnabledFeedback.FireUpdate();
            else if (args.EventId == GlsOccupancySensorBase.UsSensitivityInOccupiedStateFeedbackEventId)
                UltrasonicSensitivityInOccupiedStateFeedback.FireUpdate();
            else if (args.EventId == GlsOccupancySensorBase.UsSensitivityInVacantStateFeedbackEventId)
                UltrasonicSensitivityInVacantStateFeedback.FireUpdate();


            base.OccSensor_GlsOccupancySensorChange(device, args);
        }

        /// <summary>
        /// Sets the OrWhenVacated state
        /// </summary>
        /// <param name="state"></param>
        public void SetOrWhenVacatedState(bool state)
        {
            OccSensor.OrWhenVacated.BoolValue = state;
        }

        /// <summary>
        /// Sets the AndWhenVacated state
        /// </summary>
        /// <param name="state"></param>
        public void SetAndWhenVacatedState(bool state)
        {
            OccSensor.AndWhenVacated.BoolValue = state;
        }

        /// <summary>
        /// Enables or disables the Ultrasonic A sensor
        /// </summary>
        /// <param name="state"></param>
        public void SetUsAEnable(bool state)
        {
            if (state)
            {
                OccSensor.EnableUsA.BoolValue = state;
                OccSensor.DisableUsA.BoolValue = !state;
            }
            else
            {
                OccSensor.EnableUsA.BoolValue = state;
                OccSensor.DisableUsA.BoolValue = !state;
            }
        }


        /// <summary>
        /// Enables or disables the Ultrasonic B sensor
        /// </summary>
        /// <param name="state"></param>
        public void SetUsBEnable(bool state)
        {
            if (state)
            {
                OccSensor.EnableUsB.BoolValue = state;
                OccSensor.DisableUsB.BoolValue = !state;
            }
            else
            {
                OccSensor.EnableUsB.BoolValue = state;
                OccSensor.DisableUsB.BoolValue = !state;
            }
        }

        public void IncrementUsSensitivityInOccupiedState(bool pressRelease)
        {
            OccSensor.IncrementUsSensitivityInOccupiedState.BoolValue = pressRelease;
        }

        public void DecrementUsSensitivityInOccupiedState(bool pressRelease)
        {
            OccSensor.DecrementUsSensitivityInOccupiedState.BoolValue = pressRelease;
        }

        public void IncrementUsSensitivityInVacantState(bool pressRelease)
        {
            OccSensor.IncrementUsSensitivityInVacantState.BoolValue = pressRelease;
        }

        public void DecrementUsSensitivityInVacantState(bool pressRelease)
        {
            OccSensor.DecrementUsSensitivityInVacantState.BoolValue = pressRelease;
        }
    }
}