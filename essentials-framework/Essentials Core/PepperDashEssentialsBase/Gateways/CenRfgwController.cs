using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Gateways;
using Newtonsoft.Json;


using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;


namespace PepperDash.Essentials.Core
{
    [Description("Wrapper class for Crestron Infinet-EX Gateways")]
	public class CenRfgwController : CrestronGenericBaseDevice
	{
        private GatewayBase _Gateway;
        public GatewayBase GateWay { get { return _Gateway; } }

		/// <summary>
		/// Constructor for the on-board gateway
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		public CenRfgwController(string key, string name, GatewayBase gateway) :
			base(key, name, gateway)
		{
            _Gateway = gateway;
		}

		public static CenRfgwController GetNewExGatewayController(string key, string name, ushort ipId, ushort cresnetId,  string gatewayType)
		{
			eExGatewayType type = (eExGatewayType)Enum.Parse(typeof(eExGatewayType), gatewayType, true);
			try
			{
				var cs = Global.ControlSystem;

				GatewayBase gw = null;
				switch (type)
				{
					case eExGatewayType.Ethernet:
                        gw = new CenRfgwEx(ipId, cs);
						break;
					case eExGatewayType.EthernetShared:
                        gw = new CenRfgwExEthernetSharable(ipId, cs);
						break;
					case eExGatewayType.Cresnet:
                        gw = new CenRfgwExCresnet(cresnetId, cs);
						break;
				}
				return new CenRfgwController(key, name, gw);
			}
			catch (Exception)
			{
                Debug.Console(0, "ERROR: Cannot create EX Gateway, id {0}, type {1}", type == eExGatewayType.Cresnet ? cresnetId : ipId, gatewayType);
				return null;
			}
		}
		public static CenRfgwController GetNewErGatewayController(string key, string name, ushort ipId, ushort cresnetId, string gatewayType)
		{
			eExGatewayType type = (eExGatewayType)Enum.Parse(typeof(eExGatewayType), gatewayType, true);
			try
			{
				var cs = Global.ControlSystem;

				GatewayBase gw = null;
				switch (type)
				{
					case eExGatewayType.Ethernet:
						gw = new CenErfgwPoe(ipId, cs);
						break;
					case eExGatewayType.EthernetShared:
                        gw = new CenErfgwPoeEthernetSharable(ipId, cs);
						break;
					case eExGatewayType.Cresnet:
                        gw = new CenErfgwPoeCresnet(cresnetId, cs);
						break;
				}
				return new CenRfgwController(key, name, gw);
			}
			catch (Exception)
			{
                Debug.Console(0, "ERROR: Cannot create ER Gateway, id {0}, type {1}", type== eExGatewayType.Cresnet ? cresnetId : ipId, gatewayType);
				return null;
			}
		}

	}



	public enum eExGatewayType
	{
		Ethernet, EthernetShared, Cresnet
    }


    #region Factory
    public class CenRfgwControllerFactory : EssentialsDeviceFactory<CenRfgwController>
    {
        public CenRfgwControllerFactory()
        {
            TypeNames = new List<string>() { "cenrfgwex", "cenerfgwpoe" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {

            Debug.Console(1, "Factory Attempting to create new CEN-GWEXER Device");

            var props = JsonConvert.DeserializeObject<EssentialsRfGatewayConfig>(dc.Properties.ToString());

            var type = dc.Type.ToLower();
            var control = props.Control;
            var ipid = control.IpIdInt;
            var cresnetId = control.CresnetIdInt;
            var gatewayType = props.GatewayType;

            switch (type)
            {
                case ("cenrfgwex"):
                    return CenRfgwController.GetNewExGatewayController(dc.Key, dc.Name,
                        (ushort)ipid, (ushort)cresnetId, gatewayType);
                case ("cenerfgwpoe"):
                    return CenRfgwController.GetNewErGatewayController(dc.Key, dc.Name,
                        (ushort)ipid, (ushort)cresnetId, gatewayType);
                default:
                    return null;
            }
        }
    }
    #endregion


}