using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Gateways;
using Newtonsoft.Json;
using Crestron.SimplSharpPro.DeviceSupport;


using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash_Essentials_Core;


namespace PepperDash.Essentials.Core
{
    [Description("Wrapper class for Crestron Infinet-EX Gateways")]
    public class CenRfgwController : CrestronGenericBaseDevice, IHasReady
    {
        public event IsReadyEventHandler IsReady;

        private GatewayBase _gateway;

        public GatewayBase GateWay
        {
            get { return _gateway; }
        }

        /// <summary>
        /// Constructor for the on-board gateway
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        public CenRfgwController(string key, string name, GatewayBase gateway) :
            base(key, name, gateway)
        {
            _gateway = gateway;
            IsReady(this, new IsReadyEventArgs(true));
        }

        public CenRfgwController(string key, Func<DeviceConfig, GatewayBase> preActivationFunc, DeviceConfig config) :
            base(key, config.Name)
        {
            IsReady(this, new IsReadyEventArgs(false));
            AddPreActivationAction(() =>
            {
                _gateway = preActivationFunc(config);

                RegisterCrestronGenericBase(_gateway);
                IsReady(this, new IsReadyEventArgs(true));

            });
        }

        public static GatewayBase GetNewIpRfGateway(DeviceConfig dc)
        {
            var control = CommFactory.GetControlPropertiesConfig(dc);
            var name = dc.Name;
            var type = dc.Type;
            var key = dc.Key;
            var ipId = control.IpIdInt;

            if (type.Equals("cenrfgwex", StringComparison.InvariantCultureIgnoreCase))
            {
                return new CenRfgwEx(ipId, Global.ControlSystem);
            }
            if (type.Equals("cenerfgwpoe", StringComparison.InvariantCultureIgnoreCase))
            {
                return new CenErfgwPoe(ipId, Global.ControlSystem);
            }
            return null;
        }

        public static GatewayBase GetNewSharedIpRfGateway(DeviceConfig dc)
        {
            var control = CommFactory.GetControlPropertiesConfig(dc);
            var ipId = control.IpIdInt;

            if (dc.Type.Equals("cenrfgwex", StringComparison.InvariantCultureIgnoreCase))
            {
                return new CenRfgwExEthernetSharable(ipId, Global.ControlSystem);
            }
            if (dc.Type.Equals("cenerfgwpoe", StringComparison.InvariantCultureIgnoreCase))
            {
                return new CenErfgwPoeEthernetSharable(ipId, Global.ControlSystem);
            }
            return null;
        }

        public static GatewayBase GetCenRfgwCresnetController(DeviceConfig dc)
        {
            var control = CommFactory.GetControlPropertiesConfig(dc);
            var type = dc.Type;
            var cresnetId = control.CresnetIdInt;
            var branchId = control.ControlPortNumber;
            var parentKey = string.IsNullOrEmpty(control.ControlPortDevKey) ? "processor" : control.ControlPortDevKey;

            if (parentKey.Equals("processor", StringComparison.CurrentCultureIgnoreCase))
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new GlsOdtCCn");
                if (type.Equals("cenerfgwpoe", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new CenErfgwPoeCresnet(cresnetId, Global.ControlSystem);
                }
                if (type.Equals("cenrfgwex", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new CenRfgwExCresnet(cresnetId, Global.ControlSystem);
                }
            }
            var cresnetBridge = DeviceManager.GetDeviceForKey(parentKey) as ICresnetBridge;

            if (cresnetBridge != null)
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new GlsOdtCCn");

                if (type.Equals("cenerfgwpoe", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new CenErfgwPoeCresnet(cresnetId, cresnetBridge.Branches[branchId]);
                }
                if (type.Equals("cenrfgwex", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new CenRfgwExCresnet(cresnetId, cresnetBridge.Branches[branchId]);
                }
            }
            Debug.Console(0, "Device {0} is not a valid cresnet master", branchId);
            return null;
        }







        public enum eExGatewayType
        {
            Ethernet,
            EthernetShared,
            Cresnet
        }


        #region Factory

        public class CenRfgwControllerFactory : EssentialsDeviceFactory<CenRfgwController>
        {
            public CenRfgwControllerFactory()
            {
                TypeNames = new List<string>() {"cenrfgwex", "cenerfgwpoe"};
            }

            public override EssentialsDevice BuildDevice(DeviceConfig dc)
            {

                Debug.Console(1, "Factory Attempting to create new CEN-GWEXER Device");

                var props = JsonConvert.DeserializeObject<EssentialsRfGatewayConfig>(dc.Properties.ToString());

                var type = dc.Type.ToLower();
                var control = props.Control;
                var ipid = control.IpIdInt;
                var cresnetId = control.CresnetIdInt;

                eExGatewayType gatewayType =
                    (eExGatewayType) Enum.Parse(typeof (eExGatewayType), props.GatewayType, true);

                switch (gatewayType)
                {
                    case (eExGatewayType.Ethernet):
                        return new CenRfgwController(dc.Key, dc.Name, CenRfgwController.GetNewIpRfGateway(dc));
                    case (eExGatewayType.EthernetShared):
                        return new CenRfgwController(dc.Key, dc.Name, CenRfgwController.GetNewSharedIpRfGateway(dc));
                    case (eExGatewayType.Cresnet):
                        return new CenRfgwController(dc.Key, CenRfgwController.GetCenRfgwCresnetController, dc);
                }
                return null;
            }
        }
    }

    #endregion
}


