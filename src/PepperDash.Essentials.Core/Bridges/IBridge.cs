using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Defines the contract for IBridgeAdvanced
    /// </summary>
    public interface IBridgeAdvanced
    {
        void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge);
    }
}