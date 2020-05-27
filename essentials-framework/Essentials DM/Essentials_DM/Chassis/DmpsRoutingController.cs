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
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.DM.Config;

using Feedback = PepperDash.Essentials.Core.Feedback;

namespace PepperDash.Essentials.DM
{
    public class DmpsRoutingController : EssentialsBridgeableDevice, IRoutingNumeric, IHasFeedback
    {
        public CrestronControlSystem Dmps { get; set; }
        public ISystemControl SystemControl { get; private set; }

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

        public const int RouteOffTime = 500;
        Dictionary<PortNumberType, CTimer> RouteOffTimers = new Dictionary<PortNumberType, CTimer>();

        /// <summary>
        /// Text that represents when an output has no source routed to it
        /// </summary>
        public string NoRouteText = "";

        public static DmpsRoutingController GetDmpsRoutingController(string key, string name,
            DmpsRoutingPropertiesConfig properties)
        {
            try
            {
                ISystemControl systemControl = null;
 
                systemControl = Global.ControlSystem.SystemControl as ISystemControl;

                if (systemControl == null)
                {
                    return null;
                }

                var controller = new DmpsRoutingController(key, name, systemControl);

                controller.InputNames = properties.InputNames;
                controller.OutputNames = properties.OutputNames;

                if (!string.IsNullOrEmpty(properties.NoRouteText))
                    controller.NoRouteText = properties.NoRouteText;

                return controller;

            }
            catch (System.Exception e)
            {
                Debug.Console(0, "Error getting DMPS Controller:\r{0}", e);
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
            SystemControl = systemControl;

            InputPorts = new RoutingPortCollection<RoutingInputPort>();
            OutputPorts = new RoutingPortCollection<RoutingOutputPort>();
            VolumeControls = new Dictionary<uint, DmCardAudioOutputController>();
            TxDictionary = new Dictionary<uint, string>();
            RxDictionary = new Dictionary<uint, string>();

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

            SetupOutputCards();

            SetupInputCards();
        }

        public override bool CustomActivate()
        {
            // Set input and output names from config
            if (InputNames != null)
            {
                foreach (var kvp in InputNames)
                {
                    var input = (Dmps.SwitcherInputs[kvp.Key] as DMInput);
                    if (input != null)
                        input.Name.StringValue = kvp.Value;
                }
            }
            if (OutputNames != null)
            {
                foreach (var kvp in OutputNames)
                {
                    var output = (Dmps.SwitcherOutputs[kvp.Key] as DMOutput);
                    if (output != null)
                        output.Name.StringValue = kvp.Value;
                }
            }

            // Subscribe to events
            Dmps.DMInputChange += new DMInputEventHandler(Dmps_DMInputChange);
            Dmps.DMOutputChange += new DMOutputEventHandler(Dmps_DMOutputChange);

            return base.CustomActivate();
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new DmpsRoutingControllerJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<DmpsRoutingControllerJoinMap>(joinMapSerialized);

            bridge.AddJoinMap(Key, joinMap);

            Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            // Link up outputs
            for (uint i = 1; i <= Dmps.NumberOfSwitcherInputs; i++)
            {
                Debug.Console(2, this, "Linking Input Card {0}", i);

                var ioSlot = i;
                var ioSlotJoin = ioSlot - 1;

                //if (TxDictionary.ContainsKey(ioSlot))
                //{
                //    Debug.Console(2, "Creating Tx Feedbacks {0}", ioSlot);
                //    var TxKey = TxDictionary[ioSlot];
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

                if (VideoInputSyncFeedbacks[ioSlot] != null)
                    VideoInputSyncFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.VideoSyncStatus.JoinNumber + ioSlotJoin]);

                if (InputNameFeedbacks[ioSlot] != null)
                    InputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.InputNames.JoinNumber + ioSlotJoin]);

                trilist.SetStringSigAction(joinMap.InputNames.JoinNumber + ioSlotJoin, new Action<string>(s =>
                {
                    var inputCard = Dmps.SwitcherInputs[ioSlot] as DMInput;

                    if (inputCard != null)
                    {
                        if (inputCard.NameFeedback != null && !string.IsNullOrEmpty(inputCard.NameFeedback.StringValue) && inputCard.NameFeedback.StringValue != s)
                            if (inputCard.Name != null)
                                inputCard.Name.StringValue = s;
                    }
                }));


