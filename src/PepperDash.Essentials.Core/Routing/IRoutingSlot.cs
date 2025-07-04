using PepperDash.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.Routing;

public interface IRoutingSlot:IKeyName
{
    int SlotNumber { get; }

    eRoutingSignalType SupportedSignalTypes { get; }
}
