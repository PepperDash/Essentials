using System;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.GeneralIO;
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
}