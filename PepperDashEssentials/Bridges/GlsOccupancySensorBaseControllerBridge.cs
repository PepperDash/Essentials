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

            occController.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
            trilist.SetSigTrueAction(joinMap.ForceOccupied, () => occController.ForceOccupied());
            trilist.SetSigTrueAction(joinMap.ForceVacant, () => occController.ForceVacant());
            occController.RoomIsOccupiedFeedback.LinkInputSig(trilist.BooleanInput[joinMap.RoomOccupiedFeedback]);
            occController.RoomIsOccupiedFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.RoomVacantFeedback]);
            occController.RawOccupancyFeedback.LinkInputSig(trilist.BooleanInput[joinMap.RawOccupancyFeedback]);

            trilist.SetUShortSigAction(joinMap.Timeout, (u) => occController.SetRemoteTimeout(u));
            occController.CurrentTimeoutFeedback.LinkInputSig(trilist.UShortInput[joinMap.Timeout]);
            occController.LocalTimoutFeedback.LinkInputSig(trilist.UShortInput[joinMap.TimeoutLocalFeedback]);

            trilist.SetSigTrueAction(joinMap.EnableLedFlash, () => occController.SetLedFlashEnable(true));
            trilist.SetSigTrueAction(joinMap.DisableLedFlash, () => occController.SetLedFlashEnable(false));
            occController.LedFlashEnabledFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.EnableLedFlash]);

            trilist.SetSigTrueAction(joinMap.EnableShortTimeout, () => occController.SetShortTimeoutState(true));
            trilist.SetSigTrueAction(joinMap.DisableShortTimeout, () => occController.SetShortTimeoutState(false));
            occController.ShortTimeoutEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.EnableShortTimeout]);


        }
    }
}