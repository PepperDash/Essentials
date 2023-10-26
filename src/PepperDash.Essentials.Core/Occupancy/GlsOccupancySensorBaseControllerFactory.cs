using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.GeneralIO;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
    public class GlsOccupancySensorBaseControllerFactory : EssentialsDeviceFactory<GlsOccupancySensorBaseController>
    {
        public GlsOccupancySensorBaseControllerFactory()
        {
            TypeNames = new List<string> { "glsoirccn" };
        }


        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new GlsOirOccupancySensorController Device");

            return new GlsOirOccupancySensorController(dc.Key, GetGlsOirCCn, dc);
        }

        private static GlsOirCCn GetGlsOirCCn(DeviceConfig dc)
        {
            var control   = CommFactory.GetControlPropertiesConfig(dc);
            var cresnetId = control.CresnetIdInt;
            var branchId  = control.ControlPortNumber;
            var parentKey = string.IsNullOrEmpty(control.ControlPortDevKey) ? "processor" : control.ControlPortDevKey;

            if (parentKey.Equals("processor", StringComparison.CurrentCultureIgnoreCase))
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new GlsOirCCn", parentKey);
                return new GlsOirCCn(cresnetId, Global.ControlSystem);
            }
            var cresnetBridge = DeviceManager.GetDeviceForKey(parentKey) as IHasCresnetBranches;

            if (cresnetBridge != null)
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new GlsOirCCn", parentKey);
                return new GlsOirCCn(cresnetId, cresnetBridge.CresnetBranches[branchId]);
            }
            Debug.Console(0, "Device {0} is not a valid cresnet master", parentKey);
            return null;
        }

    }
}