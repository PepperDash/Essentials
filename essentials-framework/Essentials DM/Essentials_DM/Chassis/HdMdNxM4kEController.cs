using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DM;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.DM.Config;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;


namespace PepperDash.Essentials.DM.Chassis
{
    public class HdMdNxM4kEController : EssentialsBridgeableDevice, IRoutingInputsOutputs, IRouting
    {
        public HdMdNxM Chassis { get; private set; }

        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }
        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

        public FeedbackCollection<BoolFeedback> VideoInputSyncFeedbacks { get; private set; }
        public FeedbackCollection<IntFeedback> VideoOutputRouteFeedbacks { get; private set; }
        public FeedbackCollection<StringFeedback> InputNameFeedbacks { get; private set; }
        public FeedbackCollection<StringFeedback> OutputNameFeedbacks { get; private set; }
        public FeedbackCollection<StringFeedback> OutputRouteNameFeedbacks { get; private set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="chassis"></param>
        public HdMdNxM4kEController(string key, string name, HdMdNxM chassis,
            HdMdNxM4kEPropertiesConfig props)
            : base(key, name)
        {
            Chassis = chassis;

            var _props = props;

            VideoInputSyncFeedbacks = new FeedbackCollection<BoolFeedback>();
            VideoOutputRouteFeedbacks = new FeedbackCollection<IntFeedback>();
            InputNameFeedbacks = new FeedbackCollection<StringFeedback>();
            OutputNameFeedbacks = new FeedbackCollection<StringFeedback>();
            OutputRouteNameFeedbacks = new FeedbackCollection<StringFeedback>();

            // logical ports
            InputPorts = new RoutingPortCollection<RoutingInputPort>();
            for (uint i = 1; i <= Chassis.NumberOfInputs; i++)
            {
                InputPorts.Add(new RoutingInputPort("hdmiIn" + i, eRoutingSignalType.AudioVideo,
                    eRoutingPortConnectionType.Hdmi, i, this));
                VideoInputSyncFeedbacks.Add(new BoolFeedback(i.ToString(), () => Chassis.Inputs[i].VideoDetectedFeedback.BoolValue));
                InputNameFeedbacks.Add(new StringFeedback(i.ToString, () => _props.Inputs[i - 1].Name));
            }

            OutputPorts = new RoutingPortCollection<RoutingOutputPort>();
            for (uint i = 1; i <= Chassis.NumberOfOutputs; i++)
            {
                OutputPorts.Add(new RoutingOutputPort("hdmiOut" + i, eRoutingSignalType.AudioVideo,
                    eRoutingPortConnectionType.Hdmi, i, this));
                VideoOutputRouteFeedbacks.Add(new IntFeedback(i.ToString(), () => (int)Chassis.Outputs[i].VideoOutFeedback.Number));
            }

            // physical settings
            if (props != null && props.Inputs != null)
            {
                foreach (var kvp in props.Inputs)
                {
                    // strip "hdmiIn"
                    var inputNum = Convert.ToUInt32(kvp.Key.Substring(6));

                    var port = chassis.HdmiInputs[inputNum].HdmiInputPort;
                    // set hdcp disables
                    if (kvp.Value.DisableHdcp)
                    {
                        Debug.Console(0, this, "Configuration disables HDCP support on {0}", kvp.Key);
                        port.HdcpSupportOff();
                    }
                    else
                        port.HdcpSupportOn();
                }
            }

            Chassis.DMInputChange += new DMInputEventHandler(Chassis_DMInputChange);
            Chassis.DMOutputChange += new DMOutputEventHandler(Chassis_DMOutputChange);
        }

        void Chassis_DMOutputChange(Switch device, DMOutputEventArgs args)
        {
            if (args.EventId == DMOutputEventIds.VideoOutEventId)
            {
                foreach (var item in VideoOutputRouteFeedbacks)
                {
                    item.FireUpdate();
                }
            }
        }

        void Chassis_DMInputChange(Switch device, DMInputEventArgs args)
        {
            if (args.EventId == DMInputEventIds.VideoDetectedEventId)
            {
                foreach (var item in VideoInputSyncFeedbacks)
                {
                    item.FireUpdate();
                }
            }
        }

        public override bool CustomActivate()
        {
            var result = Chassis.Register();
            if (result != Crestron.SimplSharpPro.eDeviceRegistrationUnRegistrationResponse.Success)
            {
                Debug.Console(0, this, "Device registration failed: {0}", result);
                return false;
            }

            

            return base.CustomActivate();
        }



        #region IRouting Members

        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
        {
            // Try to make switch only when necessary.  The unit appears to toggle when already selected.
            var current = Chassis.HdmiOutputs[(uint)outputSelector].VideoOut;
            if (current != Chassis.HdmiInputs[(uint)inputSelector])
                Chassis.HdmiOutputs[(uint)outputSelector].VideoOut = Chassis.HdmiInputs[(uint)inputSelector];
        }

        #endregion

        /////////////////////////////////////////////////////


        public class HdMdNxM4kEControllerFactory : EssentialsDeviceFactory<HdMdNxM4kEController>
        {
            public HdMdNxM4kEControllerFactory()
            {
                TypeNames = new List<string>() { "hdmd4x14ke", "hdmd4x24ke", "hdmd6x24ke" };
            }

            public override EssentialsDevice BuildDevice(DeviceConfig dc)
            {


                Debug.Console(1, "Factory Attempting to create new HD-MD-NxM-4K-E Device");

                var props = JsonConvert.DeserializeObject<HdMdNxM4kEPropertiesConfig>(dc.Properties.ToString());

                var type = dc.Type.ToLower();
                var control = props.Control;
                var ipid = control.IpIdInt;
                var address = control.TcpSshProperties.Address;

                if (type.StartsWith("hdmd4x14ke"))
                {
                    return new HdMdNxM4kEController(dc.Key, dc.Name, new HdMd4x14kE(ipid, address, Global.ControlSystem), props);
                }

                else if (type.StartsWith("hdmd4x24ke"))
                {
                    return new HdMdNxM4kEController(dc.Key, dc.Name, new HdMd4x24kE(ipid, address, Global.ControlSystem), props);
                }

                else if (type.StartsWith("hdmd6x24ke"))
                {
                    return new HdMdNxM4kEController(dc.Key, dc.Name, new HdMd6x24kE(ipid, address, Global.ControlSystem), props);
                }

                return null;
            }
        }
    }
}