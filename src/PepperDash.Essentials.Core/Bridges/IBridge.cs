using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Defines the contract for IBridgeAdvanced
    /// </summary>
    public interface IBridgeAdvanced
    {
        /// <summary>
        /// Links the bridge to the API using the provided trilist, join start, join map key, and bridge.
        /// </summary>
        /// <param name="trilist">The trilist to link to.</param>
        /// <param name="joinStart">The starting join number.</param>
        /// <param name="joinMapKey">The key for the join map.</param>
        /// <param name="bridge">The EISC API bridge.</param>
        void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge);
    }
}