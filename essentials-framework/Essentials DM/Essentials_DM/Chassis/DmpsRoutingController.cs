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

using PepperDash.Essentials.DM.Config;

namespace PepperDash.Essentials.DM
{
    public class DmpsRoutingController : Device, IRoutingInputsOutputs, IRouting, IHasFeedback
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

            Dmps.DMInputChange += new DMInputEventHandler(Dmps_DMInputChange);
            Dmps.DMOutputChange +=new DMOutputEventHandler(Dmps_DMOutputChange);

            // Default to EnableAudioBreakaway
            //SystemControl.EnableAudioBreakaway. = true;

            Debug.Console(1, this, "{0} Switcher Inputs Present.", Dmps.NumberOfSwitcherInputs);
            Debug.Console(1, this, "{0} Switcher Outputs Present.", Dmps.NumberOfSwitcherOutputs);



            uint tempX = 1;

            foreach (var card in Dmps.SwitcherOutputs)
            {
                Debug.Console(1, this, "Output Card Type: {0}", card.CardInputOutputType);

                var outputCard = card as Card.Dmps3OutputBase;
            //}

            //for (uint x = 1; x <= Dmps.NumberOfSwitcherOutputs; x++)
            //{
                //var tempX = x;

               //Card.Dmps3OutputBase outputCard = Dmps.SwitcherOutputs[tempX] as Card.Dmps3OutputBase;

               if (outputCard != null)
               {
                    VideoOutputFeedbacks[tempX] = new IntFeedback(() => {
                        if(outputCard.VideoOutFeedback != null) { return (ushort)outputCard.VideoOutFeedback.Number;}
                        else { return 0; };
                    });
                    AudioOutputFeedbacks[tempX] = new IntFeedback(() =>
                    {
                        if (outputCard.AudioOutFeedback != null) { return (ushort)outputCard.AudioOutFeedback.Number; }
                        else { return 0; };
                    });

                    OutputNameFeedbacks[tempX] = new StringFeedback(() =>
                    {
                        if (outputCard.NameFeedback.StringValue != null)
                        {
                            return outputCard.NameFeedback.StringValue;
                        }
                        else
                        {
                            return "";
                        }
                    });                   

                    OutputVideoRouteNameFeedbacks[tempX] = new StringFeedback(() =>
                    {
                        if (outputCard.VideoOutFeedback != null)
                        {
                            return outputCard.VideoOutFeedback.NameFeedback.StringValue;
                        }
                        else
                        {
                            return "";
                        }
                    });
                    OutputAudioRouteNameFeedbacks[tempX] = new StringFeedback(() =>
                    {
                        if (outputCard.AudioOutFeedback != null)
                        {
                            return outputCard.AudioOutFeedback.NameFeedback.StringValue;
                        }
                        else
                        {
                            return "";

                        }
                    });

                    OutputEndpointOnlineFeedbacks[tempX] = new BoolFeedback(() => { return outputCard.EndpointOnlineFeedback; });

                    AddOutputCard(tempX, outputCard);  
  
                    tempX++;
                }
            }

            tempX = 1;

