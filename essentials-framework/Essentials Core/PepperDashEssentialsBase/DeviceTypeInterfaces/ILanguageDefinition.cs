using System;
using System.Collections.Generic;

namespace PepperDash_Essentials_Core.DeviceTypeInterfaces
{
    public interface ILanguageDefinition
    {
        string LocaleName { get; set; }
        string FriendlyName { get; set; }
        bool Enable { get; set; }
        List<LanguageLabel> UiLabels { get; set; }
        List<LanguageLabel> Sources { get; set; }
        List<LanguageLabel> Destinations { get; set; }
    }
}