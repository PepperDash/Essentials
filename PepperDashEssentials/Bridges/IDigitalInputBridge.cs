using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core.CrestronIO;

namespace PepperDash.Essentials.Bridges
{
    public static class IDigitalInputApiExtenstions
    {
        public static void LinkToApi(this IDigitalInput input, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            var joinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as IDigitalInputApiJoinMap;

            if (joinMap == null)
                joinMap = new IDigitalInputApiJoinMap();

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

    public class IDigitalInputApiJoinMap : JoinMapBase
    {
        //Digital
        public uint InputState { get; set; }

        public IDigitalInputApiJoinMap()
        {
            InputState = 1;
        }

        public override void  OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            InputState = InputState + joinOffset;
        }
    }
}