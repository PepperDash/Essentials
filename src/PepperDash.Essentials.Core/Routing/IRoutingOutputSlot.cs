using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Routing
{
    public interface IRoutingOutputSlot<TInput> : IRoutingSlot where TInput: IRoutingInputSlot
    {
        event EventHandler OutputSlotChanged;

        string RxDeviceKey { get; }

        Dictionary<eRoutingSignalType, TInput> CurrentRoutes { get; }
    }

    public abstract class RoutingOutputSlotBase<TInput> : IRoutingOutputSlot<TInput> where TInput: IRoutingInputSlot
    {
        public abstract string RxDeviceKey { get; }
        public abstract Dictionary<eRoutingSignalType, TInput> CurrentRoutes { get; }
        public abstract int SlotNumber { get; }
        public abstract eRoutingSignalType SupportedSignalTypes { get; }
        public abstract string Name { get; }
        public abstract string Key { get; }

        public abstract event EventHandler OutputSlotChanged;
    }
}
