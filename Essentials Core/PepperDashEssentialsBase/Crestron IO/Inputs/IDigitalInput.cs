using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core.Crestron_IO
{
    /// <summary>
    /// Represents a device that provides digital input
    /// </summary>
    public interface IDigitalInput
    {
        BoolFeedback InputStateFeedback { get; }
    }
}