using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.DM;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Bridges
{
    public static class HdMdxxxCEControllerApiExtensions
    {
        public static void LinkToApi(this HdMdxxxCEController hdMdPair, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            HdMdxxxCEControllerJoinMap joinMap = new HdMdxxxCEControllerJoinMap();

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<HdMdxxxCEControllerJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, hdMdPair, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            hdMdPair.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
            hdMdPair.RemoteEndDetectedFeedback.LinkInputSig(trilist.BooleanInput[joinMap.RemoteEndDetected]);

            trilist.SetSigTrueAction(joinMap.AutoRouteOn, new Action(() => hdMdPair.AutoRouteOn()));
            hdMdPair.AutoRouteOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.AutoRouteOn]);
            trilist.SetSigTrueAction(joinMap.AutoRouteOff, new Action(() => hdMdPair.AutoRouteOff()));
            hdMdPair.AutoRouteOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.AutoRouteOff]);

            trilist.SetSigTrueAction(joinMap.PriorityRoutingOn, new Action(() => hdMdPair.PriorityRouteOn()));
            hdMdPair.PriorityRoutingOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PriorityRoutingOn]);
            trilist.SetSigTrueAction(joinMap.PriorityRoutingOff, new Action(() => hdMdPair.PriorityRouteOff()));
            hdMdPair.PriorityRoutingOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.PriorityRoutingOff]);

            trilist.SetSigTrueAction(joinMap.InputOnScreenDisplayEnabled, new Action(() => hdMdPair.OnScreenDisplayEnable()));
            hdMdPair.InputOnScreenDisplayEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.InputOnScreenDisplayEnabled]);
            trilist.SetSigTrueAction(joinMap.AutoRouteOff, new Action(() => hdMdPair.OnScreenDisplayDisable()));
            hdMdPair.InputOnScreenDisplayEnabledFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.InputOnScreenDisplayDisabled]);

            trilist.SetUShortSigAction(joinMap.VideoSource, new Action<ushort>((i) => hdMdPair.ExecuteSwitch(i, null, eRoutingSignalType.Video | eRoutingSignalType.Audio)));
            hdMdPair.VideoSourceFeedback.LinkInputSig(trilist.UShortInput[joinMap.VideoSource]);

            trilist.UShortInput[joinMap.SourceCount].UShortValue = (ushort)hdMdPair.InputPorts.Count;

            foreach (var input in hdMdPair.InputPorts)
            {
                var number = Convert.ToUInt16(input.Selector);
                hdMdPair.SyncDetectedFeedbacks[number].LinkInputSig(trilist.BooleanInput[joinMap.SyncDetected + number]);
                trilist.StringInput[joinMap.SourceNames + number].StringValue = input.Key;
            }

        } 
    }
}