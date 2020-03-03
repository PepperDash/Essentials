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
    public class GlsOccupancySensorBaseController : CrestronGenericBaseDevice, IOccupancyStatusProvider
    {
        public GlsOccupancySensorBase OccSensor { get; private set; }

        public BoolFeedback RoomIsOccupiedFeedback { get; private set; }

        public BoolFeedback GraceOccupancyDetectedFeedback { get; private set; }

        public BoolFeedback RawOccupancyFeedback { get; private set; }

        public BoolFeedback PirSensorEnabledFeedback { get; private set; }

        public BoolFeedback LedFlashEnabledFeedback { get; private set; }

        public BoolFeedback ShortTimeoutEnabledFeedback { get; private set; }

        public IntFeedback PirSensitivityInVacantStateFeedback { get; private set; }

        public IntFeedback PirSensitivityInOccupiedStateFeedback { get; private set; }

        public IntFeedback CurrentTimeoutFeedback { get; private set; }

        public IntFeedback LocalTimoutFeedback { get; private set; }

        public IntFeedback InternalPhotoSensorValue { get; set; }

        public IntFeedback ExternalPhotoSensorValue { get; set; }

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

        public GlsOccupancySensorBaseController(string key, string name, GlsOccupancySensorBase sensor)
            : base(key, name, sensor)
        {
            OccSensor = sensor;
            
            RoomIsOccupiedFeedback = new BoolFeedback(RoomIsOccupiedFeedbackFunc);

            PirSensorEnabledFeedback = new BoolFeedback(() => OccSensor.PirEnabledFeedback.BoolValue);

            LedFlashEnabledFeedback = new BoolFeedback(() => OccSensor.LedFlashEnabledFeedback.BoolValue);

            ShortTimeoutEnabledFeedback = new BoolFeedback(() => OccSensor.ShortTimeoutEnabledFeedback.BoolValue);

            PirSensitivityInVacantStateFeedback = new IntFeedback(() => OccSensor.PirSensitivityInVacantStateFeedback.UShortValue);

            PirSensitivityInOccupiedStateFeedback = new IntFeedback(() => OccSensor.PirSensitivityInOccupiedStateFeedback.UShortValue);

            CurrentTimeoutFeedback = new IntFeedback(() => OccSensor.CurrentTimeoutFeedback.UShortValue);

            LocalTimoutFeedback = new IntFeedback(() => OccSensor.LocalTimeoutFeedback.UShortValue);

            GraceOccupancyDetectedFeedback = new BoolFeedback(() => OccSensor.GraceOccupancyDetectedFeedback.BoolValue);

            RawOccupancyFeedback = new BoolFeedback(() => OccSensor.RawOccupancyFeedback.BoolValue);

            InternalPhotoSensorValue = new IntFeedback(() => OccSensor.InternalPhotoSensorValueFeedback.UShortValue);

            ExternalPhotoSensorValue = new IntFeedback(() => OccSensor.ExternalPhotoSensorValueFeedback.UShortValue);

            OccSensor.BaseEvent += new Crestron.SimplSharpPro.BaseEventHandler(OccSensor_BaseEvent);

            OccSensor.GlsOccupancySensorChange += new GlsOccupancySensorChangeEventHandler(OccSensor_GlsOccupancySensorChange);
        }


        /// <summary>
        /// Catches events for feedbacks on the base class.  Any extending wrapper class should call this delegate after it checks for it's own event IDs.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="args"></param>
        protected virtual void OccSensor_GlsOccupancySensorChange(GlsOccupancySensorBase device, GlsOccupancySensorChangeEventArgs args)
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
        }

        protected virtual void OccSensor_BaseEvent(Crestron.SimplSharpPro.GenericBase device, Crestron.SimplSharpPro.BaseEventArgs args)
        {
            Debug.Console(2, this, "GlsOccupancySensorChange  EventId: {0}", args.EventId);

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
                OccSensor.EnablePir.BoolValue = state;
                OccSensor.DisablePir.BoolValue = !state;
            }
            else
            {
                OccSensor.EnablePir.BoolValue = state;
                OccSensor.DisablePir.BoolValue = !state;
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
                OccSensor.EnableLedFlash.BoolValue = state;
                OccSensor.DisableLedFlash.BoolValue = !state;
            }
            else
            {
                OccSensor.EnableLedFlash.BoolValue = state;
                OccSensor.DisableLedFlash.BoolValue = !state;
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
                OccSensor.EnableShortTimeout.BoolValue = state;
                OccSensor.DisableShortTimeout.BoolValue = !state;
            }
            else
            {
                OccSensor.EnableShortTimeout.BoolValue = state;
                OccSensor.DisableShortTimeout.BoolValue = !state;
            }
        }

        public void IncrementPirSensitivityInOccupiedState(bool pressRelease)
        {
            OccSensor.IncrementPirSensitivityInOccupiedState.BoolValue = pressRelease;
        }

        public void DecrementPirSensitivityInOccupiedState(bool pressRelease)
        {
            OccSensor.DecrementPirSensitivityInOccupiedState.BoolValue = pressRelease;
        }

        public void IncrementPirSensitivityInVacantState(bool pressRelease)
        {
            OccSensor.IncrementPirSensitivityInVacantState.BoolValue = pressRelease;
        }

        public void DecrementPirSensitivityInVacantState(bool pressRelease)
        {
            OccSensor.DecrementPirSensitivityInVacantState.BoolValue = pressRelease;
        }

        public void ForceOccupied()
        {
            OccSensor.ForceOccupied.BoolValue = true;
        }

        public void ForceVacant()
        {
            OccSensor.ForceVacant.BoolValue = true;
        }

        public void EnableRawStates(bool state)
        {
            OccSensor.EnableRawStates.BoolValue = state;
        }

        public void SetRemoteTimeout(ushort time)
        {
            OccSensor.RemoteTimeout.UShortValue = time;
        }

        public void SetInternalPhotoSensorMinChange(ushort value)
        {
            OccSensor.InternalPhotoSensorMinimumChange.UShortValue = value;
        }

        public void SetExternalPhotoSensorMinChange(ushort value)
        {
            OccSensor.ExternalPhotoSensorMinimumChange.UShortValue = value;
        }
    }
}