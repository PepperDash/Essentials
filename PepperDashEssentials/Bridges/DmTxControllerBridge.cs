using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.DM;

namespace PepperDash.Essentials.Bridges
{
    public static class DmTxControllerApiExtensions
    {
        public static void LinkToApi(this DmTxControllerBase tx, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            var joinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as DmTxControllerJoinMap;

            if (joinMap == null)
                joinMap = new DmTxControllerJoinMap();

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, tx, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            tx.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
            tx.AnyVideoInput.VideoStatus.VideoSyncFeedback.LinkInputSig(trilist.BooleanInput[joinMap.VideoSyncStatus]);
            tx.AnyVideoInput.VideoStatus.VideoResolutionFeedback.LinkInputSig(trilist.StringInput[joinMap.CurrentInputResolution]);
            tx.HdcpSupportAllFeedback.LinkInputSig(trilist.UShortInput[joinMap.HdcpSupportState]);
            trilist.SetBoolSigAction(joinMap.HdcpSupportOn, 
                new Action<bool>(b => tx.SetHdcpSupportAll(ePdtHdcpSupport.Auto)));
            tx.HdcpSupportAllFeedback.LinkInputSig(trilist.UShortInput[joinMap.HdcpSupportOn]);
            trilist.SetBoolSigAction(joinMap.HdcpSupportOff, 
                new Action<bool>(b => tx.SetHdcpSupportAll(ePdtHdcpSupport.HdcpOff)));
            if (tx is ITxRouting)
            {
                trilist.SetUShortSigAction(joinMap.VideoInput,
                    new Action<ushort>(i => (tx as ITxRouting).ExecuteNumericSwitch(i, 0, eRoutingSignalType.Video)));
                trilist.SetUShortSigAction(joinMap.AudioInput,
                    new Action<ushort>(i => (tx as ITxRouting).ExecuteNumericSwitch(i, 0, eRoutingSignalType.Audio)));

                (tx as ITxRouting).VideoSourceNumericFeedback.LinkInputSig(trilist.UShortInput[joinMap.VideoInput]);
                (tx as ITxRouting).AudioSourceNumericFeedabck.LinkInputSig(trilist.UShortInput[joinMap.AudioInput]);
            }
        }

        public class DmTxControllerJoinMap : JoinMapBase
        {
            public uint IsOnline { get; set; }
            public uint VideoSyncStatus { get; set; }
            public uint CurrentInputResolution { get; set; }
            public uint HdcpSupportOn { get; set; }
            public uint HdcpSupportOff { get; set; }
            public uint HdcpSupportState { get; set; }
            public uint VideoInput { get; set; }
            public uint AudioInput { get; set; }

            public DmTxControllerJoinMap()
            {
                // Digital
                IsOnline = 1;
                VideoSyncStatus = 2;
                HdcpSupportOn = 3;
                HdcpSupportOff = 4;
                // Serial
                CurrentInputResolution = 1;
                // Analog
                VideoInput = 1;
                AudioInput = 2;
                HdcpSupportState = 3;
            }

            public override void OffsetJoinNumbers(uint joinStart)
            {
                var joinOffset = joinStart - 1;

                IsOnline = IsOnline + joinOffset;
                VideoSyncStatus = VideoSyncStatus + joinOffset;
                HdcpSupportOn = HdcpSupportOn + joinOffset;
                HdcpSupportOff = HdcpSupportOff + joinOffset;
                CurrentInputResolution = CurrentInputResolution + joinOffset;
                VideoInput = VideoInput + joinOffset;
                AudioInput = AudioInput + joinOffset;
                HdcpSupportState = HdcpSupportState + joinOffset;
            }
        }
    }
}