using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

using PepperDash.Core;
using PepperDash.Essentials.Core;
//using PepperDash.Essentials.DM.Cards;

using PepperDash.Essentials.DM.Config;

namespace PepperDash.Essentials.DM
{
	/// <summary>
	/// Builds a controller for basic DM-RMCs with Com and IR ports and no control functions
	/// 
	/// </summary>
	public class DmChassisController : CrestronGenericBaseDevice, IDmSwitch, IRoutingInputsOutputs, IRouting, IHasFeedback
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
				if (type == "dmmd8x8") { chassis = new DmMd8x8(ipid, Global.ControlSystem); }
				else if (type == "dmmd8x8rps") { chassis = new DmMd8x8rps(ipid, Global.ControlSystem); }
				else if (type == "dmmd8x8cpu3") { chassis = new DmMd8x8Cpu3(ipid, Global.ControlSystem); }
				else if (type == "dmmd8x8cpu3rps") { chassis = new DmMd8x8Cpu3rps(ipid, Global.ControlSystem); }

				else if (type == "dmmd16x16") { chassis = new DmMd16x16(ipid, Global.ControlSystem); }
				else if (type == "dmmd16x16rps") { chassis = new DmMd16x16rps(ipid, Global.ControlSystem); }
				else if (type == "dmmd16x16cpu3") { chassis = new DmMd16x16Cpu3(ipid, Global.ControlSystem); }
				else if (type == "dmmd16x16cpu3rps") { chassis = new DmMd16x16Cpu3rps(ipid, Global.ControlSystem); }

				else if (type == "dmmd32x32") { chassis = new DmMd32x32(ipid, Global.ControlSystem); }
				else if (type == "dmmd32x32rps") { chassis = new DmMd32x32rps(ipid, Global.ControlSystem); }
				else if (type == "dmmd32x32cpu3") { chassis = new DmMd32x32Cpu3(ipid, Global.ControlSystem); }
				else if (type == "dmmd32x32cpu3rps") { chassis = new DmMd32x32Cpu3rps(ipid, Global.ControlSystem); }

                if (chassis == null)
                {
                    return null;
                }

				var controller = new DmChassisController(key, name, chassis);

				// add the cards and port names
                foreach (var kvp in properties.InputSlots)
                {
                    controller.AddInputCard(kvp.Value, kvp.Key);
                }
				foreach (var kvp in properties.OutputSlots)
				{
					controller.AddOutputCard(kvp.Value, kvp.Key);
				}

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
                    Debug.Console(1, controller, "NoRouteText not specified.  Defaulting to blank string.", controller.NoRouteText);

