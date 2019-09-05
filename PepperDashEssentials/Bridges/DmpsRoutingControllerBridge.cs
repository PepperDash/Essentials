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
    public static class DmpsRoutingControllerApiExtentions 
    {
        public static void LinkToApi(this DmpsRoutingController dmpsRouter, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            var joinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as DmpsRoutingControllerJoinMap;

            if (joinMap == null)
                joinMap = new DmpsRoutingControllerJoinMap();

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, dmpsRouter, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            // Link up outputs
            for (uint i = 1; i <= dmpsRouter.Dmps.NumberOfSwitcherInputs; i++)
            {
                Debug.Console(2, dmpsRouter, "Linking Input Card {0}", i);

                var ioSlot = i;

                //if (dmpsRouter.TxDictionary.ContainsKey(ioSlot))
                //{
                //    Debug.Console(2, "Creating Tx Feedbacks {0}", ioSlot);
                //    var TxKey = dmpsRouter.TxDictionary[ioSlot];
                //    var TxDevice = DeviceManager.GetDeviceForKey(TxKey) as DmTxControllerBase;
                //    //TxDevice.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.InputEndpointOnline + ioSlot]);
                //    // TxDevice.AnyVideoInput.VideoStatus.VideoSyncFeedback.LinkInputSig(trilist.BooleanInput[joinMap.InputEndpointOnline + ioSlot]);
                //    // trilist.SetUShortSigAction((ApiMap.HdcpSupport[ioSlot]), u => TxDevice.SetHdcpSupportAll((ePdtHdcpSupport)(u)));
                //    // TxDevice.HdcpSupportAllFeedback.LinkInputSig(trilist.UShortInput[joinMap. + ioSlot]);
                //    // trilist.UShortInput[ApiMap.HdcpSupportCapability[ioSlot]].UShortValue = TxDevice.HdcpSupportCapability;
                //}
                //else
                //{
                //    // dmChassis.VideoInputSyncFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[ApiMap.TxVideoSyncStatus[ioSlot]]);
                //}
                
                if(dmpsRouter.VideoInputSyncFeedbacks[ioSlot] != null)
                    dmpsRouter.VideoInputSyncFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.VideoSyncStatus + ioSlot]);

                if (dmpsRouter.InputNameFeedbacks[ioSlot] != null)
                    dmpsRouter.InputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.InputNames + ioSlot]);

                trilist.SetStringSigAction(joinMap.InputNames + ioSlot, new Action<string>(s => 
                    {
                        var inputCard = dmpsRouter.Dmps.SwitcherInputs[ioSlot] as DMInput;

                        if (inputCard != null)
                        {
                            if (inputCard.NameFeedback != null && !string.IsNullOrEmpty(inputCard.NameFeedback.StringValue) && inputCard.NameFeedback.StringValue != s)
                                if(inputCard.Name != null)
                                    inputCard.Name.StringValue = s;
                        }
                    }));


                if(dmpsRouter.InputEndpointOnlineFeedbacks[ioSlot] != null)
                    dmpsRouter.InputEndpointOnlineFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.InputEndpointOnline + ioSlot]);
            }

            for (uint i = 1; i <= dmpsRouter.Dmps.NumberOfSwitcherOutputs; i++)
            {
                Debug.Console(2, dmpsRouter, "Linking Output Card {0}", i);

                var ioSlot = i;
                // Control
                trilist.SetUShortSigAction(joinMap.OutputVideo + ioSlot, new Action<ushort>(o => dmpsRouter.ExecuteSwitch(o, ioSlot, eRoutingSignalType.Video)));
                trilist.SetUShortSigAction(joinMap.OutputAudio + ioSlot, new Action<ushort>(o => dmpsRouter.ExecuteSwitch(o, ioSlot, eRoutingSignalType.Audio)));

                trilist.SetStringSigAction(joinMap.OutputNames + ioSlot, new Action<string>(s =>
                {
                    var outputCard = dmpsRouter.Dmps.SwitcherOutputs[ioSlot] as DMOutput;

                    //Debug.Console(2, dmpsRouter, "Output Name String Sig Action for Output Card  {0}", ioSlot);

                    if (outputCard != null)
                    {
                        //Debug.Console(2, dmpsRouter, "Card Type: {0}", outputCard.CardInputOutputType);

                        if (!(outputCard is Crestron.SimplSharpPro.DM.Cards.Card.Dmps3CodecOutput) && outputCard.NameFeedback != null)
                        {
                            if (!string.IsNullOrEmpty(outputCard.NameFeedback.StringValue))
                            {
                                //Debug.Console(2, dmpsRouter, "NameFeedabck: {0}", outputCard.NameFeedback.StringValue);

                                if (outputCard.NameFeedback.StringValue != s && outputCard.Name != null)
                                {
                                    outputCard.Name.StringValue = s;
                                }
                            }
                        }
                    }
                }));

                // Feedback
                if(dmpsRouter.VideoOutputFeedbacks[ioSlot] != null)
                    dmpsRouter.VideoOutputFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.OutputVideo + ioSlot]);
                if (dmpsRouter.AudioOutputFeedbacks[ioSlot] != null)
                    dmpsRouter.AudioOutputFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.OutputAudio + ioSlot]);
                if (dmpsRouter.OutputNameFeedbacks[ioSlot] != null)
                    dmpsRouter.OutputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputNames + ioSlot]);
                if (dmpsRouter.OutputVideoRouteNameFeedbacks[ioSlot] != null)
                    dmpsRouter.OutputVideoRouteNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputCurrentVideoInputNames + ioSlot]);
                if (dmpsRouter.OutputAudioRouteNameFeedbacks[ioSlot] != null)
                    dmpsRouter.OutputAudioRouteNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputCurrentAudioInputNames + ioSlot]);
                if (dmpsRouter.OutputEndpointOnlineFeedbacks[ioSlot] != null)
                    dmpsRouter.OutputEndpointOnlineFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.OutputEndpointOnline + ioSlot]);
            }
        }
    }
}