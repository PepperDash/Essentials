using System.Collections.Generic;
using Crestron.SimplSharpPro.GeneralIO;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// CEN-IO-RY Controller factory
    /// </summary>
    public class CenIoRy104ControllerFactory : EssentialsDeviceFactory<CenIoRy104Controller>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CenIoRy104ControllerFactory()
        {
            TypeNames = new List<string>() { "ceniory104" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create a new CEN-IO-RY-104 Device");

            var controlPropertiesConfig = CommFactory.GetControlPropertiesConfig(dc);
            if (controlPropertiesConfig == null)
            {
                Debug.Console(1, "Factory failed to create a new CEN-IO-RY-104 Device, control properties not found");
                return null;
            }

            var ipid = controlPropertiesConfig.IpIdInt;
            if (ipid != 0) return new CenIoRy104Controller(dc.Key, dc.Name, new CenIoRy104(ipid, Global.ControlSystem));
            
            Debug.Console(1, "Factory failed to create a new CEN-IO-RY-104 Device using IP-ID-{0}", ipid);
            return null;
        }
    }
}