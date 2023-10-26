using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Devices
{
    public abstract class ReconfigurableBridgableDevice : ReconfigurableDevice, IBridgeAdvanced
    {
        protected ReconfigurableBridgableDevice(DeviceConfig config) : base(config)
        {
        }

        public abstract void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge);
    }
}