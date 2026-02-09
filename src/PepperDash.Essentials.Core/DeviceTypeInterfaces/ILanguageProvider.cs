using System;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
   
    /// <summary>
    /// Defines the contract for ILanguageProvider
    /// </summary>
    public interface ILanguageProvider
    {
        /// <summary>
        /// The current language definition
        /// </summary>
        ILanguageDefinition CurrentLanguage { get; set; }

        /// <summary>
        /// Event raised when the current language changes
        /// </summary>
        event EventHandler CurrentLanguageChanged;
    }

}
