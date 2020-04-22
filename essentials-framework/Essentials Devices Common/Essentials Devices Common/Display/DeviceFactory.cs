using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Displays
{
	public class DisplayDeviceFactory
	{
        public DisplayDeviceFactory()
        {
            var necFactory = new NecPSXMDisplayFactory() as IDeviceFactory;
            necFactory.LoadTypeFactories();

            var panasonicThFactory = new PanasonicThDisplayFactory() as IDeviceFactory;
            panasonicThFactory.LoadTypeFactories();

            var samsungMdcFactory = new SamsungMDCFactory() as IDeviceFactory;
            samsungMdcFactory.LoadTypeFactories();

            var avocorFactory = new AvocorDisplayFactory() as IDeviceFactory;
            avocorFactory.LoadTypeFactories();
        }

	}
}