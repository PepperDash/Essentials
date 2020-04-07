using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.GeneralIO;
using Crestron.SimplSharpPro;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Occupancy
{
    public class CenOdtOccupancySensorBaseController : CrestronGenericBaseDevice, IOccupancyStatusProvider
    {
        public CenOdtCPoe OccSensor { get; private set; }

        public BoolFeedback RoomIsOccupiedFeedback { get; private set; }

        public BoolFeedback GraceOccupancyDetectedFeedback { get; private set; }

        public BoolFeedback RawOccupancyFeedback { get; private set; }

        public BoolFeedback PirSensorEnabledFeedback { get; private set; }

        public BoolFeedback LedFlashEnabledFeedback { get; private set; }

        public BoolFeedback ShortTimeoutEnabledFeedback { get; private set; }

        public IntFeedback PirSensitivityInVacantStateFeedback { get; private set; }

        public IntFeedback PirSensitivityInOccupiedStateFeedback { get; private set; }

        public IntFeedback CurrentTimeoutFeedback { get; private set; }

        public IntFeedback RemoteTimeoutFeedback { get; private set; }

        public IntFeedback InternalPhotoSensorValue { get; set; }

        public IntFeedback ExternalPhotoSensorValue { get; set; }

        public BoolFeedback OrWhenVacatedFeedback { get; private set; }

        public BoolFeedback AndWhenVacatedFeedback { get; private set; }

        public BoolFeedback UltrasonicAEnabledFeedback { get; private set; }

        public BoolFeedback UltrasonicBEnabledFeedback { get; private set; }

        public IntFeedback UltrasonicSensitivityInVacantStateFeedback { get; private set; }

        public IntFeedback UltrasonicSensitivityInOccupiedStateFeedback { get; private set; }

        public BoolFeedback RawOccupancyPirFeedback { get; private set; }

        public BoolFeedback RawOccupancyUsFeedback { get; private set; }

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

        public CenOdtOccupancySensorBaseController(string key, string name, CenOdtCPoe sensor)
            : base(key, name, sensor)
        {
            OccSensor = sensor;

            RoomIsOccupiedFeedback = new BoolFeedback(RoomIsOccupiedFeedbackFunc);

            PirSensorEnabledFeedback = new BoolFeedback(() => OccSensor.PassiveInfraredSensorEnabledFeedback.BoolValue);

            LedFlashEnabledFeedback = new BoolFeedback(() => OccSensor.LedFlashEnabledFeedback.BoolValue);

            ShortTimeoutEnabledFeedback = new BoolFeedback(() => OccSensor.ShortTimeoutEnabledFeedback.BoolValue);

            PirSensitivityInVacantStateFeedback = new IntFeedback(() => (int)OccSensor.PassiveInfraredSensorSensitivityInVacantStateFeedback);

            PirSensitivityInOccupiedStateFeedback = new IntFeedback(() => (int)OccSensor.PassiveInfraredSensorSensitivityInOccupiedStateFeedback);

            CurrentTimeoutFeedback = new IntFeedback(() => OccSensor.CurrentTimeoutFeedback.UShortValue);

            RemoteTimeoutFeedback = new IntFeedback(() => OccSensor.RemoteTimeout.UShortValue);

            GraceOccupancyDetectedFeedback = new BoolFeedback(() => OccSensor.GraceOccupancyDetectedFeedback.BoolValue);

            RawOccupancyFeedback = new BoolFeedback(() => OccSensor.RawOccupancyDetectedFeedback.BoolValue);

            InternalPhotoSensorValue = new IntFeedback(() => OccSensor.InternalPhotoSensorValueFeedback.UShortValue);

            //ExternalPhotoSensorValue = new IntFeedback(() => OccSensor.ex.UShortValue);

            AndWhenVacatedFeedback = new BoolFeedback(() => OccSensor.AndWhenVacatedFeedback.BoolValue);

            OrWhenVacatedFeedback = new BoolFeedback(() => OccSensor.OrWhenVacatedFeedback.BoolValue);

            UltrasonicAEnabledFeedback = new BoolFeedback(() => OccSensor.UltrasonicSensorSideAEnabledFeedback.BoolValue);

            UltrasonicBEnabledFeedback = new BoolFeedback(() => OccSensor.UltrasonicSensorSideBEnabledFeedback.BoolValue);

            RawOccupancyPirFeedback = new BoolFeedback(() => OccSensor.RawOccupancyDetectedByPassiveInfraredSensorFeedback.BoolValue);

            RawOccupancyUsFeedback = new BoolFeedback(() => OccSensor.RawOccupancyDetectedByUltrasonicSensorFeedback.BoolValue);

            UltrasonicSensitivityInVacantStateFeedback = new IntFeedback(() => (int)OccSensor.UltrasonicSensorSensitivityInVacantStateFeedback);

            UltrasonicSensitivityInOccupiedStateFeedback = new IntFeedback(() => (int)OccSensor.UltrasonicSensorSensitivityInOccupiedStateFeedback);

            OccSensor.BaseEvent += new Crestron.SimplSharpPro.BaseEventHandler(OccSensor_BaseEvent);

            OccSensor.CenOccupancySensorChange += new GenericEventHandler(OccSensor_CenOccupancySensorChange);

        }






        /// <summary>
        /// Catches events for feedbacks on the base class.  Any extending wrapper class should call this delegate after it checks for it's own event IDs.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="args"></param>
        protected virtual void OccSensor_CenOccupancySensorChange(object device, GenericEventArgs args)
        {
            if (args.EventId == GlsOccupancySensorBase.PirEnabledFeedbackEventId)
                PirSensorEnabledFeedback.FireUpdate();
            else if (args.EventId == GlsOccupancySensorBase.LedFlashEnabledFeedbackEventId)
                LedFlashEnabledFeedback.FireUpdate();
            else if (args.EventId == GlsOccupancySensorBase.ShortTimeoutEnabledFeedbackEventId)
                ShortTimeoutEnabledFeedback.FireUpdate();
            else if (args.EventId == GlsOccupancySensorBase.PirSensitivityInOccupiedStateFeedbackEventId)
                PirSensitivityInOccupiedStateFeedback.FireUpdate();
            else if (args.EventId == GlsOccupancySensorBase.PirSensitivityInVacantStateFeedbackEventId)
                PirSensitivityInVacantStateFeedback.FireUpdate();
            else if (args.EventId == GlsOccupancySensorBase.RawOccupancyPirFeedbackEventId)
                RawOccupancyPirFeedback.FireUpdate();
            else if (args.EventId == GlsOccupancySensorBase.RawOccupancyUsFeedbackEventId)
                RawOccupancyUsFeedback.FireUpdate();
        }

        protected virtual void OccSensor_BaseEvent(Crestron.SimplSharpPro.GenericBase device, Crestron.SimplSharpPro.BaseEventArgs args)
        {
            Debug.Console(2, this, "PoEOccupancySensorChange  EventId: {0}", args.EventId);

            if (args.EventId == Crestron.SimplSharpPro.GeneralIO.GlsOccupancySensorBase.RoomOccupiedFeedbackEventId
                || args.EventId == Crestron.SimplSharpPro.GeneralIO.GlsOccupancySensorBase.RoomVacantFeedbackEventId)
            {
                Debug.Console(1, this, "Occupancy State: {0}", OccSensor.OccupancyDetectedFeedback.BoolValue);
                RoomIsOccupiedFeedback.FireUpdate();
            }
            else if (args.EventId == GlsOccupancySensorBase.TimeoutFeedbackEventId)
                CurrentTimeoutFeedback.FireUpdate();
            else if (args.EventId == GlsOccupancySensorBase.TimeoutLocalFeedbackEventId)
                LocalTimoutFeedback.FireUpdate();
            else if (args.EventId == GlsOccupancySensorBase.GraceOccupancyDetectedFeedbackEventId)
                GraceOccupancyDetectedFeedback.FireUpdate();
            else if (args.EventId == GlsOccupancySensorBase.RawOccupancyFeedbackEventId)
                RawOccupancyFeedback.FireUpdate();
            else if (args.EventId == GlsOccupancySensorBase.InternalPhotoSensorValueFeedbackEventId)
                InternalPhotoSensorValue.FireUpdate();
            else if (args.EventId == GlsOccupancySensorBase.ExternalPhotoSensorValueFeedbackEventId)
                ExternalPhotoSensorValue.FireUpdate();
            else if (args.EventId == GlsOccupancySensorBase.AndWhenVacatedFeedbackEventId)
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

        /// <summary>
        /// Enables or disables the PIR sensor
        /// </summary>
        /// <param name="state"></param>
        public void SetPirEnable(bool state)
        {
            if (state)
            {
                OccSensor.EnablePassiveInfraredSensor();
            }
            else
            {
                OccSensor.DisablePassiveInfraredSensor();
            }
        }

        /// <summary>
        /// Enables or disables the LED Flash
        /// </summary>
        /// <param name="state"></param>
        public void SetLedFlashEnable(bool state)
        {
            if (state)
            {
                OccSensor.EnableLedFlash();
            }
            else
            {
                OccSensor.DisableLedFlash();
            }
        }

        /// <summary>
        /// Enables or disables short timeout based on state
        /// </summary>
        /// <param name="state"></param>
        public void SetShortTimeoutState(bool state)
        {
            if (state)
            {
                OccSensor.EnableShortTimeout();
            }
            else
            {
                OccSensor.DisableShortTimeout();
            }
        }

        public void IncrementPirSensitivityInOccupiedState(bool pressRelease)
        {
            if ((int)OccSensor.PassiveInfraredSensorSensitivityInOccupiedStateFeedback != 3)
            {
                OccSensor.PassiveInfraredSensorSensitivityInOccupiedState = OccSensor.PassiveInfraredSensorSensitivityInOccupiedStateFeedback + 1;
            }
        }

        public void DecrementPirSensitivityInOccupiedState(bool pressRelease)
        {
            if ((int)OccSensor.PassiveInfraredSensorSensitivityInOccupiedStateFeedback != 0)
            {
                OccSensor.PassiveInfraredSensorSensitivityInOccupiedState = OccSensor.PassiveInfraredSensorSensitivityInOccupiedStateFeedback - 1;
            }
        }

        public void IncrementPirSensitivityInVacantState(bool pressRelease)
        {
            if ((int)OccSensor.PassiveInfraredSensorSensitivityInVacantStateFeedback != 3)
            {
                OccSensor.PassiveInfraredSensorSensitivityInVacantState = OccSensor.PassiveInfraredSensorSensitivityInVacantStateFeedback + 1;
            }
        }

        public void DecrementPirSensitivityInVacantState(bool pressRelease)
        {
            if ((int)OccSensor.PassiveInfraredSensorSensitivityInVacantStateFeedback != 0)
            {
                OccSensor.PassiveInfraredSensorSensitivityInVacantState = OccSensor.PassiveInfraredSensorSensitivityInVacantStateFeedback - 1;
            }
        }

        public void IncrementUsSensitivityInOccupiedState(bool pressRelease)
        {
            if ((int)OccSensor.UltrasonicSensorSensitivityInOccupiedStateFeedback < 3)
            {
                OccSensor.UltrasonicSensorSensitivityInOccupiedState = OccSensor.UltrasonicSensorSensitivityInOccupiedStateFeedback + 1;
            }
            else if ((int)OccSensor.UltrasonicSensorSensitivityInOccupiedStateFeedback > 4)
            {
                OccSensor.UltrasonicSensorSensitivityInOccupiedState = OccSensor.UltrasonicSensorSensitivityInOccupiedStateFeedback - 1;
            }
            else if ((int)OccSensor.UltrasonicSensorSensitivityInOccupiedStateFeedback == 4)
            {
                OccSensor.UltrasonicSensorSensitivityInOccupiedState = 0;
            }
        }

        public void DecrementUsSensitivityInOccupiedState(bool pressRelease)
        {
            if ((int)OccSensor.UltrasonicSensorSensitivityInOccupiedStateFeedback > 0
                && (int)OccSensor.UltrasonicSensorSensitivityInOccupiedStateFeedback < 4)
            {
                OccSensor.UltrasonicSensorSensitivityInOccupiedState = OccSensor.UltrasonicSensorSensitivityInOccupiedStateFeedback - 1;
            }
            else if ((int)OccSensor.UltrasonicSensorSensitivityInOccupiedStateFeedback > 3
                && (int)OccSensor.UltrasonicSensorSensitivityInOccupiedStateFeedback < 7)
            {
                OccSensor.UltrasonicSensorSensitivityInOccupiedState = OccSensor.UltrasonicSensorSensitivityInOccupiedStateFeedback + 1;
            }
        }

        public void IncrementUsSensitivityInVacantState(bool pressRelease)
        {
            if ((int)OccSensor.UltrasonicSensorSensitivityInVacantStateFeedback < 3)
            {
                OccSensor.UltrasonicSensorSensitivityInVacantState = OccSensor.UltrasonicSensorSensitivityInVacantStateFeedback + 1;
            }
            else if ((int)OccSensor.UltrasonicSensorSensitivityInVacantStateFeedback > 4)
            {
                OccSensor.UltrasonicSensorSensitivityInVacantState = OccSensor.UltrasonicSensorSensitivityInVacantStateFeedback - 1;
            }
            else if ((int)OccSensor.UltrasonicSensorSensitivityInVacantStateFeedback == 4)
            {
                OccSensor.UltrasonicSensorSensitivityInVacantState = 0;
            }
        }

        public void DecrementUsSensitivityInVacantState(bool pressRelease)
        {
            if ((int)OccSensor.UltrasonicSensorSensitivityInVacantStateFeedback > 0
                && (int)OccSensor.UltrasonicSensorSensitivityInVacantStateFeedback < 4)
            {
                OccSensor.UltrasonicSensorSensitivityInVacantState = OccSensor.UltrasonicSensorSensitivityInVacantStateFeedback - 1;
            }
            else if ((int)OccSensor.UltrasonicSensorSensitivityInVacantStateFeedback > 3
                && (int)OccSensor.UltrasonicSensorSensitivityInVacantStateFeedback < 7)
            {
                OccSensor.UltrasonicSensorSensitivityInVacantState = OccSensor.UltrasonicSensorSensitivityInVacantStateFeedback + 1;
            }
        }

        public void ForceOccupied()
        {
            OccSensor.ForceOccupied();
        }

        public void ForceVacant()
        {
            OccSensor.ForceVacant();
        }

        public void EnableRawStates(bool state)
        {
            if (state)
            {
                OccSensor.EnableRawStates();
            }
            else
                OccSensor.DisableRawStates();
        }

        public void SetRemoteTimeout(ushort time)
        {
            OccSensor.RemoteTimeout.UShortValue = time;
        }

        public void SetInternalPhotoSensorMinChange(ushort value)
        {
            OccSensor.InternalPhotoSensorMinimumChange.UShortValue = value;
        }

        /// <summary>
        /// Sets the OrWhenVacated state
        /// </summary>
        /// <param name="state"></param>
        public void SetOrWhenVacatedState(bool state)
        {
            if (state)
            {
                OccSensor.OrWhenVacated();
            }
        }

        /// <summary>
        /// Sets the AndWhenVacated state
        /// </summary>
        /// <param name="state"></param>
        public void SetAndWhenVacatedState(bool state)
        {
            if (state)
            {
                OccSensor.AndWhenVacated();
            }
        }

        /// <summary>
        /// Enables or disables the Ultrasonic A sensor
        /// </summary>
        /// <param name="state"></param>
        public void SetUsAEnable(bool state)
        {
            if (state)
            {
                OccSensor.EnableUltrasonicSensorSideA();
            }
            else
            {
                OccSensor.DisableUltrasonicSensorSideA();
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
                OccSensor.EnableUltrasonicSensorSideB();
            }
            else
            {
                OccSensor.DisableUltrasonicSensorSideB();
            }
        }

        /*
        public void SetExternalPhotoSensorMinChange(ushort value)
        {
            OccSensor.ExternalPhotoSensorMinimumChange.UShortValue = value;
        }
        */
    }
}