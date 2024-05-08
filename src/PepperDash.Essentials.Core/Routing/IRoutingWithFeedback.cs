using System.Collections.Generic;
using System;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines an IRouting with a feedback event
    /// </summary>
    public interface IRoutingWithFeedback : IRouting
    {
        List<RouteSwitchDescriptor> CurrentRoutes { get; }

        event EventHandler RoutingChanged;
    }
}