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

    public class DinIo8ControllerFactory : EssentialsDeviceFactory<DinIo8Controller>
    {
        public DinIo8ControllerFactory()
        {
            TypeNames = new List<string>() { "DinIo8" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new C2N-IO Device");

            return new DinIo8Controller(dc.Key, GetDinIo8Device, dc);
        }

        static DinIo8 GetDinIo8Device(DeviceConfig dc)
        {
            var control = CommFactory.GetControlPropertiesConfig(dc);
            var cresnetId = control.CresnetIdInt;
            var branchId = control.ControlPortNumber;
            var parentKey = string.IsNullOrEmpty(control.ControlPortDevKey) ? "processor" : control.ControlPortDevKey;

            if (parentKey.Equals("processor", StringComparison.CurrentCultureIgnoreCase))
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new DinIo8", parentKey);
                return new DinIo8(cresnetId, Global.ControlSystem);
            }
            var cresnetBridge = DeviceManager.GetDeviceForKey(parentKey) as IHasCresnetBranches;

            if (cresnetBridge != null)
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new DinIo8", parentKey);
                return new DinIo8(cresnetId, cresnetBridge.CresnetBranches[branchId]);
            }
            Debug.Console(0, "Device {0} is not a valid cresnet master", parentKey);
            return null;
        }
    }
}