using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.CrestronIO
{
    /// <summary>
    /// Describes an output capable of switching on and off
    /// </summary>
    public interface ISwitchedOutput
    {
        /// <summary>
        /// Feedback to indicate whether the output is on
        /// </summary>
        BoolFeedback OutputIsOnFeedback {get;}

        /// <summary>
        /// Turns the output on
        /// </summary>
        void On();

        /// <summary>
        /// Turns the output off
        /// </summary>
        void Off();
    }

    /// <summary>
    /// Describes a collection of switched outputs
    /// </summary>
    public interface ISwitchedOutputCollection
    {
        /// <summary>
        /// Dictionary of switched outputs by their port number
        /// </summary>
        Dictionary<uint, ISwitchedOutput> SwitchedOutputs { get; }
    }
}