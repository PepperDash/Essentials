using System;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.GeneralIO;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.CrestronIO
{
    public class C2NIoController:CrestronGenericBaseDevice, IComPorts, IIROutputPorts, IRelayPorts
    {
        private C2nIo _device;

        public C2NIoController(string key, Func<DeviceConfig, C2nIo> preActivationFunc, DeviceConfig config):base(key, config.Name)
        {
            AddPreActivationAction(() =>
            {
                _device = preActivationFunc(config);

                RegisterCrestronGenericBase(_device);
            });
        }

        #region Implementation of IComPorts

        public CrestronCollection<ComPort> ComPorts
        {
            get { return _device.ComPorts; }
        }

        public int NumberOfComPorts
        {
            get { return _device.NumberOfComPorts; }
        }

        #endregion

        #region Implementation of IIROutputPorts

        public CrestronCollection<IROutputPort> IROutputPorts
        {
            get { return _device.IROutputPorts; }
        }

        public int NumberOfIROutputPorts
        {
            get { return _device.NumberOfIROutputPorts; }
        }

        #endregion

        #region Implementation of IRelayPorts

        public CrestronCollection<Relay> RelayPorts
        {
            get { return _device.RelayPorts; }
        }

        public int NumberOfRelayPorts
        {
            get { return _device.NumberOfRelayPorts; }
        }

        #endregion
    }
}