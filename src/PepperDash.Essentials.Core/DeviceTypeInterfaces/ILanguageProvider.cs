using System;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
   
    /// <summary>
    /// Defines the contract for ILanguageProvider
    /// </summary>
    public interface ILanguageProvider
    {
        ILanguageDefinition CurrentLanguage { get; set; }

        event EventHandler CurrentLanguageChanged;
    }

}
