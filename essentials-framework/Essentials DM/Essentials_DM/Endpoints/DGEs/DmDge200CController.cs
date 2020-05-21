using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;

using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Crestron.SimplSharpPro.DeviceSupport;

using Crestron.SimplSharpPro.DM;

namespace PepperDash.Essentials.DM.Endpoints.DGEs
{
    /// <summary>
    /// Wrapper class for DGE-100 and DM-DGE-200-C
    /// </summary>
    [Description("Wrapper class for DM-DGE-200-C")]    
    public class DmDge200CController : Dge100Controller, IRoutingInputsOutputs
    {
        private readonly DmDge200C _dge;

        public RoutingInputPort DmIn { get; private set; }
        public RoutingOutputPort HdmiOut { get; private set; }

        public RoutingPortCollection<RoutingInputPort> InputPorts
        {
            get;
            private set;
        }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts
        {
            get;
            private set;
        }

        public DmDge200CController(string key, string name, DmDge200C device, DeviceConfig dc, CrestronTouchpanelPropertiesConfig props)
            : base(key, name, device, dc, props)
        {
            _dge = device;

            DmIn = new RoutingInputPort(DmPortName.DmIn, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.DmCat, 0, this);
            HdmiOut = new RoutingOutputPort(DmPortName.HdmiOut, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Hdmi, null, this);


            InputPorts = new RoutingPortCollection<RoutingInputPort> { DmIn };
            OutputPorts = new RoutingPortCollection<RoutingOutputPort> { HdmiOut };

            // Set Ports for CEC
            HdmiOut.Port = _dge.HdmiOut; ;

        }

        public class DmDge200CControllerFactory : EssentialsDeviceFactory<DmDge200CController>
        {
            public DmDge200CControllerFactory()
            {
                TypeNames = new List<string>() { "dmdge200c" };
            }

            public override EssentialsDevice BuildDevice(DeviceConfig dc)
            {
                var typeName = dc.Type.ToLower();
                var comm = CommFactory.GetControlPropertiesConfig(dc);
                var props = JsonConvert.DeserializeObject<CrestronTouchpanelPropertiesConfig>(dc.Properties.ToString());

                Debug.Console(1, "Factory Attempting to create new DgeController  Device");

                DmDge200C dgeDevice = null;

                if (typeName == "dmdge200c")
                    dgeDevice = new DmDge200C(comm.IpIdInt, Global.ControlSystem);

                if (dgeDevice == null)
                {
                    Debug.Console(1, "Unable to create DGE device");
                    return null;
                }

                var dgeController = new DmDge200CController(dc.Key , dc.Name, dgeDevice, dc, props);

                return dgeController;
            }
        }
    }
}