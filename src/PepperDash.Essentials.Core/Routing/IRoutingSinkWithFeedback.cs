﻿using PepperDash.Essentials.Core.Routing;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core
{
    
    /// <summary>
    /// For fixed-source endpoint devices
    /// </summary>
    public interface IRoutingSinkWithFeedback : IRoutingSinkWithSwitching
    {
        
    }

/*    /// <summary>
    /// For fixed-source endpoint devices
    /// </summary>
    public interface IRoutingSinkWithFeedback<TSelector> : IRoutingSinkWithSwitching<TSelector>
    {
        RouteSwitchDescriptor CurrentRoute { get; }

        event EventHandler InputChanged;
    }*/
}