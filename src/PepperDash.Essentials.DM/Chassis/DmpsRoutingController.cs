extern alias Full;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
using Full.Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.DM.Config;

using Feedback = PepperDash.Essentials.Core.Feedback;

namespace PepperDash.Essentials.DM
{
    public class DmpsRoutingController : EssentialsBridgeableDevice, IRoutingNumericWithFeedback, IHasFeedback
    {
        private const string NonePortKey = "none";

        public CrestronControlSystem Dmps { get; set; }
        public ISystemControl SystemControl { get; private set; }
        public bool? EnableRouting { get; private set; }

        //IroutingNumericEvent
        public event EventHandler<RoutingNumericEventArgs> NumericSwitchChange;

        //Feedback for DMPS System Control
        public BoolFeedback SystemPowerOnFeedback { get; private set; }
        public BoolFeedback SystemPowerOffFeedback { get; private set; }
        public BoolFeedback FrontPanelLockOnFeedback { get; private set; }
        public BoolFeedback FrontPanelLockOffFeedback { get; private set; }

        // Feedbacks for EssentialDM
        public Dictionary<uint, IntFeedback> VideoOutputFeedbacks { get; private set; }
        public Dictionary<uint, IntFeedback> AudioOutputFeedbacks { get; private set; }
        public Dictionary<uint, BoolFeedback> VideoInputSyncFeedbacks { get; private set; }
        public Dictionary<uint, BoolFeedback> InputEndpointOnlineFeedbacks { get; private set; }
        public Dictionary<uint, BoolFeedback> OutputEndpointOnlineFeedbacks { get; private set; }
        public Dictionary<uint, StringFeedback> InputNameFeedbacks { get; private set; }
        public Dictionary<uint, StringFeedback> OutputNameFeedbacks { get; private set; }
        public Dictionary<uint, StringFeedback> OutputVideoRouteNameFeedbacks { get; private set; }
        public Dictionary<uint, StringFeedback> OutputAudioRouteNameFeedbacks { get; private set; }

        public FeedbackCollection<Feedback> Feedbacks { get; private set; }

        // Need a couple Lists of generic Backplane ports
        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }
        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

        public Dictionary<uint, string> TxDictionary { get; set; }
        public Dictionary<uint, string> RxDictionary { get; set; }

        public Dictionary<uint, string> InputNames { get; set; }
        public Dictionary<uint, string> OutputNames { get; set; }
        public Dictionary<uint, DmCardAudioOutputController> VolumeControls { get; private set; }
        public Dictionary<uint, DmpsDigitalOutputController> DigitalAudioOutputs { get; private set; }        
        public DmpsMicrophoneController Microphones { get; private set; }

        public const int RouteOffTime = 500;
        Dictionary<PortNumberType, CTimer> RouteOffTimers = new Dictionary<PortNumberType, CTimer>();

        /// <summary>
        /// Text that represents when an output has no source routed to it
        /// </summary>
        public string NoRouteText = "";

        /// <summary>
        /// Raise an event when the status of a switch object changes.
        /// </summary>
        /// <param name="e">Arguments defined as IKeyName sender, output, input, and eRoutingSignalType</param>
        private void OnSwitchChange(RoutingNumericEventArgs e)
        {
            var newEvent = NumericSwitchChange;
            if (newEvent != null) newEvent(this, e);
        }


