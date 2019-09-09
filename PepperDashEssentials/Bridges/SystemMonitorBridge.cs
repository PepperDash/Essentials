using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.Diagnostics;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Monitoring;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Bridges
{
    public static class SystemMonitorBridge
    {
        public static void LinkToApi(this SystemMonitorController systemMonitorController, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            SystemMonitorJoinMap joinMap = new SystemMonitorJoinMap();

            var joinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

            if(!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<SystemMonitorJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.Console(2, systemMonitorController, "Linking API starting at join: {0}", joinStart);

            systemMonitorController.TimeZoneFeedback.LinkInputSig(trilist.UShortInput[joinMap.TimeZone]);
            //trilist.SetUShortSigAction(joinMap.TimeZone, new Action<ushort>(u => systemMonitorController.SetTimeZone(u)));
            systemMonitorController.TimeZoneTextFeedback.LinkInputSig(trilist.StringInput[joinMap.TimeZoneName]);

            systemMonitorController.IOControllerVersionFeedback.LinkInputSig(trilist.StringInput[joinMap.IOControllerVersion]);
            systemMonitorController.SnmpVersionFeedback.LinkInputSig(trilist.StringInput[joinMap.SnmpAppVersion]);
            systemMonitorController.BACnetAppVersionFeedback.LinkInputSig(trilist.StringInput[joinMap.BACnetAppVersion]);
            systemMonitorController.ControllerVersionFeedback.LinkInputSig(trilist.StringInput[joinMap.ControllerVersion]);

            // iterate the program status feedback collection and map all the joins
            var programSlotJoinStart = joinMap.ProgramStartJoin;

            foreach (var p in systemMonitorController.ProgramStatusFeedbackCollection)
            {
                var programNumber = p.Value.Program.Number;

                trilist.SetBoolSigAction(programSlotJoinStart + joinMap.ProgramStart, new Action<bool>
                    (b => SystemMonitor.ProgramCollection[programNumber].OperatingState = eProgramOperatingState.Start));
                p.Value.ProgramStartedFeedback.LinkInputSig(trilist.BooleanInput[programSlotJoinStart + joinMap.ProgramStart]);

                trilist.SetBoolSigAction(programSlotJoinStart + joinMap.ProgramStop, new Action<bool>
                    (b => SystemMonitor.ProgramCollection[programNumber].OperatingState = eProgramOperatingState.Stop));
                p.Value.ProgramStoppedFeedback.LinkInputSig(trilist.BooleanInput[programSlotJoinStart + joinMap.ProgramStop]);

                trilist.SetBoolSigAction(programSlotJoinStart + joinMap.ProgramRegister, new Action<bool>
                    (b => SystemMonitor.ProgramCollection[programNumber].RegistrationState = eProgramRegistrationState.Register));
                p.Value.ProgramRegisteredFeedback.LinkInputSig(trilist.BooleanInput[programSlotJoinStart + joinMap.ProgramRegister]);

                trilist.SetBoolSigAction(programSlotJoinStart + joinMap.ProgramUnregister, new Action<bool>
                    (b => SystemMonitor.ProgramCollection[programNumber].RegistrationState = eProgramRegistrationState.Unregister));
                p.Value.ProgramUnregisteredFeedback.LinkInputSig(trilist.BooleanInput[programSlotJoinStart + joinMap.ProgramUnregister]);

                programSlotJoinStart = programSlotJoinStart + joinMap.ProgramOffsetJoin;
            }
        }
    }
}