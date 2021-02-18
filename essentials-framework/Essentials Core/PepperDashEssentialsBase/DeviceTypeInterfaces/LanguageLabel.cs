using System;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    public class LanguageLabel
    {
        public string Key { get; set; }
        public string Description { get; set; }
        public string DisplayText { get; set; }
        public uint JoinNumber { get; set; }
    }
}

namespace PepperDash_Essentials_Core.DeviceTypeInterfaces
{
    [Obsolete("Use PepperDash.Essentials.Core.DeviceTypeInterfaces")]
    public class LanguageLabel: PepperDash.Essentials.Core.DeviceTypeInterfaces.LanguageLabel
    {
    }
}