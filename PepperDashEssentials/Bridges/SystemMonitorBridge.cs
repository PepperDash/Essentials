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

namespace PepperDash.Essentials.Bridges
{
    public static class SystemMonitorBridge
    {
        public static void LinkToApi(this SystemMonitorController systemMonitorController, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            var joinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as SystemMonitorJoinMap;

            if (joinMap == null)
                joinMap = new SystemMonitorJoinMap();

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, systemMonitorController, "Linking API starting at join: {0}", joinStart);

            systemMonitorController.TimeZoneFeedback.LinkInputSig(trilist.UShortInput[joinMap.TimeZone]);
            //trilist.SetUShortSigAction(joinMap.TimeZone, new Action<ushort>(u => systemMonitorController.SetTimeZone(u)));
            systemMonitorController.TimeZoneTextFeedback.LinkInputSig(trilist.StringInput[joinMap.TimeZoneName]);

            systemMonitorController.IOControllerVersionFeedback.LinkInputSig(trilist.StringInput[joinMap.IOControllerVersion]);
            systemMonitorController.SnmpVersionFeedback.LinkInputSig(trilist.StringInput[joinMap.SnmpAppVersion]);
            systemMonitorController.BACnetAppVersionFeedback.LinkInputSig(trilist.StringInput[joinMap.BACnetAppVersion]);
            systemMonitorController.ControllerVersionFeedback.LinkInputSig(trilist.StringInput[joinMap.BACnetAppVersion]);


            // iterate the program status feedback collection and map all the joins
            var programSlotJoinStart = joinMap.ProgramStartJoin;

            foreach (var p in systemMonitorController.ProgramStatusFeedbackCollection)
            {

                // TODO: link feedbacks for each program slot
                var programNumber = p.Value.Program.Number;

                Debug.Console(1, systemMonitorController, "Linking API for Program Slot: {0} starting at join: {1}", programNumber, programSlotJoinStart);

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

                p.Value.ProgramNameFeedback.LinkInputSig(trilist.StringInput[programSlotJoinStart + joinMap.ProgramName]);
                p.Value.ProgramCompileTimeFeedback.LinkInputSig(trilist.StringInput[programSlotJoinStart + joinMap.ProgramCompiledTime]);
                p.Value.CrestronDataBaseVersionFeedback.LinkInputSig(trilist.StringInput[programSlotJoinStart + joinMap.ProgramCrestronDatabaseVersion]);
                p.Value.EnvironmentVersionFeedback.LinkInputSig(trilist.StringInput[programSlotJoinStart + joinMap.ProgramEnvironmentVersion]);
                p.Value.AggregatedProgramInfoFeedback.LinkInputSig(trilist.StringInput[programSlotJoinStart + joinMap.AggregatedProgramInfo]);

                programSlotJoinStart = programSlotJoinStart + joinMap.ProgramOffsetJoin;
            }

            Debug.Console(1, systemMonitorController, "*****************************Manually Firing Feedback Updates....*****************************");

            systemMonitorController.ControllerVersionFeedback.FireUpdate();
            systemMonitorController.TimeZoneFeedback.FireUpdate();
            systemMonitorController.TimeZoneTextFeedback.FireUpdate();
            systemMonitorController.IOControllerVersionFeedback.FireUpdate();
            systemMonitorController.SnmpVersionFeedback.FireUpdate();
            systemMonitorController.BACnetAppVersionFeedback.FireUpdate();
        }


    }

    public class SystemMonitorJoinMap : JoinMapBase
    {
        /// <summary>
        /// Offset to indicate where the range of iterated program joins will start
        /// </summary>
        public uint ProgramStartJoin { get; set; }

        /// <summary>
        /// Offset between each program join set
        /// </summary>
        public uint ProgramOffsetJoin { get; set; }

        //Digital
        public uint ProgramStart { get; set; }
        public uint ProgramStop { get; set; }
        public uint ProgramRegister { get; set; }
        public uint ProgramUnregister { get; set; }

        //Analog
        public uint TimeZone { get; set; }

        //Serial
        public uint TimeZoneName { get; set; }
        public uint IOControllerVersion { get; set; }
        public uint SnmpAppVersion { get; set; }
        public uint BACnetAppVersion { get; set; }
        public uint ControllerVersion { get; set; }

        public uint ProgramName { get; set; }
        public uint ProgramCompiledTime { get; set; }
        public uint ProgramCrestronDatabaseVersion { get; set; }
        public uint ProgramEnvironmentVersion { get; set; }
        public uint AggregatedProgramInfo { get; set; }

        public SystemMonitorJoinMap()
        {
            TimeZone = 1;

            TimeZoneName = 1;
            IOControllerVersion = 2;
            SnmpAppVersion = 3;
            BACnetAppVersion = 4;
            ControllerVersion = 5;

            
            ProgramStartJoin = 10;
            
            ProgramOffsetJoin = 5;

            // Offset in groups of 5 joins
            ProgramStart = 1;
            ProgramStop = 2;
            ProgramRegister = 3;
            ProgramUnregister = 4;

            ProgramName = 1;
            ProgramCompiledTime = 2;
            ProgramCrestronDatabaseVersion = 3;
            ProgramEnvironmentVersion = 4;
            AggregatedProgramInfo = 5;
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            TimeZone = TimeZone;

            TimeZoneName = TimeZoneName + joinOffset;
            IOControllerVersion = IOControllerVersion + joinOffset;
            SnmpAppVersion = SnmpAppVersion + joinOffset;
            BACnetAppVersion = BACnetAppVersion + joinOffset;
            ControllerVersion = ControllerVersion + joinOffset;

            // Sets the initial join value where the iterated program joins will begin
            ProgramStartJoin = ProgramStartJoin + joinOffset;
        }
    }
}