using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DM;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.DM.Config;

namespace PepperDash.Essentials.DM.Chassis
{
    public class HdMdNxM4kEController : Device, IRoutingInputsOutputs, IRouting
    {
        public HdMdNxM Chassis { get; private set; }

        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }
        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }


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

            // logical ports
            InputPorts = new RoutingPortCollection<RoutingInputPort>();
            for (uint i = 1; i <= 4; i++)
            {
                InputPorts.Add(new RoutingInputPort("hdmiIn" + i, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                    eRoutingPortConnectionType.Hdmi, i, this));
            }
            OutputPorts = new RoutingPortCollection<RoutingOutputPort>();
            OutputPorts.Add(new RoutingOutputPort(DmPortName.HdmiOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, null, this));

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
            var current = Chassis.HdmiOutputs[1].VideoOut;
            if (current != Chassis.HdmiInputs[(uint)inputSelector])
                Chassis.HdmiOutputs[1].VideoOut = Chassis.HdmiInputs[(uint)inputSelector];
        }

        #endregion

        /////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static HdMdNxM4kEController GetController(string key, string name,
            string type, HdMdNxM4kEPropertiesConfig properties)
        {
            try
            {
                var ipid = properties.Control.IpIdInt;
                var address = properties.Control.TcpSshProperties.Address;

                type = type.ToLower();
                if (type == "hdmd4x14ke")
                {
                    Debug.Console(0, @"The 'hdmd4x14ke' device is not an Essentials Bridgeable device.  
                        If an essentials Bridgeable Device is required, use the 'hdmd4x14ke-bridgeable' type");
  
                    var chassis = new HdMd4x14kE(ipid, address, Global.ControlSystem);
                    return new HdMdNxM4kEController(key, name, chassis, properties);
                }
                return null;
            }
            catch (Exception e)
            {
                Debug.Console(0, "ERROR Creating device key {0}: \r{1}", key, e);
                return null;
            }
        }
    }
}