using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Defines a device that uses JoinMapBaseAdvanced for its join map
    /// </summary>
    public interface IBridgeAdvanced
    {
        void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge);
    }
}