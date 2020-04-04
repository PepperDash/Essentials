using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Bridges
{
    /// <summary>
    /// Defines a device that uses the legacy JoinMapBase for its join map
    /// </summary>
    [Obsolete("IBridgeAdvanced should be used going forward with JoinMapBaseAdvanced")]
    public interface IBridge
    {
        void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey);
    }

    /// <summary>
    /// Defines a device that uses JoinMapBaseAdvanced for its join map
    /// </summary>
    public interface IBridgeAdvanced
    {
        void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApi bridge);
    }
}