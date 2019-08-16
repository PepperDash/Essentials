using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
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
            trilist.UShortInput[joinMap.HdcpSupportCapability].UShortValue = (ushort)tx.HdcpSupportCapability;

            bool hdcpTypeSimple;

            if (tx.Hardware is DmTx4kX02CBase || tx.Hardware is DmTx4kzX02CBase)
                hdcpTypeSimple = false;
            else
                hdcpTypeSimple = true;

            if (tx is ITxRouting)
            {
                var txR = tx as ITxRouting;

                trilist.SetUShortSigAction(joinMap.VideoInput,
                    new Action<ushort>(i => txR.ExecuteNumericSwitch(i, 0, eRoutingSignalType.Video)));
                trilist.SetUShortSigAction(joinMap.AudioInput,
                    new Action<ushort>(i => txR.ExecuteNumericSwitch(i, 0, eRoutingSignalType.Audio)));

                txR.VideoSourceNumericFeedback.LinkInputSig(trilist.UShortInput[joinMap.VideoInput]);
                txR.AudioSourceNumericFeedback.LinkInputSig(trilist.UShortInput[joinMap.AudioInput]);

                trilist.UShortInput[joinMap.HdcpSupportCapability].UShortValue = (ushort)tx.HdcpSupportCapability;

                if(txR.InputPorts[DmPortName.HdmiIn] != null)
                {
                    var inputPort = txR.InputPorts[DmPortName.HdmiIn];

                    if (tx.Feedbacks["HdmiInHdcpCapability"] != null)
                        (tx.Feedbacks["HdmiInHdcpCapability"] as IntFeedback).LinkInputSig(trilist.UShortInput[joinMap.Port1HdcpState]);

                    if (inputPort.ConnectionType == eRoutingPortConnectionType.Hdmi && inputPort.Port != null)
                    {
                        var port = inputPort.Port as EndpointHdmiInput;

                        SetHdcpCapabilityAction(hdcpTypeSimple, port, joinMap.Port1HdcpState, trilist);
                    }
                }
                
                if (txR.InputPorts[DmPortName.HdmiIn1] != null)
                {
                    var inputPort = txR.InputPorts[DmPortName.HdmiIn1];

                    if (tx.Feedbacks["HdmiIn1HdcpCapability"] != null)
                        (tx.Feedbacks["HdmiIn1HdcpCapability"] as IntFeedback).LinkInputSig(trilist.UShortInput[joinMap.Port1HdcpState]);

                    if (inputPort.ConnectionType == eRoutingPortConnectionType.Hdmi && inputPort.Port != null)
                    {
                        var port = inputPort.Port as EndpointHdmiInput;

                        SetHdcpCapabilityAction(hdcpTypeSimple, port, joinMap.Port1HdcpState, trilist);
                    }
                }

                if (txR.InputPorts[DmPortName.HdmiIn2] != null)
                {
                    var inputPort = txR.InputPorts[DmPortName.HdmiIn2];

                    if (tx.Feedbacks["HdmiIn2HdcpCapability"] != null)
                        (tx.Feedbacks["HdmiIn2HdcpCapability"] as IntFeedback).LinkInputSig(trilist.UShortInput[joinMap.Port1HdcpState]);

                    if (inputPort.ConnectionType == eRoutingPortConnectionType.Hdmi && inputPort.Port != null)
                    {
                        var port = inputPort.Port as EndpointHdmiInput;

                        SetHdcpCapabilityAction(hdcpTypeSimple, port, joinMap.Port2HdcpState, trilist);
                    }
                }

            }
        }

        static void SetHdcpCapabilityAction(bool hdcpTypeSimple, EndpointHdmiInput port, uint join, BasicTriList trilist)
        {
            if (hdcpTypeSimple)
            {
                trilist.SetUShortSigAction(join,
                    new Action<ushort>(s =>
                    {
                        if (s == 0)
                        {
                            port.HdcpSupportOff();
                        }
                        else if (s > 0)
                        {
                            port.HdcpSupportOn();
                        }
                    }));
            }
            else
            {
                trilist.SetUShortSigAction(join,
                        new Action<ushort>(s =>
                        {
                            port.HdcpCapability = (eHdcpCapabilityType)s;
                        }));
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