using System;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Bridges
{
    /// <summary>
    /// Defines a device that uses the legacy JoinMapBase for its join map
    /// </summary>
    [Obsolete("IBridgeAdvanced should be used going forward with JoinMapBaseAdvanced")]
    public interface IBridge : PepperDash.Essentials.Core.Bridges.IBridge
    {
        
    }

}