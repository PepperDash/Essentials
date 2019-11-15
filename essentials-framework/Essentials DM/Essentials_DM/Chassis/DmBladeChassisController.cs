using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using Crestron.SimplSharpPro.DM.Blades;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

using PepperDash.Core;
using PepperDash.Essentials.Core;

using PepperDash.Essentials.DM.Config;

namespace PepperDash.Essentials.DM {
    /// <summary>
    /// Builds a controller for basic DM-RMCs with Com and IR ports and no control functions
    /// 
    /// </summary>
    public class DmBladeChassisController : CrestronGenericBaseDevice, IDmSwitch, IRoutingInputsOutputs, IRouting, IHasFeedback {
        public DMChassisPropertiesConfig PropertiesConfig { get; set; }

        public Switch Chassis { get; private set; }

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
        public Dictionary<uint, IntFeedback> UsbOutputRoutedToFeebacks { get; private set; }
        public Dictionary<uint, IntFeedback> UsbInputRoutedToFeebacks { get; private set; }

        public IntFeedback SystemIdFeebdack { get; private set; }
        public BoolFeedback SystemIdBusyFeedback { get; private set; }


        public Dictionary<uint, IntFeedback> InputCardHdcpCapabilityFeedbacks { get; private set; }

        public Dictionary<uint, eHdcpCapabilityType> InputCardHdcpCapabilityTypes { get; private set; }


        // Need a couple Lists of generic Backplane ports
        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }
        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

        public Dictionary<uint, string> TxDictionary { get; set; }
        public Dictionary<uint, string> RxDictionary { get; set; }

        //public Dictionary<uint, DmInputCardControllerBase> InputCards { get; private set; }
        //public Dictionary<uint, DmSingleOutputCardControllerBase> OutputCards { get; private set; }

        public Dictionary<uint, string> InputNames { get; set; }
        public Dictionary<uint, string> OutputNames { get; set; }
        public Dictionary<uint, DmCardAudioOutputController> VolumeControls { get; private set; }

        public const int RouteOffTime = 500;
        Dictionary<PortNumberType, CTimer> RouteOffTimers = new Dictionary<PortNumberType, CTimer>();

