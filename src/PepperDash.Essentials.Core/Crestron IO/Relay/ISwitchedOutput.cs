using System;
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
        BoolFeedback OutputIsOnFeedback {get;}

        void On();
        void Off();
    }
}