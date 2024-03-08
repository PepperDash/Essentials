using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.Routing
{
    public interface IRoutingInputSlot: IRoutingSlot, IOnline
    {
        string TxDeviceKey { get; }

        bool SyncDetected { get; }
    }
}
