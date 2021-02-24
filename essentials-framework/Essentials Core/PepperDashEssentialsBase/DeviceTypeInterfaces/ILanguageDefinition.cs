using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    public interface ILanguageDefinition
    {
        string LocaleName { get; set; }
        string FriendlyName { get; set; }
        bool Enable { get; set; }
        List<LanguageLabel> UiLabels { get; set; }
        List<LanguageLabel> Sources { get; set; }
        List<LanguageLabel> Destinations { get; set; }
        List<LanguageLabel> SourceGroupNames { get; set; } 
        List<LanguageLabel> DestinationGroupNames { get; set; }
        List<LanguageLabel> RoomNames { get; set; } 
    }
}

namespace PepperDash_Essentials_Core.DeviceTypeInterfaces
{
    [Obsolete("Use PepperDash.Essentials.Core.DeviceTypeInterfaces")]
    public interface ILanguageDefinition:PepperDash.Essentials.Core.DeviceTypeInterfaces.ILanguageDefinition
    {
    }
}