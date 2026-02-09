using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core.CrestronIO
{
    /// <summary>
    /// Represents a device that provides digital input
    /// </summary>
    public interface IDigitalInput
    {
        /// <summary>
        /// Feedback to indicate the state of the input
        /// </summary>
        BoolFeedback InputStateFeedback { get; }
    }
}