using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.CrestronIO
{
    /// <summary>
    /// Defines the contract for IAnalogInput
    /// </summary>
    public interface IAnalogInput
    {
        /// <summary>
        /// Get the InputValueFeedback.
        /// </summary>
        /// <remarks>
        /// Updates when the analog input value changes
        /// </remarks>
        IntFeedback InputValueFeedback { get; }
    }
}