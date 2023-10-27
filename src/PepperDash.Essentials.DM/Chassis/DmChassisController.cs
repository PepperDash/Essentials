extern alias Full;

using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpProInternal;
using Full.Newtonsoft.Json;
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
    public class DmChassisController : CrestronGenericBridgeableBaseDevice, IDmSwitchWithEndpointOnlineFeedback, IRoutingNumericWithFeedback
    {
        private const string NonePortKey = "inputCard0--None";
        public DMChassisPropertiesConfig PropertiesConfig { get; set; }

        public Switch Chassis { get; private set; }

        //IroutingNumericEvent
        public event EventHandler<RoutingNumericEventArgs> NumericSwitchChange;

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

        public Dictionary<uint, IntFeedback> InputCardHdcpStateFeedbacks { get; private set; }
        public Dictionary<uint, IntFeedback> InputStreamCardStateFeedbacks { get; private set; }
        public Dictionary<uint, IntFeedback> OutputStreamCardStateFeedbacks { get; private set; }

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

                //
                var clearInputPort = new RoutingInputPort(NonePortKey, eRoutingSignalType.AudioVideo,
                    eRoutingPortConnectionType.None, null, controller);

                controller.InputPorts.Add(clearInputPort);

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
            Chassis.BaseEvent += ChassisOnBaseEvent;
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

            InputCardHdcpStateFeedbacks = new Dictionary<uint, IntFeedback>();
            InputStreamCardStateFeedbacks = new Dictionary<uint, IntFeedback>();
            OutputStreamCardStateFeedbacks = new Dictionary<uint, IntFeedback>();
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
                    OutputStreamCardStateFeedbacks[tempX] = new IntFeedback(() =>
                    {
                        try
                        {
                            var outputCard = Chassis.Outputs[tempX];

                            if (outputCard.Card is DmcStroAV)
                            {
                                Debug.Console(2, "Found output stream card in slot: {0}.", tempX);
                                var streamCard = outputCard.Card as DmcStroAV;
                                if (streamCard.Control.StartFeedback.BoolValue == true)
                                    return 1;
                                else if (streamCard.Control.StopFeedback.BoolValue == true)
                                    return 2;
                                else if (streamCard.Control.PauseFeedback.BoolValue == true)
                                    return 3;
                                else
                                    return 0;
                            }
                            return 0;
                        }
                        catch (InvalidOperationException iopex)
                        {
                            Debug.Console(0, this, Debug.ErrorLogLevel.Warning, "Error adding output stream card in slot: {0}. Error: {1}", tempX, iopex);
                            return 0;
                        }
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

                    InputCardHdcpStateFeedbacks[tempX] = new IntFeedback(() => {
                        try
                        {
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
                            if (inputCard.Card is Dmc4kHdDspBase)
                            {
                                if (PropertiesConfig.InputSlotSupportsHdcp2[tempX])
                                {
                                    InputCardHdcpCapabilityTypes[tempX] = eHdcpCapabilityType.Hdcp2_2Support;
                                    return (int)(inputCard.Card as Dmc4kHdDspBase).HdmiInput.HdcpReceiveCapability;
                                }

                                InputCardHdcpCapabilityTypes[tempX] = eHdcpCapabilityType.HdcpAutoSupport;
                                if ((inputCard.Card as Dmc4kHdDspBase).HdmiInput.HdcpSupportOnFeedback.BoolValue)
                                    return 1;
                                return 0;
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
                        }
                        catch (InvalidOperationException iopex)
                        {
                            Debug.Console(0, this, Debug.ErrorLogLevel.Warning, "The Input Card in slot: {0} supports HDCP 2.  Please update the configuration value in the inputCardSupportsHdcp2 object to true. Error: {1}", tempX, iopex);
                            return 0;
                        }   
                    });
                    InputStreamCardStateFeedbacks[tempX] = new IntFeedback(() =>
                    {
                        try
                        {
                            var inputCard = Chassis.Inputs[tempX];

                            if (inputCard.Card is DmcStr)
                            {
                                Debug.Console(2, "Found input stream card in slot: {0}.", tempX);
                                var streamCard = inputCard.Card as DmcStr;
                                if (streamCard.Control.StartFeedback.BoolValue == true)
                                    return 1;
                                else if (streamCard.Control.StopFeedback.BoolValue == true)
                                    return 2;
                                else if (streamCard.Control.PauseFeedback.BoolValue == true)
                                    return 3;
                                else
                                    return 0;
                            }
                            return 0;
                        }
                        catch (InvalidOperationException iopex)
                        {
                            Debug.Console(0, this, Debug.ErrorLogLevel.Warning, "Error adding input stream card in slot: {0}. Error: {1}", tempX, iopex);
                            return 0;
                        }
                    });
                }
            }
        }

        private void ChassisOnBaseEvent(GenericBase device, BaseEventArgs args)
        {
            
        }

        private void RegisterForInputResolutionFeedback(IVideoAttributesBasic input, uint number, RoutingInputPortWithVideoStatuses inputPort)
        {
            if (input == null)
            {
                return;
            }

            Debug.Console(1, this, "Registering for resolution feedback for input {0} using Routing Port {1}", number, inputPort.Key);

            input.VideoAttributes.AttributeChange += (sender, args) =>
            {
                Debug.Console(1, this, "Input {0} resolution updated", number);

                Debug.Console(1, this, "Updating resolution feedback for input {0}", number);
                inputPort.VideoStatus.VideoResolutionFeedback.FireUpdate();
            };
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

            switch (type)
            {
                case "dmchd":
                {
                    var inputCard = new DmcHd(number, Chassis);
                    AddHdmiInCardPorts(number, inputCard.HdmiInput, inputCard.HdmiInput);
                }
                    break;
                case "dmchddsp":
                {
                    var inputCard = new DmcHdDsp(number, Chassis);
                    AddHdmiInCardPorts(number, inputCard.HdmiInput, inputCard.HdmiInput);
                }
                    break;
                case "dmc4khd":
                {
                    var inputCard = new Dmc4kHd(number, Chassis);
                    AddHdmiInCardPorts(number, inputCard.HdmiInput, inputCard.HdmiInput);
                }
                    break;
                case "dmc4khddsp":
                {
                    var inputCard = new Dmc4kHdDsp(number, Chassis);
                    AddHdmiInCardPorts(number, inputCard.HdmiInput, inputCard.HdmiInput);
                }
                    break;
                case "dmc4kzhd":
                {
                    var inputCard = new Dmc4kzHd(number, Chassis);
                    AddHdmiInCardPorts(number, inputCard.HdmiInput, inputCard.HdmiInput);
                    break;
                }
                case "dmc4kzhddsp":
                {
                    var inputCard = new Dmc4kzHdDsp(number, Chassis);
                    AddHdmiInCardPorts(number, inputCard.HdmiInput, inputCard.HdmiInput);
                    break;
                }
                case "dmcc":
                {
                    var inputCard = new DmcC(number, Chassis);
                    //DmInput doesn't implement ICec...cast was resulting in null anyway
                    AddDmInCardPorts(number, null, inputCard.DmInput);
                }
                    break;
                case "dmccdsp":
                {
                    var inputCard = new DmcCDsp(number, Chassis);
                    //DmInput doesn't implement ICec...cast was resulting in null anyway
                    AddDmInCardPorts(number, null, inputCard.DmInput);
                    break;
                }
                    
                case "dmc4kc":
                {
                    var inputCard = new Dmc4kC(number, Chassis);
                    AddDmInCardPorts(number, inputCard.DmInput,inputCard.DmInput);
                    break;
                }
                    
                case "dmc4kcdsp":
                {
                    var inputCard = new Dmc4kCDsp(number, Chassis);
                    AddDmInCardPorts(number, inputCard.DmInput,inputCard.DmInput);
                    break;
                }
                    
                case "dmc4kzc":
                {
                    var inputCard = new Dmc4kzC(number, Chassis);
                    AddDmInCardPorts(number, inputCard.DmInput,inputCard.DmInput);
                    break;
                }
                   
                case "dmc4kzcdsp":
                {
                    var inputCard = new Dmc4kzCDsp(number, Chassis);
                    AddDmInCardPorts(number, inputCard.DmInput, inputCard.DmInput);
                    break;
                }
                    
                case "dmccat":
                {
                    var inputCard = new DmcCat(number, Chassis);
                    AddDmInCardPorts(number, null, inputCard.DmInput);
                    break;
                }
                case "dmccatdsp":
                {
                    var inputCard = new DmcCatDsp(number, Chassis);
                    AddDmInCardPorts(number, null, inputCard.DmInput);
                    break;
                }
                case "dmcs":
                {
                    var inputCard = new DmcS(number, Chassis);
                    AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmMmFiber, null, inputCard.DmInput);
                    AddInCardHdmiAndAudioLoopPorts(number);
                    break;
                }
                case "dmcsdsp":
                {
                    var inputCard = new DmcSDsp(number, Chassis);
                    AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmMmFiber, null, inputCard.DmInput);
                    AddInCardHdmiAndAudioLoopPorts(number);
                    break;
                }
                case "dmcs2":
                {
                    var inputCard = new DmcS2(number, Chassis);
                    AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmSmFiber, null, inputCard.DmInput);
                    AddInCardHdmiAndAudioLoopPorts(number);
                    break;
                }
                case "dmcs2dsp":
                {
                    var inputCard = new DmcS2Dsp(number, Chassis);
                    AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmSmFiber, null, inputCard.DmInput);
                    AddInCardHdmiAndAudioLoopPorts(number);
                    break;
                }
                case "dmcsdi":
                {
                    var inputCard = new DmcSdi(number, Chassis);
                    AddInputPortWithDebug(number, "sdiIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Sdi, null, inputCard.SdiInput);
                    AddOutputPortWithDebug(string.Format("inputCard{0}", number), "sdiOut", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                        eRoutingPortConnectionType.Sdi, null);
                    AddInCardHdmiAndAudioLoopPorts(number);
                    break;
                }
                case "dmcdvi":
                {
                    var inputCard = new DmcDvi(number, Chassis);
                    AddInputPortWithDebug(number, "dviIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Dvi, null, inputCard.DviInput);
                    AddInputPortWithDebug(number, "audioIn", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio);
                    AddInCardHdmiLoopPort(number);
                    break;
                }
                case "dmcvga":
                {
                    var inputCard = new DmcVga(number, Chassis);
                    AddInputPortWithDebug(number, "vgaIn", eRoutingSignalType.Video, eRoutingPortConnectionType.Vga, null, inputCard.VgaInput);
                    AddInputPortWithDebug(number, "audioIn", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio);
                    AddInCardHdmiLoopPort(number);
                    break;
                }
                case "dmcvidbnc":
                {
                    var inputCard = new DmcVidBnc(number, Chassis);
                    AddInputPortWithDebug(number, "componentIn", eRoutingSignalType.Video, eRoutingPortConnectionType.Component, null, inputCard.VideoInput);
                    AddInputPortWithDebug(number, "audioIn", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio);
                    AddInCardHdmiLoopPort(number);
                    break;
                }
                case "dmcvidrcaa":
                {
                    var inputCard = new DmcVidRcaA(number, Chassis);
                    AddInputPortWithDebug(number, "componentIn", eRoutingSignalType.Video, eRoutingPortConnectionType.Component, null, inputCard.VideoInput);
                    AddInputPortWithDebug(number, "audioIn", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio);
                    AddInCardHdmiLoopPort(number);
                    break;
                }
                case "dmcvidrcad":
                {
                    var inputCard = new DmcVidRcaD(number, Chassis);
                    AddInputPortWithDebug(number, "componentIn", eRoutingSignalType.Video, eRoutingPortConnectionType.Component, null, inputCard.VideoInput);
                    AddInputPortWithDebug(number, "audioIn", eRoutingSignalType.Audio, eRoutingPortConnectionType.DigitalAudio);
                    AddInCardHdmiLoopPort(number);
                    break;
                }
                case "dmcvid4":
                {
                    var inputCard = new DmcVid4(number, Chassis);
                    AddInputPortWithDebug(number, "compositeIn1", eRoutingSignalType.Video, eRoutingPortConnectionType.Composite);
                    AddInputPortWithDebug(number, "compositeIn2", eRoutingSignalType.Video, eRoutingPortConnectionType.Composite);
                    AddInputPortWithDebug(number, "compositeIn3", eRoutingSignalType.Video, eRoutingPortConnectionType.Composite);
                    AddInputPortWithDebug(number, "compositeIn4", eRoutingSignalType.Video, eRoutingPortConnectionType.Composite);
                    AddInCardHdmiLoopPort(number);
                    break;
                }
                case "dmcstr":
                {
                    var inputCard = new DmcStr(number, Chassis);
                    AddInputPortWithDebug(number, "streamIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Streaming, null, inputCard.Source);
                    AddInCardHdmiAndAudioLoopPorts(number);
                    break;
                }
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

        void AddDmInCardPorts(uint number, ICec cecPort, IVideoAttributesBasic videoAttributes)
        {
            AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmCat, cecPort, videoAttributes);
            AddInCardHdmiAndAudioLoopPorts(number);
        }

        void AddHdmiInCardPorts(uint number, ICec cecPort)
        {
            AddInputPortWithDebug(number, "hdmiIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, cecPort);
            AddInCardHdmiAndAudioLoopPorts(number);
        }

        void AddHdmiInCardPorts(uint number, ICec cecPort, IVideoAttributesBasic videoAttributes)
        {
            AddInputPortWithDebug(number, "hdmiIn", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, cecPort, videoAttributes);
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
            switch (type)
            {
                case "dmc4khdo":
                {
                    var outputCard = new Dmc4kHdoSingle(number, Chassis);
                    var cecPort1 = outputCard.Card1.HdmiOutput;
                    var cecPort2 = outputCard.Card2.HdmiOutput;
                    AddDmcHdoPorts(number, cecPort1, cecPort2);
                }
                    break;
                case "dmc4kzhdo":
                {
                    var outputCard = new Dmc4kzHdoSingle(number, Chassis);
                    var cecPort1 = outputCard.Card1.HdmiOutput;
                    var cecPort2 = outputCard.Card2.HdmiOutput;
                    AddDmcHdoPorts(number, cecPort1, cecPort2);
                }
                    break;
                case "dmchdo":
                {
                    var outputCard = new DmcHdoSingle(number, Chassis);
                    var cecPort1 = outputCard.Card1.HdmiOutput;
                    var cecPort2 = outputCard.Card2.HdmiOutput;
                    AddDmcHdoPorts(number, cecPort1, cecPort2);
                }
                    break;
                case "dmc4kcohd":
                {
                    var outputCard = new Dmc4kCoHdSingle(number, Chassis);
                    var cecPort1 = outputCard.Card1.HdmiOutput;
                    AddDmcCoPorts(number, cecPort1);
                }
                    break;
                case "dmc4kzcohd":
                {
                    var outputCard = new Dmc4kzCoHdSingle(number, Chassis);
                    var cecPort1 = outputCard.Card1.HdmiOutput;
                    AddDmcCoPorts(number, cecPort1);
                }
                    break;
                case "dmccohd":
                {
                    var outputCard = new DmcCoHdSingle(number, Chassis);
                    var cecPort1 = outputCard.Card1.HdmiOutput;
                    AddDmcCoPorts(number, cecPort1);
                }
                    break;
                case "dmccatohd":
                {
                    var outputCard = new DmcCatoHdSingle(number, Chassis);
                    var cecPort1 = outputCard.Card1.HdmiOutput;
                    AddDmcCoPorts(number, cecPort1);
                }
                    break;
                case "dmcsohd":
                {
                    var outputCard = new DmcSoHdSingle(number, Chassis);
                    var cecPort1 = outputCard.Card1.HdmiOutput;
                    AddOutputPortWithDebug(string.Format("outputCard{0}", number), "dmOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                        eRoutingPortConnectionType.DmMmFiber, Chassis.Outputs[2 * (number - 1) + 1]);
                    AddOutputPortWithDebug(string.Format("outputCard{0}", number), "hdmiOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                        eRoutingPortConnectionType.Hdmi, Chassis.Outputs[2 * (number - 1) + 1], cecPort1);
                    AddOutputPortWithDebug(string.Format("outputCard{0}", number), "dmOut2", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                        eRoutingPortConnectionType.DmMmFiber, Chassis.Outputs[2 * (number - 1) + 2]);
                }
                    break;
                case "dmcs2ohd":
                {
                    var outputCard = new DmcS2oHdSingle(number, Chassis);
                    var cecPort1 = outputCard.Card1.HdmiOutput;
                    AddOutputPortWithDebug(string.Format("outputCard{0}", number), "dmOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                        eRoutingPortConnectionType.DmSmFiber, Chassis.Outputs[2 * (number - 1) + 1]);
                    AddOutputPortWithDebug(string.Format("outputCard{0}", number), "hdmiOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                        eRoutingPortConnectionType.Hdmi, Chassis.Outputs[2 * (number - 1) + 1], cecPort1);
                    AddOutputPortWithDebug(string.Format("outputCard{0}", number), "dmOut2", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                        eRoutingPortConnectionType.DmSmFiber, Chassis.Outputs[2 * (number - 1) + 2]);
                }
                    break;
                case "dmcstro":
                    AddOutputPortWithDebug(string.Format("outputCard{0}", number), "streamOut", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                        eRoutingPortConnectionType.Streaming, Chassis.Outputs[2 * (number - 1) + 1]);
                    break;
                default:
                    Debug.Console(1, this, "  WARNING: Output card type '{0}' is not available", type);
                    break;
            }
        }

        void AddDmcHdoPorts(uint number, ICec cecPort1, ICec cecPort2)
        {
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "hdmiOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, Chassis.Outputs[2 * (number - 1) + 1], cecPort1);
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "audioOut1", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio,
                Chassis.Outputs[2 * (number - 1) + 1]);
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "hdmiOut2", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, Chassis.Outputs[2 * (number - 1) + 2], cecPort2);
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "audioOut2", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio,
                Chassis.Outputs[2 * (number - 1) + 2]);
        }

        void AddDmcCoPorts(uint number, ICec cecPort1)
        {
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "dmOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.DmCat, Chassis.Outputs[2 * (number - 1) + 1]);
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "hdmiOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, Chassis.Outputs[2 * (number - 1) + 1], cecPort1);
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "dmOut2", eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.DmCat, Chassis.Outputs[2 * (number - 1) + 2]);
        }

        /// <summary>
        /// Adds InputPort
        /// </summary>
        void AddInputPortWithDebug(uint cardNum, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType)
        {
            //Cast is necessary here to determine the correct overload
            AddInputPortWithDebug(cardNum, portName, sigType, portType, null, null);
        }

        private void AddInputPortWithDebug(uint cardNum, string portName, eRoutingSignalType sigType,
            eRoutingPortConnectionType portType, ICec cecPort)
        {
            //Cast is necessary here to determine the correct overload
            AddInputPortWithDebug(cardNum, portName, sigType, portType, cecPort, null);
        }

        /// <summary>
        /// Adds InputPort and sets Port as ICec object. If videoAttributesBasic is defined, RoutingPort will be RoutingInputPortWithVideoStatuses
        /// </summary>
        void AddInputPortWithDebug(uint cardNum, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType, ICec cecPort, IVideoAttributesBasic videoAttributesBasic)
        {
            var portKey = string.Format("inputCard{0}--{1}", cardNum, portName);
            Debug.Console(2, this, "Adding input port '{0}'", portKey);

            RoutingInputPort inputPort;

            if (videoAttributesBasic != null)
            {
                Debug.Console(1, this, "card {0} supports IVideoAttributesBasic", cardNum);
                var statusFuncs = new VideoStatusFuncsWrapper
                {
                    VideoResolutionFeedbackFunc = () =>
                    {
                        var resolution = videoAttributesBasic.VideoAttributes.GetVideoResolutionString();
                        Debug.Console(1, this, "Updating resolution for input {0}. New resolution: {1}", cardNum, resolution);
                        return resolution;
                    }
                };
                inputPort = new RoutingInputPortWithVideoStatuses(portKey, sigType, portType,
                    Chassis.Inputs[cardNum], this, statusFuncs)
                {
                    FeedbackMatchObject = Chassis.Inputs[cardNum]
                };

                RegisterForInputResolutionFeedback(videoAttributesBasic, cardNum, inputPort as RoutingInputPortWithVideoStatuses);
            }
            else
            {
                inputPort = new RoutingInputPort(portKey, sigType, portType,
                    Chassis.Inputs[cardNum], this)
                {
                    FeedbackMatchObject = Chassis.Inputs[cardNum]
                };
            }

            if (cecPort != null)
                inputPort.Port = cecPort;

            InputPorts.Add(inputPort);
        }

        /// <summary>
        /// Adds OutputPort
        /// </summary>
        void AddOutputPortWithDebug(string cardName, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType, object selector)
        {
            AddOutputPortWithDebug(cardName, portName, sigType, portType, selector, null);
        }

        /// <summary>
        /// Adds OutputPort and sets Port as ICec object
        /// </summary>
        void AddOutputPortWithDebug(string cardName, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType, object selector, ICec cecPort)
        {
            var portKey = string.Format("{0}--{1}", cardName, portName);
            Debug.Console(2, this, "Adding output port '{0}'", portKey);
            var outputPort = new RoutingOutputPort(portKey, sigType, portType, selector, this);

            if (portName.IndexOf("Loop", StringComparison.InvariantCultureIgnoreCase) < 0)
            {
                outputPort.FeedbackMatchObject = selector;
            }
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
            try
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
                            if (InputCardHdcpStateFeedbacks[args.Number] != null)
                                InputCardHdcpStateFeedbacks[args.Number].FireUpdate();
                            else
                                Debug.Console(1, this, "No index of {0} found in InputCardHdcpCapabilityFeedbacks");
                            break;
                        }
                    case DMInputEventIds.HdcpSupportOffEventId:
                        {
                            Debug.Console(2, this, "DM Input {0} HdcpSupportOffEventId", args.Number);
                            if (InputCardHdcpStateFeedbacks[args.Number] != null)
                                InputCardHdcpStateFeedbacks[args.Number].FireUpdate();
                            else
                                Debug.Console(1, this, "No index of {0} found in InputCardHdcpCapabilityFeedbacks");
                            break;
                        }
                    case DMInputEventIds.HdcpSupportOnEventId:
                        {
                            Debug.Console(2, this, "DM Input {0} HdcpSupportOnEventId", args.Number);
                            if (InputCardHdcpStateFeedbacks[args.Number] != null)
                                InputCardHdcpStateFeedbacks[args.Number].FireUpdate();
                            else
                                Debug.Console(1, this, "No index of {0} found in InputCardHdcpCapabilityFeedbacks");
                            break;
                        }
                    case DMInputEventIds.StartEventId:
                    case DMInputEventIds.StopEventId:
                    case DMInputEventIds.PauseEventId:
                        {
                            Debug.Console(2, this, "DM Input {0} Stream Status EventId", args.Number);
                            if (InputStreamCardStateFeedbacks[args.Number] != null)
                            {
                                InputStreamCardStateFeedbacks[args.Number].FireUpdate();
                            }
                            else
                                Debug.Console(2, this, "No index of {0} found in InputStreamCardStateFeedbacks");
                            break;
                        }
                    case DMInputEventIds.HorizontalResolutionFeedbackEventId:
                    case DMInputEventIds.VerticalResolutionFeedbackEventId:
                    case DMInputEventIds.FramesPerSecondFeedbackEventId:
                    case DMInputEventIds.ResolutionEventId:
                    {
                        Debug.Console(1, this, "Input {0} resolution updated", args.Number);
                        var inputPort =
                            InputPorts.Cast<RoutingInputPortWithVideoStatuses>()
                                .FirstOrDefault((ip) => ip.Key.Contains(String.Format("inputCard{0}", args.Number)));

                        if (inputPort != null)
                        {
                            Debug.Console(1, this, "Updating resolution feedback for input {0}", args.Number);
                            inputPort.VideoStatus.VideoResolutionFeedback.FireUpdate();
                        }
                        break;
                    }
                    default:
                        {
                            Debug.Console(2, this, "DMInputChange fired for Input {0} with Unhandled EventId: {1}", args.Number, args.EventId);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Debug.Console(2, this, Debug.ErrorLogLevel.Error, "Error in Chassis_DMInputChange: {0}", ex);
            }
        }

        /// <summary>
        /// <summary>
        /// Raise an event when the status of a switch object changes.
        /// </summary>
        /// <param name="e">Arguments defined as IKeyName sender, output, input, and eRoutingSignalType</param>
        private void OnSwitchChange(RoutingNumericEventArgs e)
        {
            var newEvent = NumericSwitchChange;
            if (newEvent != null) newEvent(this, e);
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
                    
                    var inputNumber = Chassis.Outputs[output].VideoOutFeedback == null ? 0 : Chassis.
                    Outputs[output].VideoOutFeedback.Number;

                    Debug.Console(2, this, "DMSwitchVideo:{0} Routed Input:{1} Output:{2}'", Name, inputNumber, output);

                    if (VideoOutputFeedbacks.ContainsKey(output))
                    {
                        var localInputPort = InputPorts.FirstOrDefault(p => (DMInput)p.FeedbackMatchObject == Chassis.Outputs[output].VideoOutFeedback);
                        var localOutputPort =
                            OutputPorts.FirstOrDefault(p => (DMOutput) p.FeedbackMatchObject == Chassis.Outputs[output]);


                        VideoOutputFeedbacks[output].FireUpdate();
                        OnSwitchChange(new RoutingNumericEventArgs(output,
                            inputNumber,
                            localOutputPort,
                            localInputPort,
                            eRoutingSignalType.Video));
                    }

                    if (OutputVideoRouteNameFeedbacks.ContainsKey(output))
                        OutputVideoRouteNameFeedbacks[output].FireUpdate();

                    break;
                }
                case DMOutputEventIds.AudioOutEventId:
                {
                    var inputNumber = Chassis.Outputs[output].AudioOutFeedback == null ? 0 : Chassis.
                    Outputs[output].AudioOutFeedback.Number;

                    Debug.Console(2, this, "DMSwitchAudio:{0} Routed Input:{1} Output:{2}'", Name, inputNumber, output);

                    if (AudioOutputFeedbacks.ContainsKey(output))
                    {
                        var localInputPort = InputPorts.FirstOrDefault(p => (DMInput)p.FeedbackMatchObject == Chassis.Outputs[output].AudioOutFeedback);
                        var localOutputPort =
                            OutputPorts.FirstOrDefault(p => (DMOutput)p.FeedbackMatchObject == Chassis.Outputs[output]);


                        AudioOutputFeedbacks[output].FireUpdate();
                        OnSwitchChange(new RoutingNumericEventArgs(output,
                            inputNumber,
                            localOutputPort,
                            localInputPort,
                            eRoutingSignalType.Audio));
                    }

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
                case DMOutputEventIds.StartEventId:
                case DMOutputEventIds.StopEventId:
                case DMOutputEventIds.PauseEventId:
                {
                    Debug.Console(2, this, "DM Output {0} Stream Status EventId", args.Number);
                    if (OutputStreamCardStateFeedbacks[args.Number] != null)
                    {
                        OutputStreamCardStateFeedbacks[args.Number].FireUpdate();
                    }
                    else
                        Debug.Console(2, this, "No index of {0} found in OutputStreamCardStateFeedbacks");
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
            RouteOffTimers[pnt] = new CTimer(o => ExecuteSwitch(null, pnt.Selector, pnt.Type), RouteOffTime);
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

            var input = inputSelector as DMInput;//Input Selector could be null...

            var output = outputSelector as DMOutput;

            var isUsbInput = (sigType & eRoutingSignalType.UsbInput) == eRoutingSignalType.UsbInput;
            var isUsbOutput = (sigType & eRoutingSignalType.UsbOutput) == eRoutingSignalType.UsbOutput;

            if (output == null && !(isUsbOutput || isUsbInput))
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Warning,
                    "Unable to execute switch for inputSelector {0} to outputSelector {1}", inputSelector,
                    outputSelector);
                return;
            }

            // Check to see if there's an off timer waiting on this and if so, cancel
            var key = new PortNumberType(output, sigType);
            if (input == null)
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

            //var inCard = input == 0 ? null : Chassis.Inputs[input];
            //var outCard = input == 0 ? null : Chassis.Outputs[output];

            // NOTE THAT BITWISE COMPARISONS - TO CATCH ALL ROUTING TYPES 
            if ((sigType & eRoutingSignalType.Video) == eRoutingSignalType.Video)
            {
                Chassis.VideoEnter.BoolValue = true;
                if (output != null)
                {
                    output.VideoOut = input; //Chassis.Outputs[output].VideoOut = inCard;
                }
            }

            if ((sigType & eRoutingSignalType.Audio) == eRoutingSignalType.Audio)
            {
                var dmMdMnxn = Chassis as DmMDMnxn;
                if (dmMdMnxn != null)
                {
                    dmMdMnxn.AudioEnter.BoolValue = true;
                }
                if (output != null)
                {
                    output.AudioOut = input;
                }
            }

            if ((sigType & eRoutingSignalType.UsbOutput) == eRoutingSignalType.UsbOutput)
                
            {
               Chassis.USBEnter.BoolValue = true;
                if (inputSelector == null && output != null)
                {
                    //clearing the route is intended
                    output.USBRoutedTo = null;
                    return;
                }

                if (inputSelector != null && input == null)
                {
                    //input selector is DMOutput...we're doing a out to out route
                    var tempInput = inputSelector as DMOutput;

                    if (tempInput == null || output == null)
                    {
                        return;
                    }
                    output.USBRoutedTo = tempInput;
                    return;
                }

                if (input != null & output != null)
                {
                    output.USBRoutedTo = input;
                }
            }

            if((sigType & eRoutingSignalType.UsbInput) != eRoutingSignalType.UsbInput)
            {
                return;
            }

            Chassis.USBEnter.BoolValue = true;
            if (output != null)
            {
                output.USBRoutedTo = input;
                return;
            }
            var tempOutput = outputSelector as DMInput;

            if (tempOutput == null)
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Warning,
                    "Unable to execute switch for inputSelector {0} to outputSelector {1}", inputSelector,
                    outputSelector);
                return;
            }

            tempOutput.USBRoutedTo = input;
        }

        #endregion

        #region IRoutingNumeric Members

        public void ExecuteNumericSwitch(ushort inputSelector, ushort outputSelector, eRoutingSignalType sigType)
        {
            var chassisSize = (uint)Chassis.NumberOfInputs; //need this to determine USB routing values 8x8 -> 1-8 is inputs 1-8, 17-24 is outputs 1-8
            //16x16 1-16 is inputs 1-16, 17-32 is outputs 1-16
            //32x32 1-32 is inputs 1-32, 33-64 is outputs 1-32

            DMInputOutputBase dmCard;

            //Routing Input to Input or Output to Input
            if ((sigType & eRoutingSignalType.UsbInput) == eRoutingSignalType.UsbInput)
            {
                Debug.Console(2, this, "Executing USB Input switch.\r\n in:{0} output: {1}", inputSelector, outputSelector);
                if (outputSelector > chassisSize)
                {
                    uint outputIndex;

                    if (chassisSize == 8)
                    {
                        outputIndex = (uint) inputSelector - 16;
                    }
                    else
                    {
                        outputIndex = inputSelector - chassisSize;
                    }
                    dmCard = Chassis.Outputs[outputIndex];
                }
                else
                {
                    dmCard = Chassis.Inputs[inputSelector];
                }

                ExecuteSwitch(dmCard, Chassis.Inputs[outputSelector], sigType);
                return;
            }
            if ((sigType & eRoutingSignalType.UsbOutput) == eRoutingSignalType.UsbOutput)
            {
                Debug.Console(2, this, "Executing USB Output switch.\r\n in:{0} output: {1}", inputSelector, outputSelector);

                //routing Output to Output or Input to Output
                if (inputSelector > chassisSize)
                {
                    //wanting to route an output to an output. Subtract chassis size and get output, unless it's 8x8
                    //need this to determine USB routing values
                    //8x8 -> 1-8 is inputs 1-8, 17-24 is outputs 1-8
                    //16x16 1-16 is inputs 1-16, 17-32 is outputs 1-16
                    //32x32 1-32 is inputs 1-32, 33-64 is outputs 1-32
                    uint outputIndex;

                    if (chassisSize == 8)
                    {
                        outputIndex = (uint) inputSelector - 16;
                    }
                    else
                    {
                        outputIndex = inputSelector - chassisSize;
                    }

                    dmCard = Chassis.Outputs[outputIndex];
                }
                else
                {
                    dmCard = Chassis.Inputs[inputSelector];
                }
                Chassis.USBEnter.BoolValue = true;

                Debug.Console(2, this, "Routing USB for input {0} to {1}", inputSelector, dmCard);
                ExecuteSwitch(dmCard, Chassis.Outputs[outputSelector], sigType);
                return;
            }

            var inputCard = inputSelector == 0 ? null : Chassis.Inputs[inputSelector];
            var outputCard = Chassis.Outputs[outputSelector];

            ExecuteSwitch(inputCard, outputCard, sigType);
        }

        #endregion

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = GetJoinMap(joinStart, joinMapKey, bridge);

            Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            LinkChassisToApi(trilist, joinMap);

            trilist.StringInput[joinMap.Name.JoinNumber].StringValue = this.Name;

            // Link up inputs & outputs
            for (uint i = 1; i <= Chassis.NumberOfOutputs; i++)
            {
                var ioSlot = i;
                var ioSlotJoin = ioSlot - 1;

                LinkRoutingJoinsToApi(trilist, joinMap, ioSlotJoin, ioSlot);

                if (TxDictionary.ContainsKey(ioSlot))
                {
                    LinkTxToApi(trilist, ioSlot, joinMap, ioSlotJoin);
                }
                else
                {
                    LinkHdmiInputToApi(trilist, ioSlot, joinMap, ioSlotJoin);
                    LinkStreamInputToApi(trilist, ioSlot, joinMap, ioSlotJoin);
                }

                if (RxDictionary.ContainsKey(ioSlot))
                {
                    LinkRxToApi(trilist, ioSlot, joinMap, ioSlotJoin);
                }
                else
                    LinkStreamOutputToApi(trilist, ioSlot, joinMap, ioSlotJoin);
            }
        }

        private void LinkHdmiInputToApi(BasicTriList trilist, uint ioSlot, DmChassisControllerJoinMap joinMap, uint ioSlotJoin)
        {
            VideoInputSyncFeedbacks[ioSlot].LinkInputSig(trilist.BooleanInput[joinMap.VideoSyncStatus.JoinNumber + ioSlotJoin]);

            var inputPort = InputPorts[string.Format("inputCard{0}--hdmiIn", ioSlot)];
            if (inputPort == null)
            {
                return;
            }

            Debug.Console(1, "Port value for input card {0} is set", ioSlot);
            var port = inputPort.Port;

            if (port == null)
            {
                return;
            }
            if (!(port is HdmiInputWithCEC))
            {
                Debug.Console(0, this, "HDMI Input port on card {0} does not support HDCP settings.", ioSlot);
                return;
            }

            Debug.Console(1, "Port is HdmiInputWithCec");

            var hdmiInPortWCec = port as HdmiInputWithCEC;
            
            
            SetHdcpStateAction(PropertiesConfig.InputSlotSupportsHdcp2[ioSlot], hdmiInPortWCec, joinMap.HdcpSupportState.JoinNumber + ioSlotJoin, trilist);
            

            InputCardHdcpStateFeedbacks[ioSlot].LinkInputSig(
                trilist.UShortInput[joinMap.HdcpSupportState.JoinNumber + ioSlotJoin]);

            if (InputCardHdcpCapabilityTypes.ContainsKey(ioSlot))
            {
                trilist.UShortInput[joinMap.HdcpSupportCapability.JoinNumber + ioSlotJoin].UShortValue =
                    (ushort)InputCardHdcpCapabilityTypes[ioSlot];
            }
            else
            {
                trilist.UShortInput[joinMap.HdcpSupportCapability.JoinNumber + ioSlotJoin].UShortValue = 1;
            }

            var videoStatus = inputPort as RoutingInputPortWithVideoStatuses;

            if (videoStatus == null)
            {
                return;
            }

            Debug.Console(1, this, "Linking {0} to join {1} for resolution feedback.", videoStatus.Key, joinMap.InputCurrentResolution.JoinNumber + ioSlotJoin);
            videoStatus.VideoStatus.VideoResolutionFeedback.LinkInputSig(trilist.StringInput[joinMap.InputCurrentResolution.JoinNumber + ioSlotJoin]);
        }

        private void LinkStreamInputToApi(BasicTriList trilist, uint ioSlot, DmChassisControllerJoinMap joinMap, uint ioSlotJoin)
        {
            var inputPort = InputPorts[string.Format("inputCard{0}--streamIn", ioSlot)];
            if (inputPort == null)
            {
                return;
            }
            var streamCard = Chassis.Inputs[ioSlot].Card as DmcStr;
            var join = joinMap.InputStreamCardState.JoinNumber + ioSlotJoin;

            Debug.Console(1, "Port value for input card {0} is set as a stream card", ioSlot);

            trilist.SetUShortSigAction(join, s =>
            {
                if (s == 1)
                {
                    Debug.Console(2, this, "Join {0} value {1}: Setting stream state to start", join, s);
                    streamCard.Control.Start();
                }
                else if (s == 2)
                {
                    Debug.Console(2, this, "Join {0} value {1}: Setting stream state to stop", join, s);
                    streamCard.Control.Stop();
                }
                else if (s == 3)
                {
                    Debug.Console(2, this, "Join {0} value {1}: Setting stream state to pause", join, s);
                    streamCard.Control.Pause();
                }
                else
                {
                    Debug.Console(2, this, "Join {0} value {1}: Ignore stream state", join, s);
                }
            });

            InputStreamCardStateFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[join]);

            trilist.UShortInput[join].UShortValue = InputStreamCardStateFeedbacks[ioSlot].UShortValue;

            var videoStatus = inputPort as RoutingInputPortWithVideoStatuses;

            if (videoStatus != null)
            {
                videoStatus.VideoStatus.VideoResolutionFeedback.LinkInputSig(trilist.StringInput[joinMap.InputCurrentResolution.JoinNumber + ioSlotJoin]);
            }
        }

        private void LinkStreamOutputToApi(BasicTriList trilist, uint ioSlot, DmChassisControllerJoinMap joinMap, uint ioSlotJoin)
        {
            var outputPort = OutputPorts[string.Format("outputCard{0}--streamOut", ioSlot)];
            if (outputPort == null)
            {
                return;
            }
            var streamCard = Chassis.Outputs[ioSlot].Card as DmcStroAV;
            var join = joinMap.OutputStreamCardState.JoinNumber + ioSlotJoin;

            Debug.Console(1, "Port value for output card {0} is set as a stream card", ioSlot);

            trilist.SetUShortSigAction(join, s =>
            {
                if (s == 1)
                {
                    Debug.Console(2, this, "Join {0} value {1}: Setting stream state to start", join, s);
                    streamCard.Control.Start();
                }
                else if (s == 2)
                {
                    Debug.Console(2, this, "Join {0} value {1}: Setting stream state to stop", join, s);
                    streamCard.Control.Stop();
                }
                else if (s == 3)
                {
                    Debug.Console(2, this, "Join {0} value {1}: Setting stream state to pause", join, s);
                    streamCard.Control.Pause();
                }
                else
                {
                    Debug.Console(2, this, "Join {0} value {1}: Ignore stream state", join, s);
                }
            });

            OutputStreamCardStateFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[join]);

            trilist.UShortInput[join].UShortValue = OutputStreamCardStateFeedbacks[ioSlot].UShortValue;
        }

        private void LinkRxToApi(BasicTriList trilist, uint ioSlot, DmChassisControllerJoinMap joinMap, uint ioSlotJoin)
        {
            Debug.Console(2, "Creating Rx Feedbacks {0}", ioSlot);
            var rxKey = RxDictionary[ioSlot];
            var rxDevice = DeviceManager.GetDeviceForKey(rxKey) as DmRmcControllerBase;
            var hdBaseTDevice = DeviceManager.GetDeviceForKey(rxKey) as DmHdBaseTControllerBase;
            if (Chassis is DmMd8x8Cpu3 || Chassis is DmMd8x8Cpu3rps
                || Chassis is DmMd16x16Cpu3 || Chassis is DmMd16x16Cpu3rps
                || Chassis is DmMd32x32Cpu3 || Chassis is DmMd32x32Cpu3rps || hdBaseTDevice != null)
            {
                OutputEndpointOnlineFeedbacks[ioSlot].LinkInputSig(
                    trilist.BooleanInput[joinMap.OutputEndpointOnline.JoinNumber + ioSlotJoin]);
            }
            else if (rxDevice != null)
            {
                rxDevice.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.OutputEndpointOnline.JoinNumber + ioSlotJoin]);
            }
        }

        private void LinkTxToApi(BasicTriList trilist, uint ioSlot, DmChassisControllerJoinMap joinMap, uint ioSlotJoin)
        {
            Debug.Console(1, "Setting up actions and feedbacks on input card {0}", ioSlot);
            VideoInputSyncFeedbacks[ioSlot].LinkInputSig(
                trilist.BooleanInput[joinMap.VideoSyncStatus.JoinNumber + ioSlotJoin]);

            Debug.Console(2, "Creating Tx Feedbacks {0}", ioSlot);
            var txKey = TxDictionary[ioSlot];
            var txDevice = DeviceManager.GetDeviceForKey(txKey) as BasicDmTxControllerBase;

            if (txDevice == null)
            {
                return;
            }

            LinkTxOnlineFeedbackToApi(trilist, ioSlot, joinMap, ioSlotJoin, txDevice);

            LinkBasicTxToApi(trilist, joinMap, ioSlot, ioSlotJoin, txDevice);

            LinkAdvancedTxToApi(trilist, joinMap, ioSlot, ioSlotJoin, txDevice);
        }

        private void LinkBasicTxToApi(BasicTriList trilist, DmChassisControllerJoinMap joinMap, uint ioSlot,
            uint ioSlotJoin, BasicDmTxControllerBase basicTransmitter)
        {
            var advTx = basicTransmitter as DmTxControllerBase;

            if (advTx != null)
            {
                return;
            }
            var inputPort = InputPorts[string.Format("inputCard{0}--dmIn", ioSlot)];

            if (inputPort == null)
            {
                return;
            }
            var port = inputPort.Port;

            if (!(port is DMInputPortWithCec))
            {
                Debug.Console(0, this, "DM Input port on card {0} does not support HDCP settings.", ioSlot);
                return;
            }
            Debug.Console(1, "Port is DMInputPortWithCec");

            var dmInPortWCec = port as DMInputPortWithCec;

            bool supportsHdcp2;

            //added in case the InputSlotSupportsHdcp2 section isn't included in the config, or this slot is left out.
            //if the key isn't in the dictionary, supportsHdcp2 will be false
            
            if(!PropertiesConfig.InputSlotSupportsHdcp2.TryGetValue(ioSlot, out supportsHdcp2))
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Warning,
                    "Input Slot Supports HDCP2 setting not found for slot {0}. Setting to false. Program may not function as intended.",
                    ioSlot);
            }

            SetHdcpStateAction(supportsHdcp2, dmInPortWCec,
                joinMap.HdcpSupportState.JoinNumber + ioSlotJoin, trilist);

            InputCardHdcpStateFeedbacks[ioSlot].LinkInputSig(
                trilist.UShortInput[joinMap.HdcpSupportState.JoinNumber + ioSlotJoin]);

            if (InputCardHdcpCapabilityTypes.ContainsKey(ioSlot))
            {
                trilist.UShortInput[joinMap.HdcpSupportCapability.JoinNumber + ioSlotJoin].UShortValue =
                    (ushort) InputCardHdcpCapabilityTypes[ioSlot];
            }
            else
            {
                trilist.UShortInput[joinMap.HdcpSupportCapability.JoinNumber + ioSlotJoin].UShortValue = 1;
            }

            var videoStatus = inputPort as RoutingInputPortWithVideoStatuses;

            if (videoStatus == null)
            {
                return;
            }
            Debug.Console(1, this, "Linking {0} to join {1} for resolution feedback.", videoStatus.Key, joinMap.InputCurrentResolution.JoinNumber + ioSlotJoin);
            videoStatus.VideoStatus.VideoResolutionFeedback.LinkInputSig(trilist.StringInput[joinMap.InputCurrentResolution.JoinNumber + ioSlotJoin]);
        }

        private void LinkAdvancedTxToApi(BasicTriList trilist, DmChassisControllerJoinMap joinMap,
            uint ioSlot, uint ioSlotJoin, BasicDmTxControllerBase basicTransmitter)
        {
            var transmitter = basicTransmitter as DmTxControllerBase;
            if (transmitter == null) return;

            trilist.BooleanInput[joinMap.TxAdvancedIsPresent.JoinNumber + ioSlotJoin].BoolValue = true;

            transmitter.AnyVideoInput.VideoStatus.VideoSyncFeedback.LinkInputSig(
                trilist.BooleanInput[joinMap.VideoSyncStatus.JoinNumber + ioSlotJoin]);

            var txRoutingInputs = transmitter as IRoutingInputs;

            if (txRoutingInputs == null) return;

            var inputPorts =
                txRoutingInputs.InputPorts.Where(
                    (p) => p.Port is EndpointHdmiInput || p.Port is EndpointDisplayPortInput).ToList();

            if (inputPorts.Count == 0)
            {
                Debug.Console(1, this, "No HDCP-capable input ports found on transmitter for slot {0}", ioSlot);
                return;
            }

            bool supportsHdcp2;

            if (!PropertiesConfig.InputSlotSupportsHdcp2.TryGetValue(ioSlot, out supportsHdcp2))
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Warning,
                    "Input Slot Supports HDCP2 setting not found for slot {0}. Setting to false. Program may not function as intended.",
                    ioSlot);
            }

            SetHdcpStateAction(supportsHdcp2, inputPorts, joinMap.HdcpSupportState.JoinNumber + ioSlotJoin, trilist);

            if (transmitter.HdcpStateFeedback != null)
            {
                transmitter.HdcpStateFeedback.LinkInputSig(
                    trilist.UShortInput[joinMap.HdcpSupportState.JoinNumber + ioSlotJoin]);
            }
            else
            {
                Debug.Console(2, this, "Transmitter Hdcp Feedback null. Linking to card's feedback");
                InputCardHdcpStateFeedbacks[ioSlot].LinkInputSig(
                    trilist.UShortInput[joinMap.HdcpSupportState.JoinNumber + ioSlotJoin]);
            }

            trilist.UShortInput[joinMap.HdcpSupportCapability.JoinNumber + ioSlotJoin].UShortValue =
                (ushort) transmitter.HdcpSupportCapability;


            var videoStatus =
                InputPorts[string.Format("inputCard{0}--dmIn", ioSlot)] as RoutingInputPortWithVideoStatuses;

            if (videoStatus == null)
            {
                return;
            }
            Debug.Console(1, this, "Linking {0} to join {1} for resolution feedback.", videoStatus.Key,
                joinMap.InputCurrentResolution.JoinNumber + ioSlotJoin);
            videoStatus.VideoStatus.VideoResolutionFeedback.LinkInputSig(
                trilist.StringInput[joinMap.InputCurrentResolution.JoinNumber + ioSlotJoin]);
        }

        private void LinkTxOnlineFeedbackToApi(BasicTriList trilist, uint ioSlot, DmChassisControllerJoinMap joinMap,
            uint ioSlotJoin, BasicDmTxControllerBase txDevice)
        {
            var advancedTxDevice = txDevice as DmTxControllerBase;

            if ((Chassis is DmMd8x8Cpu3 || Chassis is DmMd8x8Cpu3rps
                 || Chassis is DmMd16x16Cpu3 || Chassis is DmMd16x16Cpu3rps
                 || Chassis is DmMd32x32Cpu3 || Chassis is DmMd32x32Cpu3rps) ||
                advancedTxDevice == null)
            {
                Debug.Console(2, "Linking Tx Online Feedback from Input Card {0}", ioSlot);
                InputEndpointOnlineFeedbacks[ioSlot].LinkInputSig(
                    trilist.BooleanInput[joinMap.InputEndpointOnline.JoinNumber + ioSlotJoin]);
                return;
            }

            Debug.Console(2, "Linking Tx Online Feedback from Advanced Transmitter at input {0}", ioSlot);

            advancedTxDevice.IsOnline.LinkInputSig(
                trilist.BooleanInput[joinMap.InputEndpointOnline.JoinNumber + ioSlotJoin]);
        }

        private void LinkRoutingJoinsToApi(BasicTriList trilist, DmChassisControllerJoinMap joinMap, uint ioSlotJoin,
            uint ioSlot)
        {
            // Routing Control
            trilist.SetUShortSigAction(joinMap.OutputVideo.JoinNumber + ioSlotJoin,
				o => ExecuteNumericSwitch(o, (ushort) ioSlot, eRoutingSignalType.Video));
            trilist.SetUShortSigAction(joinMap.OutputAudio.JoinNumber + ioSlotJoin,
                o => ExecuteNumericSwitch(o, (ushort) ioSlot, eRoutingSignalType.Audio));
            trilist.SetUShortSigAction(joinMap.OutputUsb.JoinNumber + ioSlotJoin,
                o => ExecuteNumericSwitch(o, (ushort) ioSlot, eRoutingSignalType.UsbOutput));
            trilist.SetUShortSigAction(joinMap.InputUsb.JoinNumber + ioSlotJoin,
                o => ExecuteNumericSwitch(o, (ushort) ioSlot, eRoutingSignalType.UsbInput));

            //Routing Feedbacks
            VideoOutputFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.OutputVideo.JoinNumber + ioSlotJoin]);
            AudioOutputFeedbacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.OutputAudio.JoinNumber + ioSlotJoin]);
            UsbOutputRoutedToFeebacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.OutputUsb.JoinNumber + ioSlotJoin]);
            UsbInputRoutedToFeebacks[ioSlot].LinkInputSig(trilist.UShortInput[joinMap.InputUsb.JoinNumber + ioSlotJoin]);

            OutputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.OutputNames.JoinNumber + ioSlotJoin]);
            InputNameFeedbacks[ioSlot].LinkInputSig(trilist.StringInput[joinMap.InputNames.JoinNumber + ioSlotJoin]);
            OutputVideoRouteNameFeedbacks[ioSlot].LinkInputSig(
                trilist.StringInput[joinMap.OutputCurrentVideoInputNames.JoinNumber + ioSlotJoin]);
            OutputAudioRouteNameFeedbacks[ioSlot].LinkInputSig(
                trilist.StringInput[joinMap.OutputCurrentAudioInputNames.JoinNumber + ioSlotJoin]);

            OutputDisabledByHdcpFeedbacks[ioSlot].LinkInputSig(
                trilist.BooleanInput[joinMap.OutputDisabledByHdcp.JoinNumber + ioSlotJoin]);
        }

        private void LinkChassisToApi(BasicTriList trilist, DmChassisControllerJoinMap joinMap)
        {
            var chassis = Chassis as DmMDMnxn;

            IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);

            trilist.SetUShortSigAction(joinMap.SystemId.JoinNumber, o =>
            {
                if (chassis != null)
                {
                    chassis.SystemId.UShortValue = o;
                }
            });

            trilist.SetSigTrueAction(joinMap.SystemId.JoinNumber, () =>
            {
                if (chassis != null)
                {
                    chassis.ApplySystemId();
                }
            });

            SystemIdFeebdack.LinkInputSig(trilist.UShortInput[joinMap.SystemId.JoinNumber]);
            SystemIdBusyFeedback.LinkInputSig(trilist.BooleanInput[joinMap.SystemId.JoinNumber]);

            EnableAudioBreakawayFeedback.LinkInputSig(trilist.BooleanInput[joinMap.EnableAudioBreakaway.JoinNumber]);
            EnableUsbBreakawayFeedback.LinkInputSig(trilist.BooleanInput[joinMap.EnableUsbBreakaway.JoinNumber]);

            trilist.SetString(joinMap.NoRouteName.JoinNumber, NoRouteText);

            trilist.OnlineStatusChange += (o, a) =>
            {
                if (!a.DeviceOnLine)
                {
                    return;
                }

                EnableAudioBreakawayFeedback.FireUpdate();
                EnableUsbBreakawayFeedback.FireUpdate();
                SystemIdBusyFeedback.FireUpdate();
                SystemIdFeebdack.FireUpdate();

                trilist.SetString(joinMap.NoRouteName.JoinNumber, NoRouteText);
            };
        }

        private DmChassisControllerJoinMap GetJoinMap(uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new DmChassisControllerJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
            {
                joinMap = JsonConvert.DeserializeObject<DmChassisControllerJoinMap>(joinMapSerialized);
            }

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                Debug.Console(0, this,
                    "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }
            return joinMap;
        }

        private void SetHdcpStateAction(bool supportsHdcp2, HdmiInputWithCEC port, uint join, BasicTriList trilist)
        {
            if (!supportsHdcp2)
            {
                trilist.SetUShortSigAction(join,
                    s =>
                    {
                        if (s == 0)
                        {
                            Debug.Console(2, this, "Join {0} value {1} Setting HdcpSupport to off", join, s); 
                            port.HdcpSupportOff();
                        }
                        else if (s > 0)
                        {
                            Debug.Console(2, this, "Join {0} value {1} Setting HdcpSupport to on", join, s); 
                            port.HdcpSupportOn();
                        }
                    });
            }
            else
            {
                trilist.SetUShortSigAction(join,
                        u =>
                        {
                            Debug.Console(2, this, "Join {0} value {1} Setting HdcpReceiveCapability to: {2}", join, u, (eHdcpCapabilityType)u); 
                            port.HdcpReceiveCapability = (eHdcpCapabilityType)u;
                        });
            }
        }

        private void SetHdcpStateAction(bool supportsHdcp2, EndpointHdmiInput port, uint join, BasicTriList trilist)
        {
            if (!supportsHdcp2)
            {
                trilist.SetUShortSigAction(join,
                    s =>
                    {
                        if (s == 0)
                        {
                            Debug.Console(2, this, "Join {0} value {1} Setting HdcpSupport to off", join, s);
                            port.HdcpSupportOff();
                        }
                        else if (s > 0)
                        {
                            Debug.Console(2, this, "Join {0} value {1} Setting HdcpSupport to on", join, s);
                            port.HdcpSupportOn();
                        }
                    });
            }
            else
            {
                trilist.SetUShortSigAction(join,
                        u =>
                        {
                            Debug.Console(2, this, "Join {0} value {1} Setting HdcpReceiveCapability to: {2}", join, u, (eHdcpCapabilityType)u);
                            port.HdcpCapability = (eHdcpCapabilityType)u;
                        });
            }
        }

        private void SetHdcpStateAction(bool supportsHdcp2, List<RoutingInputPort> ports, uint join,
            BasicTriList triList)
        {
            if (!supportsHdcp2)
            {
                triList.SetUShortSigAction(join, a =>
                {
                    foreach (var tempPort in ports.Select(port => port.Port).OfType<EndpointHdmiInput>())
                    {
                        if (a == 0)
                        {
                            tempPort.HdcpSupportOff();
                        }
                        else if (a > 0)
                        {
                            tempPort.HdcpSupportOn();
                        }
                    }
                });
            }
            else
            {
                triList.SetUShortSigAction(join, a =>
                {
                    foreach (var tempPort in ports.Select(port => port.Port).OfType<EndpointHdmiInput>())
                    {
                        tempPort.HdcpCapability = (eHdcpCapabilityType) a;
                    }
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
        public object Selector { get; private set; }
        public eRoutingSignalType Type { get; private set; }

        public PortNumberType(object selector, eRoutingSignalType type)
            : this()
        {
            Selector = selector;
            Type = type;

            if (Selector is DMOutput)
            {
                Number = (selector as DMOutput).Number;
            }
            else if (Selector is uint)
            {
                Number = (uint) selector;
            }
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
                    <DMChassisPropertiesConfig>(dc.Properties.ToString());
                return DmChassisController.
                    GetDmChassisController(dc.Key, dc.Name, type, props);
            }
            else if (type.StartsWith("dmmd128x") || type.StartsWith("dmmd64x"))
            {
                var props = JsonConvert.DeserializeObject
                    <DMChassisPropertiesConfig>(dc.Properties.ToString());
                return DmBladeChassisController.
                    GetDmChassisController(dc.Key, dc.Name, type, props);
            }

            return null;
        }
    }

}