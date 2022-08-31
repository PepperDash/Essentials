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
    /// Wrapper class for CEN-IO-IR-104 module
    /// </summary>
    [Description("Wrapper class for the CEN-IO-IR-104 module")]
    public class CenIoIr104Controller : EssentialsDevice, IIROutputPorts
    {
        public CenIoIr104 Ir104 { get; private set; }

        public CenIoIr104Controller(string key, string name, CenIoIr104 ir104)
            : base(key, name)
        {
            Ir104 = ir104;
        }

        #region IDigitalInputPorts Members

		public CrestronCollection<IROutputPort> IROutputPorts
        {
            get { return Ir104.IROutputPorts; }
        }

		public int NumberOfIROutputPorts
        {
            get { return Ir104.NumberOfIROutputPorts; }
        }

        #endregion
    }

    public class CenIoIr104ControllerFactory : EssentialsDeviceFactory<CenIoIr104Controller>
    {
        public CenIoIr104ControllerFactory()
        {
            TypeNames = new List<string>() { "cenioir104" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new CEN-IR-104 Device");

            var control = CommFactory.GetControlPropertiesConfig(dc);
            var ipid = control.IpIdInt;

            return new CenIoIr104Controller(dc.Key, dc.Name, new Crestron.SimplSharpPro.GeneralIO.CenIoIr104(ipid, Global.ControlSystem));
        }
    }

}