            foreach (var card in Dmps.SwitcherInputs)
            {
                var inputCard = card as DMInput;

                Debug.Console(1, this, "Output Card Type: {0}", card.CardInputOutputType);

            //for (uint x = 1; x <= Dmps.NumberOfSwitcherInputs; x++)
            //{
            //    var tempX = x;

            //    DMInput inputCard = Dmps.SwitcherInputs[tempX] as DMInput;

                if (inputCard != null)
                {
                    InputEndpointOnlineFeedbacks[tempX] = new BoolFeedback(() => { return inputCard.EndpointOnlineFeedback; });

                    VideoInputSyncFeedbacks[tempX] = new BoolFeedback(() =>
                    {
                        return inputCard.VideoDetectedFeedback.BoolValue;
                    });
                    InputNameFeedbacks[tempX] = new StringFeedback(() =>
                    {
                        if (inputCard.NameFeedback.StringValue != null)
                        {
                            return inputCard.NameFeedback.StringValue;
                        }
                        else
                        {
                            return "";
                        }
                    });
                }

                AddInputCard(tempX, inputCard);
            }
        }

        public override bool CustomActivate()
        {

            if (InputNames != null)
                foreach (var kvp in InputNames)
                    (Dmps.SwitcherInputs[kvp.Key] as DMInput).Name.StringValue = kvp.Value;
            if (OutputNames != null)
                foreach (var kvp in OutputNames)
                    (Dmps.SwitcherOutputs[kvp.Key] as Card.Dmps3OutputBase).Name.StringValue = kvp.Value;

            return base.CustomActivate();
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
                var hdmiInputCard = inputCard as Card.Dmps3HdmiInput;

                var cecPort = hdmiInputCard.HdmiInputPort;

                AddInputPortWithDebug(number, string.Format("hdmiIn{0}", number), eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, cecPort);              
            }
            else if (inputCard is Card.Dmps3HdmiInput)
            {
                var hdmiInputCard = inputCard as Card.Dmps3HdmiInput;

                var cecPort = hdmiInputCard.HdmiInputPort;

                AddInputPortWithDebug(number, string.Format("hdmiIn{0}", number), eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, cecPort);
                AddInputPortWithDebug(number, string.Format("audioIn{1}", number), eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio);
            }
            else if (inputCard is Card.Dmps3HdmiVgaInput)
            {
                // TODO: Build a virtual TX device and assign the ports to it

                var hdmiVgaInputCard = inputCard as Card.Dmps3HdmiVgaInput;

                DmpsInternalVirtualHdmiVgaInputController inputCardController = new DmpsInternalVirtualHdmiVgaInputController(Key +
                    string.Format("-input{0}", number), string.Format("InternalInputController-{0}", number), hdmiVgaInputCard);

                DeviceManager.AddDevice(inputCardController);

                AddInputPortWithDebug(number, string.Format("input{0}", number), eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.BackplaneOnly);
            }
            else if (inputCard is Card.Dmps3HdmiVgaBncInput)
            {
                // TODO: Build a virtual TX device and assign the ports to it

                var hdmiVgaBncInputCard = inputCard as Card.Dmps3HdmiVgaBncInput;

                DmpsInternalVirtualHdmiVgaBncInputController inputCardController = new DmpsInternalVirtualHdmiVgaBncInputController(Key +
                    string.Format("-input{0}", number), string.Format("InternalInputController-{0}", number), hdmiVgaBncInputCard);

                DeviceManager.AddDevice(inputCardController);

                AddInputPortWithDebug(number, string.Format("input{0}", number), eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.BackplaneOnly);

            }
            else if (inputCard is Card.Dmps3DmInput)
            {
                var hdmiInputCard = inputCard as Card.Dmps3DmInput;

                var cecPort = hdmiInputCard.DmInputPort;

                AddInputPortWithDebug(number, string.Format("dmIn{0}", number), eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.DmCat, cecPort);
            }
            else if (inputCard is Card.Dmps3AirMediaInput)
            {
                var airMediaInputCard = inputCard as Card.Dmps3AirMediaInput;

                AddInputPortWithDebug(number, string.Format("airMediaIn{0}", number), eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Streaming);
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
        public void AddOutputCard(uint number, Card.Dmps3OutputBase outputCard)
        {
            if (outputCard is Card.Dmps3HdmiOutput)
            {
                var hdmiOutputCard = outputCard as Card.Dmps3HdmiOutput;

                var cecPort = hdmiOutputCard.HdmiOutputPort;

                AddHdmiOutputPort(number, cecPort);
            }
            else if (outputCard is Card.Dmps3DmOutput)
            {
                var dmOutputCard = outputCard as Card.Dmps3DmOutput;

                var cecPort = dmOutputCard.DmOutputPort;

                AddDmOutputPort(number);
            }
        }

        /// <summary>
        /// Adds an HDMI output port
        /// </summary>
        /// <param name="number"></param>
        /// <param name="cecPort"></param>
        void AddHdmiOutputPort(uint number, ICec cecPort)
        {
            AddOutputPortWithDebug(number, string.Format("hdmiOut{0}", number), eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, number, cecPort);
        }

        /// <summary>
        /// Adds a DM output port
        /// </summary>
        /// <param name="number"></param>
        void AddDmOutputPort(uint number)
        {
            AddOutputPortWithDebug(number, string.Format("dmOut{0}", number), eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.DmCat, number);
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
        /// 
        /// </summary>
        void Dmps_DMOutputChange(Switch device, DMOutputEventArgs args)
        {
            var output = args.Number;

            Card.Dmps3OutputBase outputCard = Dmps.SwitcherOutputs[output] as Card.Dmps3OutputBase;

            if (args.EventId == DMOutputEventIds.VolumeEventId &&
                VolumeControls.ContainsKey(output))
            {
                VolumeControls[args.Number].VolumeEventFromChassis();
            }
            else if (args.EventId == DMOutputEventIds.OnlineFeedbackEventId)
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
            else if (args.EventId == DMOutputEventIds.OutputNameEventId)
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
            RouteOffTimers[pnt] = new CTimer(o =>
            {
                ExecuteSwitch(0, pnt.Number, pnt.Type);
            }, RouteOffTime);
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
                if (RouteOffTimers.ContainsKey(key))
                {
                    Debug.Console(2, this, "{0} cancelling route off due to new source", output);
                    RouteOffTimers[key].Stop();
                    RouteOffTimers.Remove(key);
                }
            }

            DMInput inCard = input == 0 ? null : Dmps.SwitcherInputs[input] as DMInput;

            // NOTE THAT THESE ARE NOTS - TO CATCH THE AudioVideo TYPE
            if (sigType != eRoutingSignalType.Audio)
            {
                SystemControl.VideoEnter.BoolValue = true;
                (Dmps.SwitcherOutputs[output] as Card.Dmps3OutputBase).VideoOut = inCard;
            }

            if (sigType != eRoutingSignalType.Video)
            {
                SystemControl.AudioEnter.BoolValue = true;
                (Dmps.SwitcherOutputs[output] as Card.Dmps3OutputBase).AudioOut = inCard;
            }
        }

        #endregion
    }
}