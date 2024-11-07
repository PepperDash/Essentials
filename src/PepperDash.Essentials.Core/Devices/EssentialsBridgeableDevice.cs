using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Core.Devices
{
    public abstract class EssentialsBridgeableDevice:EssentialsDevice, IBridgeAdvanced
    {
        protected EssentialsBridgeableDevice(string key) : base(key)
        {
        }

        protected EssentialsBridgeableDevice(string key, string name) : base(key, name)
        {
        }

        public abstract void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge);
    }
}