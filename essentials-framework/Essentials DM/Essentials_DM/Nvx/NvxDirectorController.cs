//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DM;
//using Crestron.SimplSharpPro.DM.Cards;
//using Crestron.SimplSharpPro.DM.Streaming;
//using Crestron.SimplSharpPro.DM.Endpoints;
//using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

//using PepperDash.Core;
//using PepperDash.Essentials.Core;
//using PepperDash.Essentials.DM.Cards;
//using PepperDash.Essentials.DM.Config;

//namespace PepperDash.Essentials.DM
//{
//    /// <summary>
//    /// Builds a controller for basic DM-RMCs with Com and IR ports and no control functions
//    /// 
//    /// </summary>
//    public class NvxDirectorController : CrestronGenericBaseDevice, IRoutingInputsOutputs, IRouting, IHasFeedback//, ICardPortsDevice
//    {
//        public NvxDirectorController Chassis { get; private set; }
		
//        // Feedbacks for EssentialDM
//        public Dictionary<uint, IntFeedback> VideoOutputFeedbacks { get; private set; }
//        public Dictionary<uint, IntFeedback> AudioOutputFeedbacks { get; private set; }
//        public Dictionary<uint, BoolFeedback> VideoInputSyncFeedbacks { get; private set; }
//        public Dictionary<uint, BoolFeedback> InputEndpointOnlineFeedbacks { get; private set; }
//        public Dictionary<uint, BoolFeedback> OutputEndpointOnlineFeedbacks { get; private set; }
//        public Dictionary<uint, StringFeedback> InputNameFeedbacks { get; private set; }
//        public Dictionary<uint, StringFeedback> OutputNameFeedbacks { get; private set; }
//        public Dictionary<uint, StringFeedback> OutputVideoRouteNameFeedbacks { get; private set; }
//        public Dictionary<uint, StringFeedback> OutputAudioRouteNameFeedbacks { get; private set; }
		
		
//        // Need a couple Lists of generic Backplane ports
//        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }
//        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

//        public Dictionary<uint, string> TxDictionary { get; set; }
//        public Dictionary<uint, string> RxDictionary { get; set; }

//        //public Dictionary<uint, DmInputCardControllerBase> InputCards { get; private set; }
//        //public Dictionary<uint, DmSingleOutputCardControllerBase> OutputCards { get; private set; }

//        public Dictionary<uint, string> InputNames { get; set; }
//        public Dictionary<uint, string> OutputNames { get; set; }
//        public Dictionary<uint, DmCardAudioOutputController> VolumeControls { get; private set; }

//        public const int RouteOffTime = 500;
//        Dictionary<PortNumberType, CTimer> RouteOffTimers = new Dictionary<PortNumberType, CTimer>();

//        /// <summary>
//        /// Factory method to create a new chassis controller from config data. Limited to 8x8 right now
//        /// </summary>
//        public static NvxDirectorController GetNvxDirectorController(string key, string name,
//            string type, DMChassisPropertiesConfig properties)
//        {
//            try
//            {
//                type = type.ToLower();
//                uint ipid = properties.Control.IpIdInt; // Convert.ToUInt16(properties.Id, 16);
//                NvxDirectorController controller = null;
				

//                if (type == "dmmd8x8")
//                {
//                    controller = new NvxDirectorController(key, name, new DmMd8x8(ipid, Global.ControlSystem));

//                    // add the cards and port names
//                    foreach (var kvp in properties.InputSlots)
//                        controller.AddInputCard(kvp.Value, kvp.Key);
//                    foreach (var kvp in properties.OutputSlots) {
//                        controller.AddOutputCard(kvp.Value, kvp.Key);
						
//                        }

