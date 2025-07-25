using System.Collections.Generic;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for ILanguageDefinition
    /// </summary>
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
