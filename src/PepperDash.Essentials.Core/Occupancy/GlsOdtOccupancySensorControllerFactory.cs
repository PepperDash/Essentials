using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.GeneralIO;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
    public class GlsOdtOccupancySensorControllerFactory : EssentialsDeviceFactory<GlsOdtOccupancySensorController>
    {
        public GlsOdtOccupancySensorControllerFactory()
        {
            TypeNames = new List<string> { "glsodtccn" };
        }


        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new GlsOccupancySensorBaseController Device");

            return new GlsOdtOccupancySensorController(dc.Key, GetGlsOdtCCn, dc);
        }

        private static GlsOdtCCn GetGlsOdtCCn(DeviceConfig dc)
        {
            var control   = CommFactory.GetControlPropertiesConfig(dc);
            var cresnetId = control.CresnetIdInt;
            var branchId  = control.ControlPortNumber;
            var parentKey = String.IsNullOrEmpty(control.ControlPortDevKey) ? "processor" : control.ControlPortDevKey;

            if (parentKey.Equals("processor", StringComparison.CurrentCultureIgnoreCase))
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new GlsOdtCCn", parentKey);
                return new GlsOdtCCn(cresnetId, Global.ControlSystem);
            }
            var cresnetBridge = DeviceManager.GetDeviceForKey(parentKey) as IHasCresnetBranches;

            if (cresnetBridge != null)
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new GlsOdtCCn", parentKey);
                return new GlsOdtCCn(cresnetId, cresnetBridge.CresnetBranches[branchId]);
            }
            Debug.Console(0, "Device {0} is not a valid cresnet master", parentKey);
            return null;
        }
    }
}