//                    foreach (var kvp in properties.VolumeControls)
//                    {
//                        // get the card
//                        // check it for an audio-compatible type
//                        // make a something-something that will make it work
//                        // retire to mountain village
//                        var outNum = kvp.Key;
//                        var card = controller.Chassis.Outputs[outNum].Card;
//                        Audio.Output audio = null;
//                        if (card is DmcHdo)
//                            audio = (card as DmcHdo).Audio;
//                        else if (card is Dmc4kHdo)
//                            audio = (card as Dmc4kHdo).Audio;
//                        if (audio == null)
//                            continue;
//                        // wire up the audio to something here...
//                        controller.AddVolumeControl(outNum, audio);
//                    }

//                    controller.InputNames = properties.InputNames;
//                    controller.OutputNames = properties.OutputNames;
//                    return controller;
//                }
//                else if (type == "dmmd16x16") {
//                    controller = new NvxDirectorController(key, name, new DmMd16x16(ipid, Global.ControlSystem));

//                    // add the cards and port names
//                    foreach (var kvp in properties.InputSlots)
//                        controller.AddInputCard(kvp.Value, kvp.Key);
//                    foreach (var kvp in properties.OutputSlots) {
//                        controller.AddOutputCard(kvp.Value, kvp.Key);

//                        }

//                    foreach (var kvp in properties.VolumeControls) {
//                        // get the card
//                        // check it for an audio-compatible type
//                        // make a something-something that will make it work
//                        // retire to mountain village
//                        var outNum = kvp.Key;
//                        var card = controller.Chassis.Outputs[outNum].Card;
//                        Audio.Output audio = null;
//                        if (card is DmcHdo)
//                            audio = (card as DmcHdo).Audio;
//                        else if (card is Dmc4kHdo)
//                            audio = (card as Dmc4kHdo).Audio;
//                        if (audio == null)
//                            continue;
//                        // wire up the audio to something here...
//                        controller.AddVolumeControl(outNum, audio);
//                        }

//                    controller.InputNames = properties.InputNames;
//                    controller.OutputNames = properties.OutputNames;
//                    return controller;
//                    }
//            }
//            catch (System.Exception e)
//            {
//                Debug.Console(0, "Error creating DM chassis:\r{0}", e);
//            }
//            return null;
//        }


//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="key"></param>
//        /// <param name="name"></param>
//        /// <param name="chassis"></param>
//        public NvxDirectorController(string key, string name, DmMDMnxn chassis)
//            : base(key, name, chassis)
//        {
//            Chassis = chassis;
//            InputPorts = new RoutingPortCollection<RoutingInputPort>();
//            OutputPorts = new RoutingPortCollection<RoutingOutputPort>();
//            VolumeControls = new Dictionary<uint, DmCardAudioOutputController>();
//            TxDictionary = new Dictionary<uint, string>();
//            RxDictionary = new Dictionary<uint, string>();
//            IsOnline.OutputChange += new EventHandler<FeedbackEventArgs>(IsOnline_OutputChange);
//            //IsOnline.OutputChange += new EventHandler<EventArgs>(this.IsOnline_OutputChange);
//            Chassis.DMInputChange += new DMInputEventHandler(Chassis_DMInputChange);
//            //Chassis.DMSystemChange += new DMSystemEventHandler(Chassis_DMSystemChange);
//            Chassis.DMOutputChange += new DMOutputEventHandler(Chassis_DMOutputChange);
//            VideoOutputFeedbacks = new Dictionary<uint, IntFeedback>();
//            AudioOutputFeedbacks = new Dictionary<uint, IntFeedback>();
//            VideoInputSyncFeedbacks = new Dictionary<uint, BoolFeedback>();
//            InputNameFeedbacks = new Dictionary<uint, StringFeedback>();
//            OutputNameFeedbacks = new Dictionary<uint, StringFeedback>();
//            OutputVideoRouteNameFeedbacks = new Dictionary<uint, StringFeedback>();
//            OutputAudioRouteNameFeedbacks = new Dictionary<uint, StringFeedback>();
//            InputEndpointOnlineFeedbacks = new Dictionary<uint, BoolFeedback>();
//            OutputEndpointOnlineFeedbacks = new Dictionary<uint, BoolFeedback>();

