using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
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
            string type, DmpsRoutingPropertiesConfig properties)
        {
            try
            {
                ISystemControl systemControl = null;

                type = type.ToLower();

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
        }

        void Dmps_DMInputChange(Switch device, DMInputEventArgs args)
        {
            //Debug.Console(2, this, "DMSwitch:{0} Input:{1} Event:{2}'", this.Name, args.Number, args.EventId.ToString());

            switch (args.EventId)
            {
                case (DMInputEventIds.OnlineFeedbackEventId):
                    {
                        Debug.Console(2, this, "DMINput OnlineFeedbackEventId for input: {0}. State: {1}", args.Number, device.Inputs[args.Number].EndpointOnlineFeedback);
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
                if (Dmps.SwitcherOutputs[output].VideoOutFeedback != null)
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
            }
            else if (args.EventId == DMOutputEventIds.AudioOutEventId)
            {
                if (Dmps.SwitcherOutputsoutput].AudioOutFeedback != null)
                {
                    Debug.Console(2, this, "DMSwitchAudio:{0} Routed Input:{1} Output:{2}'", this.Name, Chassis.Outputs[output].AudioOutFeedback.Number, output);
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
    }
}