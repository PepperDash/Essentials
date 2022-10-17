using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Common
{
    public class SetTopBoxPropertiesConfig : SourceDevicePropertiesConfigBase
    {
        public bool HasPresets { get; set; }
        public bool HasDvr { get; set; }
        public bool HasDpad { get; set; }
        public bool HasNumeric { get; set; }
        public int IrPulseTime { get; set; }

        public bool UseStandardEnterForKeypad { get; set; }

        public CommandOverride AccessoryButton1Override { get; set; }
        public CommandOverride AccessoryButton2Override { get; set; }
        //public List<CommandOverride> CommandOverrides { get; set; } 




        public ControlPropertiesConfig Control { get; set; }
    }

    /// <summary>
    /// Object for overriding standard IR commands
    /// </summary>
    public class CommandOverride
    {
        /// <summary>
        /// The command in essentials to replace
        /// </summary>
        public string Command { get; set; }
        /// <summary>
        /// the value to replace the essentials command with
        /// </summary>
        public string Label { get; set; }
    }

}