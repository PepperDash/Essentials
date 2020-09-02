using System.Collections.Generic;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;

namespace PepperDash_Essentials_Core.Devices
{
    public class GenericIRController: EssentialsBridgeableDevice
    {
        private readonly IrOutputPortController _port;

        public GenericIRController(string key, string name, IrOutputPortController irPort) : base(key, name)
        {
            _port = irPort;

            DeviceManager.AddDevice(_port);
        }

        #region Overrides of EssentialsBridgeableDevice

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }

    public class GenericIrControllerFactory : EssentialsDeviceFactory<GenericIRController>
    {
        public GenericIrControllerFactory()
        {
            TypeNames = new List<string> {"genericIrController"};
        }
        #region Overrides of EssentialsDeviceFactory<GenericIRController>

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic IR Controller Device");

            var irPort = IRPortHelper.GetIrOutputPortController(dc);

            return new GenericIRController(dc.Key, dc.Name, irPort);
        }

        #endregion
    }
}