using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.GeneralIO;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.CrestronIO
{
	/// <summary>
	/// Wrapper class for CEN-IO-COM-Xxx expander module
	/// </summary>
	[Description("Wrapper class for the CEN-IO-COM-102 & CEN-IO-COM-202 expander module")]
	public class CenIoComController : CrestronGenericBaseDevice, IComPorts
    {
        private readonly CenIoCom _cenIoCom;

        public CenIoComController(string key, string name, CenIoCom cenIo)
			:base(key, name, cenIo)
        {
	        _cenIoCom = cenIo;
        }

        #region Implementation of IComPorts

        public CrestronCollection<ComPort> ComPorts
        {
            get { return _cenIoCom.ComPorts; }
        }

        public int NumberOfComPorts
        {
            get { return _cenIoCom.NumberOfComPorts; }
        }

        #endregion

    }

	public class CenIoCom102ControllerFactory : EssentialsDeviceFactory<CenIoComController>
	{
		private const string CenIoCom102Type = "ceniocom102";
		private const string CenIoCom202Type = "ceniocom202";

        public CenIoCom102ControllerFactory()
        {
			TypeNames = new List<string> { CenIoCom102Type, CenIoCom202Type };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new CEN-IO-COM-Xxx Device");

	        var control = CommFactory.GetControlPropertiesConfig(dc);
	        if (control == null)
	        {
				Debug.Console(1, "Factory failed to create a new CEN-IO-COM-Xxx Device, control properties not found");
				return null;
	        }

	        var ipid = control.IpIdInt;
	        if (ipid < 2)
	        {
				Debug.Console(1, "Factory failed to create a new CEN-IO-COM-Xxx Device, invalid IP-ID found");
				return null;
	        }

	        switch (dc.Type)
	        {
				case CenIoCom102Type:
		        {
					return new CenIoComController(dc.Key, dc.Name, new CenIoCom102(ipid, Global.ControlSystem));
		        }
				case CenIoCom202Type:
		        {
					return new CenIoComController(dc.Key, dc.Name, new CenIoCom202(ipid, Global.ControlSystem));
		        }
				default:
		        {
					Debug.Console(1, "Factory failed to create a new CEN-IO-COM-Xxx Device, invalid type '{0}'", dc.Type);
					return null;
		        }
	        }
        }
    }
}