                if (InputEndpointOnlineFeedbacks[ioSlot] != null)
                    InputEndpointOnlineFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.InputEndpointOnline.JoinNumber + ioSlotJoin]);
            }

            for (uint i = 1; i <= Dmps.NumberOfSwitcherOutputs; i++)
            {
                Debug.Console(2, this, "Linking Output Card {0}", i);

                var ioSlot = i;
                var ioSlotJoin = ioSlot - 1;

                // Control
                trilist.SetUShortSigAction(joinMap.OutputVideo.JoinNumber + ioSlotJoin, o => ExecuteSwitch(o, ioSlot, eRoutingSignalType.Video));
                trilist.SetUShortSigAction(joinMap.OutputAudio.JoinNumber + ioSlotJoin, o => ExecuteSwitch(o, ioSlot, eRoutingSignalType.Audio));

                trilist.SetStringSigAction(joinMap.OutputNames.JoinNumber + ioSlotJoin, s =>
                {
                    var outputCard = Dmps.SwitcherOutputs[ioSlot] as DMOutput;

                    //Debug.Console(2, dmpsRouter, "Output Name String Sig Action for Output Card  {0}", ioSlot);

                    if (outputCard != null)
                    {
                        //Debug.Console(2, dmpsRouter, "Card Type: {0}", outputCard.CardInputOutputType);

                        if (!(outputCard is Card.Dmps3CodecOutput) && outputCard.NameFeedback != null)
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
                });

                // Feedback
                if (VideoOutputFeedbacks[ioSlot] != null)
                    VideoOutputFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.OutputVideo.JoinNumber + ioSlotJoin]);
                if (AudioOutputFeedbacks[ioSlot] != null)
                    AudioOutputFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.OutputAudio.JoinNumber + ioSlotJoin]);
                if (OutputNameFeedbacks[ioSlot] != null)
                    OutputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputNames.JoinNumber + ioSlotJoin]);
                if (OutputVideoRouteNameFeedbacks[ioSlot] != null)
                    OutputVideoRouteNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputCurrentVideoInputNames.JoinNumber + ioSlotJoin]);
                if (OutputAudioRouteNameFeedbacks[ioSlot] != null)
                    OutputAudioRouteNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputCurrentAudioInputNames.JoinNumber + ioSlotJoin]);
                if (OutputEndpointOnlineFeedbacks[ioSlot] != null)
                    OutputEndpointOnlineFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.OutputEndpointOnline.JoinNumber + ioSlotJoin]);
            }
        }


        /// <summary>
        /// Iterate the SwitcherOutputs collection to setup feedbacks and add routing ports
        /// </summary>
        void SetupOutputCards()
        {
            foreach (var card in Dmps.SwitcherOutputs)
            {

                var outputCard = card as DMOutput;

                Debug.Console(1, this, "Adding Output Card Number {0} Type: {1}", outputCard.Number, outputCard.CardInputOutputType.ToString());

                if (outputCard != null)
                {
                    VideoOutputFeedbacks[outputCard.Number] = new IntFeedback(() =>
                    {
                        if (outputCard.VideoOutFeedback != null) { return (ushort)outputCard.VideoOutFeedback.Number; }
                        else { return 0; };
                    });
                    AudioOutputFeedbacks[outputCard.Number] = new IntFeedback(() =>
                    {
                        if (outputCard.AudioOutFeedback != null) { return (ushort)outputCard.AudioOutFeedback.Number; }
                        else { return 0; };
                    });

                    OutputNameFeedbacks[outputCard.Number] = new StringFeedback(() =>
                    {
                        if (outputCard.NameFeedback != null && !string.IsNullOrEmpty(outputCard.NameFeedback.StringValue))
                        {
                            Debug.Console(2, this, "Output Card {0} Name: {1}", outputCard.Number, outputCard.NameFeedback.StringValue);
                            return outputCard.NameFeedback.StringValue;
                        }
                        else
                        {
                            return "";
                        }
                    });

                    OutputVideoRouteNameFeedbacks[outputCard.Number] = new StringFeedback(() =>
                    {
                        if (outputCard.VideoOutFeedback != null && outputCard.VideoOutFeedback.NameFeedback != null)
                        {
                            return outputCard.VideoOutFeedback.NameFeedback.StringValue;
                        }
                        else
                        {
                            return NoRouteText;
                        }
                    });
                    OutputAudioRouteNameFeedbacks[outputCard.Number] = new StringFeedback(() =>
                    {
                        if (outputCard.AudioOutFeedback != null && outputCard.AudioOutFeedback.NameFeedback != null)
                        {
                            return outputCard.AudioOutFeedback.NameFeedback.StringValue;
                        }
                        else
                        {
                            return NoRouteText;

                        }
                    });

                    OutputEndpointOnlineFeedbacks[outputCard.Number] = new BoolFeedback(() => { return outputCard.EndpointOnlineFeedback; });

                    AddOutputCard(outputCard.Number, outputCard);
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

                    InputEndpointOnlineFeedbacks[inputCard.Number] = new BoolFeedback(() => { return inputCard.EndpointOnlineFeedback; });

                    if (inputCard.VideoDetectedFeedback != null)
                    {
                        VideoInputSyncFeedbacks[inputCard.Number] = new BoolFeedback(() =>
                        {
                            return inputCard.VideoDetectedFeedback.BoolValue;
                        });
                    }

                    InputNameFeedbacks[inputCard.Number] = new StringFeedback(() =>
                    {
                        if (inputCard.NameFeedback != null && !string.IsNullOrEmpty(inputCard.NameFeedback.StringValue))
                        {
                            Debug.Console(2, this, "Input Card {0} Name: {1}", inputCard.Number, inputCard.NameFeedback.StringValue);
                            return inputCard.NameFeedback.StringValue;

                        }
                        else
                        {
                            Debug.Console(2, this, "Input Card {0} Name is null", inputCard.Number);
                            return "";
                        }
                    });

                    AddInputCard(inputCard.Number, inputCard);
                }
                else
                {
                    Debug.Console(2, this, "***********Input Card of type {0} is cannot be cast as DMInput*************", card.CardInputOutputType);
                }
            }
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
            else if (inputCard is Card.Dmps3AirMediaInput)
            {
                var airMediaInputCard = inputCard as Card.Dmps3AirMediaInput;

                AddInputPortWithDebug(number, string.Format("AirMediaIn{0}", number), eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Streaming);
            }
        }


        /// <summary>
        /// Adds InputPort
        /// </summary>
        void AddInputPortWithDebug(uint cardNum, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType)
        {
            var portKey = string.Format("inputCard{0}--{1}", cardNum, portName);
            Debug.Console(2, this, "Adding input port '{0}'", portKey);
            var inputPort = new RoutingInputPort(portKey, sigType, portType, cardNum, this);

            InputPorts.Add(inputPort);
        }

        /// <summary>
        /// Adds InputPort and sets Port as ICec object
        /// </summary>
        void AddInputPortWithDebug(uint cardNum, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType, ICec cecPort)
        {
            var portKey = string.Format("inputCard{0}--{1}", cardNum, portName);
            Debug.Console(2, this, "Adding input port '{0}'", portKey);
            var inputPort = new RoutingInputPort(portKey, sigType, portType, cardNum, this);

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

                return;
            }
            else if (outputCard is Card.Dmps3DmOutput)
            {
                var dmOutputCard = outputCard as Card.Dmps3DmOutput;

                var cecPort = dmOutputCard.DmOutputPort;

                AddDmOutputPort(number);
                return;
            }
            else if (outputCard is Card.Dmps3ProgramOutput)
            {
                AddAudioOnlyOutputPort(number, "Program");

                var programOutput = new DmpsAudioOutputController(string.Format("processor-programAudioOutput"), "Program Audio Output", outputCard as Card.Dmps3OutputBase);

                DeviceManager.AddDevice(programOutput);

                return;
            }
            else if (outputCard is Card.Dmps3AuxOutput)
            {
                if (outputCard.CardInputOutputType == eCardInputOutputType.Dmps3Aux1Output)
                {
                    AddAudioOnlyOutputPort(number, "Aux1");

                    var aux1Output = new DmpsAudioOutputController(string.Format("processor-aux1AudioOutput"), "Program Audio Output", outputCard as Card.Dmps3OutputBase);

                    DeviceManager.AddDevice(aux1Output);
                }
                else if (outputCard.CardInputOutputType == eCardInputOutputType.Dmps3Aux2Output)
                {
                    AddAudioOnlyOutputPort(number, "Aux2");

                    var aux2Output = new DmpsAudioOutputController(string.Format("processor-aux2AudioOutput"), "Program Audio Output", outputCard as Card.Dmps3OutputBase);

                    DeviceManager.AddDevice(aux2Output);
                }
                return;
            }
            else if (outputCard is Card.Dmps3CodecOutput)
            {
                if (number == (uint)CrestronControlSystem.eDmps300cOutputs.Codec1 
                    || number == (uint)CrestronControlSystem.eDmps3200cOutputs.Codec1
                    || number == (uint)CrestronControlSystem.eDmps3300cAecOutputs.Codec1
                    || number == (uint)CrestronControlSystem.eDmps34K250COutputs.Codec1
                    || number == (uint)CrestronControlSystem.eDmps34K350COutputs.Codec1)
                    AddAudioOnlyOutputPort(number, CrestronControlSystem.eDmps300cOutputs.Codec1.ToString());
                else if (number == (uint)CrestronControlSystem.eDmps300cOutputs.Codec2
                    || number == (uint)CrestronControlSystem.eDmps3200cOutputs.Codec2
                    || number == (uint)CrestronControlSystem.eDmps3300cAecOutputs.Codec2
                    || number == (uint)CrestronControlSystem.eDmps34K250COutputs.Codec2
                    || number == (uint)CrestronControlSystem.eDmps34K350COutputs.Codec2)
                    AddAudioOnlyOutputPort(number, CrestronControlSystem.eDmps300cOutputs.Codec2.ToString());
                return;
            }
            else if (outputCard is Card.Dmps3DialerOutput)
            {
                AddAudioOnlyOutputPort(number, "Dialer");
                return;
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
                return;
            }
            else if (outputCard is Card.Dmps3AecOutput)
            {
                AddAudioOnlyOutputPort(number, "Aec");
                return;
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
            AddOutputPortWithDebug(number, portName, eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio, number);
        }

        /// <summary>
        /// Adds an HDMI output port
        /// </summary>
        /// <param name="number"></param>
        /// <param name="cecPort"></param>
        void AddHdmiOutputPort(uint number, ICec cecPort)
        {
            AddOutputPortWithDebug(number, string.Format("hdmiOut{0}", number), eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, number, cecPort);
        }

        /// <summary>
        /// Adds a DM output port
        /// </summary>
        /// <param name="number"></param>
        void AddDmOutputPort(uint number)
        {
            AddOutputPortWithDebug(number, string.Format("dmOut{0}", number), eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmCat, number);
        }

        /// <summary>
        /// Adds OutputPort
        /// </summary>
        void AddOutputPortWithDebug(uint cardNum, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType, object selector)
        {
            var portKey = string.Format("outputCard{0}--{1}", cardNum, portName);
            Debug.Console(2, this, "Adding output port '{0}'", portKey);
            OutputPorts.Add(new RoutingOutputPort(portKey, sigType, portType, selector, this));
        }

        /// <summary>
        /// Adds OutputPort and sets Port as ICec object
        /// </summary>
        void AddOutputPortWithDebug(uint cardNum, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType, object selector, ICec cecPort)
        {
            var portKey = string.Format("outputCard{0}--{1}", cardNum, portName);
            Debug.Console(2, this, "Adding output port '{0}'", portKey);
            var outputPort = new RoutingOutputPort(portKey, sigType, portType, selector, this);

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
            //Debug.Console(2, this, "DMSwitch:{0} Input:{1} Event:{2}'", this.Name, args.Number, args.EventId.ToString());

            switch (args.EventId)
            {
                case (DMInputEventIds.OnlineFeedbackEventId):
                    {
                        Debug.Console(2, this, "DM Input OnlineFeedbackEventId for input: {0}. State: {1}", args.Number, device.Inputs[args.Number].EndpointOnlineFeedback);
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
                        InputNameFeedbacks[args.Number].FireUpdate();
                        break;
                    }
            }
        }
        void Dmps_DMOutputChange(Switch device, DMOutputEventArgs args)
        {
            Debug.Console(2, this, "DMOutputChange Output: {0} EventId: {1}", args.Number, args.EventId.ToString());

            var output = args.Number;

            DMOutput outputCard = Dmps.SwitcherOutputs[output] as DMOutput;

            if (args.EventId == DMOutputEventIds.VolumeEventId &&
                VolumeControls.ContainsKey(output))
            {
                VolumeControls[args.Number].VolumeEventFromChassis();
            }
            else if (args.EventId == DMOutputEventIds.OnlineFeedbackEventId
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
            }
            else if (args.EventId == DMOutputEventIds.AudioOutEventId)
            {
                if (outputCard != null && outputCard.AudioOutFeedback != null)
                {
                    Debug.Console(2, this, "DMSwitchAudio:{0} Routed Input:{1} Output:{2}'", this.Name, outputCard.AudioOutFeedback.Number, output);
                }
                if (AudioOutputFeedbacks.ContainsKey(output))
                {
                    AudioOutputFeedbacks[output].FireUpdate();
                }
            }
            else if (args.EventId == DMOutputEventIds.OutputNameEventId
                && OutputNameFeedbacks.ContainsKey(output))
            {
                Debug.Console(2, this, "DM Output {0} NameFeedbackEventId", output);
                OutputNameFeedbacks[output].FireUpdate();
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
            RouteOffTimers[pnt] = new CTimer(o => ExecuteSwitch(0, pnt.Number, pnt.Type), RouteOffTime);
        }

        #region IRouting Members

        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType sigType)
        {
            try
            {

                Debug.Console(2, this, "Attempting a DM route from input {0} to output {1} {2}", inputSelector, outputSelector, sigType);

                var input = Convert.ToUInt32(inputSelector); // Cast can sometimes fail
                var output = Convert.ToUInt32(outputSelector);

                if (input <= Dmps.NumberOfSwitcherInputs && output <= Dmps.NumberOfSwitcherOutputs)
                {
                    // Check to see if there's an off timer waiting on this and if so, cancel
                    var key = new PortNumberType(output, sigType);
                    if (input == 0)
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

                    DMInput inCard = input == 0 ? null : Dmps.SwitcherInputs[input] as DMInput;
                    DMOutput outCard = output == 0 ? null : Dmps.SwitcherOutputs[output] as DMOutput;

                    //if (inCard != null)
                    //{
                        // NOTE THAT BITWISE COMPARISONS - TO CATCH ALL ROUTING TYPES 
                        if ((sigType | eRoutingSignalType.Video) == eRoutingSignalType.Video)
                        {

                            //SystemControl.VideoEnter.BoolValue = true;
                            if (outCard != null)
                                outCard.VideoOut = inCard;
                        }

                        if ((sigType | eRoutingSignalType.Audio) == eRoutingSignalType.Audio)
                        {
                            if (outCard != null)
                                outCard.AudioOut = inCard;
                        }

                        if ((sigType | eRoutingSignalType.UsbOutput) == eRoutingSignalType.UsbOutput)
                        {
                            if (outCard != null)
                                outCard.USBRoutedTo = inCard;
                        }

                        if ((sigType | eRoutingSignalType.UsbInput) == eRoutingSignalType.UsbInput)
                        {
                            if (inCard != null)
                                inCard.USBRoutedTo = outCard;
                        }
                    //}
                    //else
                    //{
                    //    Debug.Console(1, this, "Unable to execute route from input {0} to output {1}.  Input card not available", inputSelector, outputSelector);
                    //}

                }
                else
                {
                    Debug.Console(1, this, "Unable to execute route from input {0} to output {1}", inputSelector, outputSelector);
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
            ExecuteSwitch(inputSelector, outputSelector, sigType);
        }

        #endregion
    }
}