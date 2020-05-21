using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;

using Crestron.SimplSharpPro.DM;


using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.DM.Endpoints.DGEs
{
    [Description("Wrapper class for DGE-100")]    
    public class Dge100Controller : CrestronGenericBaseDevice, IComPorts, IIROutputPorts, IHasBasicTriListWithSmartObject, ICec
    {
        private readonly Dge100 _dge;

        public BasicTriListWithSmartObject Panel { get { return _dge; } }

        private DeviceConfig _dc;

        CrestronTouchpanelPropertiesConfig PropertiesConfig;

        public Dge100Controller(string key, string name, Dge100 device, DeviceConfig dc, CrestronTouchpanelPropertiesConfig props)
            :base(key, name, device)
        {
            _dge = device;

            _dc = dc;

            PropertiesConfig = props;
        }

        #region IComPorts Members

        public CrestronCollection<ComPort> ComPorts
        {
            get { return _dge.ComPorts; }
        }

        public int NumberOfComPorts
        {
            get { return _dge.NumberOfComPorts; }
        }

        #endregion

        #region IIROutputPorts Members

        public CrestronCollection<IROutputPort> IROutputPorts
        {
            get { return _dge.IROutputPorts; }
        }

        public int NumberOfIROutputPorts
        {
            get { return _dge.NumberOfIROutputPorts; }
        }

        #endregion

        #region ICec Members
        public Cec StreamCec { get { return _dge.HdmiOut.StreamCec; } }
        #endregion

    }

    public class Dge100ControllerFactory : EssentialsDeviceFactory<Dge100Controller>
    {
        public Dge100ControllerFactory()
        {
            TypeNames = new List<string>() { "dge100" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            var typeName = dc.Type.ToLower();
            var comm = CommFactory.GetControlPropertiesConfig(dc);
            var props = JsonConvert.DeserializeObject<CrestronTouchpanelPropertiesConfig>(dc.Properties.ToString());

            Debug.Console(1, "Factory Attempting to create new DgeController Device");

            Dge100 dgeDevice = null;
            if (typeName == "dge100")
                dgeDevice = new Dge100(comm.IpIdInt, Global.ControlSystem);

            if (dgeDevice == null)
            {
                Debug.Console(1, "Unable to create DGE device");
                return null;
            }

            var dgeController = new Dge100Controller(dc.Key, dc.Name, dgeDevice, dc, props);

            return dgeController;
        }
    }
}