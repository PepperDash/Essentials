using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.GeneralIO;
using PepperDash.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
    public class GlsOirOccupancySensorController:GlsOccupancySensorBaseController
    {
        private GlsOirCCn _occSensor;

        public GlsOirOccupancySensorController(string key, Func<DeviceConfig, GlsOirCCn> preActivationFunc,DeviceConfig config) : this(key,config.Name, preActivationFunc, config)
        {
        }

        public GlsOirOccupancySensorController(string key, string name, Func<DeviceConfig, GlsOirCCn> preActivationFunc, DeviceConfig config) : base(key, name, config)
        {
            AddPreActivationAction(() =>
            {
                _occSensor = preActivationFunc(config);

                RegisterCrestronGenericBase(_occSensor);

                RegisterGlsOccupancySensorBaseController(_occSensor);
            });
        }

        #region Overrides of CrestronGenericBridgeableBaseDevice

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            LinkOccSensorToApi(this, trilist, joinStart, joinMapKey, bridge);
        }

        #endregion
    }

    public class GlsOccupancySensorBaseControllerFactory : EssentialsDeviceFactory<GlsOccupancySensorBaseController>
    {
        public GlsOccupancySensorBaseControllerFactory()
        {
            TypeNames = new List<string> { "glsoirccn" };
        }


        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new GlsOirOccupancySensorController Device");

            return new GlsOirOccupancySensorController(dc.Key, GetGlsOirCCn, dc);
        }

        private static GlsOirCCn GetGlsOirCCn(DeviceConfig dc)
        {
            var control = CommFactory.GetControlPropertiesConfig(dc);
            var cresnetId = control.CresnetIdInt;
            var branchId = control.ControlPortNumber;
            var parentKey = string.IsNullOrEmpty(control.ControlPortDevKey) ? "processor" : control.ControlPortDevKey;

            if (parentKey.Equals("processor", StringComparison.CurrentCultureIgnoreCase))
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new GlsOirCCn", parentKey);
                return new GlsOirCCn(cresnetId, Global.ControlSystem);
            }
            var cresnetBridge = DeviceManager.GetDeviceForKey(parentKey) as IHasCresnetBranches;

            if (cresnetBridge != null)
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new GlsOirCCn", parentKey);
                return new GlsOirCCn(cresnetId, cresnetBridge.CresnetBranches[branchId]);
            }
            Debug.Console(0, "Device {0} is not a valid cresnet master", parentKey);
            return null;
        }

    }
}