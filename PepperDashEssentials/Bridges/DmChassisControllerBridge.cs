using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;

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
                trilist.SetUShortSigAction(joinMap.OutputUsb + ioSlot, new Action<ushort>(o => dmChassis.ExecuteSwitch(o, ioSlot, eRoutingSignalType.UsbOutput)));
                trilist.SetUShortSigAction(joinMap.InputUsb + ioSlot, new Action<ushort>(o => dmChassis.ExecuteSwitch(o, ioSlot, eRoutingSignalType.UsbInput)));

				if (dmChassis.TxDictionary.ContainsKey(ioSlot))
				{
					Debug.Console(2, "Creating Tx Feedbacks {0}", ioSlot);
					var TxKey = dmChassis.TxDictionary[ioSlot];
					var TxDevice = DeviceManager.GetDeviceForKey(TxKey) as DmTxControllerBase;
                    if (dmChassis.Chassis is DmMd8x8Cpu3 || dmChassis.Chassis is DmMd8x8Cpu3rps
                        || dmChassis.Chassis is DmMd16x16Cpu3 || dmChassis.Chassis is DmMd16x16Cpu3rps
                        || dmChassis.Chassis is DmMd32x32Cpu3 || dmChassis.Chassis is DmMd32x32Cpu3rps)
                    {
                        dmChassis.InputEndpointOnlineFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.InputEndpointOnline + ioSlot]);
                    }
                    else if (TxDevice != null)
                    {
                        TxDevice.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.InputEndpointOnline + ioSlot]);
                    }
					// TxDevice.AnyVideoInput.VideoStatus.VideoSyncFeedback.LinkInputSig(trilist.BooleanInput[joinMap.InputEndpointOnline + ioSlot]);
					// trilist.SetUShortSigAction((ApiMap.HdcpSupport[ioSlot]), u => TxDevice.SetHdcpSupportAll((ePdtHdcpSupport)(u)));
					// TxDevice.HdcpSupportAllFeedback.LinkInputSig(trilist.UShortInput[joinMap. + ioSlot]);
					// trilist.UShortInput[ApiMap.HdcpSupportCapability[ioSlot]].UShortValue = TxDevice.HdcpSupportCapability;
				}
				else
				{
					// dmChassis.VideoInputSyncFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[ApiMap.TxVideoSyncStatus[ioSlot]]);
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
                dmChassis.UsbOutputRoutedToFeebacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.OutputUsb + ioSlot]);
                dmChassis.UsbInputRoutedToFeebacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.InputUsb + ioSlot]);

                dmChassis.VideoInputSyncFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.VideoSyncStatus + ioSlot]);

                dmChassis.OutputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputNames + ioSlot]);
                dmChassis.InputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.InputNames + ioSlot]);
                dmChassis.OutputVideoRouteNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputCurrentVideoInputNames + ioSlot]);
                dmChassis.OutputAudioRouteNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputCurrentAudioInputNames + ioSlot]);
            }
        }


        public class DmChassisControllerJoinMap : JoinMapBase
        {
            public uint IsOnline { get; set; }
            public uint OutputVideo { get; set; }
            public uint OutputAudio { get; set; }
            public uint OutputUsb { get; set; }
            public uint InputUsb { get; set; }
            public uint VideoSyncStatus { get; set; }
            public uint InputNames { get; set; }
            public uint OutputNames { get; set; }
            public uint OutputCurrentVideoInputNames { get; set; }
            public uint OutputCurrentAudioInputNames { get; set; }
            public uint InputCurrentResolution { get; set; }
            public uint InputEndpointOnline { get; set; }
            public uint OutputEndpointOnline { get; set; }
            //public uint HdcpSupport { get; set; }
            //public uint HdcpSupportCapability { get; set; }


            public DmChassisControllerJoinMap()
            {
                IsOnline = 11;
                OutputVideo = 100; //101-299
                OutputAudio = 300; //301-499
                OutputUsb = 500; //501-699
                InputUsb = 700; //701-899
                VideoSyncStatus = 100; //101-299
                InputNames = 100; //101-299
                OutputNames = 300; //301-499
                OutputCurrentVideoInputNames = 2000; //2001-2199
                OutputCurrentAudioInputNames = 2200; //2201-2399
                InputCurrentResolution = 2400; // 2401-2599
                InputEndpointOnline = 500; //501-699
                OutputEndpointOnline = 700; //701-899
                //HdcpSupport = 1000; //1001-1199
                //HdcpSupportCapability = 1200; //1201-1399

            }

            public override void OffsetJoinNumbers(uint joinStart)
            {
                var joinOffset = joinStart - 1;

                IsOnline = IsOnline + joinOffset;
                OutputVideo = OutputVideo + joinOffset;
                OutputAudio = OutputAudio + joinOffset;
                OutputUsb = OutputUsb + joinOffset;
                InputUsb = InputUsb + joinOffset;
                VideoSyncStatus = VideoSyncStatus + joinOffset;
                InputNames = InputNames + joinOffset;
                OutputNames = OutputNames + joinOffset;
                OutputCurrentVideoInputNames = OutputCurrentVideoInputNames + joinOffset;
                OutputCurrentAudioInputNames = OutputCurrentAudioInputNames + joinOffset;
                InputCurrentResolution = InputCurrentResolution + joinOffset;
                InputEndpointOnline = InputEndpointOnline + joinOffset;
                OutputEndpointOnline = OutputEndpointOnline + joinOffset;
                //HdcpSupport = HdcpSupport + joinOffset;
                //HdcpSupportCapability = HdcpSupportCapability + joinOffset;
            }
        }
    }
}