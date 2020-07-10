using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.GeneralIO;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Core
{
    [Description("Wrapper class for Dual Technology GLS Occupancy Sensors")]
    public class GlsOdtOccupancySensorController : GlsOccupancySensorBaseController
    {
        public new GlsOdtCCn OccSensor { get; private set; }

        public BoolFeedback OrWhenVacatedFeedback { get; private set; }

        public BoolFeedback AndWhenVacatedFeedback { get; private set; }

        public BoolFeedback UltrasonicAEnabledFeedback { get; private set; }

        public BoolFeedback UltrasonicBEnabledFeedback { get; private set; }

        public IntFeedback UltrasonicSensitivityInVacantStateFeedback { get; private set; }

        public IntFeedback UltrasonicSensitivityInOccupiedStateFeedback { get; private set; }

        public BoolFeedback RawOccupancyPirFeedback { get; private set; }

        public BoolFeedback RawOccupancyUsFeedback { get; private set; }


        public GlsOdtOccupancySensorController(string key, Func<DeviceConfig, GlsOdtCCn> preActivationFunc,
            DeviceConfig config)
            : base(key, config.Name)
        {
            AddPreActivationAction(() =>
            {
                OccSensor = preActivationFunc(config);

                RegisterCrestronGenericBase(OccSensor);

                RegisterGlsOdtSensorBaseController(OccSensor);

                AndWhenVacatedFeedback = new BoolFeedback(() => OccSensor.AndWhenVacatedFeedback.BoolValue);

                OrWhenVacatedFeedback = new BoolFeedback(() => OccSensor.OrWhenVacatedFeedback.BoolValue);

                UltrasonicAEnabledFeedback = new BoolFeedback(() => OccSensor.UsAEnabledFeedback.BoolValue);

                UltrasonicBEnabledFeedback = new BoolFeedback(() => OccSensor.UsBEnabledFeedback.BoolValue);

                RawOccupancyPirFeedback = new BoolFeedback(() => OccSensor.RawOccupancyPirFeedback.BoolValue);

                RawOccupancyUsFeedback = new BoolFeedback(() => OccSensor.RawOccupancyUsFeedback.BoolValue);

                UltrasonicSensitivityInVacantStateFeedback = new IntFeedback(() => OccSensor.UsSensitivityInVacantStateFeedback.UShortValue);

                UltrasonicSensitivityInOccupiedStateFeedback = new IntFeedback(() => OccSensor.UsSensitivityInOccupiedStateFeedback.UShortValue);

            });       
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
        /// Overrides the base class event delegate to fire feedbacks for event IDs that pertain to this extended class.
        /// Then calls the base delegate method to ensure any common event IDs are captured.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="args"></param>
        protected override void OccSensor_BaseEvent(Crestron.SimplSharpPro.GenericBase device, Crestron.SimplSharpPro.BaseEventArgs args)
        {
             if (args.EventId == GlsOccupancySensorBase.RawOccupancyPirFeedbackEventId)
                RawOccupancyPirFeedback.FireUpdate();
            else if (args.EventId == GlsOccupancySensorBase.RawOccupancyUsFeedbackEventId)
                RawOccupancyUsFeedback.FireUpdate();

            base.OccSensor_BaseEvent(device, args);
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
            OccSensor.EnableUsA.BoolValue = state;
            OccSensor.DisableUsA.BoolValue = !state;
        }


        /// <summary>
        /// Enables or disables the Ultrasonic B sensor
        /// </summary>
        /// <param name="state"></param>
        public void SetUsBEnable(bool state)
        {
            OccSensor.EnableUsB.BoolValue = state;
            OccSensor.DisableUsB.BoolValue = !state;
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

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            LinkOccSensorToApi(this, trilist, joinStart, joinMapKey, bridge);
        }

        #region PreActivation

        private static GlsOdtCCn GetGlsOdtCCn(DeviceConfig dc)
        {
            var control = CommFactory.GetControlPropertiesConfig(dc);
            var cresnetId = control.CresnetIdInt;
            var branchId = control.ControlPortNumber;
            var parentKey = string.IsNullOrEmpty(control.ControlPortDevKey) ? "processor" : control.ControlPortDevKey;

            if (parentKey.Equals("processor", StringComparison.CurrentCultureIgnoreCase))
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new GlsOdtCCn", parentKey);
                return new GlsOdtCCn(cresnetId, Global.ControlSystem);
            }
            var cresnetBridge = DeviceManager.GetDeviceForKey(parentKey) as IHasCresnetBranches;

            if (cresnetBridge != null)
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new GlsOdtCCn", parentKey);
                return new GlsOdtCCn(cresnetId, cresnetBridge.CresnetBranches[branchId]);
            }
            Debug.Console(0, "Device {0} is not a valid cresnet master", parentKey);
            return null;
        }
        #endregion

        public class GlsOdtOccupancySensorControllerFactory : EssentialsDeviceFactory<GlsOdtOccupancySensorController>
        {
            public GlsOdtOccupancySensorControllerFactory()
            {
                TypeNames = new List<string>() { "glsodtccn" };
            }


            public override EssentialsDevice BuildDevice(DeviceConfig dc)
            {
                Debug.Console(1, "Factory Attempting to create new GlsOccupancySensorBaseController Device");

                return new GlsOdtOccupancySensorController(dc.Key, GetGlsOdtCCn, dc);
            }

        }
    }



}