//            for (uint x = 1; x <= Chassis.NumberOfOutputs; x++) 
//            {
//                var tempX = x;

//                VideoOutputFeedbacks[tempX] = new IntFeedback(() => { 
//                    if (Chassis.Outputs[tempX].VideoOutFeedback != null) { return (ushort)Chassis.Outputs[tempX].VideoOutFeedback.Number;} 
//                    else { return 0; };
//                    });
//                AudioOutputFeedbacks[tempX] = new IntFeedback(() => {
//                    if (Chassis.Outputs[tempX].AudioOutFeedback != null) { return (ushort)Chassis.Outputs[tempX].AudioOutFeedback.Number; } 
//                    else { return 0; };
//                    });
//                VideoInputSyncFeedbacks[tempX] = new BoolFeedback(() => {			
//                    return Chassis.Inputs[tempX].VideoDetectedFeedback.BoolValue;
//                    });
//                InputNameFeedbacks[tempX] = new StringFeedback(() => {
//                        if (Chassis.Inputs[tempX].NameFeedback.StringValue != null) 
//                        {
//                             return Chassis.Inputs[tempX].NameFeedback.StringValue;
//                        }
//                        else 
//                        {
//                            return "";
//                        }				
//                    });
//                OutputNameFeedbacks[tempX] = new StringFeedback(() => {
//                        if (Chassis.Outputs[tempX].NameFeedback.StringValue != null) 
//                        {
//                            return Chassis.Outputs[tempX].NameFeedback.StringValue;
//                        }
//                        else 
//                        {
//                            return "";
//                        }
//                    });
//                OutputVideoRouteNameFeedbacks[tempX] = new StringFeedback(() => 
//                {
//                        if (Chassis.Outputs[tempX].VideoOutFeedback != null) 
//                        {
//                            return Chassis.Outputs[tempX].VideoOutFeedback.NameFeedback.StringValue;
//                        }
//                        else 
//                        {
//                            return "";
//                        }
//                    });
//                OutputAudioRouteNameFeedbacks[tempX] = new StringFeedback(() =>
//                    {
//                        if (Chassis.Outputs[tempX].AudioOutFeedback != null)
//                        {
//                            return Chassis.Outputs[tempX].AudioOutFeedback.NameFeedback.StringValue;
//                        }
//                        else
//                        {
//                            return "";

//                        }
//                    });
//                InputEndpointOnlineFeedbacks[tempX] = new BoolFeedback(() => { return Chassis.Inputs[tempX].EndpointOnlineFeedback; });

//                OutputEndpointOnlineFeedbacks[tempX] = new BoolFeedback(() => { return Chassis.Outputs[tempX].EndpointOnlineFeedback; });
//            }
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="type"></param>
//        /// <param name="number"></param>
//        public void AddInputCard(string type, uint number)
//        {
//            Debug.Console(2, this, "Adding input card '{0}', slot {1}", type, number);

