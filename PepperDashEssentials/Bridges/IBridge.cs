using System;

namespace PepperDash.Essentials.Bridges
{
    /// <summary>
    /// Defines a device that uses the legacy JoinMapBase for its join map
    /// </summary>
    [Obsolete("IBridgeAdvanced should be used going forward with JoinMapBaseAdvanced")]
    public interface IBridge:Core.Bridges.IBridge
    {
    }
}