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
            var joinMap = new SystemMonitorJoinMap();

            var joinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

            if(!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<SystemMonitorJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.Console(2, systemMonitorController, "Linking API starting at join: {0}", joinStart);

            systemMonitorController.TimeZoneFeedback.LinkInputSig(trilist.UShortInput[joinMap.TimeZone]);
            systemMonitorController.TimeZoneTextFeedback.LinkInputSig(trilist.StringInput[joinMap.TimeZoneName]);

            systemMonitorController.IoControllerVersionFeedback.LinkInputSig(trilist.StringInput[joinMap.IOControllerVersion]);
            systemMonitorController.SnmpVersionFeedback.LinkInputSig(trilist.StringInput[joinMap.SnmpAppVersion]);
            systemMonitorController.BaCnetAppVersionFeedback.LinkInputSig(trilist.StringInput[joinMap.BACnetAppVersion]);
            systemMonitorController.ControllerVersionFeedback.LinkInputSig(trilist.StringInput[joinMap.ControllerVersion]);
            systemMonitorController.SerialNumberFeedback.LinkInputSig(trilist.StringInput[joinMap.SerialNumber]);
            systemMonitorController.ModelFeedback.LinkInputSig(trilist.StringInput[joinMap.Model]);
            systemMonitorController.UptimeFeedback.LinkInputSig(trilist.StringInput[joinMap.Uptime]);
            systemMonitorController.LastStartFeedback.LinkInputSig(trilist.StringInput[joinMap.LastBoot]);

            // iterate the program status feedback collection and map all the joins
            LinkProgramInfoJoins(systemMonitorController, trilist, joinMap);

            LinkEthernetInfoJoins(systemMonitorController, trilist, joinMap);
        }

        private static void LinkEthernetInfoJoins(SystemMonitorController systemMonitorController, BasicTriList trilist, SystemMonitorJoinMap joinMap)
        {
            var ethernetSlotJoinStart = joinMap.EthernetStartJoin;

            foreach (var fb in systemMonitorController.EthernetStatusFeedbackCollection)
            {
                fb.Value.CurrentIpAddressFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.CurrentIpAddress]);
                fb.Value.CurrentSubnetMaskFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.CurrentSubnetMask]);
                fb.Value.CurrentDefaultGatewayFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.CurrentDefaultGateway]);
                fb.Value.StaticIpAddressFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.StaticIpAddress]);
                fb.Value.StaticSubnetMaskFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.StaticSubnetMask]);
                fb.Value.StaticDefaultGatewayFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.StaticDefaultGateway]);
                fb.Value.HostNameFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.HostName]);
                fb.Value.MacAddressFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.MacAddress]);
                fb.Value.DomainFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.Domain]);
                fb.Value.DnsServerFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.DnsServer]);
                fb.Value.DhcpStatusFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.DhcpStatus]);

                ethernetSlotJoinStart += joinMap.EthernetOffsetJoin;
            }
        }

        private static void LinkProgramInfoJoins(SystemMonitorController systemMonitorController, BasicTriList trilist,
            SystemMonitorJoinMap joinMap)
        {
            var programSlotJoinStart = joinMap.ProgramStartJoin;

            foreach (var p in systemMonitorController.ProgramStatusFeedbackCollection)
            {
                var programNumber = p.Value.Program.Number;

                trilist.SetBoolSigAction(programSlotJoinStart + joinMap.ProgramStart,
                    b => SystemMonitor.ProgramCollection[programNumber].OperatingState = eProgramOperatingState.Start);
                p.Value.ProgramStartedFeedback.LinkInputSig(trilist.BooleanInput[programSlotJoinStart + joinMap.ProgramStart]);

                trilist.SetBoolSigAction(programSlotJoinStart + joinMap.ProgramStop,
                    b => SystemMonitor.ProgramCollection[programNumber].OperatingState = eProgramOperatingState.Stop);
                p.Value.ProgramStoppedFeedback.LinkInputSig(trilist.BooleanInput[programSlotJoinStart + joinMap.ProgramStop]);

                trilist.SetBoolSigAction(programSlotJoinStart + joinMap.ProgramRegister,
                    b => SystemMonitor.ProgramCollection[programNumber].RegistrationState = eProgramRegistrationState.Register);
                p.Value.ProgramRegisteredFeedback.LinkInputSig(
                    trilist.BooleanInput[programSlotJoinStart + joinMap.ProgramRegister]);

                trilist.SetBoolSigAction(programSlotJoinStart + joinMap.ProgramUnregister,
                    b => SystemMonitor.ProgramCollection[programNumber].RegistrationState = eProgramRegistrationState.Unregister);
                p.Value.ProgramUnregisteredFeedback.LinkInputSig(
                    trilist.BooleanInput[programSlotJoinStart + joinMap.ProgramUnregister]);

                p.Value.ProgramNameFeedback.LinkInputSig(trilist.StringInput[programSlotJoinStart + joinMap.ProgramName]);
                p.Value.ProgramCompileTimeFeedback.LinkInputSig(
                    trilist.StringInput[programSlotJoinStart + joinMap.ProgramCompiledTime]);
                p.Value.CrestronDataBaseVersionFeedback.LinkInputSig(
                    trilist.StringInput[programSlotJoinStart + joinMap.ProgramCrestronDatabaseVersion]);
                p.Value.EnvironmentVersionFeedback.LinkInputSig(
                    trilist.StringInput[programSlotJoinStart + joinMap.ProgramEnvironmentVersion]);
                p.Value.AggregatedProgramInfoFeedback.LinkInputSig(
                    trilist.StringInput[programSlotJoinStart + joinMap.AggregatedProgramInfo]);

                programSlotJoinStart = programSlotJoinStart + joinMap.ProgramOffsetJoin;
            }
        }
    }
}