//            if (type == "dmcHd")
//            {
//                var inputCard = new DmcHd(number, this.Chassis);
//                var cecPort = inputCard.HdmiInput as ICec;
//                AddHdmiInCardPorts(number, cecPort);
//            }
//            else if (type == "dmcHdDsp")
//            {
//                var inputCard = new DmcHdDsp(number, this.Chassis);
//                var cecPort = inputCard.HdmiInput as ICec;
//                AddHdmiInCardPorts(number, cecPort);
//            }
//            else if (type == "dmc4kHd")
//            {
//                var inputCard = new Dmc4kHd(number, this.Chassis);
//                var cecPort = inputCard.HdmiInput as ICec;
//                AddHdmiInCardPorts(number, cecPort);
//            }
//            else if (type == "dmc4kHdDsp")
//            {
//                var inputCard = new Dmc4kHdDsp(number, this.Chassis);
//                var cecPort = inputCard.HdmiInput as ICec;
//                AddHdmiInCardPorts(number, cecPort);
//            }
//            else if (type == "dmc4kzHd")
//            {
//                var inputCard = new Dmc4kzHd(number, this.Chassis);
//                var cecPort = inputCard.HdmiInput as ICec;
//                AddHdmiInCardPorts(number, cecPort);
//            }
//            else if (type == "dmc4kzHdDsp")
//            {
//                var inputCard = new Dmc4kzHdDsp(number, this.Chassis);
//                var cecPort = inputCard.HdmiInput as ICec;
//                AddHdmiInCardPorts(number, cecPort);
//            }
//            else if (type == "dmcC")
//            {
//                new DmcC(number, this.Chassis);
//                AddDmInCardPorts(number);
//            }
//            else if (type == "dmcCDsp")
//            {
//                new DmcCDsp(number, this.Chassis);
//                AddDmInCardPorts(number);
//            }
//            else if (type == "dmc4kC")
//            {
//                new Dmc4kC(number, this.Chassis);
//                AddDmInCardPorts(number);
//            }
//            else if (type == "dmc4kCDsp")
//            {
//                new Dmc4kCDsp(number, this.Chassis);
//                AddDmInCardPorts(number);
//            }
//            else if (type == "dmc4kzC")
//            {
//                new Dmc4kzC(number, this.Chassis);
//                AddDmInCardPorts(number);
//            }
//            else if (type == "dmc4kzCDsp")
//            {
//                new Dmc4kzCDsp(number, this.Chassis);
//                AddDmInCardPorts(number);
//            }
//            else if (type == "dmcCat")
//            {
//                new DmcCat(number, this.Chassis);
//                AddDmInCardPorts(number);
//            }
//            else if (type == "dmcCatDsp")
//            {
//                new DmcCatDsp(number, this.Chassis);
//                AddDmInCardPorts(number);
//            }
//            else if (type == "dmcS")
//            {
//                new DmcS(number, Chassis);
//                AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmMmFiber);
//                AddInCardHdmiAndAudioLoopPorts(number);
//            }
//            else if (type == "dmcSDsp")
//            {
//                new DmcSDsp(number, Chassis);
//                AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmMmFiber);
//                AddInCardHdmiAndAudioLoopPorts(number);
//            }
//            else if (type == "dmcS2")
//            {
//                new DmcS2(number, Chassis);
//                AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmSmFiber);
//                AddInCardHdmiAndAudioLoopPorts(number);
//            }
//            else if (type == "dmcS2Dsp")
//            {
//                new DmcS2Dsp(number, Chassis);
//                AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmSmFiber);
//                AddInCardHdmiAndAudioLoopPorts(number);
//            }
//            else if (type == "dmcSdi")
//            {
//                new DmcSdi(number, Chassis);
//                AddInputPortWithDebug(number, "sdiIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Sdi);
//                AddOutputPortWithDebug(number, "sdiOut", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Sdi, null);
//                AddInCardHdmiAndAudioLoopPorts(number);
//            }
//            else if (type == "dmcDvi")
//            {
//                new DmcDvi(number, Chassis);
//                AddInputPortWithDebug(number, "dviIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Dvi);
//                AddInputPortWithDebug(number, "audioIn", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio);
//                AddInCardHdmiLoopPort(number);
//            }
//            else if (type == "dmcVga")
//            {
//                new DmcVga(number, Chassis);
//                AddInputPortWithDebug(number, "vgaIn", eRoutingSignalType.Video, eRoutingPortConnectionType.Vga);
//                AddInputPortWithDebug(number, "audioIn", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio);
//                AddInCardHdmiLoopPort(number);
//            }
//            else if (type == "dmcVidBnc")
//            {
//                new DmcVidBnc(number, Chassis);
//                AddInputPortWithDebug(number, "componentIn", eRoutingSignalType.Video, eRoutingPortConnectionType.Component);
//                AddInputPortWithDebug(number, "audioIn", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio);
//                AddInCardHdmiLoopPort(number);
//            }
//            else if (type == "dmcVidRcaA")
//            {
//                new DmcVidRcaA(number, Chassis);
//                AddInputPortWithDebug(number, "componentIn", eRoutingSignalType.Video, eRoutingPortConnectionType.Component);
//                AddInputPortWithDebug(number, "audioIn", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio);
//                AddInCardHdmiLoopPort(number);
//            }
//            else if (type == "dmcVidRcaD")
//            {
//                new DmcVidRcaD(number, Chassis);
//                AddInputPortWithDebug(number, "componentIn", eRoutingSignalType.Video, eRoutingPortConnectionType.Component);
//                AddInputPortWithDebug(number, "audioIn", eRoutingSignalType.Audio, eRoutingPortConnectionType.DigitalAudio);
//                AddInCardHdmiLoopPort(number);
//            }
//            else if (type == "dmcVid4")
//            {
//                new DmcVid4(number, Chassis);
//                AddInputPortWithDebug(number, "compositeIn1", eRoutingSignalType.Video, eRoutingPortConnectionType.Composite);
//                AddInputPortWithDebug(number, "compositeIn2", eRoutingSignalType.Video, eRoutingPortConnectionType.Composite);
//                AddInputPortWithDebug(number, "compositeIn3", eRoutingSignalType.Video, eRoutingPortConnectionType.Composite);
//                AddInputPortWithDebug(number, "compositeIn4", eRoutingSignalType.Video, eRoutingPortConnectionType.Composite);
//                AddInCardHdmiLoopPort(number);
//            }
//            else if (type == "dmcStr")
//            {
//                new DmcStr(number, Chassis);
//                AddInputPortWithDebug(number, "streamIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Streaming);
//                AddInCardHdmiAndAudioLoopPorts(number);
//            }
//        }