                controller.PropertiesConfig = properties;
				return controller;
			}
			catch (System.Exception e)
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
			VideoInputSyncFeedbacks = new Dictionary<uint, BoolFeedback>();
			InputNameFeedbacks = new Dictionary<uint, StringFeedback>();
			OutputNameFeedbacks = new Dictionary<uint, StringFeedback>();
			OutputVideoRouteNameFeedbacks = new Dictionary<uint, StringFeedback>();
            OutputAudioRouteNameFeedbacks = new Dictionary<uint, StringFeedback>();
            InputEndpointOnlineFeedbacks = new Dictionary<uint, BoolFeedback>();
            OutputEndpointOnlineFeedbacks = new Dictionary<uint, BoolFeedback>();

            SystemIdFeebdack = new IntFeedback(() => { return (Chassis as DmMDMnxn).SystemIdFeedback.UShortValue; });
            SystemIdBusyFeedback = new BoolFeedback(() => { return (Chassis as DmMDMnxn).SystemIdBusy.BoolValue; });
            InputCardHdcpCapabilityFeedbacks = new Dictionary<uint, IntFeedback>();
            InputCardHdcpCapabilityTypes = new Dictionary<uint, eHdcpCapabilityType>();
		}

        public override bool CustomActivate()
        {
            Debug.Console(2, this, "Setting up feedbacks.");

            // Setup Output Card Feedbacks
            for (uint x = 1; x <= Chassis.NumberOfOutputs; x++)
            {
                var tempX = x;

                Debug.Console(2, this, "Setting up feedbacks for output slot: {0}", tempX);

                if (Chassis.Outputs[tempX] != null)
                {
                    VideoOutputFeedbacks[tempX] = new IntFeedback(() =>
                    {
                        if (Chassis.Outputs[tempX].VideoOutFeedback != null) { return (ushort)Chassis.Outputs[tempX].VideoOutFeedback.Number; }
                        else { return 0; };
                    });
                    AudioOutputFeedbacks[tempX] = new IntFeedback(() =>
                    {
                        if (Chassis.Outputs[tempX].AudioOutFeedback != null) { return (ushort)Chassis.Outputs[tempX].AudioOutFeedback.Number; }
                        else { return 0; };
                    });
                    UsbOutputRoutedToFeebacks[tempX] = new IntFeedback(() =>
                    {
                        if (Chassis.Outputs[tempX].USBRoutedToFeedback != null) { return (ushort)Chassis.Outputs[tempX].USBRoutedToFeedback.Number; }
                        else { return 0; };
                    });

                    OutputNameFeedbacks[tempX] = new StringFeedback(() =>
                    {
                        if (Chassis.Outputs[tempX].NameFeedback != null)
                        {
                            return Chassis.Outputs[tempX].NameFeedback.StringValue;
                        }
                        else
                        {
                            return "";
                        }
                    });
                    OutputVideoRouteNameFeedbacks[tempX] = new StringFeedback(() =>
                    {
                        if (Chassis.Outputs[tempX].VideoOutFeedback != null)
                        {
                            return Chassis.Outputs[tempX].VideoOutFeedback.NameFeedback.StringValue;
                        }
                        else
                        {
                            return NoRouteText;
                        }
                    });
                    OutputAudioRouteNameFeedbacks[tempX] = new StringFeedback(() =>
                    {
                        if (Chassis.Outputs[tempX].AudioOutFeedback != null)
                        {
                            return Chassis.Outputs[tempX].AudioOutFeedback.NameFeedback.StringValue;
                        }
                        else
                        {
                            return NoRouteText;

                        }
                    });

                    OutputEndpointOnlineFeedbacks[tempX] = new BoolFeedback(() =>
                    {
                        return Chassis.Outputs[tempX].EndpointOnlineFeedback;
                    });
                }
                else
                {
                    Debug.Console(2, this, "No Output Card defined in slot: {0}", tempX);
                }
            };

            // Setup Input Card Feedbacks
            for (uint x = 1; x <= Chassis.NumberOfInputs; x++)
            {
                var tempX = x;

                Debug.Console(2, this, "Setting up feedbacks for input slot: {0}", tempX);

                CheckForHdcp2Property(tempX);

                if (Chassis.Inputs[tempX] != null)
                {

                    UsbInputRoutedToFeebacks[tempX] = new IntFeedback(() =>
                    {
                        if (Chassis.Inputs[tempX].USBRoutedToFeedback != null) { return (ushort)Chassis.Inputs[tempX].USBRoutedToFeedback.Number; }
                        else { return 0; };
                    });
                    VideoInputSyncFeedbacks[tempX] = new BoolFeedback(() =>
                    {
                        if (Chassis.Inputs[tempX].VideoDetectedFeedback != null)
                            return Chassis.Inputs[tempX].VideoDetectedFeedback.BoolValue;
                        else
                            return false;
                    });
                    InputNameFeedbacks[tempX] = new StringFeedback(() =>
                    {
                        if (Chassis.Inputs[tempX].NameFeedback != null)
                        {
                            return Chassis.Inputs[tempX].NameFeedback.StringValue;
                        }
                        else
                        {
                            return "";
                        }
                    });

                    InputEndpointOnlineFeedbacks[tempX] = new BoolFeedback(() =>
                    {
                        return Chassis.Inputs[tempX].EndpointOnlineFeedback;
                    });

                    InputCardHdcpCapabilityFeedbacks[tempX] = new IntFeedback(() =>
                    {
                        var inputCard = Chassis.Inputs[tempX];

                        Debug.Console(2, this, "Adding InputCardHdcpCapabilityFeedback for slot: {0}", inputCard);

                        if (inputCard.Card is DmcHd)
                        {
                            InputCardHdcpCapabilityTypes[tempX] = eHdcpCapabilityType.HdcpAutoSupport;

                            if ((inputCard.Card as DmcHd).HdmiInput.HdcpSupportOnFeedback.BoolValue)
                                return 1;
                            else
                                return 0;
                        }
                        else if (inputCard.Card is DmcHdDsp)
                        {
                            InputCardHdcpCapabilityTypes[tempX] = eHdcpCapabilityType.HdcpAutoSupport;

                            if ((inputCard.Card as DmcHdDsp).HdmiInput.HdcpSupportOnFeedback.BoolValue)
                                return 1;
                            else
                                return 0;
                        }
                        else if (inputCard.Card is Dmc4kHdBase)
                        {
                            InputCardHdcpCapabilityTypes[tempX] = eHdcpCapabilityType.Hdcp2_2Support;

                            return (int)(inputCard.Card as Dmc4kHdBase).HdmiInput.HdcpReceiveCapability;
                        }
                        else if (inputCard.Card is Dmc4kCBase)
                        {
                            if (PropertiesConfig.InputSlotSupportsHdcp2[tempX])
                            {
                                InputCardHdcpCapabilityTypes[tempX] = eHdcpCapabilityType.HdcpAutoSupport;

                                return (int)(inputCard.Card as Dmc4kCBase).DmInput.HdcpReceiveCapability;
                            }
                            else if ((inputCard.Card as Dmc4kCBase).DmInput.HdcpSupportOnFeedback.BoolValue)
                                return 1;
                            else
                                return 0;
                        }
                        else if (inputCard.Card is Dmc4kCDspBase)
                        {
                            if (PropertiesConfig.InputSlotSupportsHdcp2[tempX])
                            {
                                InputCardHdcpCapabilityTypes[tempX] = eHdcpCapabilityType.HdcpAutoSupport;

                                return (int)(inputCard.Card as Dmc4kCDspBase).DmInput.HdcpReceiveCapability;
                            }
                            else if ((inputCard.Card as Dmc4kCDspBase).DmInput.HdcpSupportOnFeedback.BoolValue)
                                return 1;
                            else
                                return 0;
                        }
                        else
                            return 0;
                    });
                }
                else
                {
                    Debug.Console(2, this, "No Input Card defined in slot: {0}", tempX);
                }
            }

            return base.CustomActivate();
        }

        /// <summary>
        /// Checks for presence of config property defining if the input card supports HDCP2.
        /// If not found, assumes false.
        /// </summary>
        /// <param name="inputSlot">Input Slot</param>
        void CheckForHdcp2Property(uint inputSlot)
        {
            if (!PropertiesConfig.InputSlotSupportsHdcp2.ContainsKey(inputSlot))
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Warning,
@"Properties Config does not define inputSlotSupportsHdcp2 entry for input card: {0}.  Assuming false.  
If HDCP2 is required, HDCP control/feedback will not fucntion correctly!", inputSlot);
                PropertiesConfig.InputSlotSupportsHdcp2.Add(inputSlot, false);
            }
            else
                Debug.Console(2, this, "inputSlotSupportsHdcp2 for input card: {0} = {1}", inputSlot, PropertiesConfig.InputSlotSupportsHdcp2[inputSlot]);
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
                AddOutputPortWithDebug(string.Format("inputCard{0}", number), "sdiOut", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Sdi, null);
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
            AddOutputPortWithDebug(string.Format("inputCard{0}", number), "hdmiLoopOut", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, null);
            AddOutputPortWithDebug(string.Format("inputCard{0}", number), "audioLoopOut", eRoutingSignalType.Audio, eRoutingPortConnectionType.Hdmi, null);
        }

        void AddInCardHdmiLoopPort(uint number)
        {
            AddOutputPortWithDebug(string.Format("inputCard{0}", number), "hdmiLoopOut", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, null);
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
            else if (type == "dmc4kzhdo")
            {
                var outputCard = new Dmc4kzHdoSingle(number, Chassis);
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
                AddOutputPortWithDebug(string.Format("outputCard{0}", number), "dmOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmMmFiber, 2 * (number - 1) + 1);
                AddOutputPortWithDebug(string.Format("outputCard{0}", number), "hdmiOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, 2 * (number - 1) + 1, cecPort1);
                AddOutputPortWithDebug(string.Format("outputCard{0}", number), "dmOut2", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmMmFiber, 2 * (number - 1) + 2);

            }
            else if (type == "dmcs2ohd")
            {
                var outputCard = new DmcS2oHdSingle(number, Chassis);
                var cecPort1 = outputCard.Card1.HdmiOutput;
                AddOutputPortWithDebug(string.Format("outputCard{0}", number), "dmOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmSmFiber, 2 * (number - 1) + 1);
                AddOutputPortWithDebug(string.Format("outputCard{0}", number), "hdmiOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, 2 * (number - 1) + 1, cecPort1);
                AddOutputPortWithDebug(string.Format("outputCard{0}", number), "dmOut2", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmSmFiber, 2 * (number - 1) + 2);
            }
            else if (type == "dmcstro")
            {
                var outputCard = new DmcStroSingle(number, Chassis);
                AddOutputPortWithDebug(string.Format("outputCard{0}", number), "streamOut", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Streaming, 2 * (number - 1) + 1);
            }

            else
                Debug.Console(1, this, "  WARNING: Output card type '{0}' is not available", type);
        }

        void AddDmcHdoPorts(uint number, ICec cecPort1, ICec cecPort2)
        {
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "hdmiOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, 2 * (number - 1) + 1, cecPort1);
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "audioOut1", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio, 2 * (number - 1) + 1);
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "hdmiOut2", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, 2 * (number - 1) + 2, cecPort2);
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "audioOut2", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio, 2 * (number - 1) + 2);
        }

        void AddDmcCoPorts(uint number, ICec cecPort1)
        {
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "dmOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmCat, 2 * (number - 1) + 1);
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "hdmiOut1", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, 2 * (number - 1) + 1, cecPort1);
            AddOutputPortWithDebug(string.Format("outputCard{0}", number), "dmOut2", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmCat, 2 * (number - 1) + 2);
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

            if (inputPort != null)
            {
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
            }
		}

		void Chassis_DMInputChange(Switch device, DMInputEventArgs args)
        {
				
		    switch (args.EventId) {
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
                        if(UsbInputRoutedToFeebacks[args.Number] != null)
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
                    Debug.Console(2, this, "Output {0} DMOutputEventIds.EndpointOnlineEventId fired. State: {1}", args.Number, Chassis.Outputs[output].EndpointOnlineFeedback);
                    OutputEndpointOnlineFeedbacks[output].FireUpdate();
                    break;
                }
                case DMOutputEventIds.OnlineFeedbackEventId:
                {
                    Debug.Console(2, this, "Output {0} DMInputEventIds.OnlineFeedbackEventId fired. State: {1}", args.Number, Chassis.Outputs[output].EndpointOnlineFeedback);
                    OutputEndpointOnlineFeedbacks[output].FireUpdate();
                    break;
                }
                case DMOutputEventIds.VideoOutEventId:
                {
                    if (Chassis.Outputs[output].VideoOutFeedback != null)
                    {
                        Debug.Console(2, this, "DMSwitchVideo:{0} Routed Input:{1} Output:{2}'", this.Name, Chassis.Outputs[output].VideoOutFeedback.Number, output);
                    }
                    if (VideoOutputFeedbacks.ContainsKey(output))
                    {
                        VideoOutputFeedbacks[output].FireUpdate();

                    }
                    if (OutputVideoRouteNameFeedbacks.ContainsKey(output))
                    {
                        OutputVideoRouteNameFeedbacks[output].FireUpdate();
                    }
                    break;
                }
                case DMOutputEventIds.AudioOutEventId:
                {
                    if (Chassis.Outputs[output].AudioOutFeedback != null)
                    {
                        Debug.Console(2, this, "DMSwitchAudio:{0} Routed Input:{1} Output:{2}'", this.Name, Chassis.Outputs[output].AudioOutFeedback.Number, output);
                    }
                    if (AudioOutputFeedbacks.ContainsKey(output))
                    {
                        AudioOutputFeedbacks[output].FireUpdate();
                    }
                    if (OutputAudioRouteNameFeedbacks.ContainsKey(output))
                    {
                        OutputAudioRouteNameFeedbacks[output].FireUpdate();
                    }
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
			RouteOffTimers[pnt] = new CTimer(o =>
			{
				ExecuteSwitch(0, pnt.Number, pnt.Type);
			}, RouteOffTime);
		}


		// Send out sigs when coming online
		void IsOnline_OutputChange(object sender, EventArgs e)
		{
			if (IsOnline.BoolValue)
			{
                (Chassis as DmMDMnxn).EnableAudioBreakaway.BoolValue = true;
                (Chassis as DmMDMnxn).EnableUSBBreakaway.BoolValue = true;

				if (InputNames != null)
					foreach (var kvp in InputNames)
						Chassis.Inputs[kvp.Key].Name.StringValue = kvp.Value;
				if (OutputNames != null)
					foreach(var kvp in OutputNames)
						Chassis.Outputs[kvp.Key].Name.StringValue = kvp.Value;
			}
		}

		#region IRouting Members

		public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType sigType)
		{
			Debug.Console(2, this, "Making an awesome DM route from {0} to {1} {2}", inputSelector, outputSelector, sigType);

			var input = Convert.ToUInt32(inputSelector); // Cast can sometimes fail
			var output = Convert.ToUInt32(outputSelector);
			// Check to see if there's an off timer waiting on this and if so, cancel
			var key = new PortNumberType(output, sigType);
            if (input == 0)
            {
                StartOffTimer(key);
            }
            else
			{
				if(RouteOffTimers.ContainsKey(key))
				{
					Debug.Console(2, this, "{0} cancelling route off due to new source", output);
					RouteOffTimers[key].Stop();
					RouteOffTimers.Remove(key);
				}
			}

            var inCard = input == 0 ? null : Chassis.Inputs[input];
            var outCard = input == 0 ? null : Chassis.Outputs[output];

			// NOTE THAT BITWISE COMPARISONS - TO CATCH ALL ROUTING TYPES 
			if ((sigType | eRoutingSignalType.Video) == eRoutingSignalType.Video)
			{
				Chassis.VideoEnter.BoolValue = true;
				Chassis.Outputs[output].VideoOut = inCard;
			}

			if ((sigType | eRoutingSignalType.Audio) == eRoutingSignalType.Audio)
			{
                (Chassis as DmMDMnxn).AudioEnter.BoolValue = true;
				Chassis.Outputs[output].AudioOut = inCard;
			}

            if ((sigType | eRoutingSignalType.UsbOutput) == eRoutingSignalType.UsbOutput)
            {
                Chassis.USBEnter.BoolValue = true;
                if (Chassis.Outputs[output] != null)
                    Chassis.Outputs[output].USBRoutedTo = inCard;
            }

            if ((sigType | eRoutingSignalType.UsbInput) == eRoutingSignalType.UsbInput)
            {
                Chassis.USBEnter.BoolValue = true;
                if(Chassis.Inputs[input] != null)
                    Chassis.Inputs[input].USBRoutedTo = outCard; 
            }
		}

		#endregion
    }

	public struct PortNumberType
	{
		public uint Number { get; private set; }
		public eRoutingSignalType Type { get; private set; }

		public PortNumberType(uint number, eRoutingSignalType type) : this()
		{
			Number = number;
			Type = type;
		}
	}
}