        public static DmpsRoutingController GetDmpsRoutingController(string key, string name,
            DmpsRoutingPropertiesConfig properties)
        {
            try
            {
                var systemControl = Global.ControlSystem.SystemControl;

                if (systemControl == null)
                {
                    return null;
                }

                var controller = new DmpsRoutingController(key, name, systemControl)
                {
                    InputNames = properties.InputNames,
                    OutputNames = properties.OutputNames
                };

                if (!string.IsNullOrEmpty(properties.NoRouteText))
                    controller.NoRouteText = properties.NoRouteText;

                return controller;

            }
            catch (Exception e)
            {
                Debug.Console(0, "Error getting DMPS Controller:\r\n{0}", e);
            }
            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="chassis"></param>
        public DmpsRoutingController(string key, string name, ISystemControl systemControl)
            : base(key, name)
        {
            Dmps = Global.ControlSystem;

            switch (systemControl.SystemControlType)
            {
                case eSystemControlType.Dmps34K150CSystemControl:
                    SystemControl = systemControl as Dmps34K150CSystemControl;
                    SystemPowerOnFeedback = new BoolFeedback(() => { return true; });
                    SystemPowerOffFeedback = new BoolFeedback(() => { return false; });
                    break;
                case eSystemControlType.Dmps34K200CSystemControl:
                case eSystemControlType.Dmps34K250CSystemControl:
                case eSystemControlType.Dmps34K300CSystemControl:
                case eSystemControlType.Dmps34K350CSystemControl:
                    SystemControl = systemControl as Dmps34K300CSystemControl;
                    SystemPowerOnFeedback = new BoolFeedback(() => { return true; });
                    SystemPowerOffFeedback = new BoolFeedback(() => { return false; });
                    break;
                default:
                    SystemControl = systemControl as Dmps3SystemControl;
                    SystemPowerOnFeedback = new BoolFeedback(() =>
                    {
                        return ((Dmps3SystemControl)SystemControl).SystemPowerOnFeedBack.BoolValue;
                    });
                    SystemPowerOffFeedback = new BoolFeedback(() =>
                    {
                        return ((Dmps3SystemControl)SystemControl).SystemPowerOffFeedBack.BoolValue;
                    });
                    break;
            }
            Debug.Console(1, this, "DMPS Type = {0}, 4K Type = {1}", systemControl.SystemControlType, Global.ControlSystemIsDmps4kType);

            InputPorts = new RoutingPortCollection<RoutingInputPort>();
            OutputPorts = new RoutingPortCollection<RoutingOutputPort>();
            VolumeControls = new Dictionary<uint, DmCardAudioOutputController>();
            DigitalAudioOutputs = new Dictionary<uint, DmpsDigitalOutputController>();
            TxDictionary = new Dictionary<uint, string>();
            RxDictionary = new Dictionary<uint, string>();

            FrontPanelLockOnFeedback = new BoolFeedback(() =>
            {
                return SystemControl.FrontPanelLockOnFeedback.BoolValue;
            });
            FrontPanelLockOffFeedback = new BoolFeedback(() =>
            {
                return SystemControl.FrontPanelLockOffFeedback.BoolValue;
            });

            VideoOutputFeedbacks = new Dictionary<uint, IntFeedback>();
            AudioOutputFeedbacks = new Dictionary<uint, IntFeedback>();
            VideoInputSyncFeedbacks = new Dictionary<uint, BoolFeedback>();
            InputNameFeedbacks = new Dictionary<uint, StringFeedback>();
            OutputNameFeedbacks = new Dictionary<uint, StringFeedback>();
            OutputVideoRouteNameFeedbacks = new Dictionary<uint, StringFeedback>();
            OutputAudioRouteNameFeedbacks = new Dictionary<uint, StringFeedback>();
            InputEndpointOnlineFeedbacks = new Dictionary<uint, BoolFeedback>();
            OutputEndpointOnlineFeedbacks = new Dictionary<uint, BoolFeedback>();

            Debug.Console(1, this, "{0} Switcher Inputs Present.", Dmps.SwitcherInputs.Count);
            Debug.Console(1, this, "{0} Switcher Outputs Present.", Dmps.SwitcherOutputs.Count);

            Debug.Console(1, this, "{0} Inputs in ControlSystem", Dmps.NumberOfSwitcherInputs);
            Debug.Console(1, this, "{0} Outputs in ControlSystem", Dmps.NumberOfSwitcherOutputs);

            SetupOutputCards();

            SetupInputCards();

            Microphones = new DmpsMicrophoneController(Dmps);
        }

        public override bool CustomActivate()
        {
            // Set input and output names from config
            SetInputNames();

            SetOutputNames();

            // Subscribe to events
            Dmps.DMInputChange += Dmps_DMInputChange;
            Dmps.DMOutputChange += Dmps_DMOutputChange;
            Dmps.DMSystemChange += Dmps_DMSystemChange;
            
            foreach (var x in VideoOutputFeedbacks)
            {
                x.Value.FireUpdate();
            }
            foreach (var x in AudioOutputFeedbacks)
            {
                x.Value.FireUpdate();
            }
            foreach (var x in VideoInputSyncFeedbacks)
            {
                x.Value.FireUpdate();
            }
            foreach (var x in InputEndpointOnlineFeedbacks)
            {
                x.Value.FireUpdate();
            }
            foreach (var x in InputNameFeedbacks)
            {
                x.Value.FireUpdate();
            }
            foreach (var x in OutputNameFeedbacks)
            {
                x.Value.FireUpdate();
            }
            foreach (var x in OutputEndpointOnlineFeedbacks)
            {
                x.Value.FireUpdate();
            }

            SystemPowerOnFeedback.FireUpdate();
            SystemPowerOffFeedback.FireUpdate();

            FrontPanelLockOnFeedback.FireUpdate();
            FrontPanelLockOffFeedback.FireUpdate();

            return base.CustomActivate();
        }

        private void SetOutputNames()
        {
            if (OutputNames == null)
            {
                return;
            }
            foreach (var kvp in OutputNames)
            {
                var output = (Dmps.SwitcherOutputs[kvp.Key] as DMOutput);
                if (output != null)
                {
                    if (output.Name.Supported && kvp.Value.Length > 0)
                    {
                        output.Name.StringValue = kvp.Value;
                    }
                }
            }
        }

        private void SetInputNames()
        {
            if (InputNames == null)
            {
                return;
            }
            foreach (var kvp in InputNames)
            {
                var input = (Dmps.SwitcherInputs[kvp.Key] as DMInput);
                if (input != null)
                {
                    if (input.Name.Supported && kvp.Value.Length > 0)
                    {
                        input.Name.StringValue = kvp.Value;
                    }
                }
            }
        }

        public void SetRoutingEnable(bool enable)
        {
            CrestronEnvironment.Sleep(1000);
            EnableRouting = enable;
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new DmpsRoutingControllerJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<DmpsRoutingControllerJoinMap>(joinMapSerialized);

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                Debug.Console(0, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            //Link up system power only for non-4k DMPS3
            if (SystemControl is Dmps3SystemControl)
            {
                trilist.SetBoolSigAction(joinMap.SystemPowerOn.JoinNumber, a => { if (a) { ((Dmps3SystemControl)SystemControl).SystemPowerOn(); } });
                trilist.SetBoolSigAction(joinMap.SystemPowerOff.JoinNumber, a => { if (a) { ((Dmps3SystemControl)SystemControl).SystemPowerOff(); } });
            }

            SystemPowerOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.SystemPowerOn.JoinNumber]);
            SystemPowerOffFeedback.LinkInputSig(trilist.BooleanInput[joinMap.SystemPowerOff.JoinNumber]);

            trilist.SetBoolSigAction(joinMap.FrontPanelLockOn.JoinNumber, a => { if (a) {SystemControl.FrontPanelLockOn();}});
            trilist.SetBoolSigAction(joinMap.FrontPanelLockOff.JoinNumber, a => { if (a) {SystemControl.FrontPanelLockOff();}});
            
            FrontPanelLockOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.FrontPanelLockOn.JoinNumber]);
            FrontPanelLockOffFeedback.LinkInputSig(trilist.BooleanInput[joinMap.FrontPanelLockOff.JoinNumber]);

            trilist.SetBoolSigAction(joinMap.EnableRouting.JoinNumber, SetRoutingEnable);

            // Link up outputs
            LinkInputsToApi(trilist, joinMap);
            LinkOutputsToApi(trilist, joinMap);
        }

        private void LinkOutputsToApi(BasicTriList trilist, DmpsRoutingControllerJoinMap joinMap)
        {
            for (uint i = 1; i <= Dmps.SwitcherOutputs.Count; i++)
            {
                Debug.Console(2, this, "Linking Output Card {0}", i);

                var ioSlot = i;
                var ioSlotJoin = ioSlot - 1;

                // Control
                trilist.SetUShortSigAction(joinMap.OutputVideo.JoinNumber + ioSlotJoin,
                    o => ExecuteNumericSwitch(o, (ushort) ioSlot, eRoutingSignalType.Video));
                trilist.SetUShortSigAction(joinMap.OutputAudio.JoinNumber + ioSlotJoin,
                    o => ExecuteNumericSwitch(o, (ushort) ioSlot, eRoutingSignalType.Audio));

                trilist.SetStringSigAction(joinMap.OutputNames.JoinNumber + ioSlotJoin, s =>
                {
                    var outputCard = Dmps.SwitcherOutputs[ioSlot] as DMOutput;

                    //Debug.Console(2, dmpsRouter, "Output Name String Sig Action for Output Card  {0}", ioSlot);

                    if (outputCard == null)
                    {
                        return;
                    }
                    //Debug.Console(2, dmpsRouter, "Card Type: {0}", outputCard.CardInputOutputType);

                    if (outputCard is Card.Dmps3CodecOutput || outputCard.NameFeedback == null)
                    {
                        return;
                    }
                    if (string.IsNullOrEmpty(outputCard.NameFeedback.StringValue))
                    {
                        return;
                    }
                    //Debug.Console(2, dmpsRouter, "NameFeedabck: {0}", outputCard.NameFeedback.StringValue);

                    if (outputCard.NameFeedback.StringValue != s && outputCard.Name != null)
                    {
                        outputCard.Name.StringValue = s;
                    }
                });

                // Feedback
                if (VideoOutputFeedbacks[ioSlot] != null)
                {
                    VideoOutputFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.OutputVideo.JoinNumber + ioSlotJoin]);
                }
                if (AudioOutputFeedbacks[ioSlot] != null)
                {
                    AudioOutputFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.OutputAudio.JoinNumber + ioSlotJoin]);
                }
                if (OutputNameFeedbacks[ioSlot] != null)
                {
                    if (Dmps.SwitcherOutputs[ioSlot].CardInputOutputType == eCardInputOutputType.Dmps3DmOutput ||
                        Dmps.SwitcherOutputs[ioSlot].CardInputOutputType == eCardInputOutputType.Dmps3DmOutputBackend ||
                        Dmps.SwitcherOutputs[ioSlot].CardInputOutputType == eCardInputOutputType.Dmps3HdmiOutput ||
                        Dmps.SwitcherOutputs[ioSlot].CardInputOutputType == eCardInputOutputType.Dmps3HdmiOutputBackend ||
                        Dmps.SwitcherOutputs[ioSlot].CardInputOutputType == eCardInputOutputType.Dmps3DmHdmiAudioOutput)
                    {
                        OutputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputVideoNames.JoinNumber + ioSlotJoin]);
                        OutputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputNames.JoinNumber + ioSlotJoin]);
                        OutputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputAudioNames.JoinNumber + ioSlotJoin]);
                    }
                    else
                    {
                        OutputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputNames.JoinNumber + ioSlotJoin]);
                        OutputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputAudioNames.JoinNumber + ioSlotJoin]);
                    }
                }
                if (OutputVideoRouteNameFeedbacks[ioSlot] != null)
                {
                    OutputVideoRouteNameFeedbacks[ioSlot].LinkInputSig(
                        trilist.StringInput[joinMap.OutputCurrentVideoInputNames.JoinNumber + ioSlotJoin]);
                }
                if (OutputAudioRouteNameFeedbacks[ioSlot] != null)
                {
                    OutputAudioRouteNameFeedbacks[ioSlot].LinkInputSig(
                        trilist.StringInput[joinMap.OutputCurrentAudioInputNames.JoinNumber + ioSlotJoin]);
                }
                if (OutputEndpointOnlineFeedbacks[ioSlot] != null)
                {
                    OutputEndpointOnlineFeedbacks[ioSlot].LinkInputSig(
                        trilist.BooleanInput[joinMap.OutputEndpointOnline.JoinNumber + ioSlotJoin]);
                }
            }
        }

        private void LinkInputsToApi(BasicTriList trilist, DmpsRoutingControllerJoinMap joinMap)
        {
            if (Global.ControlSystemIsDmps4k3xxType)
            {
                //Add DMPS-4K mixer input names to end of inputs
                trilist.StringInput[joinMap.InputAudioNames.JoinNumber + (uint)Dmps.SwitcherInputs.Count + 4].StringValue = "Digital Mixer 1";
                trilist.StringInput[joinMap.InputAudioNames.JoinNumber + (uint)Dmps.SwitcherInputs.Count + 5].StringValue = "Digital Mixer 2";
            }
            for (uint i = 1; i <= Dmps.SwitcherInputs.Count; i++)
            {
                Debug.Console(2, this, "Linking Input Card {0}", i);

                var ioSlot = i;
                var ioSlotJoin = ioSlot - 1;

                if (VideoInputSyncFeedbacks.ContainsKey(ioSlot) && VideoInputSyncFeedbacks[ioSlot] != null)
                {
                    VideoInputSyncFeedbacks[ioSlot].LinkInputSig(
                        trilist.BooleanInput[joinMap.VideoSyncStatus.JoinNumber + ioSlotJoin]);
                }

                if (InputNameFeedbacks.ContainsKey(ioSlot) && InputNameFeedbacks[ioSlot] != null)
                {
                    if (Dmps.SwitcherInputs[ioSlot] is Card.Dmps3AnalogAudioInput)
                    {
                        for (uint j = ioSlot; j < ioSlot + 5; j++)
                        {
                            InputNameFeedbacks[j].LinkInputSig(trilist.StringInput[joinMap.InputAudioNames.JoinNumber + j - 1]);
                        }
                    }
                    else
                    {
                        InputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.InputNames.JoinNumber + ioSlotJoin]);
                        InputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.InputVideoNames.JoinNumber + ioSlotJoin]);
                        InputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.InputAudioNames.JoinNumber + ioSlotJoin]);
                    }
                }

                trilist.SetStringSigAction(joinMap.InputNames.JoinNumber + ioSlotJoin, s =>
                {
                    var inputCard = Dmps.SwitcherInputs[ioSlot] as DMInput;

                    if (inputCard == null)
                    {
                        return;
                    }

                    if (inputCard.NameFeedback == null || string.IsNullOrEmpty(inputCard.NameFeedback.StringValue) ||
                        inputCard.NameFeedback.StringValue == s)
                    {
                        return;
                    }

                    if (inputCard.Name != null)
                    {
                        inputCard.Name.StringValue = s;
                    }
                });


                if (InputEndpointOnlineFeedbacks.ContainsKey(ioSlot) && InputEndpointOnlineFeedbacks[ioSlot] != null)
                {
                    InputEndpointOnlineFeedbacks[ioSlot].LinkInputSig(
                        trilist.BooleanInput[joinMap.InputEndpointOnline.JoinNumber + ioSlotJoin]);
                }
            }
        }


        /// <summary>
        /// Iterate the SwitcherOutputs collection to setup feedbacks and add routing ports
        /// </summary>
        void SetupOutputCards()
        {
            foreach (var card in Dmps.SwitcherOutputs)
            {
                try
                {
                    Debug.Console(1, this, "Output Card Type: {0}", card.CardInputOutputType);

                    var outputCard = card as DMOutput;

                    if (outputCard == null)
                    {
                        Debug.Console(1, this, "Output card {0} is not a DMOutput", card.CardInputOutputType);
                        continue;
                    }

                    Debug.Console(1, this, "Adding Output Card Number {0} Type: {1}", outputCard.Number, outputCard.CardInputOutputType.ToString());
                    VideoOutputFeedbacks[outputCard.Number] = new IntFeedback(() =>
                    {
                        if (outputCard.VideoOutFeedback != null) { return (ushort)outputCard.VideoOutFeedback.Number; }
                        return 0;
                        ;
                    });
                    AudioOutputFeedbacks[outputCard.Number] = new IntFeedback(() =>
                    {
                        if (!Global.ControlSystemIsDmps4k3xxType)
                        {
                            if (outputCard.AudioOutFeedback != null)
                            {
                                return (ushort)outputCard.AudioOutFeedback.Number;
                            }
                            return 0;
                        }
                        else
                        {

                            if (outputCard is Card.Dmps3DmOutputBackend || outputCard is Card.Dmps3HdmiOutputBackend)
                            {
                                //Special cases for DMPS-4K digital audio output
                                if (DigitalAudioOutputs[outputCard.Number].AudioSourceNumericFeedback.UShortValue == 0)
                                    return 0;
                                else if (DigitalAudioOutputs[outputCard.Number].AudioSourceNumericFeedback.UShortValue == 1)
                                    return (ushort)Dmps.SwitcherInputs.Count + 5;
                                else if (DigitalAudioOutputs[outputCard.Number].AudioSourceNumericFeedback.UShortValue == 2)
                                    return (ushort)Dmps.SwitcherInputs.Count + 6;
                                else if (DigitalAudioOutputs[outputCard.Number].AudioSourceNumericFeedback.UShortValue == 3)
                                    return (ushort)outputCard.VideoOutFeedback.Number;
                                else
                                    return 0;
                            }
                            else if (outputCard.AudioOutSourceFeedback == eDmps34KAudioOutSource.NoRoute)
                            {
                                //Fixes for weird audio indexing on DMPS3-4K
                                return 0;
                            }
                            else if (outputCard.AudioOutSourceFeedback == eDmps34KAudioOutSource.AirMedia8)
                            {
                                //Fixes for weird audio indexing on DMPS3-4K
                                return 8;
                            }
                            else if (outputCard.AudioOutSourceFeedback == eDmps34KAudioOutSource.AirMedia9)
                            {
                                //Fixes for weird audio indexing on DMPS3-4K
                                return 9;
                            }
                            else if ((ushort)outputCard.AudioOutSourceFeedback <= 5)
                            {
                                //Move analog inputs to after regular dm cards
                                return (ushort)outputCard.AudioOutSourceFeedback + (ushort)Dmps.SwitcherInputs.Count - 1;
                            }
                            else
                            {
                                //Fixes for weird audio indexing on DMPS3-4K
                                return (ushort)outputCard.AudioOutSourceFeedback - 5;
                            }
                        }
                    });

                    OutputNameFeedbacks[outputCard.Number] = new StringFeedback(() =>
                    {
                        if(OutputNames.ContainsKey(outputCard.Number))
                        {
                            return OutputNames[outputCard.Number];
                        }
                        else if (outputCard.NameFeedback != null && outputCard.NameFeedback != CrestronControlSystem.NullStringOutputSig && !string.IsNullOrEmpty(outputCard.NameFeedback.StringValue))
                        {
                            Debug.Console(2, this, "Output Card {0} Name: {1}", outputCard.Number, outputCard.NameFeedback.StringValue);
                            return outputCard.NameFeedback.StringValue;
                        }
                        return "";
                    });

                    OutputVideoRouteNameFeedbacks[outputCard.Number] = new StringFeedback(() =>
                    {
                        if (outputCard.VideoOutFeedback != null && outputCard.VideoOutFeedback.NameFeedback != null)
                        {
                            return outputCard.VideoOutFeedback.NameFeedback.StringValue;
                        }
                        return NoRouteText;
                    });
                    OutputAudioRouteNameFeedbacks[outputCard.Number] = new StringFeedback(() =>
                    {
                        if (!Global.ControlSystemIsDmps4k3xxType)
                        {
                            if (outputCard.AudioOutFeedback != null && outputCard.AudioOutFeedback.NameFeedback != null)
                            {
                                return outputCard.AudioOutFeedback.NameFeedback.StringValue;
                            }
                        }
                        else
                        {
                            if (outputCard is Card.Dmps3DmOutputBackend || outputCard is Card.Dmps3HdmiOutputBackend)
                            {
                                //Special cases for DMPS-4K digital audio output
                                if (DigitalAudioOutputs[outputCard.Number].AudioSourceNumericFeedback.UShortValue == 0)
                                    return NoRouteText;
                                else if (DigitalAudioOutputs[outputCard.Number].AudioSourceNumericFeedback.UShortValue == 1)
                                    return "Digital Mix 1";
                                else if (DigitalAudioOutputs[outputCard.Number].AudioSourceNumericFeedback.UShortValue == 2)
                                    return "Digital Mix 2";
                                else if (DigitalAudioOutputs[outputCard.Number].AudioSourceNumericFeedback.UShortValue == 3)
                                    return outputCard.VideoOutFeedback.NameFeedback.StringValue;
                                else
                                    return NoRouteText;
                            }
                            else
                            {
                                return outputCard.AudioOutSourceFeedback.ToString();
                            }
                        }
                        return NoRouteText;
                    });

                    OutputEndpointOnlineFeedbacks[outputCard.Number] = new BoolFeedback(() => outputCard.EndpointOnlineFeedback);

                    AddOutputCard(outputCard.Number, outputCard);
                }
                catch (Exception ex)
                {
                    Debug.LogError(Debug.ErrorLogLevel.Error, string.Format("DMPS Controller exception creating output card: {0}", ex));
                }
            }
        }

        /// <summary>
        /// Iterate the SwitcherInputs collection to setup feedbacks and add routing ports
        /// </summary>
        void SetupInputCards()
        {
            foreach (var card in Dmps.SwitcherInputs)
            {
                var inputCard = card as DMInput;

                if (inputCard != null)
                {
                    Debug.Console(1, this, "Adding Input Card Number {0} Type: {1}", inputCard.Number, inputCard.CardInputOutputType.ToString());

                    InputEndpointOnlineFeedbacks[inputCard.Number] = new BoolFeedback(() => inputCard.EndpointOnlineFeedback);

                    if (inputCard.VideoDetectedFeedback != null && inputCard.VideoDetectedFeedback.Supported)
                    {
                        VideoInputSyncFeedbacks[inputCard.Number] = new BoolFeedback(() => inputCard.VideoDetectedFeedback.BoolValue);
                    }

                    InputNameFeedbacks[inputCard.Number] = new StringFeedback(() =>
                    {
                        if (InputNames.ContainsKey(inputCard.Number))
                        {
                            return InputNames[inputCard.Number];
                        }
                        else if (inputCard.NameFeedback != null && inputCard.NameFeedback != CrestronControlSystem.NullStringOutputSig && !string.IsNullOrEmpty(inputCard.NameFeedback.StringValue))
                        {
                            Debug.Console(2, this, "Input Card {0} Name: {1}", inputCard.Number, inputCard.NameFeedback.StringValue);
                            return inputCard.NameFeedback.StringValue;
                        }
                        return "";
                    });

                    AddInputCard(inputCard.Number, inputCard);
                }
                else
                {
                    Debug.Console(2, this, "***********Input Card of type {0} is cannot be cast as DMInput*************", card.CardInputOutputType);
                }
            }

            InputPorts.Add(new RoutingInputPort(NonePortKey, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.None, null, this));
        }

        /// <summary>
        /// Builds the appropriate ports aand callst the appropreate add port method
        /// </summary>
        /// <param name="number"></param>
        /// <param name="inputCard"></param>
        public void AddInputCard(uint number, DMInput inputCard)
        {
            if (inputCard is Card.Dmps3HdmiInputWithoutAnalogAudio)
            {
                var hdmiInputCard = inputCard as Card.Dmps3HdmiInputWithoutAnalogAudio;

                var cecPort = hdmiInputCard.HdmiInputPort;

                AddInputPortWithDebug(number, string.Format("HdmiIn{0}", number), eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, cecPort);              
            }
            else if (inputCard is Card.Dmps3HdmiInput)
            {
                var hdmiInputCard = inputCard as Card.Dmps3HdmiInput;
                var cecPort = hdmiInputCard.HdmiInputPort;

                AddInputPortWithDebug(number, string.Format("HdmiIn{0}", number), eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, cecPort);
                AddInputPortWithDebug(number, string.Format("HudioIn{1}", number), eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio);
            }
            else if (inputCard is Card.Dmps3HdmiVgaInput)
            {
                var hdmiVgaInputCard = inputCard as Card.Dmps3HdmiVgaInput;

                DmpsInternalVirtualHdmiVgaInputController inputCardController = new DmpsInternalVirtualHdmiVgaInputController(Key +
                    string.Format("-HdmiVgaIn{0}", number), string.Format("InternalInputController-{0}", number), hdmiVgaInputCard);

                DeviceManager.AddDevice(inputCardController);

                AddInputPortWithDebug(number, string.Format("HdmiVgaIn{0}", number), eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.BackplaneOnly);
            }
            else if (inputCard is Card.Dmps3HdmiVgaBncInput)
            {
                var hdmiVgaBncInputCard = inputCard as Card.Dmps3HdmiVgaBncInput;

                DmpsInternalVirtualHdmiVgaBncInputController inputCardController = new DmpsInternalVirtualHdmiVgaBncInputController(Key +
                    string.Format("-HdmiVgaBncIn{0}", number), string.Format("InternalInputController-{0}", number), hdmiVgaBncInputCard);

                DeviceManager.AddDevice(inputCardController);

                AddInputPortWithDebug(number, string.Format("HdmiVgaBncIn{0}", number), eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.BackplaneOnly);

            }
            else if (inputCard is Card.Dmps3DmInput)
            {
                var hdmiInputCard = inputCard as Card.Dmps3DmInput;
                var cecPort = hdmiInputCard.DmInputPort;

                AddInputPortWithDebug(number, string.Format("DmIn{0}", number), eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmCat, cecPort);
            }
            else if (inputCard is Card.Dmps3VgaInput)
            {
                AddInputPortWithDebug(number, string.Format("VgaIn{0}", number), eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Vga);
            }
            else if (inputCard is Card.Dmps3AirMediaInput)
            {
                AddInputPortWithDebug(number, string.Format("AirMediaIn{0}", number), eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Streaming);
            }
            else if (inputCard is Card.Dmps3AnalogAudioInput)
            {
                for (uint i = 0; i <= 4; i++)
                {
                    uint j = i + 1;
                    uint input = i + number;
                    InputNameFeedbacks[input] = new StringFeedback(() =>
                    {
                        if (InputNames.ContainsKey(input))
                        {
                            return InputNames[input];
                        }
                        else
                        {
                            return String.Format("Aux Input {0}", j);
                        }
                    });
                }
            }
        }


        /// <summary>
        /// Adds InputPort
        /// </summary>
        private void AddInputPortWithDebug(uint cardNum, string portName, eRoutingSignalType sigType,
            eRoutingPortConnectionType portType)
        {
            AddInputPortWithDebug(cardNum, portName, sigType, portType, null);
        }

        /// <summary>
        /// Adds InputPort and sets Port as ICec object
        /// </summary>
        void AddInputPortWithDebug(uint cardNum, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType, ICec cecPort)
        {
            var portKey = string.Format("inputCard{0}--{1}", cardNum, portName);
            Debug.Console(2, this, "Adding input port '{0}'", portKey);
            var inputPort = new RoutingInputPort(portKey, sigType, portType, Dmps.SwitcherInputs[cardNum], this)
            {
                FeedbackMatchObject = Dmps.SwitcherInputs[cardNum]
            }; 

            if (cecPort != null)
                inputPort.Port = cecPort;

            InputPorts.Add(inputPort);
        }


        /// <summary>
        /// Builds the appropriate ports and calls the appropriate add port method
        /// </summary>
        /// <param name="number"></param>
        /// <param name="outputCard"></param>
        public void AddOutputCard(uint number, DMOutput outputCard)
        {
            if (outputCard is Card.Dmps3HdmiOutput)
            {
                var hdmiOutputCard = outputCard as Card.Dmps3HdmiOutput;

                var cecPort = hdmiOutputCard.HdmiOutputPort;

                AddHdmiOutputPort(number, cecPort);

                var audioOutput = new DmpsAudioOutputController(string.Format("processor-digitalAudioOutput{0}", number), string.Format("Hdmi Audio Output {0}", number), outputCard as Card.Dmps3HdmiOutput);
                DeviceManager.AddDevice(audioOutput);
            }
            else if (outputCard is Card.Dmps3HdmiOutputBackend)
            {
                var hdmiOutputCard = outputCard as Card.Dmps3HdmiOutputBackend;
                var cecPort = hdmiOutputCard.HdmiOutputPort;
                AddHdmiOutputPort(number, cecPort);
                var audioOutput = new DmpsDigitalOutputController(string.Format("processor-avRouting-HdmiAudioOut{0}", number), string.Format("Hdmi Audio Output {0} Router", number), hdmiOutputCard);
                DigitalAudioOutputs.Add(number, audioOutput);
                DeviceManager.AddDevice(audioOutput);
            }
            else if (outputCard is Card.Dmps3DmOutput)
            {
                AddDmOutputPort(number);
                var audioOutput = new DmpsAudioOutputController(string.Format("processor-digitalAudioOutput{0}", number), string.Format("Dm Audio Output {0}", number), outputCard as Card.Dmps3DmOutput);
                DeviceManager.AddDevice(audioOutput);
            }
            else if (outputCard is Card.Dmps3DmOutputBackend)
            {
                AddDmOutputPort(number);
                var audioOutput = new DmpsDigitalOutputController(string.Format("processor-avRouting-DmAudioOut{0}", number), string.Format("Dm Audio Output {0} Router", number), outputCard as Card.Dmps3DmOutputBackend);
                DigitalAudioOutputs.Add(number, audioOutput);
                DeviceManager.AddDevice(audioOutput);
            }
            else if (outputCard is Card.Dmps3DmHdmiAudioOutput)
            {
                var hdmiOutputCard = outputCard as Card.Dmps3DmHdmiAudioOutput;
                var cecPort = hdmiOutputCard.HdmiOutputPort;
                AddHdmiOutputPort(number, cecPort);
                AddDmOutputPort(number);
                AddAudioOnlyOutputPort(number, "Program");

                var audioOutput = new DmpsAudioOutputController(string.Format("processor-programAudioOutput", number), string.Format("Program Audio Output {0}", number), hdmiOutputCard, hdmiOutputCard.AudioOutputStream);
                DeviceManager.AddDevice(audioOutput);
                var digitalAudioOutput = new DmpsAudioOutputController(string.Format("processor-digitalAudioOutput{0}", number), string.Format("Hdmi Audio Output {0}", number), hdmiOutputCard, hdmiOutputCard.DmHdmiOutputStream);
                DeviceManager.AddDevice(digitalAudioOutput);
            }
            else if (outputCard is Card.Dmps3ProgramOutput)
            {
                AddAudioOnlyOutputPort(number, "Program");

                var programOutput = new DmpsAudioOutputController(string.Format("processor-programAudioOutput"), "Program Audio Output", outputCard as Card.Dmps3ProgramOutput);

                DeviceManager.AddDevice(programOutput);
            }
            else if (outputCard is Card.Dmps3AuxOutput)
            {
                switch (outputCard.CardInputOutputType)
                {
                    case eCardInputOutputType.Dmps3Aux1Output:
                        {
                            AddAudioOnlyOutputPort(number, "Aux1");

                            var aux1Output = new DmpsAudioOutputController(string.Format("processor-aux1AudioOutput"), "Aux1 Audio Output", outputCard as Card.Dmps3Aux1Output);

                            DeviceManager.AddDevice(aux1Output);
                        }
                        break;
                    case eCardInputOutputType.Dmps3Aux2Output:
                        {
                            AddAudioOnlyOutputPort(number, "Aux2");

                            var aux2Output = new DmpsAudioOutputController(string.Format("processor-aux2AudioOutput"), "Aux2 Audio Output", outputCard as Card.Dmps3Aux2Output);

                            DeviceManager.AddDevice(aux2Output);
                        }
                        break;
                }
            }
            else if (outputCard is Card.Dmps3CodecOutput)
            {
                switch (number)
                {
                    case (uint)CrestronControlSystem.eDmps34K350COutputs.Codec1:
                    case (uint)CrestronControlSystem.eDmps34K250COutputs.Codec1:
                    case (uint)CrestronControlSystem.eDmps3300cAecOutputs.Codec1:
                        AddAudioOnlyOutputPort(number, CrestronControlSystem.eDmps300cOutputs.Codec1.ToString());
                        break;
                    case (uint)CrestronControlSystem.eDmps34K350COutputs.Codec2:
                    case (uint)CrestronControlSystem.eDmps34K250COutputs.Codec2:
                    case (uint)CrestronControlSystem.eDmps3300cAecOutputs.Codec2:
                        AddAudioOnlyOutputPort(number, CrestronControlSystem.eDmps300cOutputs.Codec2.ToString());
                        break;
                }
            }
            else if (outputCard is Card.Dmps3DialerOutput)
            {
                AddAudioOnlyOutputPort(number, "Dialer");
            }
            else if (outputCard is Card.Dmps3AecOutput)
            {
                AddAudioOnlyOutputPort(number, "Aec");
            }
            else if (outputCard is Card.Dmps3DigitalMixOutput)
            {
                if (number == (uint)CrestronControlSystem.eDmps34K250COutputs.Mix1
                    || number == (uint)CrestronControlSystem.eDmps34K300COutputs.Mix1
                    || number == (uint)CrestronControlSystem.eDmps34K350COutputs.Mix1)
                    AddAudioOnlyOutputPort(number, CrestronControlSystem.eDmps34K250COutputs.Mix1.ToString());
                if (number == (uint)CrestronControlSystem.eDmps34K250COutputs.Mix2
                    || number == (uint)CrestronControlSystem.eDmps34K300COutputs.Mix2
                    || number == (uint)CrestronControlSystem.eDmps34K350COutputs.Mix2)
                    AddAudioOnlyOutputPort(number, CrestronControlSystem.eDmps34K250COutputs.Mix2.ToString());

                var audioOutput = new DmpsAudioOutputController(string.Format("processor-digitalAudioOutput{0}", number % 2 + 1), string.Format("Digital Audio Mix {0}", number % 2 + 1), outputCard as Card.Dmps3DigitalMixOutput);
                DeviceManager.AddDevice(audioOutput);
            }
            else
            {
                Debug.Console(1, this, "Output Card is of a type not currently handled:", outputCard.CardInputOutputType.ToString());
            }
        }

        /// <summary>
        /// Adds an Audio only output port
        /// </summary>
        /// <param name="number"></param>
        void AddAudioOnlyOutputPort(uint number, string portName)
        {
            AddOutputPortWithDebug(number, portName, eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio, Dmps.SwitcherOutputs[number]);
        }

        /// <summary>
        /// Adds an HDMI output port
        /// </summary>
        /// <param name="number"></param>
        /// <param name="cecPort"></param>
        void AddHdmiOutputPort(uint number, ICec cecPort)
        {
            AddOutputPortWithDebug(number, string.Format("hdmiOut{0}", number), eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, Dmps.SwitcherOutputs[number], cecPort);
        }

        /// <summary>
        /// Adds a DM output port
        /// </summary>
        /// <param name="number"></param>
        void AddDmOutputPort(uint number)
        {
            AddOutputPortWithDebug(number, string.Format("dmOut{0}", number), eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmCat, Dmps.SwitcherOutputs[number]);
        }

        /// <summary>
        /// Adds OutputPort
        /// </summary>
        void AddOutputPortWithDebug(uint cardNum, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType, object selector)
        {
            AddOutputPortWithDebug(cardNum, portName, sigType, portType, selector, null);
        }

        /// <summary>
        /// Adds OutputPort and sets Port as ICec object
        /// </summary>
        void AddOutputPortWithDebug(uint cardNum, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType, object selector, ICec cecPort)
        {
            var portKey = string.Format("outputCard{0}--{1}", cardNum, portName);
            Debug.Console(2, this, "Adding output port '{0}'", portKey);
            var outputPort = new RoutingOutputPort(portKey, sigType, portType, selector, this)
            {
                FeedbackMatchObject = Dmps.SwitcherOutputs[cardNum]
            };

            if (cecPort != null)
                outputPort.Port = cecPort;

            OutputPorts.Add(outputPort);
        }

        /// <summary>
        /// 
        /// </summary>
        void AddVolumeControl(uint number, Audio.Output audio)
        {
            VolumeControls.Add(number, new DmCardAudioOutputController(audio));
        }

        void Dmps_DMInputChange(Switch device, DMInputEventArgs args)
        {
            Debug.Console(2, this, "DMInputChange Input: {0} EventId: {1}", args.Number, args.EventId.ToString());
            try
            {
                switch (args.EventId)
                {
                    case (DMInputEventIds.OnlineFeedbackEventId):
                        {
                            Debug.Console(2, this, "DM Input OnlineFeedbackEventId for input: {0}. State: {1}", args.Number, device.Inputs[args.Number].EndpointOnlineFeedback);
                            InputEndpointOnlineFeedbacks[args.Number].FireUpdate();
                            break;
                        }
                    case (DMInputEventIds.EndpointOnlineEventId):
                        {
                            Debug.Console(2, this, "DM Input EndpointOnlineEventId for input: {0}. State: {1}", args.Number, device.Inputs[args.Number].EndpointOnlineFeedback);
                            InputEndpointOnlineFeedbacks[args.Number].FireUpdate();
                            break;
                        }
                    case (DMInputEventIds.VideoDetectedEventId):
                        {
                            Debug.Console(2, this, "DM Input {0} VideoDetectedEventId", args.Number);
                            VideoInputSyncFeedbacks[args.Number].FireUpdate();
                            break;
                        }
                    case (DMInputEventIds.InputNameEventId):
                        {
                            Debug.Console(2, this, "DM Input {0} NameFeedbackEventId", args.Number);
                            if(InputNameFeedbacks.ContainsKey(args.Number))
                            {
                                InputNameFeedbacks[args.Number].FireUpdate();
                            }
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Debug.Console(0, Debug.ErrorLogLevel.Notice, "DMSwitch Input Change:{0} Input:{1} Event:{2}\rException: {3}", this.Name, args.Number, args.EventId.ToString(), e.ToString());
            }
        }
        void Dmps_DMOutputChange(Switch device, DMOutputEventArgs args)
        {
            if (args.EventId == DMOutputEventIds.OutputVuFeedBackEventId)
            {
                //Frequently called event that isn't needed
                return;
            }

            Debug.Console(2, this, "DMOutputChange Output: {0} EventId: {1}", args.Number, args.EventId.ToString());
            var output = args.Number;

            DMOutput outputCard = Dmps.SwitcherOutputs[output] as DMOutput;

            if (args.EventId == DMOutputEventIds.VolumeEventId && VolumeControls.ContainsKey(output))
            {
                VolumeControls[args.Number].VolumeEventFromChassis();
            }
            else if (args.EventId == DMOutputEventIds.OnlineFeedbackEventId
                && OutputEndpointOnlineFeedbacks.ContainsKey(output))
            {
                OutputEndpointOnlineFeedbacks[output].FireUpdate();
            }
            else if (args.EventId == DMOutputEventIds.EndpointOnlineEventId
                && OutputEndpointOnlineFeedbacks.ContainsKey(output))
            {
                OutputEndpointOnlineFeedbacks[output].FireUpdate();
            }
            else if (args.EventId == DMOutputEventIds.VideoOutEventId)
            {
                if (outputCard != null && outputCard.VideoOutFeedback != null)
                {
                    Debug.Console(2, this, "DMSwitchVideo:{0} Routed Input:{1} Output:{2}'", this.Name, outputCard.VideoOutFeedback.Number, output);
                }
                if (VideoOutputFeedbacks.ContainsKey(output))
                {
                    VideoOutputFeedbacks[output].FireUpdate();
                }
                if (OutputVideoRouteNameFeedbacks.ContainsKey(output))
                {
                    OutputVideoRouteNameFeedbacks[output].FireUpdate();
                }
                if (outputCard is Card.Dmps3DmOutputBackend || outputCard is Card.Dmps3HdmiOutputBackend)
                {
                    if (AudioOutputFeedbacks.ContainsKey(output))
                    {
                        AudioOutputFeedbacks[output].FireUpdate();
                    }
                    if (OutputAudioRouteNameFeedbacks.ContainsKey(output))
                    {
                        OutputAudioRouteNameFeedbacks[output].FireUpdate();
                    }
                }
            }
            else if (args.EventId == DMOutputEventIds.AudioOutEventId)
            {
                if (!Global.ControlSystemIsDmps4k3xxType)
                {
                    if (outputCard != null && outputCard.AudioOutFeedback != null)
                    {
                        Debug.Console(2, this, "DMSwitchAudio:{0} Routed Input:{1} Output:{2}'", this.Name,
                            outputCard.AudioOutFeedback.Number, output);
                    }
                }
                else
                {
                    if (outputCard != null)
                    {
                        if (outputCard is Card.Dmps3DmOutputBackend || outputCard is Card.Dmps3HdmiOutputBackend)
                        {
                            DigitalAudioOutputs[output].AudioSourceNumericFeedback.FireUpdate();
                        }
                        else
                        {
                            Debug.Console(2, this, "DMSwitchAudio:{0} Routed Input:{1} Output:{2}'", Name,
                                outputCard.AudioOutSourceFeedback, output);
                        }
                    }
                }
                if (AudioOutputFeedbacks.ContainsKey(output))
                {
                    AudioOutputFeedbacks[output].FireUpdate();
                }
                if (OutputAudioRouteNameFeedbacks.ContainsKey(output))
                {
                    OutputAudioRouteNameFeedbacks[output].FireUpdate();
                }
            }
            else if (args.EventId == DMOutputEventIds.OutputNameEventId
                && OutputNameFeedbacks.ContainsKey(output))
            {
                OutputNameFeedbacks[output].FireUpdate();
            }
            else if (args.EventId == DMOutputEventIds.DigitalMixerAudioSourceFeedBackEventId)
            {
                if (AudioOutputFeedbacks.ContainsKey(output))
                {
                    AudioOutputFeedbacks[output].FireUpdate();
                }
                if (OutputAudioRouteNameFeedbacks.ContainsKey(output))
                {
                    OutputAudioRouteNameFeedbacks[output].FireUpdate();
                }
            }
        }

        void Dmps_DMSystemChange(Switch device, DMSystemEventArgs args)
        {
            switch (args.EventId)
            {
                case DMSystemEventIds.SystemPowerOnEventId:
                case DMSystemEventIds.SystemPowerOffEventId:
                {
                    SystemPowerOnFeedback.FireUpdate();
                    SystemPowerOffFeedback.FireUpdate();
                    break;
                }
                case DMSystemEventIds.FrontPanelLockOnEventId:
                case DMSystemEventIds.FrontPanelLockOffEventId:
                {
                    FrontPanelLockOnFeedback.FireUpdate();
                    FrontPanelLockOffFeedback.FireUpdate();
                    break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pnt"></param>
        void StartOffTimer(PortNumberType pnt)
        {
            if (RouteOffTimers.ContainsKey(pnt))
                return;
            RouteOffTimers[pnt] = new CTimer(o => ExecuteSwitch(null, pnt.Selector, pnt.Type), RouteOffTime);
        }

        #region IRouting Members

        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType sigType)
        {
            try
            {
                if (EnableRouting == false)
                {
                    return;
                }

                Debug.Console(2, this, "Attempting a DM route from input {0} to output {1} {2}", inputSelector, outputSelector, sigType);

                var input = inputSelector as DMInput;
                var output = outputSelector as DMOutput;

                if (output == null)
                {
                    Debug.Console(0, this, Debug.ErrorLogLevel.Warning,
                        "Unable to execute switch for inputSelector {0} to outputSelector {1}", inputSelector,
                        outputSelector);
                    return;
                }

                var sigTypeIsUsbOrVideo = ((sigType & eRoutingSignalType.Video) == eRoutingSignalType.Video) ||
                                          ((sigType & eRoutingSignalType.UsbInput) == eRoutingSignalType.UsbInput) ||
                                          ((sigType & eRoutingSignalType.UsbOutput) == eRoutingSignalType.UsbOutput);

                if (input == null || (input.Number <= Dmps.NumberOfSwitcherInputs && output.Number <= Dmps.NumberOfSwitcherOutputs &&
                     sigTypeIsUsbOrVideo) ||
                    (input.Number <= (Dmps.NumberOfSwitcherInputs) && output.Number <= Dmps.NumberOfSwitcherOutputs &&
                     (sigType & eRoutingSignalType.Audio) == eRoutingSignalType.Audio))
                {
                    // Check to see if there's an off timer waiting on this and if so, cancel
                    var key = new PortNumberType(output, sigType);
                    if (input == null)
                    {
                        StartOffTimer(key);
                    }
                    else if (key.Number > 0)
                    {
                        if (RouteOffTimers.ContainsKey(key))
                        {
                            Debug.Console(2, this, "{0} cancelling route off due to new source", output);
                            RouteOffTimers[key].Stop();
                            RouteOffTimers.Remove(key);
                        }
                    }

                    // NOTE THAT BITWISE COMPARISONS - TO CATCH ALL ROUTING TYPES 
                    if ((sigType & eRoutingSignalType.Video) == eRoutingSignalType.Video)
                    {
                            output.VideoOut = input;
                    }

                    if ((sigType & eRoutingSignalType.Audio) == eRoutingSignalType.Audio)
                    {
                        if (!Global.ControlSystemIsDmps4k3xxType)
                        {
                            output.AudioOut = input;
                        }
                        else
                        {
                            if (input == null)
                            {
                                output.AudioOutSource = eDmps34KAudioOutSource.NoRoute;
                            }
                            else if (input.CardInputOutputType == eCardInputOutputType.Dmps3AirMediaInput || input.CardInputOutputType == eCardInputOutputType.Dmps3AirMediaNoStreamingInput)
                            {
                                //Special case for weird AirMedia indexing
                                if (Dmps.SystemControl.SystemControlType == eSystemControlType.Dmps34K250CSystemControl)
                                    output.AudioOutSource = eDmps34KAudioOutSource.AirMedia8;
                                else if (Dmps.SystemControl.SystemControlType == eSystemControlType.Dmps34K350CSystemControl)
                                    output.AudioOutSource = eDmps34KAudioOutSource.AirMedia9;
                            }
                            else if (input.Number < Dmps.SwitcherInputs.Count)
                            {
                                //Shift video inputs by 5 for weird DMPS3-4K indexing
                                output.AudioOutSource = (eDmps34KAudioOutSource)(input.Number + 5);
                            }
                            else
                            {
                                //Shift analog inputs back to inputs 1-5
                                output.AudioOutSource = (eDmps34KAudioOutSource)(input.Number - Dmps.SwitcherInputs.Count + 1);
                            }
                        }
                    }

                    if ((sigType & eRoutingSignalType.UsbOutput) == eRoutingSignalType.UsbOutput)
                    {
                            output.USBRoutedTo = input;
                    }

                    if ((sigType & eRoutingSignalType.UsbInput) != eRoutingSignalType.UsbInput)
                    {
                        return;
                    }
                    if (input != null)
                        input.USBRoutedTo = output;
                }
                else
                {
                    Debug.Console(1, this, "Unable to execute route from input {0} to output {1}", inputSelector,
                        outputSelector);
                }
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error executing switch: {0}", e);
            }
        }

        #endregion

        #region IRoutingNumeric Members

        public void ExecuteNumericSwitch(ushort inputSelector, ushort outputSelector, eRoutingSignalType sigType)
        {
            if (EnableRouting == false)
            {
                return;
            }

            Debug.Console(1, this, "Attempting a numeric switch from input {0} to output {1} {2}", inputSelector, outputSelector, sigType);

            if((sigType & eRoutingSignalType.Video) == eRoutingSignalType.Video)
            {
                if (inputSelector <= Dmps.SwitcherInputs.Count && outputSelector <= Dmps.SwitcherOutputs.Count)
                {
                    var input = inputSelector == 0 ? null : Dmps.SwitcherInputs[inputSelector];
                    var output = Dmps.SwitcherOutputs[outputSelector];

                    ExecuteSwitch(input, output, sigType);
                }
            }
            else if ((sigType & eRoutingSignalType.Audio) == eRoutingSignalType.Audio)
            {
                //Special case for DMPS-4K digital audio output
                if (Global.ControlSystemIsDmps4k3xxType)
                {
                    if (DigitalAudioOutputs.ContainsKey(outputSelector))
                    {
                        if (inputSelector == 0) //Clear action
                        {
                            DigitalAudioOutputs[outputSelector].ExecuteNumericSwitch(0, 0, eRoutingSignalType.Audio);
                        }
                        else if (inputSelector < Dmps.SwitcherInputs.Count) //DMPS-4K video inputs, set to audio follows video
                        {
                            DigitalAudioOutputs[outputSelector].ExecuteNumericSwitch(3, 0, eRoutingSignalType.Audio);
                            //Force video route since it is now set so audio follows video
                            ExecuteNumericSwitch(inputSelector, outputSelector, eRoutingSignalType.Video);
                        }
                        else if (inputSelector == Dmps.SwitcherInputs.Count + 5)
                        {
                            //Set to mix 1
                            DigitalAudioOutputs[outputSelector].ExecuteNumericSwitch(1, 0, eRoutingSignalType.Audio);
                        }
                        else if (inputSelector == Dmps.SwitcherInputs.Count + 6)
                        {
                            //Set to mix 2
                            DigitalAudioOutputs[outputSelector].ExecuteNumericSwitch(2, 0, eRoutingSignalType.Audio);
                        }
                    }
                    else if (inputSelector <= (Dmps.SwitcherInputs.Count + 4) && outputSelector <= Dmps.SwitcherOutputs.Count)
                    {
                        var output = Dmps.SwitcherOutputs[outputSelector] as DMOutput;
                        if (inputSelector == 0)
                        {
                            output.AudioOutSource = eDmps34KAudioOutSource.NoRoute;
                        }
                        else if(inputSelector >= (Dmps.SwitcherInputs.Count))
                        {
                            //Shift analog inputs back to inputs 1-5
                            Debug.Console(1, this, "Attempting analog route input {0} to output {1}", inputSelector - Dmps.SwitcherInputs.Count + 1, outputSelector);
                            output.AudioOutSource = (eDmps34KAudioOutSource)(inputSelector - Dmps.SwitcherInputs.Count + 1);
                        }
                        else if (inputSelector < Dmps.SwitcherInputs.Count)
                        {
                            var input = Dmps.SwitcherInputs[inputSelector] as DMInput;
                            if (input.CardInputOutputType == eCardInputOutputType.Dmps3AirMediaInput || input.CardInputOutputType == eCardInputOutputType.Dmps3AirMediaNoStreamingInput)
                            {
                                //Special case for weird AirMedia indexing
                                if (Dmps.SystemControl.SystemControlType == eSystemControlType.Dmps34K250CSystemControl)
                                    output.AudioOutSource = eDmps34KAudioOutSource.AirMedia8;
                                else if (Dmps.SystemControl.SystemControlType == eSystemControlType.Dmps34K350CSystemControl)
                                    output.AudioOutSource = eDmps34KAudioOutSource.AirMedia9;
                            }
                            else
                            {
                                //Shift video inputs by 5 for weird DMPS3-4K indexing
                                output.AudioOutSource = (eDmps34KAudioOutSource)(inputSelector + 5);
                            }
                        }
                    }
                }

                else if (inputSelector <= Dmps.SwitcherInputs.Count && outputSelector <= Dmps.SwitcherOutputs.Count)
                {
                    var output = Dmps.SwitcherOutputs[outputSelector] as DMOutput;
                    var input = inputSelector == 0 ? null : Dmps.SwitcherInputs[inputSelector] as DMInput;
                    output.AudioOut = input;
                }
            }
        }

        #endregion
    }
}