//        void AddDmInCardPorts(uint number)
//        {
//            AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmCat);
//            AddInCardHdmiAndAudioLoopPorts(number);
//        }

//        void AddHdmiInCardPorts(uint number, ICec cecPort)
//        {
//            AddInputPortWithDebug(number, "hdmiIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, cecPort);
//            AddInCardHdmiAndAudioLoopPorts(number);
//        }

//        void AddInCardHdmiAndAudioLoopPorts(uint number)
//        {
//            AddOutputPortWithDebug(number, "hdmiLoopOut", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, null);
//            AddOutputPortWithDebug(number, "audioLoopOut", eRoutingSignalType.Audio, eRoutingPortConnectionType.Hdmi, null);
//        }

//        void AddInCardHdmiLoopPort(uint number)
//        {
//            AddOutputPortWithDebug(number, "hdmiLoopOut", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, null);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="type"></param>
//        /// <param name="number"></param>
//        public void AddOutputCard(string type, uint number)
//        {
//            Debug.Console(2, this, "Adding output card '{0}', slot {1}", type, number);
//            if (type == "dmc4kHdo")
//            {
//                var outputCard = new Dmc4kHdoSingle(number, Chassis);
//                var cecPort1 = outputCard.Card1.HdmiOutput;
//                var cecPort2 = outputCard.Card2.HdmiOutput;
//                AddDmcHdoPorts(number, cecPort1, cecPort2);
//            }
//            else if (type == "dmcHdo")
//            {
//                var outputCard = new DmcHdoSingle(number, Chassis);
//                var cecPort1 = outputCard.Card1.HdmiOutput;
//                var cecPort2 = outputCard.Card2.HdmiOutput;
//                AddDmcHdoPorts(number, cecPort1, cecPort2);
//            }
//            else if (type == "dmc4kCoHd")
//            {
//                var outputCard = new Dmc4kCoHdSingle(number, Chassis);
//                var cecPort1 = outputCard.Card1.HdmiOutput;
//                AddDmcCoPorts(number, cecPort1);
//            }
//            else if (type == "dmc4kzCoHd")
//            {
//                var outputCard = new Dmc4kzCoHdSingle(number, Chassis);
//                var cecPort1 = outputCard.Card1.HdmiOutput;
//                AddDmcCoPorts(number, cecPort1);
//            }
//            else if (type == "dmcCoHd")
//            {
//                var outputCard = new DmcCoHdSingle(number, Chassis);
//                var cecPort1 = outputCard.Card1.HdmiOutput;
//                AddDmcCoPorts(number, cecPort1);
//            }
//            else if (type == "dmCatoHd")
//            {
//                var outputCard = new DmcCatoHdSingle(number, Chassis);
//                var cecPort1 = outputCard.Card1.HdmiOutput;
//                AddDmcCoPorts(number, cecPort1);
//            }
//            else if (type == "dmcSoHd")
//            {
//                var outputCard = new DmcSoHdSingle(number, Chassis);
//                var cecPort1 = outputCard.Card1.HdmiOutput;
//                AddOutputPortWithDebug(number, "dmOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmMmFiber, 2 * (number - 1) + 1);
//                AddOutputPortWithDebug(number, "hdmiOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, 2 * (number - 1) + 1, cecPort1);
//                AddOutputPortWithDebug(number, "dmOut2", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmMmFiber, 2 * (number - 1) + 2);

