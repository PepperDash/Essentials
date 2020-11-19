using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
        public interface ILanguageProvider
    {
        ILanguageDefinition CurrentLanguage { get; set; }

        event EventHandler CurrentLanguageChanged;
    }

}