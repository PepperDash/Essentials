//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro.DeviceSupport;
//using PepperDash.Core;
//using PepperDash.Essentials.Core;
//using PepperDash.Essentials.Devices.Common;

//namespace PepperDash.Essentials.Bridges
//{
//    public static class DigitalLoggerApiExtensions
//    {
//        public static void LinkToApi(this DigitalLogger DigitalLogger, BasicTriList trilist, uint joinStart, string joinMapKey)
//        {
//            var joinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as DigitalLoggerJoinMap;

//            if (joinMap == null)
//                joinMap = new DigitalLoggerJoinMap();

//            joinMap.OffsetJoinNumbers(joinStart);
//            Debug.Console(1, DigitalLogger, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
//            for (uint i = 1; i <= DigitalLogger.CircuitCount; i++)
//            {
//                var circuit = i;
//                DigitalLogger.CircuitNameFeedbacks[circuit - 1].LinkInputSig(trilist.StringInput[joinMap.CircuitNames + circuit]);
//                DigitalLogger.CircuitIsCritical[circuit - 1].LinkInputSig(trilist.BooleanInput[joinMap.CircuitIsCritical + circuit]);
//                DigitalLogger.CircuitState[circuit - 1].LinkInputSig(trilist.BooleanInput[joinMap.CircuitState + circuit]);
//                trilist.SetSigTrueAction(joinMap.CircuitCycle + circuit, () => DigitalLogger.CycleCircuit(circuit - 1));
//                trilist.SetSigTrueAction(joinMap.CircuitOnCmd + circuit, () => DigitalLogger.TurnOnCircuit(circuit - 1));
//                trilist.SetSigTrueAction(joinMap.CircuitOffCmd + circuit, () => DigitalLogger.TurnOffCircuit(circuit - 1));

//            }
//        }
//    }
//    public class DigitalLoggerJoinMap : JoinMapBase
//    {
//        public uint IsOnline { get; set; }
//        public uint CircuitNames { get; set; }
//        public uint CircuitState { get; set; }
//        public uint CircuitCycle { get; set; }
//        public uint CircuitIsCritical { get; set; }
//        public uint CircuitOnCmd { get; set; }
//        public uint CircuitOffCmd { get; set; }
//        public DigitalLoggerJoinMap()
//        {
//            // Digital
//            IsOnline = 9;
//            CircuitState = 0;
//            CircuitCycle = 0;
//            CircuitIsCritical = 10;
//            CircuitOnCmd = 10;
//            CircuitOffCmd = 20;
//            // Serial
//            CircuitNames = 0;
//            // Analog
//        }

//        public override void OffsetJoinNumbers(uint joinStart)
//        {
//            var joinOffset = joinStart - 1;

//            IsOnline = IsOnline + joinOffset;
//            CircuitNames = CircuitNames + joinOffset;
//            CircuitState = CircuitState + joinOffset;
//            CircuitCycle = CircuitCycle + joinOffset;
//            CircuitIsCritical = CircuitIsCritical + joinOffset;
//            CircuitOnCmd = CircuitOnCmd + joinOffset;
//            CircuitOffCmd = CircuitOffCmd + joinOffset;



//        }
//    }

//}