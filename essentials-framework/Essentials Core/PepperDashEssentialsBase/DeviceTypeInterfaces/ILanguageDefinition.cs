using System;
using System.Collections.Generic;

namespace PepperDash_Essentials_Core.DeviceTypeInterfaces
{
    public interface ILanguageDefinition
    {
        string LocaleName { get; set; }
        string FriendlyName { get; set; }
        bool Enable { get; set; }
        List<ILanguageLabel> UiLabels { get; set; }
        List<ILanguageLabel> Sources { get; set; }
        List<ILanguageLabel> Destinations { get; set; }
    }
}