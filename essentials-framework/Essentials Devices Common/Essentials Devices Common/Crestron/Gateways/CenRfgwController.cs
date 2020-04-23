using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Gateways;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;


namespace PepperDash.Essentials.Devices.Common
{
	public class CenRfgwController : CrestronGenericBaseDevice
	{
		public GatewayBase Gateway { get { return Hardware as GatewayBase; } }

		/// <summary>
		/// Constructor for the on-board gateway
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		public CenRfgwController(string key, string name, GatewayBase gateway) :
			base(key, name, gateway)
		{
		}

		public static CenRfgwController GetNewExGatewayController(string key, string name, string id, string gatewayType)
		{
			uint uid;
			eExGatewayType type;
			try
			{
				uid = Convert.ToUInt32(id, 16);
				type = (eExGatewayType)Enum.Parse(typeof(eExGatewayType), gatewayType, true);
				var cs = Global.ControlSystem;

				GatewayBase gw = null;
				switch (type)
				{
					case eExGatewayType.Ethernet:
						gw = new CenRfgwEx(uid, cs);
						break;
					case eExGatewayType.EthernetShared:
						gw = new CenRfgwExEthernetSharable(uid, cs);
						break;
					case eExGatewayType.Cresnet:
						gw = new CenRfgwExCresnet(uid, cs);
						break;
				}
				return new CenRfgwController(key, name, gw);
			}
			catch (Exception)
			{
				Debug.Console(0, "ERROR: Cannot create EX Gateway, id {0}, type {1}", id, gatewayType);
				return null;
			}
		}
		public static CenRfgwController GetNewErGatewayController(string key, string name, string id, string gatewayType)
		{
			uint uid;
			eExGatewayType type;
			try
			{
				uid = Convert.ToUInt32(id, 16);
				type = (eExGatewayType)Enum.Parse(typeof(eExGatewayType), gatewayType, true);
				var cs = Global.ControlSystem;

				GatewayBase gw = null;
				switch (type)
				{
					case eExGatewayType.Ethernet:
						gw = new CenErfgwPoe(uid, cs);
						break;
					case eExGatewayType.EthernetShared:
						gw = new CenErfgwPoeEthernetSharable(uid, cs);
						break;
					case eExGatewayType.Cresnet:
						gw = new CenErfgwPoeCresnet(uid, cs);
						break;
				}
				return new CenRfgwController(key, name, gw);
			}
			catch (Exception)
			{
				Debug.Console(0, "ERROR: Cannot create EX Gateway, id {0}, type {1}", id, gatewayType);
				return null;
			}
		}


		/// <summary>
		/// Gets the actual Crestron EX gateway for a given key. "processor" or the key of
		/// a CenRfgwExController in DeviceManager
		/// </summary>
		/// <param name="key"></param>
		/// <returns>Either processor GW or Gateway property of CenRfgwExController</returns>
		public static GatewayBase GetExGatewayBaseForKey(string key)
		{
			key = key.ToLower();
			if (key == "processor" && Global.ControlSystem.SupportsInternalRFGateway)
				return Global.ControlSystem.ControllerRFGatewayDevice;
			var gwCont = DeviceManager.GetDeviceForKey(key) as CenRfgwController;
			if (gwCont != null)
				return gwCont.Gateway;

			return null;
		}
	}

	public enum eExGatewayType
	{
		Ethernet, EthernetShared, Cresnet
	}

    public class CenRfgwControllerFactory : EssentialsDeviceFactory<CenRfgwController>
    {
        public CenRfgwControllerFactory()
        {
            TypeNames = new List<string>() { "cenrfgwex", "cenerfgwpoe" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new CEN-GWEXER Device");
            return CenRfgwController.GetNewExGatewayController(dc.Key, dc.Name,
                dc.Properties.Value<string>("id"), dc.Properties.Value<string>("gatewayType"));
        }
    }

}