//            }
//            else if (type == "dmcS2oHd")
//            {
//                var outputCard = new DmcS2oHdSingle(number, Chassis);
//                var cecPort1 = outputCard.Card1.HdmiOutput;
//                AddOutputPortWithDebug(number, "dmOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmSmFiber, 2 * (number - 1) + 1);
//                AddOutputPortWithDebug(number, "hdmiOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, 2 * (number - 1) + 1, cecPort1);
//                AddOutputPortWithDebug(number, "dmOut2", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmSmFiber, 2 * (number - 1) + 2);
//            }
//            else if (type == "dmcStro")
//            {
//                var outputCard = new DmcStroSingle(number, Chassis);
//                AddOutputPortWithDebug(number, "streamOut", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Streaming, 2 * (number - 1) + 1);
//            }

//            else
//                Debug.Console(1, this, "  WARNING: Output card type '{0}' is not available", type);
//        }

//        void AddDmcHdoPorts(uint number, ICec cecPort1, ICec cecPort2)
//        {
//            AddOutputPortWithDebug(number, "hdmiOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, 2 * (number - 1) + 1, cecPort1);
//            AddOutputPortWithDebug(number, "audioOut1", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio, 2 * (number - 1) + 1);
//            AddOutputPortWithDebug(number, "hdmiOut2", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, 2 * (number - 1) + 2, cecPort2);
//            AddOutputPortWithDebug(number, "audioOut2", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio, 2 * (number - 1) + 2);
//        }

//        void AddDmcCoPorts(uint number, ICec cecPort1)
//        {
//            AddOutputPortWithDebug(number, "dmOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmCat, 2 * (number - 1) + 1);
//            AddOutputPortWithDebug(number, "hdmiOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, 2 * (number - 1) + 1, cecPort1);
//            AddOutputPortWithDebug(number, "dmOut2", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmCat, 2 * (number - 1) + 2);
//        }


//        /// <summary>
//        /// Adds InputPort
//        /// </summary>
//        void AddInputPortWithDebug(uint cardNum, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType)
//        {
//            var portKey = string.Format("inputCard{0}--{1}", cardNum, portName);
//            Debug.Console(2, this, "Adding input port '{0}'", portKey);
//            var inputPort = new RoutingInputPort(portKey, sigType, portType, cardNum, this);

//            InputPorts.Add(inputPort);
//        }

