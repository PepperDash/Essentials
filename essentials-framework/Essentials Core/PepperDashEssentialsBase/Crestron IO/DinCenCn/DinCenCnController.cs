using System;
using System.Collections;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.GeneralIO;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using System.Linq;
using PepperDash_Essentials_Core.Crestron_IO.DinCenCn;


namespace PepperDash.Essentials.Core
{
    public class DinCenCn2Controller : CrestronGenericBaseDevice, IHasCresnetBranches
    {
        private readonly DinCenCn2 _device;

        public CrestronCollection<CresnetBranch> CresnetBranches
        {
            get {
                return _device != null ? _device.Branches : null;
            }
        }

        public DinCenCn2Controller(string key, string name, DinCenCn2 device, DeviceConfig config)
            : base(key, name, device)
        {
            _device = device;
        }

        public class DinCenCn2ControllerFactory : EssentialsDeviceFactory<DinCenCn2Controller>
        {
            public DinCenCn2ControllerFactory()
            {
                TypeNames = new List<string>() { "dincencn2", "dincencn2poe", "din-cencn2", "din-cencn2-poe" };
            }

            public override EssentialsDevice BuildDevice(DeviceConfig dc)
            {
                Debug.Console(1, "Factory Attempting to create new C2N-RTHS Device");

                var control = CommFactory.GetControlPropertiesConfig(dc);
                var ipid = control.IpIdInt;

                if (dc.Type.ToLower().Contains("poe"))
                {
                    return new DinCenCn2Controller(dc.Key, dc.Name, new DinCenCn2Poe(ipid, Global.ControlSystem), dc);
                }

                return new DinCenCn2Controller(dc.Key, dc.Name, new DinCenCn2(ipid, Global.ControlSystem), dc);
            }
        }
    }
}