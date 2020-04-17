using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.GeneralIO;
using PepperDash.Essentials.Core.Config;


using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Wrapper class for CEN-IO-DIGIN-104 digital input module
    /// </summary>
    [Description("Wrapper class for the CEN-IO-DIGIN-104 diginal input module")]
    public class CenIoDigIn104Controller : EssentialsDevice, IDigitalInputPorts
    {
        public CenIoDi104 Di104 { get; private set; }

        public CenIoDigIn104Controller(string key, string name, CenIoDi104 di104)
            : base(key, name)
        {
            Di104 = di104;
        }

        #region IDigitalInputPorts Members

        public CrestronCollection<DigitalInput> DigitalInputPorts
        {
            get { return Di104.DigitalInputPorts; }
        }

        public int NumberOfDigitalInputPorts
        {
            get { return Di104.NumberOfDigitalInputPorts; }
        }

        #endregion
    }

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
            var ipid = control.IpIdInt;

            return new CenIoDigIn104Controller(dc.Key, dc.Name, new Crestron.SimplSharpPro.GeneralIO.CenIoDi104(ipid, Global.ControlSystem));
        }
    }

}