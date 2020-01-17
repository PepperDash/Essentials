using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;

using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.DM.Endpoints.DGEs
{
    /// <summary>
    /// Wrapper class for DGE-100 and DM-DGE-200-C
    /// </summary>
    public class DgeController : CrestronGenericBaseDevice, IComPorts, IIROutputPorts
    {
        public Dge100 DigitalGraphicsEngine { get; private set; }

        public DeviceConfig DeviceConfig { get; private set; }

        CrestronTouchpanelPropertiesConfig PropertiesConfig;

        public DgeController(string key, string name, Dge100 device, DeviceConfig dc, CrestronTouchpanelPropertiesConfig props)
            :base(key, name, device)
        {
            DigitalGraphicsEngine = device;

            DeviceConfig = dc;

            PropertiesConfig = props;
        }

        #region IComPorts Members

        public CrestronCollection<ComPort> ComPorts
        {
            get { return DigitalGraphicsEngine.ComPorts; }
        }

        public int NumberOfComPorts
        {
            get { return DigitalGraphicsEngine.NumberOfComPorts; }
        }

        #endregion

        #region IIROutputPorts Members

        public CrestronCollection<IROutputPort> IROutputPorts
        {
            get { return DigitalGraphicsEngine.IROutputPorts; }
        }

        public int NumberOfIROutputPorts
        {
            get { return DigitalGraphicsEngine.NumberOfIROutputPorts; }
        }

        #endregion
    }
}