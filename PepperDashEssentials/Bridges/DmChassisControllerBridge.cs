using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.DM;

namespace PepperDash.Essentials.Bridges
{
    public static class DmChassisControllerApiExtentions 
    {
        public static void LinkToApi(this DmChassisController dmChassis, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            var joinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as DmChassisControllerJoinMap;

            if (joinMap == null)
                joinMap = new DmChassisControllerJoinMap();

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, dmChassis, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            dmChassis.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);

            // Link up outputs
            for (uint i = 1; i <= dmChassis.Chassis.NumberOfOutputs; i++)
            {
                var ioSlot = i;

                // Control
                trilist.SetUShortSigAction(joinMap.OutputVideo + ioSlot, new Action<ushort>(o => dmChassis.ExecuteSwitch(o, ioSlot, eRoutingSignalType.Video)));
                trilist.SetUShortSigAction(joinMap.OutputAudio + ioSlot, new Action<ushort>(o => dmChassis.ExecuteSwitch(o, ioSlot, eRoutingSignalType.Audio)));
				if (dmChassis.TxDictionary.ContainsKey(ioSlot))
				{
					Debug.Console(2, "Creating Tx Feedbacks {0}", ioSlot);
					var txKey = dmChassis.TxDictionary[ioSlot];
                    var basicTxDevice = DeviceManager.GetDeviceForKey(txKey) as BasicDmTxControllerBase;

					var txDevice = basicTxDevice as DmTxControllerBase;

                    if (dmChassis.Chassis is DmMd8x8Cpu3 || dmChassis.Chassis is DmMd8x8Cpu3rps
                        || dmChassis.Chassis is DmMd16x16Cpu3 || dmChassis.Chassis is DmMd16x16Cpu3rps
                        || dmChassis.Chassis is DmMd32x32Cpu3 || dmChassis.Chassis is DmMd32x32Cpu3rps)
                    {
                        dmChassis.InputEndpointOnlineFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.InputEndpointOnline + ioSlot]);
                    }
                    else
                    {
                        if (txDevice != null)
                        {
                            txDevice.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.InputEndpointOnline + ioSlot]);
                        }
                    }

                    if(basicTxDevice != null && txDevice == null)
                        trilist.BooleanInput[joinMap.TxAdvancedIsPresent + ioSlot].BoolValue = true;


                    if (txDevice != null)
                    {
                        bool hdcpTypeSimple;

                        if (txDevice.Hardware is DmTx4kX02CBase || txDevice.Hardware is DmTx4kzX02CBase)
                            hdcpTypeSimple = false;
                        else
                            hdcpTypeSimple = true;

                        txDevice.AnyVideoInput.VideoStatus.VideoSyncFeedback.LinkInputSig(trilist.BooleanInput[joinMap.InputEndpointOnline + ioSlot]);
                        //TxDevice.SetPortHdcpCapability((eHdcpCapabilityType)(u),));

                        trilist.UShortInput[joinMap.HdcpSupportCapability + ioSlot].UShortValue = (ushort)txDevice.HdcpSupportCapability;

                        if (txDevice is ITxRouting)
                        {
                            var txR = txDevice as ITxRouting;

                            if (hdcpTypeSimple)
                            {

                                if (txR.InputPorts[DmPortName.HdmiIn] != null)
                                {
                                    var inputPort = txR.InputPorts[DmPortName.HdmiIn];

                                    if (txDevice.Feedbacks["HdmiInHdcpCapability"] != null)
                                        (txDevice.Feedbacks["HdmiInHdcpCapability"] as IntFeedback).LinkInputSig(trilist.UShortInput[joinMap.HdcpSupportState + ioSlot]);

                                    if (inputPort.ConnectionType == eRoutingPortConnectionType.Hdmi && inputPort.Port != null)
                                    {
                                        var port = inputPort.Port as EndpointHdmiInput;

                                        SetHdcpCapabilityAction(hdcpTypeSimple, port, joinMap.HdcpSupportState + ioSlot, trilist);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var inputPort = dmChassis.InputPorts[string.Format("inputCard{0}--hdmiIn", ioSlot)];
                        if(inputPort != null)
                        {
                            var hdmiInPort = inputPort.Port;

                            if (hdmiInPort != null)
                            {
                                if (hdmiInPort is HdmiInputWithCEC)
                                {
                                    var hdmiInPortWCec = hdmiInPort as HdmiInputWithCEC;

                                    if (hdmiInPortWCec.HdcpSupportedLevel != eHdcpSupportedLevel.Unknown)
                                        SetHdcpCapabilityAction(true, hdmiInPortWCec, joinMap.HdcpSupportState + ioSlot, trilist);

                                    //TODO: Figure out how the heck to get feedback properly.
                                }

                            }
                        }

                    }
				}
				else
				{
					dmChassis.VideoInputSyncFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.VideoSyncStatus + ioSlot]);

                    var inputPort = dmChassis.InputPorts[string.Format("inputCard{0}-hdmiIn", ioSlot)].Port as EndpointHdmiInput;

                    if (inputPort != null)
                    {
                        //trilist.SetUShortSigAction((joinMap.HdcpSupport + ioSlot), u => 
                    }
				}
				if (dmChassis.RxDictionary.ContainsKey(ioSlot))
				{
					Debug.Console(2, "Creating Rx Feedbacks {0}", ioSlot);
					var RxKey = dmChassis.RxDictionary[ioSlot];
					var RxDevice = DeviceManager.GetDeviceForKey(RxKey) as DmRmcControllerBase;
                    if (dmChassis.Chassis is DmMd8x8Cpu3 || dmChassis.Chassis is DmMd8x8Cpu3rps
                        || dmChassis.Chassis is DmMd16x16Cpu3 || dmChassis.Chassis is DmMd16x16Cpu3rps
                        || dmChassis.Chassis is DmMd32x32Cpu3 || dmChassis.Chassis is DmMd32x32Cpu3rps)
                    {
                        dmChassis.OutputEndpointOnlineFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.OutputEndpointOnline + ioSlot]);
                    }
                    else if (RxDevice != null)
                    {
                        RxDevice.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.OutputEndpointOnline + ioSlot]);
                    }
				}
                // Feedback
                dmChassis.VideoOutputFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.OutputVideo + ioSlot]);
                dmChassis.AudioOutputFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.OutputAudio + ioSlot]);

                 dmChassis.VideoInputSyncFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.VideoSyncStatus + ioSlot]);

                dmChassis.OutputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputNames + ioSlot]);
                dmChassis.InputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.InputNames + ioSlot]);
                dmChassis.OutputVideoRouteNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputCurrentVideoInputNames + ioSlot]);
                dmChassis.OutputAudioRouteNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputCurrentAudioInputNames + ioSlot]);
            }
        }

        static void SetHdcpCapabilityAction(bool hdcpTypeSimple, HdmiInputWithCEC port, uint join, BasicTriList trilist)
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
                            port.HdcpReceiveCapability = (eHdcpCapabilityType)s;
                        }));
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

        public class DmChassisControllerJoinMap : JoinMapBase
        {
            //Digital
            public uint IsOnline { get; set; }
            public uint VideoSyncStatus { get; set; }
            public uint InputEndpointOnline { get; set; }
            public uint OutputEndpointOnline { get; set; }
            public uint TxAdvancedIsPresent { get; set; } // indicates that there is an attached transmitter that should be bridged to be interacted with

            //Analog
            public uint OutputVideo { get; set; }
            public uint OutputAudio { get; set; }
            public uint HdcpSupportState { get; set; }
            public uint HdcpSupportCapability { get; set; }

            //SErial
            public uint InputNames { get; set; }
            public uint OutputNames { get; set; }
            public uint OutputCurrentVideoInputNames { get; set; }
            public uint OutputCurrentAudioInputNames { get; set; }
            public uint InputCurrentResolution { get; set; }



            public DmChassisControllerJoinMap()
            {
                //Digital 
                IsOnline = 11;
                VideoSyncStatus = 100; //101-299
                InputEndpointOnline = 500; //501-699
                OutputEndpointOnline = 700; //701-899
                TxAdvancedIsPresent = 1000; //1001-1199

                //Analog
                OutputVideo = 100; //101-299
                OutputAudio = 300; //301-499
                HdcpSupportState = 1000; //1001-1199
                HdcpSupportCapability = 1200; //1201-1399


                //Serial
                InputNames = 100; //101-299
                OutputNames = 300; //301-499
                OutputCurrentVideoInputNames = 2000; //2001-2199
                OutputCurrentAudioInputNames = 2200; //2201-2399
                InputCurrentResolution = 2400; // 2401-2599


            }

            public override void OffsetJoinNumbers(uint joinStart)
            {
                var joinOffset = joinStart - 1;

                IsOnline = IsOnline + joinOffset;
                OutputVideo = OutputVideo + joinOffset;
                OutputAudio = OutputAudio + joinOffset;
                VideoSyncStatus = VideoSyncStatus + joinOffset;
                InputNames = InputNames + joinOffset;
                OutputNames = OutputNames + joinOffset;
                OutputCurrentVideoInputNames = OutputCurrentVideoInputNames + joinOffset;
                OutputCurrentAudioInputNames = OutputCurrentAudioInputNames + joinOffset;
                InputCurrentResolution = InputCurrentResolution + joinOffset;
                InputEndpointOnline = InputEndpointOnline + joinOffset;
                OutputEndpointOnline = OutputEndpointOnline + joinOffset;
                HdcpSupportState = HdcpSupportState + joinOffset;
                HdcpSupportCapability = HdcpSupportCapability + joinOffset;
            }
        }
    }
}