using System;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Represents a LanguageLabel
    /// </summary>
    public class LanguageLabel
    {
        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the DisplayText
        /// </summary>
        public string DisplayText { get; set; }
        /// <summary>
        /// Gets or sets the JoinNumber
        /// </summary>
        public uint JoinNumber { get; set; }
    }
}