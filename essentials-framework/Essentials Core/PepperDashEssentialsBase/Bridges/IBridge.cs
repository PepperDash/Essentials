using System;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Defines a device that uses JoinMapBaseAdvanced for its join map
    /// </summary>
    public interface IBridgeAdvanced:IKeyed
    {
        void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge);
    }

    public interface IBridge:IKeyed
    {
        void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey);
    }   
}