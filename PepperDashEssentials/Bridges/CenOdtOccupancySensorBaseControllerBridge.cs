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
    public static class CenOdtOccupancySensorBaseControllerApiExtensions
    {
        public static void LinkToApi(this CenOdtOccupancySensorBaseController occController, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            CenOdtOccupancySensorBaseJoinMap joinMap = new CenOdtOccupancySensorBaseJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<CenOdtOccupancySensorBaseJoinMap>(joinMapSerialized);


            Debug.Console(1, occController, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            occController.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.Online.JoinNumber]);
            trilist.StringInput[joinMap.Name.JoinNumber].StringValue = occController.Name;

            trilist.OnlineStatusChange += new Crestron.SimplSharpPro.OnlineStatusChangeEventHandler((d, args) =>
            {
                if (args.DeviceOnLine)
                {
                    trilist.StringInput[joinMap.Name.JoinNumber].StringValue = occController.Name;
                }
            }
            );

            // Occupied status
            trilist.SetSigTrueAction(joinMap.ForceOccupied.JoinNumber, new Action(() => occController.ForceOccupied()));
            trilist.SetSigTrueAction(joinMap.ForceVacant.JoinNumber, new Action(() => occController.ForceVacant()));
            occController.RoomIsOccupiedFeedback.LinkInputSig(trilist.BooleanInput[joinMap.RoomOccupiedFeedback.JoinNumber]);
            occController.RoomIsOccupiedFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.RoomVacantFeedback.JoinNumber]);
            occController.RawOccupancyFeedback.LinkInputSig(trilist.BooleanInput[joinMap.RawOccupancyFeedback.JoinNumber]);
            trilist.SetBoolSigAction(joinMap.EnableRawStates.JoinNumber, new Action<bool>((b) => occController.EnableRawStates(b)));

            // Timouts
            trilist.SetUShortSigAction(joinMap.Timeout.JoinNumber, new Action<ushort>((u) => occController.SetRemoteTimeout(u)));
            occController.CurrentTimeoutFeedback.LinkInputSig(trilist.UShortInput[joinMap.Timeout.JoinNumber]);
            occController.RemoteTimeoutFeedback.LinkInputSig(trilist.UShortInput[joinMap.TimeoutLocalFeedback.JoinNumber]);

            // LED Flash
            trilist.SetSigTrueAction(joinMap.EnableLedFlash.JoinNumber, new Action(() => occController.SetLedFlashEnable(true)));
            trilist.SetSigTrueAction(joinMap.DisableLedFlash.JoinNumber, new Action(() => occController.SetLedFlashEnable(false)));
            occController.LedFlashEnabledFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.EnableLedFlash.JoinNumber]);

            // Short Timeout
            trilist.SetSigTrueAction(joinMap.EnableShortTimeout.JoinNumber, new Action(() => occController.SetShortTimeoutState(true)));
            trilist.SetSigTrueAction(joinMap.DisableShortTimeout.JoinNumber, new Action(() => occController.SetShortTimeoutState(false)));
            occController.ShortTimeoutEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.EnableShortTimeout.JoinNumber]);

            // PIR Sensor
            trilist.SetSigTrueAction(joinMap.EnablePir.JoinNumber, new Action(() => occController.SetPirEnable(true)));
            trilist.SetSigTrueAction(joinMap.DisablePir.JoinNumber, new Action(() => occController.SetPirEnable(false)));
            occController.PirSensorEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.EnablePir.JoinNumber]);

            // PIR Sensitivity in Occupied State
            trilist.SetBoolSigAction(joinMap.IncrementPirInOccupiedState.JoinNumber, new Action<bool>((b) => occController.IncrementPirSensitivityInOccupiedState(b)));
            trilist.SetBoolSigAction(joinMap.DecrementPirInOccupiedState.JoinNumber, new Action<bool>((b) => occController.DecrementPirSensitivityInOccupiedState(b)));
            occController.PirSensitivityInOccupiedStateFeedback.LinkInputSig(trilist.UShortInput[joinMap.PirSensitivityInOccupiedState.JoinNumber]);

            // PIR Sensitivity in Vacant State
            trilist.SetBoolSigAction(joinMap.IncrementPirInVacantState.JoinNumber, new Action<bool>((b) => occController.IncrementPirSensitivityInVacantState(b)));
            trilist.SetBoolSigAction(joinMap.DecrementPirInVacantState.JoinNumber, new Action<bool>((b) => occController.DecrementPirSensitivityInVacantState(b)));
            occController.PirSensitivityInVacantStateFeedback.LinkInputSig(trilist.UShortInput[joinMap.PirSensitivityInVacantState.JoinNumber]);

            // OR When Vacated
            trilist.SetBoolSigAction(joinMap.OrWhenVacated.JoinNumber, new Action<bool>((b) => occController.SetOrWhenVacatedState(b)));
            occController.OrWhenVacatedFeedback.LinkInputSig(trilist.BooleanInput[joinMap.OrWhenVacated.JoinNumber]);

            // AND When Vacated
            trilist.SetBoolSigAction(joinMap.AndWhenVacated.JoinNumber, new Action<bool>((b) => occController.SetAndWhenVacatedState(b)));
            occController.AndWhenVacatedFeedback.LinkInputSig(trilist.BooleanInput[joinMap.AndWhenVacated.JoinNumber]);

            // Ultrasonic A Sensor
            trilist.SetSigTrueAction(joinMap.EnableUsA.JoinNumber, new Action(() => occController.SetUsAEnable(true)));
            trilist.SetSigTrueAction(joinMap.DisableUsA.JoinNumber, new Action(() => occController.SetUsAEnable(false)));
            occController.UltrasonicAEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.EnableUsA.JoinNumber]);

            // Ultrasonic B Sensor
            trilist.SetSigTrueAction(joinMap.EnableUsB.JoinNumber, new Action(() => occController.SetUsBEnable(true)));
            trilist.SetSigTrueAction(joinMap.DisableUsB.JoinNumber, new Action(() => occController.SetUsBEnable(false)));
            occController.UltrasonicAEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.EnableUsB.JoinNumber]);

            // US Sensitivity in Occupied State
            trilist.SetBoolSigAction(joinMap.IncrementUsInOccupiedState.JoinNumber, new Action<bool>((b) => occController.IncrementUsSensitivityInOccupiedState(b)));
            trilist.SetBoolSigAction(joinMap.DecrementUsInOccupiedState.JoinNumber, new Action<bool>((b) => occController.DecrementUsSensitivityInOccupiedState(b)));
            occController.UltrasonicSensitivityInOccupiedStateFeedback.LinkInputSig(trilist.UShortInput[joinMap.UsSensitivityInOccupiedState.JoinNumber]);

            // US Sensitivity in Vacant State
            trilist.SetBoolSigAction(joinMap.IncrementUsInVacantState.JoinNumber, new Action<bool>((b) => occController.IncrementUsSensitivityInVacantState(b)));
            trilist.SetBoolSigAction(joinMap.DecrementUsInVacantState.JoinNumber, new Action<bool>((b) => occController.DecrementUsSensitivityInVacantState(b)));
            occController.UltrasonicSensitivityInVacantStateFeedback.LinkInputSig(trilist.UShortInput[joinMap.UsSensitivityInVacantState.JoinNumber]);

            //Sensor Raw States
            occController.RawOccupancyPirFeedback.LinkInputSig(trilist.BooleanInput[joinMap.RawOccupancyPirFeedback.JoinNumber]);
            occController.RawOccupancyUsFeedback.LinkInputSig(trilist.BooleanInput[joinMap.RawOccupancyUsFeedback.JoinNumber]);

        }
    }
}