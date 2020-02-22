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

using Newtonsoft.Json;

namespace PepperDash.Essentials.Bridges
{
    public static class DmChassisControllerApiExtentions
    {
        public static void LinkToApi(this DmChassisController dmChassis, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            DmChassisControllerJoinMap joinMap = new DmChassisControllerJoinMap();

            var joinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<DmChassisControllerJoinMap>(joinMapSerialized);
            

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, dmChassis, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            var chassis = dmChassis.Chassis as DmMDMnxn;

            dmChassis.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);

            trilist.SetUShortSigAction(joinMap.SystemId, new Action<ushort>(o => chassis.SystemId.UShortValue = o));
            trilist.SetSigTrueAction(joinMap.SystemId, new Action(() => chassis.ApplySystemId()));

            dmChassis.SystemIdFeebdack.LinkInputSig(trilist.UShortInput[joinMap.SystemId]);
            dmChassis.SystemIdBusyFeedback.LinkInputSig(trilist.BooleanInput[joinMap.SystemId]);

            // Link up outputs
            for (uint i = 1; i <= dmChassis.Chassis.NumberOfOutputs; i++)
            {
                var ioSlot = i;

                // Control
                trilist.SetUShortSigAction(joinMap.OutputVideo + ioSlot, new Action<ushort>(o => dmChassis.ExecuteSwitch(o, ioSlot, eRoutingSignalType.Video)));
                trilist.SetUShortSigAction(joinMap.OutputAudio + ioSlot, new Action<ushort>(o => dmChassis.ExecuteSwitch(o, ioSlot, eRoutingSignalType.Audio)));
                trilist.SetUShortSigAction(joinMap.OutputUsb + ioSlot, new Action<ushort>(o => dmChassis.ExecuteSwitch(o, ioSlot, eRoutingSignalType.UsbOutput)));
                trilist.SetUShortSigAction(joinMap.InputUsb + ioSlot, new Action<ushort>(o => dmChassis.ExecuteSwitch(o, ioSlot, eRoutingSignalType.UsbInput)));

                if (dmChassis.TxDictionary.ContainsKey(ioSlot))
                {
                    Debug.Console(2, "Creating Tx Feedbacks {0}", ioSlot);
                    var txKey = dmChassis.TxDictionary[ioSlot];
                    var basicTxDevice = DeviceManager.GetDeviceForKey(txKey) as BasicDmTxControllerBase;

                    var advancedTxDevice = basicTxDevice as DmTxControllerBase;

                    if (dmChassis.Chassis is DmMd8x8Cpu3 || dmChassis.Chassis is DmMd8x8Cpu3rps
                        || dmChassis.Chassis is DmMd16x16Cpu3 || dmChassis.Chassis is DmMd16x16Cpu3rps
                        || dmChassis.Chassis is DmMd32x32Cpu3 || dmChassis.Chassis is DmMd32x32Cpu3rps)
                    {
                        dmChassis.InputEndpointOnlineFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.InputEndpointOnline + ioSlot]);
                    }
                    else
                    {
                        if (advancedTxDevice != null)
                        {
                            advancedTxDevice.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.InputEndpointOnline + ioSlot]);
                            Debug.Console(2, "Linking Tx Online Feedback from Advanced Transmitter at input {0}", ioSlot);
                        }
                        else if (dmChassis.InputEndpointOnlineFeedbacks[ioSlot] != null)
                        {
                            Debug.Console(2, "Linking Tx Online Feedback from Input Card {0}", ioSlot);
                            dmChassis.InputEndpointOnlineFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.InputEndpointOnline + ioSlot]);                           
                        }
                    }

                    if (basicTxDevice != null && advancedTxDevice == null)
                        trilist.BooleanInput[joinMap.TxAdvancedIsPresent + ioSlot].BoolValue = true;

                    if (advancedTxDevice != null)
                    {
                        advancedTxDevice.AnyVideoInput.VideoStatus.VideoSyncFeedback.LinkInputSig(trilist.BooleanInput[joinMap.VideoSyncStatus + ioSlot]);
                    }
                    else if(advancedTxDevice == null || basicTxDevice != null)
                    {
                        Debug.Console(1, "Setting up actions and feedbacks on input card {0}", ioSlot);
                        dmChassis.VideoInputSyncFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.VideoSyncStatus + ioSlot]);

                        var inputPort = dmChassis.InputPorts[string.Format("inputCard{0}--hdmiIn", ioSlot)];
                        if (inputPort != null)
                        {
                            Debug.Console(1, "Port value for input card {0} is set", ioSlot);
                            var port = inputPort.Port;

                            if (port != null)
                            {
                                if (port is HdmiInputWithCEC)
                                {
                                    Debug.Console(1, "Port is HdmiInputWithCec");

                                    var hdmiInPortWCec = port as HdmiInputWithCEC;

                                    if (hdmiInPortWCec.HdcpSupportedLevel != eHdcpSupportedLevel.Unknown)
                                    {
                                        SetHdcpStateAction(true, hdmiInPortWCec, joinMap.HdcpSupportState + ioSlot, trilist);
                                    }

                                    dmChassis.InputCardHdcpCapabilityFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.HdcpSupportState + ioSlot]);

                                    if(dmChassis.InputCardHdcpCapabilityTypes.ContainsKey(ioSlot))
                                        trilist.UShortInput[joinMap.HdcpSupportCapability + ioSlot].UShortValue = (ushort)dmChassis.InputCardHdcpCapabilityTypes[ioSlot];
                                    else
                                        trilist.UShortInput[joinMap.HdcpSupportCapability + ioSlot].UShortValue = 1;
                                }
                            }
                        }
                        else
                        {
                            inputPort = dmChassis.InputPorts[string.Format("inputCard{0}--dmIn", ioSlot)];

                            if(inputPort != null)
                            {
                                var port = inputPort.Port;

                                if (port is DMInputPortWithCec)
                                {
                                    Debug.Console(1, "Port is DMInputPortWithCec");

                                    var dmInPortWCec = port as DMInputPortWithCec;

                                    if (dmInPortWCec != null && dmChassis.PropertiesConfig.InputSlotSupportsHdcp2 != null)
                                    {
                                        if (dmChassis.PropertiesConfig.InputSlotSupportsHdcp2 != null)
                                        {
                                            bool inputSlotSupportsHdcp2;
                                            if (dmChassis.PropertiesConfig.InputSlotSupportsHdcp2.TryGetValue(ioSlot, out inputSlotSupportsHdcp2))
                                            {
                                                SetHdcpStateAction(inputSlotSupportsHdcp2, dmInPortWCec, joinMap.HdcpSupportState + ioSlot, trilist);
                                            }
                                            else
                                            {
                                                Debug.Console(2, dmChassis, "It looks like you haven't defined whether InputSlot-{0} supports HDCP2 in Config...", ioSlot);
                                            }
                                        }
                                        else
                                        {
                                            Debug.Console(2, dmChassis, "It looks like you haven't defined whether your inputSlots support HDCP2 in Config...", ioSlot);
                                        }
                                    }

                                    dmChassis.InputCardHdcpCapabilityFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.HdcpSupportState + ioSlot]);

                                    if (dmChassis.InputCardHdcpCapabilityTypes.ContainsKey(ioSlot))
                                        trilist.UShortInput[joinMap.HdcpSupportCapability + ioSlot].UShortValue = (ushort)dmChassis.InputCardHdcpCapabilityTypes[ioSlot];
                                    else
                                        trilist.UShortInput[joinMap.HdcpSupportCapability + ioSlot].UShortValue = 1;
                                }
                            }
                        }
                    }
                }
                else
                {
                    dmChassis.VideoInputSyncFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.VideoSyncStatus + ioSlot]);

                    var inputPort = dmChassis.InputPorts[string.Format("inputCard{0}--hdmiIn", ioSlot)];
                    if (inputPort != null)
                    {
                        var hdmiPort = inputPort.Port as EndpointHdmiInput;

                        if (hdmiPort != null)
                        {
                            SetHdcpStateAction(true, hdmiPort, joinMap.HdcpSupportState + ioSlot, trilist);
                            dmChassis.InputCardHdcpCapabilityFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.HdcpSupportState + ioSlot]);
                        }
                    }
                }
                if (dmChassis.RxDictionary.ContainsKey(ioSlot))
                {
                    Debug.Console(2, "Creating Rx Feedbacks {0}", ioSlot);
                    var rxKey = dmChassis.RxDictionary[ioSlot];
                    var rxDevice = DeviceManager.GetDeviceForKey(rxKey) as DmRmcControllerBase;
                    var hdBaseTDevice = DeviceManager.GetDeviceForKey(rxKey) as DmHdBaseTControllerBase;
                    if (dmChassis.Chassis is DmMd8x8Cpu3 || dmChassis.Chassis is DmMd8x8Cpu3rps
                        || dmChassis.Chassis is DmMd16x16Cpu3 || dmChassis.Chassis is DmMd16x16Cpu3rps
                        || dmChassis.Chassis is DmMd32x32Cpu3 || dmChassis.Chassis is DmMd32x32Cpu3rps || hdBaseTDevice != null)
                    {
                        dmChassis.OutputEndpointOnlineFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.OutputEndpointOnline + ioSlot]);
                    }
                    else if (rxDevice != null)
                    {
                        rxDevice.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.OutputEndpointOnline + ioSlot]);
                    }          
                }

                // Feedback
                dmChassis.VideoOutputFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.OutputVideo + ioSlot]);
                dmChassis.AudioOutputFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.OutputAudio + ioSlot]);
                dmChassis.UsbOutputRoutedToFeebacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.OutputUsb + ioSlot]);
                dmChassis.UsbInputRoutedToFeebacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.InputUsb + ioSlot]);


                dmChassis.OutputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputNames + ioSlot]);
                dmChassis.InputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.InputNames + ioSlot]);
                dmChassis.OutputVideoRouteNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputCurrentVideoInputNames + ioSlot]);
                dmChassis.OutputAudioRouteNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputCurrentAudioInputNames + ioSlot]);
            }
        }

        static void SetHdcpStateAction(bool hdcpTypeSimple, HdmiInputWithCEC port, uint join, BasicTriList trilist)
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
                        new Action<ushort>(u =>
                        {
                            port.HdcpReceiveCapability = (eHdcpCapabilityType)u;
                        }));
            }
        }

        static void SetHdcpStateAction(bool hdcpTypeSimple, EndpointHdmiInput port, uint join, BasicTriList trilist)
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
                        new Action<ushort>(u =>
                        {
                            port.HdcpCapability = (eHdcpCapabilityType)u;
                        }));
            }
        }

        static void SetHdcpStateAction(bool supportsHdcp2, DMInputPortWithCec port, uint join, BasicTriList trilist)
        {
            if (!supportsHdcp2)
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
                        new Action<ushort>(u =>
                        {
                            port.HdcpReceiveCapability = (eHdcpCapabilityType)u;
                        }));
            }
        }

    }
}