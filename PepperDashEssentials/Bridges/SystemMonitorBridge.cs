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




            foreach (var p in SystemMonitor.ProgramCollection)
            {
                
            }
        }
    }

    public class SystemMonitorJoinMap : JoinMapBase
    {
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
        public uint SNMPAppVersion { get; set; }
        public uint BACnetAppVersion { get; set; }
        public uint ControllerVersion { get; set; }

        public uint ProgramName { get; set; }
        public uint ProgramCompiledTime { get; set; }
        public uint ProgramCrestronDatabaseVersion { get; set; }
        public uint ProgramEnvironmentVersion { get; set; }


        public SystemMonitorJoinMap()
        {
            TimeZone = 1;

            TimeZoneName = 1;
            IOControllerVersion = 2;
            SNMPAppVersion = 3;
            BACnetAppVersion = 4;
            ControllerVersion = 5;

            ProgramStart = 11;
            ProgramStop = 12;
            ProgramRegister = 13;
            ProgramUnregister = 14;

            ProgramName = 11;
            ProgramCompiledTime = 12;
            ProgramCrestronDatabaseVersion = 13;
            ProgramEnvironmentVersion = 14;
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            TimeZone = TimeZone;

            TimeZoneName = TimeZoneName + joinOffset;
            IOControllerVersion = IOControllerVersion + joinOffset;
            SNMPAppVersion = SNMPAppVersion + joinOffset;
            BACnetAppVersion = BACnetAppVersion + joinOffset;
            ControllerVersion = ControllerVersion + joinOffset;

            ProgramStart = ProgramStart + joinOffset;
            ProgramStop = ProgramStop + joinOffset;
            ProgramRegister = ProgramRegister + joinOffset;
            ProgramUnregister = ProgramUnregister;

            ProgramName = ProgramName + joinOffset;
            ProgramCompiledTime = ProgramCompiledTime + joinOffset;
            ProgramCrestronDatabaseVersion = ProgramCrestronDatabaseVersion + joinOffset;
            ProgramEnvironmentVersion = ProgramEnvironmentVersion + joinOffset;
        }
    }
}