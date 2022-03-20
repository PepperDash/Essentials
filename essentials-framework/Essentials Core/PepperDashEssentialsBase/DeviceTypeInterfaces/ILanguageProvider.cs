using System;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
   
    public interface ILanguageProvider
    {
        ILanguageDefinition CurrentLanguage { get; set; }

        event EventHandler CurrentLanguageChanged;
    }

}

namespace PepperDash_Essentials_Core.DeviceTypeInterfaces
{
    [Obsolete("Use PepperDash.Essentials.Core.DeviceTypeInterfaces")]
    public interface ILanguageProvider
    {
        ILanguageDefinition CurrentLanguage { get; set; }

        event EventHandler CurrentLanguageChanged;
    }

}