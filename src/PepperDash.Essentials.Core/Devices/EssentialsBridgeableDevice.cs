using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Base class for devices that can be bridged to an EISC API.
    /// </summary>
    public abstract class EssentialsBridgeableDevice : EssentialsDevice, IBridgeAdvanced
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EssentialsBridgeableDevice"/> class with the specified key.
        /// </summary>
        /// <param name="key">The unique key for the device.</param>
        protected EssentialsBridgeableDevice(string key) : base(key)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EssentialsBridgeableDevice"/> class with the specified key and name.
        /// </summary>
        /// <param name="key">The unique key for the device.</param>
        /// <param name="name">The display name for the device.</param>
        protected EssentialsBridgeableDevice(string key, string name) : base(key, name)
        {
        }

        /// <inheritdoc />
        public abstract void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge);
    }
}