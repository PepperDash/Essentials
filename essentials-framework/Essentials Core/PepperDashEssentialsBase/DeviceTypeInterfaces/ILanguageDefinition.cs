using System;

namespace PepperDash_Essentials_Core.DeviceTypeInterfaces
{
    public interface ILanguageDefinition
    {
        ILanguageDefinition CurrentLanguage { get; set; }

        event EventHandler CurrentLanguageChanged;
    }
}