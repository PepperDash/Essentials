using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Devices
{
    public class GenericIrControllerFactory : EssentialsDeviceFactory<GenericIrController>
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

            return new GenericIrController(dc.Key, dc.Name, irPort);
        }

        #endregion
    }
}