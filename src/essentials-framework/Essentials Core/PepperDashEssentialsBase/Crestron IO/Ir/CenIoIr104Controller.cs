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
    public class CenIoIr104Controller : CrestronGenericBaseDevice, IIROutputPorts
    {
	    private readonly CenIoIr104 _ir104;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="ir104"></param>
        public CenIoIr104Controller(string key, string name, CenIoIr104 ir104)
            : base(key, name, ir104)
        {
            _ir104 = ir104;
        }

        #region IDigitalInputPorts Members

		/// <summary>
		/// IR port collection
		/// </summary>
		public CrestronCollection<IROutputPort> IROutputPorts
        {
            get { return _ir104.IROutputPorts; }
        }

		/// <summary>
		/// Number of relay ports property
		/// </summary>
		public int NumberOfIROutputPorts
        {
            get { return _ir104.NumberOfIROutputPorts; }
        }

        #endregion
    }

	/// <summary>
	/// CEN-IO-IR-104 controller fatory
	/// </summary>
    public class CenIoIr104ControllerFactory : EssentialsDeviceFactory<CenIoIr104Controller>
    {
		/// <summary>
		/// Constructor
		/// </summary>
        public CenIoIr104ControllerFactory()
        {
            TypeNames = new List<string>() { "cenioir104" };
        }

		/// <summary>
		/// Build device CEN-IO-IR-104
		/// </summary>
		/// <param name="dc"></param>
		/// <returns></returns>
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new CEN-IO-IR-104 Device");

            var control = CommFactory.GetControlPropertiesConfig(dc);
			if (control == null)
			{
				Debug.Console(1, "Factory failed to create a new CEN-IO-IR-104 Device, control properties not found");
				return null;
			}

            var ipid = control.IpIdInt;
			if(ipid != 0) return new CenIoIr104Controller(dc.Key, dc.Name, new CenIoIr104(ipid, Global.ControlSystem));

			Debug.Console(1, "Factory failed to create a new CEN-IO-IR-104 Device using IP-ID-{0}", ipid);
			return null;
        }
    }

}