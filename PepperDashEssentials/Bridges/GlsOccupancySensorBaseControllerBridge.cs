using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Essentials.Devices.Common.Occupancy;

using PepperDash.Essentials.Core;
using PepperDash.Core;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Bridges
{
    public static class GlsOccupancySensorBaseControllerApiExtensions
    {
        public static void LinkToApi(this GlsOccupancySensorBaseController occController, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            GlsOccupancySensorBaseJoinMap joinMap = new GlsOccupancySensorBaseJoinMap();

            var joinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<GlsOccupancySensorBaseJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, occController, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            #region Single and Dual Sensor Stuff
            occController.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);

            // Occupied status
            trilist.SetSigTrueAction(joinMap.ForceOccupied, new Action(() => occController.ForceOccupied()));
            trilist.SetSigTrueAction(joinMap.ForceVacant, new Action(() => occController.ForceVacant()));
            occController.RoomIsOccupiedFeedback.LinkInputSig(trilist.BooleanInput[joinMap.RoomOccupiedFeedback]);
            occController.RoomIsOccupiedFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.RoomVacantFeedback]);
            occController.RawOccupancyFeedback.LinkInputSig(trilist.BooleanInput[joinMap.RawOccupancyFeedback]);

            // Timouts
            trilist.SetUShortSigAction(joinMap.Timeout, new Action<ushort>((u) => occController.SetRemoteTimeout(u)));
            occController.CurrentTimeoutFeedback.LinkInputSig(trilist.UShortInput[joinMap.Timeout]);
            occController.LocalTimoutFeedback.LinkInputSig(trilist.UShortInput[joinMap.TimeoutLocalFeedback]);

            // LED Flash
            trilist.SetSigTrueAction(joinMap.EnableLedFlash, new Action(() => occController.SetLedFlashEnable(true)));
            trilist.SetSigTrueAction(joinMap.DisableLedFlash, new Action(() => occController.SetLedFlashEnable(false)));
            occController.LedFlashEnabledFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.EnableLedFlash]);

            // Short Timeout
            trilist.SetSigTrueAction(joinMap.EnableShortTimeout, new Action(() => occController.SetShortTimeoutState(true)));
            trilist.SetSigTrueAction(joinMap.DisableShortTimeout, new Action(() => occController.SetShortTimeoutState(false)));
            occController.ShortTimeoutEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.EnableShortTimeout]);

            // PIR Sensor
            trilist.SetSigTrueAction(joinMap.EnablePir, new Action(() => occController.SetPirEnable(true)));
            trilist.SetSigTrueAction(joinMap.DisablePir, new Action(() => occController.SetPirEnable(false)));
            occController.PirSensorEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.EnablePir]);
            
            // PIR Sensitivity in Occupied State
            trilist.SetBoolSigAction(joinMap.IncrementPirInOccupiedState, new Action<bool>((b) => occController.IncrementPirSensitivityInOccupiedState(b)));
            trilist.SetBoolSigAction(joinMap.DecrementPirInOccupiedState, new Action<bool>((b) => occController.DecrementPirSensitivityInOccupiedState(b)));
            occController.PirSensitivityInOccupiedStateFeedback.LinkInputSig(trilist.UShortInput[joinMap.PirSensitivityInOccupiedState]);

            // PIR Sensitivity in Vacant State
            trilist.SetBoolSigAction(joinMap.IncrementPirInVacantState, new Action<bool>((b) => occController.IncrementPirSensitivityInVacantState(b)));
            trilist.SetBoolSigAction(joinMap.DecrementPirInVacantState, new Action<bool>((b) => occController.DecrementPirSensitivityInVacantState(b)));
            occController.PirSensitivityInVacantStateFeedback.LinkInputSig(trilist.UShortInput[joinMap.PirSensitivityInVacantState]);
            #endregion

            #region Dual Technology Sensor Stuff
            var odtOccController = occController as GlsOdtOccupancySensorController;

            if (odtOccController != null)
            {
                // OR When Vacated
                trilist.SetBoolSigAction(joinMap.OrWhenVacated, new Action<bool>((b) => odtOccController.SetOrWhenVacatedState(b)));
                odtOccController.OrWhenVacatedFeedback.LinkInputSig(trilist.BooleanInput[joinMap.OrWhenVacated]);

                // AND When Vacated
                trilist.SetBoolSigAction(joinMap.AndWhenVacated, new Action<bool>((b) => odtOccController.SetAndWhenVacatedState(b)));
                odtOccController.AndWhenVacatedFeedback.LinkInputSig(trilist.BooleanInput[joinMap.AndWhenVacated]);

                // Ultrasonic A Sensor
                trilist.SetSigTrueAction(joinMap.EnableUsA, new Action(() => odtOccController.SetUsAEnable(true)));
                trilist.SetSigTrueAction(joinMap.DisableUsA, new Action(() => odtOccController.SetUsAEnable(false)));
                odtOccController.UltrasonicAEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.EnableUsA]);

                // Ultrasonic B Sensor
                trilist.SetSigTrueAction(joinMap.EnableUsB, new Action(() => odtOccController.SetUsBEnable(true)));
                trilist.SetSigTrueAction(joinMap.DisableUsB, new Action(() => odtOccController.SetUsBEnable(false)));
                odtOccController.UltrasonicAEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.EnableUsB]);

                // US Sensitivity in Occupied State
                trilist.SetBoolSigAction(joinMap.IncrementUsInOccupiedState, new Action<bool>((b) => odtOccController.IncrementUsSensitivityInOccupiedState(b)));
                trilist.SetBoolSigAction(joinMap.DecrementUsInOccupiedState, new Action<bool>((b) => odtOccController.DecrementUsSensitivityInOccupiedState(b)));
                odtOccController.UltrasonicSensitivityInOccupiedStateFeedback.LinkInputSig(trilist.UShortInput[joinMap.UsSensitivityInOccupiedState]);

                // US Sensitivity in Vacant State
                trilist.SetBoolSigAction(joinMap.IncrementUsInVacantState, new Action<bool>((b) => odtOccController.IncrementUsSensitivityInVacantState(b)));
                trilist.SetBoolSigAction(joinMap.DecrementUsInVacantState, new Action<bool>((b) => odtOccController.DecrementUsSensitivityInVacantState(b)));
                odtOccController.UltrasonicSensitivityInVacantStateFeedback.LinkInputSig(trilist.UShortInput[joinMap.UsSensitivityInVacantState]);

                //Sensor Raw States
                odtOccController.RawOccupancyPirFeedback.LinkInputSig(trilist.BooleanInput[joinMap.RawOccupancyPirFeedback]);
                odtOccController.RawOccupancyUsFeedback.LinkInputSig(trilist.BooleanInput[joinMap.RawOccupancyUsFeedback]);

            }
            #endregion
        }
    }
}