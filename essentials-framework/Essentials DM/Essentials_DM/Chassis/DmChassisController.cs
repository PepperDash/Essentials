
using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using Crestron.SimplSharpPro.DM.Endpoints;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.DM.Config;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.DM
{
    /// <summary>
    /// Builds a controller for basic DM-RMCs with Com and IR ports and no control functions
    /// 
    /// </summary>
    [Description("Wrapper class for all DM-MD chassis variants from 8x8 to 32x32")]
    public class DmChassisController : CrestronGenericBridgeableBaseDevice, IDmSwitch, IRoutingNumeric
    {
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
        public Dictionary<uint, BoolFeedback> OutputDisabledByHdcpFeedbacks { get; private set; }

        public IntFeedback SystemIdFeebdack { get; private set; }
        public BoolFeedback SystemIdBusyFeedback { get; private set; }
        public BoolFeedback EnableAudioBreakawayFeedback { get; private set; }
        public BoolFeedback EnableUsbBreakawayFeedback { get; private set; }

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
        /// Text that represents when an output has no source routed to it
        /// </summary>
        public string NoRouteText = "";

        /// <summary>
        /// Factory method to create a new chassis controller from config data. Limited to 8x8 right now
        /// </summary>
        public static DmChassisController GetDmChassisController(string key, string name,
                                                                 string type, DMChassisPropertiesConfig properties)
        {
            try
            {
                type = type.ToLower();
                uint ipid = properties.Control.IpIdInt;

                DmMDMnxn chassis = null;
                switch (type) {
                    case "dmmd8x8":
                        chassis = new DmMd8x8(ipid, Global.ControlSystem);
                        break;
                    case "dmmd8x8rps":
                        chassis = new DmMd8x8rps(ipid, Global.ControlSystem);
                        break;
                    case "dmmd8x8cpu3":
                        chassis = new DmMd8x8Cpu3(ipid, Global.ControlSystem);
                        break;
                    case "dmmd8x8cpu3rps":
                        chassis = new DmMd8x8Cpu3rps(ipid, Global.ControlSystem);
                        break;
                    case "dmmd16x16":
                        chassis = new DmMd16x16(ipid, Global.ControlSystem);
                        break;
                    case "dmmd16x16rps":
                        chassis = new DmMd16x16rps(ipid, Global.ControlSystem);
                        break;
                    case "dmmd16x16cpu3":
                        chassis = new DmMd16x16Cpu3(ipid, Global.ControlSystem);
                        break;
                    case "dmmd16x16cpu3rps":
                        chassis = new DmMd16x16Cpu3rps(ipid, Global.ControlSystem);
                        break;
                    case "dmmd32x32":
                        chassis = new DmMd32x32(ipid, Global.ControlSystem);
                        break;
                    case "dmmd32x32rps":
                        chassis = new DmMd32x32rps(ipid, Global.ControlSystem);
                        break;
                    case "dmmd32x32cpu3":
                        chassis = new DmMd32x32Cpu3(ipid, Global.ControlSystem);
                        break;
                    case "dmmd32x32cpu3rps":
                        chassis = new DmMd32x32Cpu3rps(ipid, Global.ControlSystem);
                        break;
                }

                if (chassis == null)
                    return null;

                var controller = new DmChassisController(key, name, chassis);

                // add the cards and port names
                foreach (var kvp in properties.InputSlots)
                    controller.AddInputCard(kvp.Value, kvp.Key);
                
                foreach (var kvp in properties.OutputSlots)
                    controller.AddOutputCard(kvp.Value, kvp.Key);

                foreach (var kvp in properties.VolumeControls)
                {
                    // get the card
                    // check it for an audio-compatible type
                    // make a something-something that will make it work
                    // retire to mountain village
                    var outNum = kvp.Key;
                    var card = controller.Chassis.Outputs[outNum].Card;
                    Audio.Output audio = null;
                    if (card is DmcHdo)
                        audio = (card as DmcHdo).Audio;
                    else if (card is Dmc4kHdo)
                        audio = (card as Dmc4kHdo).Audio;
                    if (audio == null)
                        continue;

                    // wire up the audio to something here...
                    controller.AddVolumeControl(outNum, audio);
                }

                controller.InputNames = properties.InputNames;
                controller.OutputNames = properties.OutputNames;

                if (!string.IsNullOrEmpty(properties.NoRouteText))
                {
                    controller.NoRouteText = properties.NoRouteText;
                    Debug.Console(1, controller, "Setting No Route Text value to: {0}", controller.NoRouteText);
                }
                else
                {
                    Debug.Console(1, controller, "NoRouteText not specified.  Defaulting to blank string.", controller.NoRouteText);
                }

                controller.PropertiesConfig = properties;
                return controller;
            }
            catch (Exception e)
            {
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
        public DmChassisController(string key, string name, DmMDMnxn chassis)
            : base(key, name, chassis)
        {
            Chassis = chassis;
            InputPorts = new RoutingPortCollection<RoutingInputPort>();
            OutputPorts = new RoutingPortCollection<RoutingOutputPort>();
            VolumeControls = new Dictionary<uint, DmCardAudioOutputController>();
            TxDictionary = new Dictionary<uint, string>();
            RxDictionary = new Dictionary<uint, string>();
            IsOnline.OutputChange += new EventHandler<FeedbackEventArgs>(IsOnline_OutputChange);
            Chassis.DMInputChange += new DMInputEventHandler(Chassis_DMInputChange);
            Chassis.DMSystemChange += new DMSystemEventHandler(Chassis_DMSystemChange);
            Chassis.DMOutputChange += new DMOutputEventHandler(Chassis_DMOutputChange);
            VideoOutputFeedbacks = new Dictionary<uint, IntFeedback>();
            AudioOutputFeedbacks = new Dictionary<uint, IntFeedback>();
            UsbOutputRoutedToFeebacks = new Dictionary<uint, IntFeedback>();
            UsbInputRoutedToFeebacks = new Dictionary<uint, IntFeedback>();
            OutputDisabledByHdcpFeedbacks = new Dictionary<uint, BoolFeedback>();
            VideoInputSyncFeedbacks = new Dictionary<uint, BoolFeedback>();
            InputNameFeedbacks = new Dictionary<uint, StringFeedback>();
            OutputNameFeedbacks = new Dictionary<uint, StringFeedback>();
            OutputVideoRouteNameFeedbacks = new Dictionary<uint, StringFeedback>();
            OutputAudioRouteNameFeedbacks = new Dictionary<uint, StringFeedback>();
            InputEndpointOnlineFeedbacks = new Dictionary<uint, BoolFeedback>();
            OutputEndpointOnlineFeedbacks = new Dictionary<uint, BoolFeedback>();

            SystemIdFeebdack = new IntFeedback(() => { return (Chassis as DmMDMnxn).SystemIdFeedback.UShortValue; });
            SystemIdBusyFeedback = new BoolFeedback(() => { return (Chassis as DmMDMnxn).SystemIdBusy.BoolValue; });
            EnableAudioBreakawayFeedback =
                new BoolFeedback(() => (Chassis as DmMDMnxn).EnableAudioBreakawayFeedback.BoolValue);
            EnableUsbBreakawayFeedback =
                new BoolFeedback(() => (Chassis as DmMDMnxn).EnableUSBBreakawayFeedback.BoolValue);

            InputCardHdcpCapabilityFeedbacks = new Dictionary<uint, IntFeedback>();
            InputCardHdcpCapabilityTypes = new Dictionary<uint, eHdcpCapabilityType>();

            for (uint x = 1; x <= Chassis.NumberOfOutputs; x++)
            {
                var tempX = x;

                if (Chassis.Outputs[tempX] != null)
                {
                    VideoOutputFeedbacks[tempX] = new IntFeedback(() => {
                        if (Chassis.Outputs[tempX].VideoOutFeedback != null)
                            return (ushort)Chassis.Outputs[tempX].VideoOutFeedback.Number;

                        return 0;
                    });
                    AudioOutputFeedbacks[tempX] = new IntFeedback(() => {
                        if (Chassis.Outputs[tempX].AudioOutFeedback != null)
                            return (ushort)Chassis.Outputs[tempX].AudioOutFeedback.Number;

                        return 0;
                    });
                    UsbOutputRoutedToFeebacks[tempX] = new IntFeedback(() => {
                        if (Chassis.Outputs[tempX].USBRoutedToFeedback != null)
                            return (ushort)Chassis.Outputs[tempX].USBRoutedToFeedback.Number;

                        return 0;
                    });

                    OutputNameFeedbacks[tempX] = new StringFeedback(() => {
                        if (Chassis.Outputs[tempX].NameFeedback != null)
                            return Chassis.Outputs[tempX].NameFeedback.StringValue;

                        return "";
                    });
                    OutputVideoRouteNameFeedbacks[tempX] = new StringFeedback(() => {
                        if (Chassis.Outputs[tempX].VideoOutFeedback != null)
                            return Chassis.Outputs[tempX].VideoOutFeedback.NameFeedback.StringValue;

                        return NoRouteText;
                    });
                    OutputAudioRouteNameFeedbacks[tempX] = new StringFeedback(() => {
                        if (Chassis.Outputs[tempX].AudioOutFeedback != null)
                            return Chassis.Outputs[tempX].AudioOutFeedback.NameFeedback.StringValue;

                        return NoRouteText;
                    });
                    OutputEndpointOnlineFeedbacks[tempX] = new BoolFeedback(() => Chassis.Outputs[tempX].EndpointOnlineFeedback);
                    
                    OutputDisabledByHdcpFeedbacks[tempX] = new BoolFeedback(() => {
                        var output = Chassis.Outputs[tempX];

                        var hdmiTxOutput = output as Card.HdmiTx;
                        if (hdmiTxOutput != null)
                            return hdmiTxOutput.HdmiOutput.DisabledByHdcp.BoolValue;

                        var dmHdmiOutput = output as Card.DmHdmiOutput;
                        if (dmHdmiOutput != null)
                            return dmHdmiOutput.DisabledByHdcpFeedback.BoolValue;

                        var dmsDmOutAdvanced = output as Card.DmsDmOutAdvanced;
                        if (dmsDmOutAdvanced != null)
                            return dmsDmOutAdvanced.DisabledByHdcpFeedback.BoolValue;

                        var dmps3HdmiAudioOutput = output as Card.Dmps3HdmiAudioOutput;
                        if (dmps3HdmiAudioOutput != null)
                            return dmps3HdmiAudioOutput.HdmiOutputPort.DisabledByHdcpFeedback.BoolValue;

                        var dmps3HdmiOutput = output as Card.Dmps3HdmiOutput;
                        if (dmps3HdmiOutput != null)
                            return dmps3HdmiOutput.HdmiOutputPort.DisabledByHdcpFeedback.BoolValue;

                        var dmps3HdmiOutputBackend = output as Card.Dmps3HdmiOutputBackend;
                        if (dmps3HdmiOutputBackend != null)
                            return dmps3HdmiOutputBackend.HdmiOutputPort.DisabledByHdcpFeedback.BoolValue;

                        // var hdRx4kX10HdmiOutput = output as HdRx4kX10HdmiOutput;
                        // if (hdRx4kX10HdmiOutput != null)
                        //     return hdRx4kX10HdmiOutput.HdmiOutputPort.DisabledByHdcpFeedback.BoolValue;

                        // var hdMdNxMHdmiOutput = output as HdMdNxMHdmiOutput;
                        // if (hdMdNxMHdmiOutput != null)
                        //     return hdMdNxMHdmiOutput.HdmiOutputPort.DisabledByHdcpFeedback.BoolValue;

                        return false;
                    });
                }

                if (Chassis.Inputs[tempX] != null)
                {
                    UsbInputRoutedToFeebacks[tempX] = new IntFeedback(() => {
                        if (Chassis.Inputs[tempX].USBRoutedToFeedback != null)
                            return (ushort)Chassis.Inputs[tempX].USBRoutedToFeedback.Number;

                        return 0;
                    });
                    VideoInputSyncFeedbacks[tempX] = new BoolFeedback(() => {
                        if (Chassis.Inputs[tempX].VideoDetectedFeedback != null)
                            return Chassis.Inputs[tempX].VideoDetectedFeedback.BoolValue;
                        
                        return false;
                    });
                    InputNameFeedbacks[tempX] = new StringFeedback(() => {
                        if (Chassis.Inputs[tempX].NameFeedback != null)
                            return Chassis.Inputs[tempX].NameFeedback.StringValue;

                        return "";
                    });

                    InputEndpointOnlineFeedbacks[tempX] = new BoolFeedback(() => { return Chassis.Inputs[tempX].EndpointOnlineFeedback; });

                    InputCardHdcpCapabilityFeedbacks[tempX] = new IntFeedback(() => {
                        var inputCard = Chassis.Inputs[tempX];

                        if (inputCard.Card is DmcHd)
                        {
                            InputCardHdcpCapabilityTypes[tempX] = eHdcpCapabilityType.HdcpAutoSupport;

                            if ((inputCard.Card as DmcHd).HdmiInput.HdcpSupportOnFeedback.BoolValue)
                                return 1;
                            return 0;
                        }

                        if (inputCard.Card is DmcHdDsp)
                        {
                            InputCardHdcpCapabilityTypes[tempX] = eHdcpCapabilityType.HdcpAutoSupport;

                            if ((inputCard.Card as DmcHdDsp).HdmiInput.HdcpSupportOnFeedback.BoolValue)
                                return 1;
                            return 0;
                        }
                        if (inputCard.Card is Dmc4kHdBase)
                        {
                            InputCardHdcpCapabilityTypes[tempX] = eHdcpCapabilityType.Hdcp2_2Support;
                            return (int)(inputCard.Card as Dmc4kHdBase).HdmiInput.HdcpReceiveCapability;
                        }
                        if (inputCard.Card is Dmc4kCBase)
                        {
                            if (PropertiesConfig.InputSlotSupportsHdcp2[tempX])
                            {
                                InputCardHdcpCapabilityTypes[tempX] = eHdcpCapabilityType.HdcpAutoSupport;
                                return (int)(inputCard.Card as Dmc4kCBase).DmInput.HdcpReceiveCapability;
                            }

                            if ((inputCard.Card as Dmc4kCBase).DmInput.HdcpSupportOnFeedback.BoolValue)
                                return 1;
                            return 0;
                        }
                        if (inputCard.Card is Dmc4kCDspBase)
                        {
                            if (PropertiesConfig.InputSlotSupportsHdcp2[tempX])
                            {
                                InputCardHdcpCapabilityTypes[tempX] = eHdcpCapabilityType.HdcpAutoSupport;
                                return (int)(inputCard.Card as Dmc4kCDspBase).DmInput.HdcpReceiveCapability;
                            }

                            if ((inputCard.Card as Dmc4kCDspBase).DmInput.HdcpSupportOnFeedback.BoolValue)
                                return 1;
                            
                            return 0;
                        }
                        return 0;
                    });
                }
            }
        }

        private void ChassisOnBaseEvent(GenericBase device, BaseEventArgs args)
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="number"></param>
        public void AddInputCard(string type, uint number)
        {
            Debug.Console(2, this, "Adding input card '{0}', slot {1}", type, number);

            type = type.ToLower();

            if (type == "dmchd")
            {
                var inputCard = new DmcHd(number, this.Chassis);
                var cecPort = inputCard.HdmiInput as ICec;
                AddHdmiInCardPorts(number, cecPort);
            }
            else if (type == "dmchddsp")
            {
                var inputCard = new DmcHdDsp(number, this.Chassis);
                var cecPort = inputCard.HdmiInput as ICec;
                AddHdmiInCardPorts(number, cecPort);
            }
            else if (type == "dmc4khd")
            {
                var inputCard = new Dmc4kHd(number, this.Chassis);
                var cecPort = inputCard.HdmiInput as ICec;
                AddHdmiInCardPorts(number, cecPort);
            }
            else if (type == "dmc4khddsp")
            {
                var inputCard = new Dmc4kHdDsp(number, this.Chassis);
                var cecPort = inputCard.HdmiInput as ICec;
                AddHdmiInCardPorts(number, cecPort);
            }
            else if (type == "dmc4kzhd")
            {
                var inputCard = new Dmc4kzHd(number, this.Chassis);
                var cecPort = inputCard.HdmiInput as ICec;
                AddHdmiInCardPorts(number, cecPort);
            }
            else if (type == "dmc4kzhddsp")
            {
                var inputCard = new Dmc4kzHdDsp(number, this.Chassis);
                var cecPort = inputCard.HdmiInput as ICec;
                AddHdmiInCardPorts(number, cecPort);
            }
            else if (type == "dmcc")
            {
                var inputCard = new DmcC(number, this.Chassis);
                var cecPort = inputCard.DmInput as ICec;
                AddDmInCardPorts(number, cecPort);
            }
            else if (type == "dmccdsp")
            {
                var inputCard = new DmcCDsp(number, this.Chassis);
                var cecPort = inputCard.DmInput as ICec;
                AddDmInCardPorts(number, cecPort);
            }
            else if (type == "dmc4kc")
            {
                var inputCard = new Dmc4kC(number, this.Chassis);
                var cecPort = inputCard.DmInput as ICec;
                AddDmInCardPorts(number, cecPort);
            }
            else if (type == "dmc4kcdsp")
            {
                var inputCard = new Dmc4kCDsp(number, this.Chassis);
                var cecPort = inputCard.DmInput as ICec;
                AddDmInCardPorts(number, cecPort);
            }
            else if (type == "dmc4kzc")
            {
                var inputCard = new Dmc4kzC(number, this.Chassis);
                var cecPort = inputCard.DmInput as ICec;
                AddDmInCardPorts(number, cecPort);
            }
            else if (type == "dmc4kzcdsp")
            {
                var inputCard = new Dmc4kzCDsp(number, this.Chassis);
                var cecPort = inputCard.DmInput as ICec;
                AddDmInCardPorts(number, cecPort);
            }
            else if (type == "dmccat")
            {
                new DmcCat(number, this.Chassis);
                AddDmInCardPorts(number);
            }
            else if (type == "dmccatdsp")
            {
                new DmcCatDsp(number, this.Chassis);
                AddDmInCardPorts(number);
            }
            else if (type == "dmcs")
            {
                new DmcS(number, Chassis);
                AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmMmFiber);
                AddInCardHdmiAndAudioLoopPorts(number);
            }
            else if (type == "dmcsdsp")
            {
                new DmcSDsp(number, Chassis);
                AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmMmFiber);
                AddInCardHdmiAndAudioLoopPorts(number);
            }
            else if (type == "dmcs2")
            {
                new DmcS2(number, Chassis);
                AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmSmFiber);
                AddInCardHdmiAndAudioLoopPorts(number);
            }
            else if (type == "dmcs2dsp")
            {
                new DmcS2Dsp(number, Chassis);
                AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmSmFiber);
                AddInCardHdmiAndAudioLoopPorts(number);
            }
            else if (type == "dmcsdi")
            {
                new DmcSdi(number, Chassis);
                AddInputPortWithDebug(number, "sdiIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Sdi);
                AddOutputPortWithDebug(string.Format("inputCard{0}", number), "sdiOut", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                    eRoutingPortConnectionType.Sdi, null);
                AddInCardHdmiAndAudioLoopPorts(number);
            }
            else if (type == "dmcdvi")
            {
                new DmcDvi(number, Chassis);
                AddInputPortWithDebug(number, "dviIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Dvi);
                AddInputPortWithDebug(number, "audioIn", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio);
                AddInCardHdmiLoopPort(number);
            }
            else if (type == "dmcvga")
            {
                new DmcVga(number, Chassis);
                AddInputPortWithDebug(number, "vgaIn", eRoutingSignalType.Video, eRoutingPortConnectionType.Vga);
                AddInputPortWithDebug(number, "audioIn", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio);
                AddInCardHdmiLoopPort(number);
            }
            else if (type == "dmcvidbnc")
            {
                new DmcVidBnc(number, Chassis);
                AddInputPortWithDebug(number, "componentIn", eRoutingSignalType.Video, eRoutingPortConnectionType.Component);
                AddInputPortWithDebug(number, "audioIn", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio);
                AddInCardHdmiLoopPort(number);
            }
            else if (type == "dmcvidrcaa")
            {
                new DmcVidRcaA(number, Chassis);
                AddInputPortWithDebug(number, "componentIn", eRoutingSignalType.Video, eRoutingPortConnectionType.Component);
                AddInputPortWithDebug(number, "audioIn", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio);
                AddInCardHdmiLoopPort(number);
            }
            else if (type == "dmcvidrcad")
            {
                new DmcVidRcaD(number, Chassis);
                AddInputPortWithDebug(number, "componentIn", eRoutingSignalType.Video, eRoutingPortConnectionType.Component);
                AddInputPortWithDebug(number, "audioIn", eRoutingSignalType.Audio, eRoutingPortConnectionType.DigitalAudio);
                AddInCardHdmiLoopPort(number);
            }
            else if (type == "dmcvid4")
            {
                new DmcVid4(number, Chassis);
                AddInputPortWithDebug(number, "compositeIn1", eRoutingSignalType.Video, eRoutingPortConnectionType.Composite);
                AddInputPortWithDebug(number, "compositeIn2", eRoutingSignalType.Video, eRoutingPortConnectionType.Composite);
                AddInputPortWithDebug(number, "compositeIn3", eRoutingSignalType.Video, eRoutingPortConnectionType.Composite);
                AddInputPortWithDebug(number, "compositeIn4", eRoutingSignalType.Video, eRoutingPortConnectionType.Composite);
                AddInCardHdmiLoopPort(number);
            }
            else if (type == "dmcstr")
            {
                new DmcStr(number, Chassis);
                AddInputPortWithDebug(number, "streamIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Streaming);
                AddInCardHdmiAndAudioLoopPorts(number);
            }
        }

        void AddDmInCardPorts(uint number)
        {
            AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmCat);
            AddInCardHdmiAndAudioLoopPorts(number);
        }

        void AddDmInCardPorts(uint number, ICec cecPort)
        {
            AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmCat, cecPort);
            AddInCardHdmiAndAudioLoopPorts(number);
        }

        void AddHdmiInCardPorts(uint number, ICec cecPort)
        {
            AddInputPortWithDebug(number, "hdmiIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, cecPort);
            AddInCardHdmiAndAudioLoopPorts(number);
        }

        void AddInCardHdmiAndAudioLoopPorts(uint number)
        {
            AddOutputPortWithDebug(string.Format("inputCard{0}", number), "hdmiLoopOut", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, null);
            AddOutputPortWithDebug(string.Format("inputCard{0}", number), "audioLoopOut", eRoutingSignalType.Audio, eRoutingPortConnectionType.Hdmi, null);
        }

        void AddInCardHdmiLoopPort(uint number)
        {
            AddOutputPortWithDebug(string.Format("inputCard{0}", number), "hdmiLoopOut", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="number"></param>
        public void AddOutputCard(string type, uint number)
        {
            type = type.ToLower();

            Debug.Console(2, this, "Adding output card '{0}', slot {1}", type, number);
            if (type == "dmc4khdo")
            {
                var outputCard = new Dmc4kHdoSingle(number, Chassis);
                var cecPort1 = outputCard.Card1.HdmiOutput;
                var cecPort2 = outputCard.Card2.HdmiOutput;
                AddDmcHdoPorts(number, cecPort1, cecPort2);
            }
            else if (type == "dmchdo")
            {
                var outputCard = new DmcHdoSingle(number, Chassis);
                var cecPort1 = outputCard.Card1.HdmiOutput;
                var cecPort2 = outputCard.Card2.HdmiOutput;
                AddDmcHdoPorts(number, cecPort1, cecPort2);
            }
            else if (type == "dmc4kcohd")
            {
                var outputCard = new Dmc4kCoHdSingle(number, Chassis);
                var cecPort1 = outputCard.Card1.HdmiOutput;
                AddDmcCoPorts(number, cecPort1);
            }
            else if (type == "dmc4kzcohd")
            {
                var outputCard = new Dmc4kzCoHdSingle(number, Chassis);
                var cecPort1 = outputCard.Card1.HdmiOutput;
                AddDmcCoPorts(number, cecPort1);
            }
            else if (type == "dmccohd")
            {
                var outputCard = new DmcCoHdSingle(number, Chassis);
                var cecPort1 = outputCard.Card1.HdmiOutput;
                AddDmcCoPorts(number, cecPort1);
            }
            else if (type == "dmccatohd")
            {
                var outputCard = new DmcCatoHdSingle(number, Chassis);
                var cecPort1 = outputCard.Card1.HdmiOutput;
                AddDmcCoPorts(number, cecPort1);
            }
            else if (type == "dmcsohd")
            {
                var outputCard = new DmcSoHdSingle(number, Chassis);
                var cecPort1 = outputCard.Card1.HdmiOutput;
                AddOutputPortWithDebug(string.Format("outputCard{0}", number), "dmOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                    eRoutingPortConnectionType.DmMmFiber, 2 * (number - 1) + 1);
                AddOutputPortWithDebug(string.Format("outputCard{0}", number), "hdmiOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                    eRoutingPortConnectionType.Hdmi, 2 * (number - 1) + 1, cecPort1);
                AddOutputPortWithDebug(string.Format("outputCard{0}", number), "dmOut2", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                    eRoutingPortConnectionType.DmMmFiber, 2 * (number - 1) + 2);
            }
            else if (type == "dmcs2ohd")
            {
                var outputCard = new DmcS2oHdSingle(number, Chassis);
                var cecPort1 = outputCard.Card1.HdmiOutput;
                AddOutputPortWithDebug(string.Format("outputCard{0}", number), "dmOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                    eRoutingPortConnectionType.DmSmFiber, 2 * (number - 1) + 1);
                AddOutputPortWithDebug(string.Format("outputCard{0}", number), "hdmiOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                    eRoutingPortConnectionType.Hdmi, 2 * (number - 1) + 1, cecPort1);
                AddOutputPortWithDebug(string.Format("outputCard{0}", number), "dmOut2", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                    eRoutingPortConnectionType.DmSmFiber, 2 * (number - 1) + 2);
            }
            else if (type == "dmcstro")
            {
                var outputCard = new DmcStroSingle(number, Chassis);
                AddOutputPortWithDebug(string.Format("outputCard{0}", number), "streamOut", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                    eRoutingPortConnectionType.Streaming, 2 * (number - 1) + 1);
            }

            else
                Debug.Console(1, this, "  WARNING: Output card type '{0}' is not available", type);
        }

        void AddDmcHdoPorts(uint number, ICec cecPort1, ICec cecPort2)
        {
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "hdmiOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, 2 * (number - 1) + 1, cecPort1);
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "audioOut1", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio,
                2 * (number - 1) + 1);
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "hdmiOut2", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, 2 * (number - 1) + 2, cecPort2);
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "audioOut2", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio,
                2 * (number - 1) + 2);
        }

        void AddDmcCoPorts(uint number, ICec cecPort1)
        {
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "dmOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.DmCat, 2 * (number - 1) + 1);
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "hdmiOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, 2 * (number - 1) + 1, cecPort1);
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "dmOut2", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.DmCat, 2 * (number - 1) + 2);
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
        /// Adds OutputPort
        /// </summary>
        void AddOutputPortWithDebug(string cardName, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType, object selector)
        {
            var portKey = string.Format("{0}--{1}", cardName, portName);
            Debug.Console(2, this, "Adding output port '{0}'", portKey);
            OutputPorts.Add(new RoutingOutputPort(portKey, sigType, portType, selector, this));
        }

        /// <summary>
        /// Adds OutputPort and sets Port as ICec object
        /// </summary>
        void AddOutputPortWithDebug(string cardName, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType, object selector, ICec cecPort)
        {
            var portKey = string.Format("{0}--{1}", cardName, portName);
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

        //public void SetInputHdcpSupport(uint input, ePdtHdcpSupport hdcpSetting)
        //{

        //}

        void Chassis_DMSystemChange(Switch device, DMSystemEventArgs args)
        {
            switch (args.EventId)
            {
                case DMSystemEventIds.SystemIdEventId:
                {
                    Debug.Console(2, this, "SystemIdEvent Value: {0}", (Chassis as DmMDMnxn).SystemIdFeedback.UShortValue);
                    SystemIdFeebdack.FireUpdate();
                    break;
                }
                case DMSystemEventIds.SystemIdBusyEventId:
                {
                    Debug.Console(2, this, "SystemIdBusyEvent State: {0}", (Chassis as DmMDMnxn).SystemIdBusy.BoolValue);
                    SystemIdBusyFeedback.FireUpdate();
                    break;
                }
                case DMSystemEventIds.AudioBreakawayEventId:
                {
                    Debug.Console(2, this, "AudioBreakaway Event: value: {0}",
                        (Chassis as DmMDMnxn).EnableAudioBreakawayFeedback.BoolValue);
                    EnableAudioBreakawayFeedback.FireUpdate();
                    break;
                }
                case DMSystemEventIds.USBBreakawayEventId:
                {
                    Debug.Console(2, this, "USBBreakaway Event: value: {0}",
                        (Chassis as DmMDMnxn).EnableUSBBreakawayFeedback.BoolValue);
                    EnableUsbBreakawayFeedback.FireUpdate();
                    break;
                }
            }
        }

        void Chassis_DMInputChange(Switch device, DMInputEventArgs args)
        {
            switch (args.EventId)
            {
                case DMInputEventIds.EndpointOnlineEventId:
                {
                    Debug.Console(2, this, "DM Input EndpointOnlineEventId for input: {0}. State: {1}", args.Number, device.Inputs[args.Number].EndpointOnlineFeedback);
                    InputEndpointOnlineFeedbacks[args.Number].FireUpdate();
                    break;
                }
                case DMInputEventIds.OnlineFeedbackEventId:
                {
                    Debug.Console(2, this, "DM Input OnlineFeedbackEventId for input: {0}. State: {1}", args.Number, device.Inputs[args.Number].EndpointOnlineFeedback);
                    InputEndpointOnlineFeedbacks[args.Number].FireUpdate();
                    break;
                }
                case DMInputEventIds.VideoDetectedEventId:
                {
                    Debug.Console(2, this, "DM Input {0} VideoDetectedEventId", args.Number);
                    VideoInputSyncFeedbacks[args.Number].FireUpdate();
                    break;
                }
                case DMInputEventIds.InputNameEventId:
                {
                    Debug.Console(2, this, "DM Input {0} NameFeedbackEventId", args.Number);
                    InputNameFeedbacks[args.Number].FireUpdate();
                    break;
                }
                case DMInputEventIds.UsbRoutedToEventId:
                {
                    Debug.Console(2, this, "DM Input {0} UsbRoutedToEventId", args.Number);
                    if (UsbInputRoutedToFeebacks[args.Number] != null)
                        UsbInputRoutedToFeebacks[args.Number].FireUpdate();
                    else
                        Debug.Console(1, this, "No index of {0} found in UsbInputRoutedToFeedbacks");
                    break;
                }
                case DMInputEventIds.HdcpCapabilityFeedbackEventId:
                {
                    Debug.Console(2, this, "DM Input {0} HdcpCapabilityFeedbackEventId", args.Number);
                    if (InputCardHdcpCapabilityFeedbacks[args.Number] != null)
                        InputCardHdcpCapabilityFeedbacks[args.Number].FireUpdate();
                    else
                        Debug.Console(1, this, "No index of {0} found in InputCardHdcpCapabilityFeedbacks");
                    break;
                }
                default:
                {
                    Debug.Console(2, this, "DMInputChange fired for Input {0} with Unhandled EventId: {1}", args.Number, args.EventId);
                    break;
                }
            }
        }

        /// 
        /// </summary>
        void Chassis_DMOutputChange(Switch device, DMOutputEventArgs args)
        {
            var output = args.Number;

            switch (args.EventId)
            {
                case DMOutputEventIds.VolumeEventId:
                {
                    if (VolumeControls.ContainsKey(output))
                    {
                        VolumeControls[args.Number].VolumeEventFromChassis();
                    }

                    break;
                }
                case DMOutputEventIds.EndpointOnlineEventId:
                {
                    Debug.Console(2, this, "Output {0} DMOutputEventIds.EndpointOnlineEventId fired. State: {1}", args.Number,
                        Chassis.Outputs[output].EndpointOnlineFeedback);
                    OutputEndpointOnlineFeedbacks[output].FireUpdate();
                    break;
                }
                case DMOutputEventIds.OnlineFeedbackEventId:
                {
                    Debug.Console(2, this, "Output {0} DMInputEventIds.OnlineFeedbackEventId fired. State: {1}", args.Number,
                        Chassis.Outputs[output].EndpointOnlineFeedback);
                    OutputEndpointOnlineFeedbacks[output].FireUpdate();
                    break;
                }
                case DMOutputEventIds.VideoOutEventId:
                {
                    if (Chassis.Outputs[output].VideoOutFeedback != null)
                        Debug.Console(2, this, "DMSwitchVideo:{0} Routed Input:{1} Output:{2}'", this.Name, Chassis.Outputs[output].VideoOutFeedback.Number, output);

                    if (VideoOutputFeedbacks.ContainsKey(output))
                        VideoOutputFeedbacks[output].FireUpdate();

                    if (OutputVideoRouteNameFeedbacks.ContainsKey(output))
                        OutputVideoRouteNameFeedbacks[output].FireUpdate();

                    break;
                }
                case DMOutputEventIds.AudioOutEventId:
                {
                    if (Chassis.Outputs[output].AudioOutFeedback != null)
                        Debug.Console(2, this, "DMSwitchAudio:{0} Routed Input:{1} Output:{2}'", this.Name, Chassis.Outputs[output].AudioOutFeedback.Number, output);

                    if (AudioOutputFeedbacks.ContainsKey(output))
                        AudioOutputFeedbacks[output].FireUpdate();

                    if (OutputAudioRouteNameFeedbacks.ContainsKey(output))
                        OutputAudioRouteNameFeedbacks[output].FireUpdate();

                    break;
                }
                case DMOutputEventIds.OutputNameEventId:
                {
                    Debug.Console(2, this, "DM Output {0} NameFeedbackEventId", output);
                    OutputNameFeedbacks[output].FireUpdate();
                    break;
                }
                case DMOutputEventIds.UsbRoutedToEventId:
                {
                    Debug.Console(2, this, "DM Output {0} UsbRoutedToEventId", args.Number);
                    UsbOutputRoutedToFeebacks[args.Number].FireUpdate();
                    break;
                }
                case DMOutputEventIds.DisabledByHdcpEventId:
                {
                    Debug.Console(2, this, "DM Output {0} DisabledByHdcpEventId", args.Number);
                    OutputDisabledByHdcpFeedbacks[args.Number].FireUpdate();
                    break;
                }
                default:
                {
                    Debug.Console(2, this, "DMOutputChange fired for Output {0} with Unhandled EventId: {1}", args.Number, args.EventId);
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
            RouteOffTimers[pnt] = new CTimer(o => { ExecuteSwitch(0, pnt.Number, pnt.Type); }, RouteOffTime);
        }

        // Send out sigs when coming online
        void IsOnline_OutputChange(object sender, EventArgs e)
        {
            if (IsOnline.BoolValue)
            {
                (Chassis as DmMDMnxn).EnableAudioBreakaway.BoolValue = true;
                (Chassis as DmMDMnxn).EnableUSBBreakaway.BoolValue = true;


                EnableAudioBreakawayFeedback.FireUpdate();
                EnableUsbBreakawayFeedback.FireUpdate();

                if (InputNames != null)
                    foreach (var kvp in InputNames)
                        Chassis.Inputs[kvp.Key].Name.StringValue = kvp.Value;
                if (OutputNames != null)
                    foreach (var kvp in OutputNames)
                        Chassis.Outputs[kvp.Key].Name.StringValue = kvp.Value;
            }
        }

        #region IRouting Members
        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType sigType)
        {
            Debug.Console(2, this, "Making an awesome DM route from {0} to {1} {2}", inputSelector, outputSelector, sigType);

            var input = Convert.ToUInt32(inputSelector); // Cast can sometimes fail
            var output = Convert.ToUInt32(outputSelector);

            var chassisSize = (uint) Chassis.NumberOfInputs; //need this to determine USB routing values 8x8 -> 1-8 is inputs 1-8, 17-24 is outputs 1-8
                                                      //16x16 1-16 is inputs 1-16, 17-32 is outputs 1-16
                                                      //32x32 1-32 is inputs 1-32, 33-64 is outputs 1-32

            // Check to see if there's an off timer waiting on this and if so, cancel
            var key = new PortNumberType(output, sigType);
            if (input == 0)
            {
                StartOffTimer(key);
            }
            else
            {
                if (RouteOffTimers.ContainsKey(key))
                {
                    Debug.Console(2, this, "{0} cancelling route off due to new source", output);
                    RouteOffTimers[key].Stop();
                    RouteOffTimers.Remove(key);
                }
            }

            var inCard = input == 0 ? null : Chassis.Inputs[input];
            var outCard = input == 0 ? null : Chassis.Outputs[output];

            // NOTE THAT BITWISE COMPARISONS - TO CATCH ALL ROUTING TYPES 
            if ((sigType & eRoutingSignalType.Video) == eRoutingSignalType.Video)
            {
                Chassis.VideoEnter.BoolValue = true;
                Chassis.Outputs[output].VideoOut = inCard;
            }

            if ((sigType & eRoutingSignalType.Audio) == eRoutingSignalType.Audio)
            {
                (Chassis as DmMDMnxn).AudioEnter.BoolValue = true;
                Chassis.Outputs[output].AudioOut = inCard;
            }

            if ((sigType & eRoutingSignalType.UsbOutput) == eRoutingSignalType.UsbOutput)
            {
                //using base here because USB can be routed between 2 output cards or 2 input cards
                DMInputOutputBase dmCard;

                Debug.Console(2, this, "Executing USB Output switch.\r\n in:{0} output: {1}", input, output);

                if (input > chassisSize)
                {
                    //wanting to route an output to an output. Subtract chassis size and get output, unless it's 8x8
                    //need this to determine USB routing values
                    //8x8 -> 1-8 is inputs 1-8, 17-24 is outputs 1-8
                    //16x16 1-16 is inputs 1-16, 17-32 is outputs 1-16
                    //32x32 1-32 is inputs 1-32, 33-64 is outputs 1-32
                    uint outputIndex;

                    if (chassisSize == 8)
                    {
                        outputIndex = input - 16;
                    }
                    else
                    {
                        outputIndex = input - chassisSize;
                    }
                    dmCard = Chassis.Outputs[outputIndex];
                }
                else
                {
                    dmCard = inCard;
                }
                Chassis.USBEnter.BoolValue = true;
                if (Chassis.Outputs[output] != null)
                {
                    Debug.Console(2, this, "Routing USB for input {0} to {1}", Chassis.Outputs[input], dmCard);
                    Chassis.Outputs[output].USBRoutedTo = dmCard;
                }
            }

            if ((sigType & eRoutingSignalType.UsbInput) == eRoutingSignalType.UsbInput)
            {
                //using base here because USB can be routed between 2 output cards or 2 input cards
                DMInputOutputBase dmCard;

                Debug.Console(2, this, "Executing USB Input switch.\r\n in:{0} output: {1}", input, output);

                if (output > chassisSize)
                {
                    //wanting to route an input to an output. Subtract chassis size and get output, unless it's 8x8
                    //need this to determine USB routing values
                    //8x8 -> 1-8 is inputs 1-8, 17-24 is outputs 1-8
                    //16x16 1-16 is inputs 1-16, 17-32 is outputs 1-16
                    //32x32 1-32 is inputs 1-32, 33-64 is outputs 1-32
                    uint outputIndex;

                    if (chassisSize == 8)
                    {
                        outputIndex = input - 16;
                    }
                    else
                    {
                        outputIndex = input - chassisSize;
                    }
                    dmCard = Chassis.Outputs[outputIndex];
                }
                else
                {
                    dmCard = Chassis.Inputs[input];
                }

                

                Chassis.USBEnter.BoolValue = true;

                if (Chassis.Inputs[output] == null)
                {
                    return;
                }
                Debug.Console(2, this, "Routing USB for input {0} to {1}", Chassis.Inputs[output], dmCard);
                Chassis.Inputs[output].USBRoutedTo = dmCard;
            }
        }
        #endregion

        #region IRoutingNumeric Members

        public void ExecuteNumericSwitch(ushort inputSelector, ushort outputSelector, eRoutingSignalType sigType)
        {
            ExecuteSwitch(inputSelector, outputSelector, sigType);
        }

        #endregion

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new DmChassisControllerJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<DmChassisControllerJoinMap>(joinMapSerialized);

            bridge.AddJoinMap(Key, joinMap);

            Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            var chassis = Chassis as DmMDMnxn;

            IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);

            trilist.SetUShortSigAction(joinMap.SystemId.JoinNumber, o =>
            {
                if (chassis != null)
                    chassis.SystemId.UShortValue = o;
            });

            trilist.SetSigTrueAction(joinMap.SystemId.JoinNumber, () =>
            {
                                                                 if (chassis != null) chassis.ApplySystemId();
            });

            SystemIdFeebdack.LinkInputSig(trilist.UShortInput[joinMap.SystemId.JoinNumber]);
            SystemIdBusyFeedback.LinkInputSig(trilist.BooleanInput[joinMap.SystemId.JoinNumber]);

            EnableAudioBreakawayFeedback.LinkInputSig(trilist.BooleanInput[joinMap.EnableAudioBreakaway.JoinNumber]);
            EnableUsbBreakawayFeedback.LinkInputSig(trilist.BooleanInput[joinMap.EnableUsbBreakaway.JoinNumber]);

            trilist.OnlineStatusChange += (o, a) =>
            {
                if (!a.DeviceOnLine) return;

                EnableAudioBreakawayFeedback.FireUpdate();
                EnableUsbBreakawayFeedback.FireUpdate();
                SystemIdBusyFeedback.FireUpdate();
                SystemIdFeebdack.FireUpdate();
            };

            // Link up outputs
            for (uint i = 1; i <= Chassis.NumberOfOutputs; i++)
            {
                var ioSlot = i;
                var ioSlotJoin = ioSlot - 1;

                // Control
                trilist.SetUShortSigAction(joinMap.OutputVideo.JoinNumber + ioSlotJoin, o => ExecuteSwitch(o, ioSlot, eRoutingSignalType.Video));
                trilist.SetUShortSigAction(joinMap.OutputAudio.JoinNumber + ioSlotJoin, o => ExecuteSwitch(o, ioSlot, eRoutingSignalType.Audio));
                trilist.SetUShortSigAction(joinMap.OutputUsb.JoinNumber + ioSlotJoin, o => ExecuteSwitch(o, ioSlot, eRoutingSignalType.UsbOutput));
                trilist.SetUShortSigAction(joinMap.InputUsb.JoinNumber + ioSlotJoin, o => ExecuteSwitch(o, ioSlot, eRoutingSignalType.UsbInput));

                if (TxDictionary.ContainsKey(ioSlot))
                {
                    Debug.Console(2, "Creating Tx Feedbacks {0}", ioSlot);
                    var txKey = TxDictionary[ioSlot];
                    var basicTxDevice = DeviceManager.GetDeviceForKey(txKey) as BasicDmTxControllerBase;

                    var advancedTxDevice = basicTxDevice as DmTxControllerBase;

                    if (Chassis is DmMd8x8Cpu3 || Chassis is DmMd8x8Cpu3rps
                        || Chassis is DmMd16x16Cpu3 || Chassis is DmMd16x16Cpu3rps
                        || Chassis is DmMd32x32Cpu3 || Chassis is DmMd32x32Cpu3rps)
                    {
                        InputEndpointOnlineFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.InputEndpointOnline.JoinNumber + ioSlotJoin]);
                    }
                    else
                    {
                        if (advancedTxDevice != null)
                        {
                            advancedTxDevice.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.InputEndpointOnline.JoinNumber + ioSlotJoin]);
                            Debug.Console(2, "Linking Tx Online Feedback from Advanced Transmitter at input {0}", ioSlot);
                        }
                        else if (InputEndpointOnlineFeedbacks[ioSlot] != null)
                        {
                            Debug.Console(2, "Linking Tx Online Feedback from Input Card {0}", ioSlot);
                            InputEndpointOnlineFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.InputEndpointOnline.JoinNumber + ioSlotJoin]);
                        }
                    }

                    if (basicTxDevice != null && advancedTxDevice == null)
                        trilist.BooleanInput[joinMap.TxAdvancedIsPresent.JoinNumber + ioSlotJoin].BoolValue = true;

                    if (advancedTxDevice != null)
                    {
                        advancedTxDevice.AnyVideoInput.VideoStatus.VideoSyncFeedback.LinkInputSig(trilist.BooleanInput[joinMap.VideoSyncStatus.JoinNumber + ioSlotJoin]);
                    }
                    else if (advancedTxDevice == null || basicTxDevice != null)
                    {
                        Debug.Console(1, "Setting up actions and feedbacks on input card {0}", ioSlot);
                        VideoInputSyncFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.VideoSyncStatus.JoinNumber + ioSlotJoin]);

                        var inputPort = InputPorts[string.Format("inputCard{0}--hdmiIn", ioSlot)];
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
                                        SetHdcpStateAction(true, hdmiInPortWCec, joinMap.HdcpSupportState.JoinNumber + ioSlotJoin, trilist);
                                    }

                                    InputCardHdcpCapabilityFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.HdcpSupportState.JoinNumber + ioSlotJoin]);

                                    if (InputCardHdcpCapabilityTypes.ContainsKey(ioSlot))
                                        trilist.UShortInput[joinMap.HdcpSupportCapability.JoinNumber + ioSlotJoin].UShortValue = (ushort)InputCardHdcpCapabilityTypes[ioSlot];
                                    else
                                        trilist.UShortInput[joinMap.HdcpSupportCapability.JoinNumber + ioSlotJoin].UShortValue = 1;
                                }
                            }
                        }
                        else
                        {
                            inputPort = InputPorts[string.Format("inputCard{0}--dmIn", ioSlot)];

                            if (inputPort != null)
                            {
                                var port = inputPort.Port;

                                if (port is DMInputPortWithCec)
                                {
                                    Debug.Console(1, "Port is DMInputPortWithCec");

                                    var dmInPortWCec = port as DMInputPortWithCec;

                                    if (dmInPortWCec != null)
                                    {
                                        SetHdcpStateAction(PropertiesConfig.InputSlotSupportsHdcp2[ioSlot], dmInPortWCec, joinMap.HdcpSupportState.JoinNumber + ioSlotJoin, trilist);
                                    }

                                    InputCardHdcpCapabilityFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.HdcpSupportState.JoinNumber + ioSlotJoin]);

                                    if (InputCardHdcpCapabilityTypes.ContainsKey(ioSlot))
                                        trilist.UShortInput[joinMap.HdcpSupportCapability.JoinNumber + ioSlotJoin].UShortValue = (ushort)InputCardHdcpCapabilityTypes[ioSlot];
                                    else
                                        trilist.UShortInput[joinMap.HdcpSupportCapability.JoinNumber + ioSlotJoin].UShortValue = 1;
                                }
                            }
                        }
                    }
                }
                else
                {
                    VideoInputSyncFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.VideoSyncStatus.JoinNumber + ioSlotJoin]);

                    var inputPort = InputPorts[string.Format("inputCard{0}--hdmiIn", ioSlot)];
                    if (inputPort != null)
                    {
                        var hdmiPort = inputPort.Port as EndpointHdmiInput;

                        if (hdmiPort != null)
                        {
                            SetHdcpStateAction(true, hdmiPort, joinMap.HdcpSupportState.JoinNumber + ioSlotJoin, trilist);
                            InputCardHdcpCapabilityFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.HdcpSupportState.JoinNumber + ioSlotJoin]);
                        }
                    }
                }

                if (RxDictionary.ContainsKey(ioSlot))
                {
                    Debug.Console(2, "Creating Rx Feedbacks {0}", ioSlot);
                    var rxKey = RxDictionary[ioSlot];
                    var rxDevice = DeviceManager.GetDeviceForKey(rxKey) as DmRmcControllerBase;
                    var hdBaseTDevice = DeviceManager.GetDeviceForKey(rxKey) as DmHdBaseTControllerBase;
                    if (Chassis is DmMd8x8Cpu3 || Chassis is DmMd8x8Cpu3rps
                        || Chassis is DmMd16x16Cpu3 || Chassis is DmMd16x16Cpu3rps
                        || Chassis is DmMd32x32Cpu3 || Chassis is DmMd32x32Cpu3rps || hdBaseTDevice != null)
                    {
                        OutputEndpointOnlineFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.OutputEndpointOnline.JoinNumber + ioSlotJoin]);
                    }
                    else if (rxDevice != null)
                    {
                        rxDevice.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.OutputEndpointOnline.JoinNumber + ioSlotJoin]);
                    }
                }

                // Feedback
                VideoOutputFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.OutputVideo.JoinNumber + ioSlotJoin]);
                AudioOutputFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.OutputAudio.JoinNumber + ioSlotJoin]);
                UsbOutputRoutedToFeebacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.OutputUsb.JoinNumber + ioSlotJoin]);
                UsbInputRoutedToFeebacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.InputUsb.JoinNumber + ioSlotJoin]);

                OutputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputNames.JoinNumber + ioSlotJoin]);
                InputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.InputNames.JoinNumber + ioSlotJoin]);
                OutputVideoRouteNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputCurrentVideoInputNames.JoinNumber + ioSlotJoin]);
                OutputAudioRouteNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputCurrentAudioInputNames.JoinNumber + ioSlotJoin]);

                OutputDisabledByHdcpFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.OutputDisabledByHdcp.JoinNumber + ioSlotJoin]);
            }
        }

        private void SetHdcpStateAction(bool hdcpTypeSimple, HdmiInputWithCEC port, uint join, BasicTriList trilist)
        {
            if (hdcpTypeSimple)
            {
                trilist.SetUShortSigAction(join,
                    s =>
                    {
                        if (s == 0)
                        {
                            port.HdcpSupportOff();
                        }
                        else if (s > 0)
                        {
                            port.HdcpSupportOn();
                        }
                    });
            }
            else
            {
                trilist.SetUShortSigAction(join,
                        u =>
                        {
                            port.HdcpReceiveCapability = (eHdcpCapabilityType)u;
                        });
            }
        }

        private void SetHdcpStateAction(bool hdcpTypeSimple, EndpointHdmiInput port, uint join, BasicTriList trilist)
        {
            if (hdcpTypeSimple)
            {
                trilist.SetUShortSigAction(join,
                    s =>
                    {
                        if (s == 0)
                        {
                            port.HdcpSupportOff();
                        }
                        else if (s > 0)
                        {
                            port.HdcpSupportOn();
                        }
                    });
            }
            else
            {
                trilist.SetUShortSigAction(join,
                        u =>
                        {
                            port.HdcpCapability = (eHdcpCapabilityType)u;
                        });
            }
        }

        private void SetHdcpStateAction(bool supportsHdcp2, DMInputPortWithCec port, uint join, BasicTriList trilist)
        {
            if (!supportsHdcp2)
            {
                trilist.SetUShortSigAction(join,
                    s =>
                    {
                        if (s == 0)
                        {
                            port.HdcpSupportOff();
                        }
                        else if (s > 0)
                        {
                            port.HdcpSupportOn();
                        }
                    });
            }
            else
            {
                trilist.SetUShortSigAction(join,
                        u =>
                        {
                            port.HdcpReceiveCapability = (eHdcpCapabilityType)u;
                        });
            }
        }
    }

    public struct PortNumberType
    {
        public uint Number { get; private set; }
        public eRoutingSignalType Type { get; private set; }

        public PortNumberType(uint number, eRoutingSignalType type)
            : this()
        {
            Number = number;
            Type = type;
        }
    }

    public class DmChassisControllerFactory : EssentialsDeviceFactory<DmChassisController>
    {
        public DmChassisControllerFactory()
        {
            TypeNames = new List<string>() { "dmmd8x8", "dmmd8x8rps", "dmmd8x8cpu3", "dmmd8x8cpu3rps", 
                "dmmd16x16", "dmmd16x16rps", "dmmd16x16cpu3", "dmmd16x16cpu3rps", 
                "dmmd32x32", "dmmd32x32rps", "dmmd32x32cpu3", "dmmd32x32cpu3rps", 
                "dmmd64x64", "dmmd128x128" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            var type = dc.Type.ToLower();

            Debug.Console(1, "Factory Attempting to create new DmChassisController Device");

            if (type.StartsWith("dmmd8x") || type.StartsWith("dmmd16x") || type.StartsWith("dmmd32x"))
            {

                var props = JsonConvert.DeserializeObject
                    <PepperDash.Essentials.DM.Config.DMChassisPropertiesConfig>(dc.Properties.ToString());
                return PepperDash.Essentials.DM.DmChassisController.
                    GetDmChassisController(dc.Key, dc.Name, type, props);
            }
            else if (type.StartsWith("dmmd128x") || type.StartsWith("dmmd64x"))
            {
                var props = JsonConvert.DeserializeObject
                    <PepperDash.Essentials.DM.Config.DMChassisPropertiesConfig>(dc.Properties.ToString());
                return PepperDash.Essentials.DM.DmBladeChassisController.
                    GetDmChassisController(dc.Key, dc.Name, type, props);
            }

            return null;
        }
    }

}

