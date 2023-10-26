using System.Collections.Generic;
using Crestron.SimplSharpPro.GeneralIO;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
    public class CenIoDigIn104ControllerFactory : EssentialsDeviceFactory<CenIoDigIn104Controller>
    {
        public CenIoDigIn104ControllerFactory()
        {
            TypeNames = new List<string>() { "ceniodigin104" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new CEN-DIGIN-104 Device");

            var control = CommFactory.GetControlPropertiesConfig(dc);
            if (control == null)
            {
                Debug.Console(1, "Factory failed to create a new CEN-DIGIN-104 Device, control properties not found");
                return null;
            }
            var ipid = control.IpIdInt;
            if (ipid != 0) return new CenIoDigIn104Controller(dc.Key, dc.Name, new CenIoDi104(ipid, Global.ControlSystem));

            Debug.Console(1, "Factory failed to create a new CEN-IO-IR-104 Device using IP-ID-{0}", ipid);
            return null;
        }
    }
}