using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.GeneralIO;
using PepperDash.Core;
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

    public class C2NIoControllerFactory : EssentialsDeviceFactory<C2NIoController>
    {
        public C2NIoControllerFactory()
        {
            TypeNames = new List<string>() { "c2nio" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new C2N-IO Device");

            return new C2NIoController(dc.Key, GetC2NIoDevice, dc);
        }

        static C2nIo GetC2NIoDevice(DeviceConfig dc)
        {
            var control = CommFactory.GetControlPropertiesConfig(dc);
            var cresnetId = control.CresnetIdInt;
            var branchId = control.ControlPortNumber;
            var parentKey = string.IsNullOrEmpty(control.ControlPortDevKey) ? "processor" : control.ControlPortDevKey;

            if (parentKey.Equals("processor", StringComparison.CurrentCultureIgnoreCase))
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new C2nIo", parentKey);
                return new C2nIo(cresnetId, Global.ControlSystem);
            }
            var cresnetBridge = DeviceManager.GetDeviceForKey(parentKey) as IHasCresnetBranches;

            if (cresnetBridge != null)
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new C2nIo", parentKey);
                return new C2nIo(cresnetId, cresnetBridge.CresnetBranches[branchId]);
            }
            Debug.Console(0, "Device {0} is not a valid cresnet master", parentKey);
            return null;
        }
    }
}