//        /// <summary>
//        /// Adds InputPort and sets Port as ICec object
//        /// </summary>
//        void AddInputPortWithDebug(uint cardNum, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType, ICec cecPort)
//        {
//            var portKey = string.Format("inputCard{0}--{1}", cardNum, portName);
//            Debug.Console(2, this, "Adding input port '{0}'", portKey);
//            var inputPort = new RoutingInputPort(portKey, sigType, portType, cardNum, this);

//            if (cecPort != null)
//                inputPort.Port = cecPort;

//            InputPorts.Add(inputPort);
//        }

//        /// <summary>
//        /// Adds OutputPort
//        /// </summary>
//        void AddOutputPortWithDebug(uint cardNum, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType, object selector)
//        {
//            var portKey = string.Format("outputCard{0}--{1}", cardNum, portName);
//            Debug.Console(2, this, "Adding output port '{0}'", portKey);
//            OutputPorts.Add(new RoutingOutputPort(portKey, sigType, portType, selector, this));
//        }

//        /// <summary>
//        /// Adds OutputPort and sets Port as ICec object
//        /// </summary>
//        void AddOutputPortWithDebug(uint cardNum, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType, object selector, ICec cecPort)
//        {
//            var portKey = string.Format("outputCard{0}--{1}", cardNum, portName);
//            Debug.Console(2, this, "Adding output port '{0}'", portKey);
//            var outputPort = new RoutingOutputPort(portKey, sigType, portType, selector, this);

//            if (cecPort != null)
//                outputPort.Port = cecPort;

//            OutputPorts.Add(outputPort);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        void AddVolumeControl(uint number, Audio.Output audio)
//        {
//            VolumeControls.Add(number, new DmCardAudioOutputController(audio));
//        }

//        //public void SetInputHdcpSupport(uint input, ePdtHdcpSupport hdcpSetting)
//        //{

//        //}


//        void Chassis_DMSystemChange(Switch device, DMSystemEventArgs args) {

//            }
//        void Chassis_DMInputChange(Switch device, DMInputEventArgs args) {
//            //Debug.Console(2, this, "DMSwitch:{0} Input:{1} Event:{2}'", this.Name, args.Number, args.EventId.ToString());
				
//            switch (args.EventId) {
//                case (DMInputEventIds.OnlineFeedbackEventId): {
//                    Debug.Console(2, this, "DMINput OnlineFeedbackEventId for input: {0}. State: {1}", args.Number, device.Inputs[args.Number].EndpointOnlineFeedback);
//                    InputEndpointOnlineFeedbacks[args.Number].FireUpdate();
//                    break;
//                    }
//                case (DMInputEventIds.VideoDetectedEventId): {
//                    Debug.Console(2, this, "DM Input {0} VideoDetectedEventId", args.Number);
//                    VideoInputSyncFeedbacks[args.Number].FireUpdate();
//                    break;
//                    }
//                case (DMInputEventIds.InputNameEventId): {
//                    Debug.Console(2, this, "DM Input {0} NameFeedbackEventId", args.Number);
//                    InputNameFeedbacks[args.Number].FireUpdate();
//                    break;
//                    }
//                }
//            }
//        /// 
//        /// </summary>
//        void Chassis_DMOutputChange(Switch device, DMOutputEventArgs args)
//        {

//            //This should be a switch case JTA 2018-07-02
//            var output = args.Number;
//            if (args.EventId == DMOutputEventIds.VolumeEventId &&
//                VolumeControls.ContainsKey(output))
//            {
//                VolumeControls[args.Number].VolumeEventFromChassis();
//            }
//            else if (args.EventId == DMOutputEventIds.OnlineFeedbackEventId)
//            {
//                OutputEndpointOnlineFeedbacks[output].FireUpdate();
//            }
//            else if (args.EventId == DMOutputEventIds.VideoOutEventId)
//            {
//                if (Chassis.Outputs[output].VideoOutFeedback != null)
//                {
//                    Debug.Console(2, this, "DMSwitchVideo:{0} Routed Input:{1} Output:{2}'", this.Name, Chassis.Outputs[output].VideoOutFeedback.Number, output);
//                }
//                if (VideoOutputFeedbacks.ContainsKey(output))
//                {
//                    VideoOutputFeedbacks[output].FireUpdate();

