﻿using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Routing
{
    public interface IRoutingOutputSlot : IRoutingSlot
    {
        event EventHandler OutputSlotChanged;

        string RxDeviceKey { get; }

        Dictionary<eRoutingSignalType, IRoutingInputSlot> CurrentRoutes { get; }
    }    
}
