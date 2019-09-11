using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Bridges
{
    public static class DigitalLoggerApiExtensions
    {
        public static void LinkToApi(this DigitalLogger DigitalLogger, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            DigitalLoggerJoinMap joinMap = new DigitalLoggerJoinMap();

            var joinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<DigitalLoggerJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, DigitalLogger, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            for (uint i = 1; i <= DigitalLogger.CircuitCount; i++)
            {
                var circuit = i;
                DigitalLogger.CircuitNameFeedbacks[circuit - 1].LinkInputSig(trilist.StringInput[joinMap.CircuitNames + circuit]);
                DigitalLogger.CircuitIsCritical[circuit - 1].LinkInputSig(trilist.BooleanInput[joinMap.CircuitIsCritical + circuit]);
                DigitalLogger.CircuitState[circuit - 1].LinkInputSig(trilist.BooleanInput[joinMap.CircuitState + circuit]);
                trilist.SetSigTrueAction(joinMap.CircuitCycle + circuit, () => DigitalLogger.CycleCircuit(circuit - 1));
                trilist.SetSigTrueAction(joinMap.CircuitOnCmd + circuit, () => DigitalLogger.TurnOnCircuit(circuit - 1));
                trilist.SetSigTrueAction(joinMap.CircuitOffCmd + circuit, () => DigitalLogger.TurnOffCircuit(circuit - 1));

            }
        }
    }
}