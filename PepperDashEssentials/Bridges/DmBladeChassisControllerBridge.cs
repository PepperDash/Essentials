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

namespace PepperDash.Essentials.Bridges {
    public static class DmBladeChassisControllerApiExtentions {
        public static void LinkToApi(this DmBladeChassisController dmChassis, BasicTriList trilist, uint joinStart, string joinMapKey) {
            DmBladeChassisControllerJoinMap joinMap = new DmBladeChassisControllerJoinMap();

            var joinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<DmBladeChassisControllerJoinMap>(joinMapSerialized);


            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, dmChassis, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            var chassis = dmChassis.Chassis as BladeSwitch;

            dmChassis.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);


            // Link up outputs
            for (uint i = 1; i <= dmChassis.Chassis.NumberOfOutputs; i++) {
                var ioSlot = i;

                // Control
                trilist.SetUShortSigAction(joinMap.OutputVideo + ioSlot, new Action<ushort>(o => dmChassis.ExecuteSwitch(o, ioSlot, eRoutingSignalType.Video)));

                if (dmChassis.TxDictionary.ContainsKey(ioSlot)) {
                    Debug.Console(2, "Creating Tx Feedbacks {0}", ioSlot);
                    var txKey = dmChassis.TxDictionary[ioSlot];
                    var basicTxDevice = DeviceManager.GetDeviceForKey(txKey) as BasicDmTxControllerBase;

                    var advancedTxDevice = basicTxDevice as DmTxControllerBase;

                    if (dmChassis.Chassis is DmMd128x128 || dmChassis.Chassis is DmMd64x64) {
                        dmChassis.InputEndpointOnlineFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.InputEndpointOnline + ioSlot]);
                    }
                    else {
                        if (advancedTxDevice != null) {
                            advancedTxDevice.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.InputEndpointOnline + ioSlot]);
                            Debug.Console(2, "Linking Tx Online Feedback from Advanced Transmitter at input {0}", ioSlot);
                        }
                        else if (dmChassis.InputEndpointOnlineFeedbacks[ioSlot] != null) {
                            Debug.Console(2, "Linking Tx Online Feedback from Input Card {0}", ioSlot);
                            dmChassis.InputEndpointOnlineFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.InputEndpointOnline + ioSlot]);
                        }
                    }

                    if (basicTxDevice != null && advancedTxDevice == null)
                        trilist.BooleanInput[joinMap.TxAdvancedIsPresent + ioSlot].BoolValue = true;

                    if (advancedTxDevice != null) {
                        advancedTxDevice.AnyVideoInput.VideoStatus.VideoSyncFeedback.LinkInputSig(trilist.BooleanInput[joinMap.VideoSyncStatus + ioSlot]);
                    }
                    else if (advancedTxDevice == null || basicTxDevice != null) {
                        Debug.Console(1, "Setting up actions and feedbacks on input card {0}", ioSlot);
                        dmChassis.VideoInputSyncFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.VideoSyncStatus + ioSlot]);

                        var inputPort = dmChassis.InputPorts[string.Format("inputCard{0}--hdmiIn", ioSlot)];
                        if (inputPort != null) {
                            Debug.Console(1, "Port value for input card {0} is set", ioSlot);
                            var port = inputPort.Port;

                            if (port != null) {
                                if (port is HdmiInputWithCEC) {
                                    Debug.Console(1, "Port is HdmiInputWithCec");

                                    var hdmiInPortWCec = port as HdmiInputWithCEC;

                                    if (hdmiInPortWCec.HdcpSupportedLevel != eHdcpSupportedLevel.Unknown) {
                                        SetHdcpStateAction(true, hdmiInPortWCec, joinMap.HdcpSupportState + ioSlot, trilist);
                                    }

                                    dmChassis.InputCardHdcpCapabilityFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.HdcpSupportState + ioSlot]);

                                    if (dmChassis.InputCardHdcpCapabilityTypes.ContainsKey(ioSlot))
                                        trilist.UShortInput[joinMap.HdcpSupportCapability + ioSlot].UShortValue = (ushort)dmChassis.InputCardHdcpCapabilityTypes[ioSlot];
                                    else
                                        trilist.UShortInput[joinMap.HdcpSupportCapability + ioSlot].UShortValue = 1;
                                }
                            }
                        }
                        else {
                            inputPort = dmChassis.InputPorts[string.Format("inputCard{0}--dmIn", ioSlot)];

                            if (inputPort != null) {
                                var port = inputPort.Port;

                                if (port is DMInputPortWithCec) {
                                    Debug.Console(1, "Port is DMInputPortWithCec");

                                    var dmInPortWCec = port as DMInputPortWithCec;

                                    if (dmInPortWCec != null) {
                                        SetHdcpStateAction(dmChassis.PropertiesConfig.InputSlotSupportsHdcp2[ioSlot], dmInPortWCec, joinMap.HdcpSupportState + ioSlot, trilist);
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
                else {
                    dmChassis.VideoInputSyncFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.VideoSyncStatus + ioSlot]);

                    var inputPort = dmChassis.InputPorts[string.Format("inputCard{0}--hdmiIn", ioSlot)];
                    if (inputPort != null) {
                        var hdmiPort = inputPort.Port as EndpointHdmiInput;

                        if (hdmiPort != null) {
                            SetHdcpStateAction(true, hdmiPort, joinMap.HdcpSupportState + ioSlot, trilist);
                            dmChassis.InputCardHdcpCapabilityFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.HdcpSupportState + ioSlot]);
                        }
                    }
                }
                if (dmChassis.RxDictionary.ContainsKey(ioSlot)) {
                    Debug.Console(2, "Creating Rx Feedbacks {0}", ioSlot);
                    //var rxKey = dmChassis.RxDictionary[ioSlot];
                    //var rxDevice = DeviceManager.GetDeviceForKey(rxKey) as DmRmcControllerBase;
                    //var hdBaseTDevice = DeviceManager.GetDeviceForKey(rxKey) as DmHdBaseTControllerBase;
                    //if (hdBaseTDevice != null) {
                        dmChassis.OutputEndpointOnlineFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.OutputEndpointOnline + ioSlot]);
                    //}
                    //else if (rxDevice != null) {
                    //    rxDevice.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.OutputEndpointOnline + ioSlot]);
                    //}
                }

                // Feedback
                dmChassis.VideoOutputFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.OutputVideo + ioSlot]);


                dmChassis.OutputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputNames + ioSlot]);
                dmChassis.InputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.InputNames + ioSlot]);
                dmChassis.OutputVideoRouteNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputCurrentVideoInputNames + ioSlot]);
            }
        }

        static void SetHdcpStateAction(bool hdcpTypeSimple, HdmiInputWithCEC port, uint join, BasicTriList trilist) {
            if (hdcpTypeSimple) {
                trilist.SetUShortSigAction(join,
                    new Action<ushort>(s => {
                        if (s == 0) {
                            port.HdcpSupportOff();
                        }
                        else if (s > 0) {
                            port.HdcpSupportOn();
                        }
                    }));
            }
            else {
                trilist.SetUShortSigAction(join,
                        new Action<ushort>(u => {
                            port.HdcpReceiveCapability = (eHdcpCapabilityType)u;
                        }));
            }
        }

        static void SetHdcpStateAction(bool hdcpTypeSimple, EndpointHdmiInput port, uint join, BasicTriList trilist) {
            if (hdcpTypeSimple) {
                trilist.SetUShortSigAction(join,
                    new Action<ushort>(s => {
                        if (s == 0) {
                            port.HdcpSupportOff();
                        }
                        else if (s > 0) {
                            port.HdcpSupportOn();
                        }
                    }));
            }
            else {
                trilist.SetUShortSigAction(join,
                        new Action<ushort>(u => {
                            port.HdcpCapability = (eHdcpCapabilityType)u;
                        }));
            }
        }

        static void SetHdcpStateAction(bool supportsHdcp2, DMInputPortWithCec port, uint join, BasicTriList trilist) {
            if (!supportsHdcp2) {
                trilist.SetUShortSigAction(join,
                    new Action<ushort>(s => {
                        if (s == 0) {
                            port.HdcpSupportOff();
                        }
                        else if (s > 0) {
                            port.HdcpSupportOn();
                        }
                    }));
            }
            else {
                trilist.SetUShortSigAction(join,
                        new Action<ushort>(u => {
                            port.HdcpReceiveCapability = (eHdcpCapabilityType)u;
                        }));
            }
        }

    }
}