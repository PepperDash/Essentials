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
    public interface IDigitalOutput
    {
        /// <summary>
        /// Feedback to indicate the state of the output
        /// </summary>
        BoolFeedback OutputStateFeedback { get; }

        /// <summary>
        /// Sets the output state
        /// </summary>
        /// <param name="state">The desired state of the output</param>
        void SetOutput(bool state);
    }
}