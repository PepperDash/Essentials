using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Devices.Common
{
    public class SetTopBoxPropertiesConfig : PepperDash.Essentials.Core.Config.SourceDevicePropertiesConfigBase
    {
        public bool HasPresets { get; set; }
        public bool HasDvr { get; set; }
        public bool HasDpad { get; set; }
        public bool HasNumeric { get; set; }
        public int IrPulseTime { get; set; }

        public ControlPropertiesConfig Control { get; set; }
    }
}