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
            tx.HdcpSupportAllFeedback.LinkInputSig(trilist.UShortInput[joinMap.HdcpSupportCapability]);
            //trilist.SetUShortSigAction(joinMap.Port1HdcpState,
            //    new Action<ushort>(u => tx.SetPortHdcpSupport((ePdtHdcpSupport)u)));

            //trilist.SetUShortSigAction(joinMap.Port2HdcpState,
            //    new Action<ushort>(u => tx.SetHdcpSupportAll((ePdtHdcpSupport)u)));
            if (tx is ITxRouting)
            {
                trilist.SetUShortSigAction(joinMap.VideoInput,
                    new Action<ushort>(i => (tx as ITxRouting).ExecuteNumericSwitch(i, 0, eRoutingSignalType.Video)));
                trilist.SetUShortSigAction(joinMap.AudioInput,
                    new Action<ushort>(i => (tx as ITxRouting).ExecuteNumericSwitch(i, 0, eRoutingSignalType.Audio)));

                (tx as ITxRouting).VideoSourceNumericFeedback.LinkInputSig(trilist.UShortInput[joinMap.VideoInput]);
                (tx as ITxRouting).AudioSourceNumericFeedback.LinkInputSig(trilist.UShortInput[joinMap.AudioInput]);
            }
        }

        public class DmTxControllerJoinMap : JoinMapBase
        {
            public uint IsOnline { get; set; }
            public uint VideoSyncStatus { get; set; }
            public uint CurrentInputResolution { get; set; }
            public uint HdcpSupportCapability { get; set; }
            public uint VideoInput { get; set; }
            public uint AudioInput { get; set; }
            public uint Port1HdcpState { get; set; }
            public uint Port2HdcpState { get; set; }


            public DmTxControllerJoinMap()
            {
                // Digital
                IsOnline = 1;
                VideoSyncStatus = 2;
                // Serial
                CurrentInputResolution = 1;
                // Analog
                VideoInput = 1;
                AudioInput = 2;
                HdcpSupportCapability = 3;
                Port1HdcpState = 4;
                Port2HdcpState = 5;

            }

            public override void OffsetJoinNumbers(uint joinStart)
            {
                var joinOffset = joinStart - 1;

                IsOnline = IsOnline + joinOffset;
                VideoSyncStatus = VideoSyncStatus + joinOffset;
                CurrentInputResolution = CurrentInputResolution + joinOffset;
                VideoInput = VideoInput + joinOffset;
                AudioInput = AudioInput + joinOffset;
                HdcpSupportCapability = HdcpSupportCapability + joinOffset;
                Port1HdcpState = Port1HdcpState + joinOffset;
                Port2HdcpState = Port2HdcpState + joinOffset;
            }
        }
    }
}