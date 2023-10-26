using System;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.GeneralIO;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.CrestronIO
{
    public class DinIo8Controller:CrestronGenericBaseDevice, IIOPorts
    {
        private DinIo8 _device;

        public DinIo8Controller(string key, Func<DeviceConfig, DinIo8> preActivationFunc, DeviceConfig config):base(key, config.Name)
        {
            AddPreActivationAction(() =>
            {
                _device = preActivationFunc(config);

                RegisterCrestronGenericBase(_device);
            });
        }

        #region Implementation of IIOPorts

        public CrestronCollection<Versiport> VersiPorts
        {
            get { return _device.VersiPorts; }
        }

        public int NumberOfVersiPorts
        {
            get { return _device.NumberOfVersiPorts; }
        }

        #endregion


    }
}