        /// <summary>
        /// Factory method to create a new chassis controller from config data. Limited to 8x8 right now
        /// </summary>
        public static DmBladeChassisController GetDmChassisController(string key, string name,
            string type, DMChassisPropertiesConfig properties) {
            try {
                type = type.ToLower();
                uint ipid = properties.Control.IpIdInt;

                BladeSwitch chassis = null;
                if (type == "dmmd64x64") { chassis = new DmMd64x64(ipid, Global.ControlSystem); }
                else if (type == "dmmd128x128") { chassis = new DmMd128x128(ipid, Global.ControlSystem); }


                if (chassis == null) {
                    return null;
                }

                var controller = new DmBladeChassisController(key, name, chassis);
                // add the cards and port names
                foreach (var kvp in properties.InputSlots)
                    controller.AddInputBlade(kvp.Value, kvp.Key);
                foreach (var kvp in properties.OutputSlots) {
                    controller.AddOutputBlade(kvp.Value, kvp.Key);
                }

                foreach (var kvp in properties.VolumeControls) {
                    // get the card
                    // check it for an audio-compatible type
                    // make a something-something that will make it work
                    // retire to mountain village
                    var outNum = kvp.Key;

                    var card = controller.Chassis.Outputs[outNum].Card;
                    Audio.Output audio = null;
                    if (card is DmHdmi4kOutputBladeCard)
                        audio = (card as DmHdmi4kOutputBladeCard).Hdmi4kOutput.Audio;
                    if (audio == null)
                        continue;
                    // wire up the audio to something here...
                    controller.AddVolumeControl(outNum, audio);
                }

                controller.InputNames = properties.InputNames;
                controller.OutputNames = properties.OutputNames;
                controller.PropertiesConfig = properties;
                return controller;
            }
            catch (System.Exception e) {
                Debug.Console(0, "Error creating DM chassis:\r{0}", e);
            }
            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="chassis"></param>
        public DmBladeChassisController(string key, string name, BladeSwitch chassis)
            : base(key, name, chassis) {
            Chassis = chassis;
            InputPorts = new RoutingPortCollection<RoutingInputPort>();
            OutputPorts = new RoutingPortCollection<RoutingOutputPort>();
            VolumeControls = new Dictionary<uint, DmCardAudioOutputController>();
            TxDictionary = new Dictionary<uint, string>();
            RxDictionary = new Dictionary<uint, string>();
            IsOnline.OutputChange += new EventHandler<FeedbackEventArgs>(IsOnline_OutputChange);
            Chassis.DMInputChange += new DMInputEventHandler(Chassis_DMInputChange);
            Chassis.DMOutputChange += new DMOutputEventHandler(Chassis_DMOutputChange);
            VideoOutputFeedbacks = new Dictionary<uint, IntFeedback>();
            UsbOutputRoutedToFeebacks = new Dictionary<uint, IntFeedback>();
            UsbInputRoutedToFeebacks = new Dictionary<uint, IntFeedback>();
            VideoInputSyncFeedbacks = new Dictionary<uint, BoolFeedback>();
            InputNameFeedbacks = new Dictionary<uint, StringFeedback>();
            OutputNameFeedbacks = new Dictionary<uint, StringFeedback>();
            OutputVideoRouteNameFeedbacks = new Dictionary<uint, StringFeedback>();
            OutputAudioRouteNameFeedbacks = new Dictionary<uint, StringFeedback>();
            InputEndpointOnlineFeedbacks = new Dictionary<uint, BoolFeedback>();
            OutputEndpointOnlineFeedbacks = new Dictionary<uint, BoolFeedback>();

            InputCardHdcpCapabilityFeedbacks = new Dictionary<uint, IntFeedback>();
            InputCardHdcpCapabilityTypes = new Dictionary<uint, eHdcpCapabilityType>();

            CrestronConsole.PrintLine("GotHere 1");
            for (uint x = 1; x <= Chassis.NumberOfOutputs; x++) {
                var tempX = x;

                if (Chassis.Outputs[tempX] != null) {
                    CrestronConsole.PrintLine("GotHere 2");
                    VideoOutputFeedbacks[tempX] = new IntFeedback(() => {
                        if (Chassis.Outputs[tempX].VideoOutFeedback != null) { return (ushort)Chassis.Outputs[tempX].VideoOutFeedback.Number; }
                        else { return 0; };
                    });

                    OutputNameFeedbacks[tempX] = new StringFeedback(() => {
                        if (Chassis.Outputs[tempX].NameFeedback != null) {
                            CrestronConsole.PrintLine("GotHere 3");
                            return Chassis.Outputs[tempX].NameFeedback.StringValue;
                        }
                        else {
                            return "";
                        }
                    });
                    OutputVideoRouteNameFeedbacks[tempX] = new StringFeedback(() => {
                        if (Chassis.Outputs[tempX].VideoOutFeedback != null) {
                            CrestronConsole.PrintLine("GotHere 4");
                            return Chassis.Outputs[tempX].VideoOutFeedback.NameFeedback.StringValue;
                        }
                        else {
                            return "";
                        }
                    });

                    OutputEndpointOnlineFeedbacks[tempX] = new BoolFeedback(() => {
                        CrestronConsole.PrintLine("GotHere 5");
                        return Chassis.Outputs[tempX].EndpointOnlineFeedback;
                    });
                }

                if (Chassis.Inputs[tempX] != null) {
                    UsbInputRoutedToFeebacks[tempX] = new IntFeedback(() => {
                        CrestronConsole.PrintLine("GotHere 6");
                        if (Chassis.Inputs[tempX].USBRoutedToFeedback != null) { return (ushort)Chassis.Inputs[tempX].USBRoutedToFeedback.Number; }
                        else { return 0; };
                    });
                    VideoInputSyncFeedbacks[tempX] = new BoolFeedback(() => {
                        CrestronConsole.PrintLine("GotHere 7");
                        if (Chassis.Inputs[tempX].VideoDetectedFeedback != null)
                            return Chassis.Inputs[tempX].VideoDetectedFeedback.BoolValue;
                        else
                            return false;
                    });
                    InputNameFeedbacks[tempX] = new StringFeedback(() => {
                        CrestronConsole.PrintLine("GotHere 8");
                        if (Chassis.Inputs[tempX].NameFeedback != null) {
                            return Chassis.Inputs[tempX].NameFeedback.StringValue;
                        }
                        else {
                            return "";
                        }
                    });

                    InputEndpointOnlineFeedbacks[tempX] = new BoolFeedback(() => {
                        CrestronConsole.PrintLine("GotHere 9");
                        return Chassis.Inputs[tempX].EndpointOnlineFeedback;
                    });

                    InputCardHdcpCapabilityFeedbacks[tempX] = new IntFeedback(() => {
                        var inputCard = Chassis.Inputs[tempX];

                        if (inputCard.Card is DmHdmi4kInputBladeCard) {
                            InputCardHdcpCapabilityTypes[tempX] = eHdcpCapabilityType.Hdcp2_2Support;

                            if ((inputCard.Card as DmHdmi4kInputBladeCard).Hdmi4kInput.HdcpSupportOnFeedback.BoolValue)
                                return 1;
                            else
                                return 0;
                        }

                        if (inputCard.Card is DmC4kInputBladeCard) {
                            InputCardHdcpCapabilityTypes[tempX] = eHdcpCapabilityType.Hdcp2_2Support;

                            if ((inputCard.Card as DmC4kInputBladeCard).DmInput.HdcpCapabilityFeedback.Equals(eHdcpCapabilityType.HdcpSupportOff))
                                return 0;
                            else
                                return 1;
                        }

                        else
                            return 0;
                    });
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="number"></param>
        public void AddInputBlade(string type, uint number) {
            Debug.Console(2, this, "Adding input blade '{0}', slot {1}", type, number);

            type = type.ToLower();

            if (type == "dmb4kihd") {
                var inputBlade = new Dmb4kIHd(number, this.Chassis);
                foreach (var item in inputBlade.Inputs) {
                    var card = (item.Card as DmHdmi4kInputBladeCard).Hdmi4kInput;
                    var cecPort = card as ICec;
                    AddHdmiInBladePorts(item.Number, cecPort);
                }
            }

            else if (type == "dmb4kihddnt") {
                var inputBlade = new Dmb4kIHd(number, this.Chassis);
                foreach (var item in inputBlade.Inputs) {
                    var card = (item.Card as DmHdmi4kInputBladeCard).Hdmi4kInput;
                    var cecPort = card as ICec;
                    AddHdmiInBladePorts(item.Number, cecPort);
                }
            }

            else if (type == "dmb4kic") {
                var inputBlade = new Dmb4kIC(number, this.Chassis);
                foreach (var item in inputBlade.Inputs) {
                    AddDmInBladePorts(item.Number);
                }
            }

            else if (type == "dmbis") {
                var inputBlade = new DmbIS(number, this.Chassis);
                foreach (var item in inputBlade.Inputs) {
                    AddDmInMmFiberPorts(item.Number);
                }
            }
            else if (type == "dmbis2") {
                var inputBlade = new DmbIS2(number, this.Chassis);
                foreach (var item in inputBlade.Inputs) {
                    AddDmInSmFiberPorts(item.Number);
                }
            }
        }



        void AddHdmiInBladePorts(uint number, ICec cecPort) {
            AddInputPortWithDebug(number, "hdmiIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmCat, cecPort);
        }

        void AddDmInBladePorts(uint number) {
            AddInputPortWithDebug(number, "dmCIn", eRoutingSignalType.Video, eRoutingPortConnectionType.DmCat);
        }

        void AddDmInMmFiberPorts(uint number) {
            AddInputPortWithDebug(number, "dmMmIn", eRoutingSignalType.Video, eRoutingPortConnectionType.DmMmFiber);
        }

        void AddDmInSmFiberPorts(uint number) {
            AddInputPortWithDebug(number, "dmSmIn", eRoutingSignalType.Video, eRoutingPortConnectionType.DmSmFiber);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="number"></param>
        public void AddOutputBlade(string type, uint number) {
            type = type.ToLower();

            Debug.Console(2, this, "Adding output blade '{0}', slot {1}", type, number);
            if (type == "dmb4kohd") {
                var outputBlade = new Dmb4KOHD(number, Chassis);
                foreach (var item in outputBlade.Outputs) {
                    AddHdmiOutBladePorts(item.Number);
                }
            }

            else if (type == "dmb4kohddnt") {
                var outputBlade = new Dmb4KOHD(number, Chassis);
                foreach (var item in outputBlade.Outputs) {
                    AddHdmiOutBladePorts(item.Number);
                }
            }

            else if (type == "dmb4koc") {
                var outputBlade = new Dmb4KOC(number, Chassis);
                foreach (var item in outputBlade.Outputs) {
                    AddDmOutBladePorts(item.Number);
                }
            }
            else if (type == "dmb4koc") {
                var outputBlade = new Dmb4KOC(number, Chassis);
                foreach (var item in outputBlade.Outputs) {
                    AddDmOutBladePorts(item.Number);
                }
            }
            else if (type == "dmbos") {
                var outputBlade = new DmbOS(number, Chassis);
                foreach (var item in outputBlade.Outputs) {
                    AddDmOutMmFiberBladePorts(item.Number);
                }
            }
            else if (type == "dmbos2") {
                var outputBlade = new DmbOS2(number, Chassis);
                foreach (var item in outputBlade.Outputs) {
                    AddDmOutSmFiberBladePorts(item.Number);
                }
            }
        }

        void AddHdmiOutBladePorts(uint number) {
            AddOutputPortWithDebug(String.Format("outputBlade{0}", (number / 8 > 0 ? 1 : number / 8)), String.Format("hdmiOut{0}", number) , eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, number);
        }

        void AddDmOutBladePorts(uint number) {
            AddOutputPortWithDebug(String.Format("outputBlade{0}", (number / 8 > 0 ? 1 : number / 8)), String.Format("dmOut{0}", number), eRoutingSignalType.Video, eRoutingPortConnectionType.DmCat, number);
        }

        void AddDmOutMmFiberBladePorts(uint number) {
            AddOutputPortWithDebug(String.Format("outputBlade{0}", (number / 8 > 0 ? 1 : number / 8)), String.Format("dmOut{0}", number), eRoutingSignalType.Video, eRoutingPortConnectionType.DmMmFiber, number);
        }

        void AddDmOutSmFiberBladePorts(uint number) {
            AddOutputPortWithDebug(String.Format("outputBlade{0}", (number / 8 > 0 ? 1 : number / 8)), String.Format("dmOut{0}", number), eRoutingSignalType.Video, eRoutingPortConnectionType.DmSmFiber, number);
        }


        /// <summary>
        /// Adds InputPort
        /// </summary>
        void AddInputPortWithDebug(uint cardNum, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType) {
            var portKey = string.Format("inputCard{0}--{1}", cardNum, portName);
            Debug.Console(2, this, "Adding input port '{0}'", portKey);
            var inputPort = new RoutingInputPort(portKey, sigType, portType, cardNum, this);

            InputPorts.Add(inputPort);
        }

        /// <summary>
        /// Adds InputPort and sets Port as ICec object
        /// </summary>
        void AddInputPortWithDebug(uint cardNum, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType, ICec cecPort) {
            var portKey = string.Format("inputCard{0}--{1}", cardNum, portName);
            Debug.Console(2, this, "Adding input port '{0}'", portKey);
            var inputPort = new RoutingInputPort(portKey, sigType, portType, cardNum, this);

            if (inputPort != null) {
                if (cecPort != null)
                    inputPort.Port = cecPort;

                InputPorts.Add(inputPort);
            }
            else
                Debug.Console(2, this, "inputPort is null");
        }


        /// <summary>
        /// Adds OutputPort
        /// </summary>
        void AddOutputPortWithDebug(string cardName, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType, object selector) {
            var portKey = string.Format("{0}--{1}", cardName, portName);
            Debug.Console(2, this, "Adding output port '{0}'", portKey);
            OutputPorts.Add(new RoutingOutputPort(portKey, sigType, portType, selector, this));
        }


        /// <summary>
        /// 
        /// </summary>
        void AddVolumeControl(uint number, Audio.Output audio) {
            VolumeControls.Add(number, new DmCardAudioOutputController(audio));
        }

        //public void SetInputHdcpSupport(uint input, ePdtHdcpSupport hdcpSetting)
        //{

        //}


        void Chassis_DMInputChange(Switch device, DMInputEventArgs args) {

            switch (args.EventId) {
                case DMInputEventIds.EndpointOnlineEventId: {
                        Debug.Console(2, this, "DM Input EndpointOnlineEventId for input: {0}. State: {1}", args.Number, device.Inputs[args.Number].EndpointOnlineFeedback);
                        InputEndpointOnlineFeedbacks[args.Number].FireUpdate();
                        break;
                    }
                case DMInputEventIds.OnlineFeedbackEventId: {
                        Debug.Console(2, this, "DM Input OnlineFeedbackEventId for input: {0}. State: {1}", args.Number, device.Inputs[args.Number].EndpointOnlineFeedback);
                        InputEndpointOnlineFeedbacks[args.Number].FireUpdate();
                        break;
                    }
                case DMInputEventIds.VideoDetectedEventId: {
                        Debug.Console(2, this, "DM Input {0} VideoDetectedEventId", args.Number);
                        VideoInputSyncFeedbacks[args.Number].FireUpdate();
                        break;
                    }
                case DMInputEventIds.InputNameEventId: {
                        Debug.Console(2, this, "DM Input {0} NameFeedbackEventId", args.Number);
                        InputNameFeedbacks[args.Number].FireUpdate();
                        break;
                    }
                case DMInputEventIds.HdcpCapabilityFeedbackEventId: {
                        Debug.Console(2, this, "DM Input {0} HdcpCapabilityFeedbackEventId", args.Number);
                        InputCardHdcpCapabilityFeedbacks[args.Number].FireUpdate();
                        break;
                    }
                default: {
                        Debug.Console(2, this, "DMInputChange fired for Input {0} with Unhandled EventId: {1}", args.Number, args.EventId);
                        break;
                    }
            }
        }
        /// 
        /// </summary>
        void Chassis_DMOutputChange(Switch device, DMOutputEventArgs args) {

            //This should be a switch case JTA 2018-07-02
            var output = args.Number;

            switch (args.EventId) {
                case DMOutputEventIds.VolumeEventId: {
                        if (VolumeControls.ContainsKey(output)) {
                            VolumeControls[args.Number].VolumeEventFromChassis();
                        }
                        break;
                    }
                case DMOutputEventIds.EndpointOnlineEventId: {
                        Debug.Console(2, this, "Output {0} DMOutputEventIds.EndpointOnlineEventId fired. State: {1}", args.Number, Chassis.Outputs[output].EndpointOnlineFeedback);
                        OutputEndpointOnlineFeedbacks[output].FireUpdate();
                        break;
                    }
                case DMOutputEventIds.OnlineFeedbackEventId: {
                        Debug.Console(2, this, "Output {0} DMInputEventIds.OnlineFeedbackEventId fired. State: {1}", args.Number, Chassis.Outputs[output].EndpointOnlineFeedback);
                        OutputEndpointOnlineFeedbacks[output].FireUpdate();
                        break;
                    }
                case DMOutputEventIds.VideoOutEventId: {
                        if (Chassis.Outputs[output].VideoOutFeedback != null) {
                            Debug.Console(2, this, "DMSwitchVideo:{0} Routed Input:{1} Output:{2}'", this.Name, Chassis.Outputs[output].VideoOutFeedback.Number, output);
                        }
                        if (VideoOutputFeedbacks.ContainsKey(output)) {
                            VideoOutputFeedbacks[output].FireUpdate();

                        }
                        if (OutputVideoRouteNameFeedbacks.ContainsKey(output)) {
                            OutputVideoRouteNameFeedbacks[output].FireUpdate();
                        }
                        break;
                    }
                case DMOutputEventIds.OutputNameEventId: {
                        Debug.Console(2, this, "DM Output {0} NameFeedbackEventId", output);
                        OutputNameFeedbacks[output].FireUpdate();
                        break;
                    }
                default: {
                        Debug.Console(2, this, "DMOutputChange fired for Output {0} with Unhandled EventId: {1}", args.Number, args.EventId);
                        break;
                    }
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pnt"></param>
        void StartOffTimer(PortNumberType pnt) {
            if (RouteOffTimers.ContainsKey(pnt))
                return;
            RouteOffTimers[pnt] = new CTimer(o => {
                ExecuteSwitch(0, pnt.Number, pnt.Type);
            }, RouteOffTime);
        }


        // Send out sigs when coming online
        void IsOnline_OutputChange(object sender, EventArgs e) {
            if (IsOnline.BoolValue) {
                Chassis.EnableUSBBreakaway.BoolValue = true;

                if (InputNames != null)
                    foreach (var kvp in InputNames)
                        Chassis.Inputs[kvp.Key].Name.StringValue = kvp.Value;
                if (OutputNames != null)
                    foreach (var kvp in OutputNames)
                        Chassis.Outputs[kvp.Key].Name.StringValue = kvp.Value;
            }
        }

        #region IRouting Members

        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType sigType) {
            Debug.Console(2, this, "Making an awesome DM route from {0} to {1} {2}", inputSelector, outputSelector, sigType);

            var input = Convert.ToUInt32(inputSelector); // Cast can sometimes fail
            var output = Convert.ToUInt32(outputSelector);
            // Check to see if there's an off timer waiting on this and if so, cancel
            var key = new PortNumberType(output, sigType);
            if (input == 0) {
                StartOffTimer(key);
            }
            else {
                if (RouteOffTimers.ContainsKey(key)) {
                    Debug.Console(2, this, "{0} cancelling route off due to new source", output);
                    RouteOffTimers[key].Stop();
                    RouteOffTimers.Remove(key);
                }
            }



            var inCard = input == 0 ? null : Chassis.Inputs[input];
            var outCard = input == 0 ? null : Chassis.Outputs[output];

            // NOTE THAT BITWISE COMPARISONS - TO CATCH ALL ROUTING TYPES 
            if ((sigType | eRoutingSignalType.Video) == eRoutingSignalType.Video) {
                Chassis.VideoEnter.BoolValue = true;
                Chassis.Outputs[output].VideoOut = inCard;
            }
        }

        #endregion

    }

    /*
    public struct PortNumberType {
        public uint Number { get; private set; }
        public eRoutingSignalType Type { get; private set; }

        public PortNumberType(uint number, eRoutingSignalType type)
            : this() {
            Number = number;
            Type = type;
        }
    }*/
}