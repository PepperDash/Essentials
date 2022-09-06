using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.GeneralIO;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Wrapper class for CEN-IO-RY-104 relay module
    /// </summary>
    [Description("Wrapper class for the CEN-IO-RY-104 relay module")]
    public class CenIoRy104Controller : CrestronGenericBaseDevice, IRelayPorts
    {
        private readonly CenIoRy104 _ry104;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="ry104"></param>
        public CenIoRy104Controller(string key, string name, CenIoRy104 ry104)
            : base(key, name, ry104)
        {
            _ry104 = ry104;
        }

        /// <summary>
        /// Relay port collection
        /// </summary>
        public CrestronCollection<Relay> RelayPorts
        {
            get { return _ry104.RelayPorts; }
        }
    
        /// <summary>
        /// Number of relay ports property
        /// </summary>
        public int NumberOfRelayPorts
        {
            get { return _ry104.NumberOfRelayPorts; }
        }        
    }

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