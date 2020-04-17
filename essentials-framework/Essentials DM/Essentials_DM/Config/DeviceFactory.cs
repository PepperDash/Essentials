using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.AirMedia;
using Crestron.SimplSharpPro.UI;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.DM.AirMedia;
using PepperDash.Essentials.DM.Endpoints.DGEs;

namespace PepperDash.Essentials.DM
{
    /// <summary>
    /// Responsible for loading the type factories for this library
    /// </summary>
	public class DmDeviceFactory
	{
        public DmDeviceFactory()
        {
            Debug.Console(1, "Essentials.DM Factory Adding Types...");

            var dmChassisFactory = new DmChassisControllerFactory() as IDeviceFactory;
            dmChassisFactory.LoadTypeFactories();

            var dmTxFactory = new DmTxControllerFactory() as IDeviceFactory;
            dmTxFactory.LoadTypeFactories();

            var dmRxFactory = new DmRmcControllerFactory() as IDeviceFactory;
            dmRxFactory.LoadTypeFactories();

            var hdMdFactory = new HdMdxxxCEControllerFactory() as IDeviceFactory;
            hdMdFactory.LoadTypeFactories();
        }

	}

}