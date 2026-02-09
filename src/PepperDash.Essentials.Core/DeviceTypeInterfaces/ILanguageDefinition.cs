using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for ILanguageDefinition
    /// </summary>
    public interface ILanguageDefinition
    {
        /// <summary>
        /// The locale name for the language definition
        /// </summary>
        string LocaleName { get; set; }

        /// <summary>
        /// The friendly name for the language definition
        /// </summary>
        string FriendlyName { get; set; }

        /// <summary>
        /// Indicates whether the language definition is enabled
        /// </summary>
        bool Enable { get; set; }

        /// <summary>
        /// The UI labels for the language definition
        /// </summary>
        List<LanguageLabel> UiLabels { get; set; }

        /// <summary>
        /// The source and destination labels for the language definition
        /// </summary>
        List<LanguageLabel> Sources { get; set; }

        /// <summary>
        /// The destination labels for the language definition
        /// </summary>
        List<LanguageLabel> Destinations { get; set; }

        /// <summary>
        /// The source group names for the language definition
        /// </summary>
        List<LanguageLabel> SourceGroupNames { get; set; } 

        /// <summary>
        /// The destination group names for the language definition
        /// </summary>
        List<LanguageLabel> DestinationGroupNames { get; set; }

        /// <summary>
        /// The room names for the language definition
        /// </summary>
        List<LanguageLabel> RoomNames { get; set; } 
    }
}
