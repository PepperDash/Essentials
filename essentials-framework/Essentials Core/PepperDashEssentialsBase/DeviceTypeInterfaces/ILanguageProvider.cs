using System;
using System.Collections.Generic;

namespace PepperDash_Essentials_Core.DeviceTypeInterfaces
{
        public interface ILanguageProvider
    {
        ILanguageDefinition CurrentLanguage { get; set; }

        event EventHandler CurrentLanguageChanged;
    }

}