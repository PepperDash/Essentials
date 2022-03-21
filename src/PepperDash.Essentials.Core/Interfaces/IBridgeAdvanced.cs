using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Core.Interfaces
{
    /// <summary>
    /// Defines a device that uses JoinMapBaseAdvanced for its join map
    /// </summary>
    public interface IBridgeAdvanced
    {
        void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge);
    }
}