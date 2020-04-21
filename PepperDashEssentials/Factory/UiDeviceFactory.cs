using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.PageManagers;
using PepperDash.Essentials.DM.Endpoints.DGEs;


namespace PepperDash.Essentials
{
    /// <summary>
    /// Responsible for loading all of the UI device types for this library
    /// </summary>
    public class UiDeviceFactory
    {
        public UiDeviceFactory()
        {
            var dgeControllerFactory = new DgeControllerFactory() as IDeviceFactory;
            dgeControllerFactory.LoadTypeFactories();

            var essentialsTouchpanelControllerFactory = new EssentialsTouchpanelControllerFactory() as IDeviceFactory;
            essentialsTouchpanelControllerFactory.LoadTypeFactories();
        }

    }
}