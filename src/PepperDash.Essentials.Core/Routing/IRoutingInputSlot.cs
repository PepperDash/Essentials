using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.Routing
{
    public interface IRoutingInputSlot: IRoutingSlot, IOnline, IVideoSync
    {
        string TxDeviceKey { get; }
    }

    public abstract class RoutingInputSlotBase : IRoutingInputSlot
    {
        public abstract string TxDeviceKey { get; }
        public abstract int SlotNumber { get; }
        public abstract eRoutingSignalType SupportedSignalTypes { get; }
        public abstract string Name { get; }
        public abstract BoolFeedback IsOnline { get; }
        public abstract bool VideoSyncDetected { get; }
        public abstract string Key { get; }

        public abstract event EventHandler VideoSyncChanged;
    }
}
