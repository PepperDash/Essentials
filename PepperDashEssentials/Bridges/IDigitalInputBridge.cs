using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.CrestronIO;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Bridges
{
    public static class IDigitalInputApiExtenstions
    {
        public static void LinkToApi(this IDigitalInput input, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            IDigitalInputJoinMap joinMap = new IDigitalInputJoinMap();

            var joinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<IDigitalInputJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

            try
            {
                Debug.Console(1, input as Device, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

                // Link feedback for input state
                input.InputStateFeedback.LinkInputSig(trilist.BooleanInput[joinMap.InputState]);
            }
            catch (Exception e)
            {
                Debug.Console(1, input as Device, "Unable to link device '{0}'.  Input is null", (input as Device).Key);
                Debug.Console(1, input as Device, "Error: {0}", e);
                return;
            }
        }
    } 
}