//                }
//                if (OutputVideoRouteNameFeedbacks.ContainsKey(output))
//                {
//                    OutputVideoRouteNameFeedbacks[output].FireUpdate();
//                }
//            }
//            else if (args.EventId == DMOutputEventIds.AudioOutEventId)
//            {
//                if (Chassis.Outputs[output].AudioOutFeedback != null)
//                {
//                    Debug.Console(2, this, "DMSwitchAudio:{0} Routed Input:{1} Output:{2}'", this.Name, Chassis.Outputs[output].AudioOutFeedback.Number, output);
//                }
//                if (AudioOutputFeedbacks.ContainsKey(output))
//                {
//                    AudioOutputFeedbacks[output].FireUpdate();
//                }
//            }
//            else if (args.EventId == DMOutputEventIds.OutputNameEventId)
//            {
//                Debug.Console(2, this, "DM Output {0} NameFeedbackEventId", output);
//                OutputNameFeedbacks[output].FireUpdate();
//            }

//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pnt"></param>
//        void StartOffTimer(PortNumberType pnt)
//        {
//            if (RouteOffTimers.ContainsKey(pnt))
//                return;
//            RouteOffTimers[pnt] = new CTimer(o =>
//            {
//                ExecuteSwitch(0, pnt.Number, pnt.Type);
//            }, RouteOffTime);
//        }


//        // Send out sigs when coming online
//        void IsOnline_OutputChange(object sender, EventArgs e)
//        {
//            if (IsOnline.BoolValue)
//            {
//                if (InputNames != null)
//                    foreach (var kvp in InputNames)
//                        Chassis.Inputs[kvp.Key].Name.StringValue = kvp.Value;
//                if (OutputNames != null)
//                    foreach(var kvp in OutputNames)
//                        Chassis.Outputs[kvp.Key].Name.StringValue = kvp.Value;
//            }
//        }

//        #region IRouting Members

//        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType sigType)
//        {
//            Debug.Console(2, this, "Making an awesome DM route from {0} to {1} {2}", inputSelector, outputSelector, sigType);

//            var input = Convert.ToUInt32(inputSelector); // Cast can sometimes fail
//            var output = Convert.ToUInt32(outputSelector);
//            // Check to see if there's an off timer waiting on this and if so, cancel
//            var key = new PortNumberType(output, sigType);
//            if (input == 0)
//            {
//                StartOffTimer(key);
//            }
//            else
//            {
//                if(RouteOffTimers.ContainsKey(key))
//                {
//                    Debug.Console(2, this, "{0} cancelling route off due to new source", output);
//                    RouteOffTimers[key].Stop();
//                    RouteOffTimers.Remove(key);
//                }
//            }

//            Card.DMICard inCard = input == 0 ? null : Chassis.Inputs[input];

//            // NOTE THAT THESE ARE NOTS - TO CATCH THE AudioVideo TYPE
//            if (sigType != eRoutingSignalType.Audio)
//            {
//                Chassis.VideoEnter.BoolValue = true;
//                Chassis.Outputs[output].VideoOut = inCard;
//            }

//            if (sigType != eRoutingSignalType.Video)
//            {
//                Chassis.AudioEnter.BoolValue = true;
//                Chassis.Outputs[output].AudioOut = inCard;
//            }
//        }

//        #endregion

//    }

//    //public struct PortNumberType
//    //{
//    //    public uint Number { get; private set; }
//    //    public eRoutingSignalType Type { get; private set; }

//    //    public PortNumberType(uint number, eRoutingSignalType type) : this()
//    //    {
//    //        Number = number;
//    //        Type = type;
//    //    }
//    //}
//}