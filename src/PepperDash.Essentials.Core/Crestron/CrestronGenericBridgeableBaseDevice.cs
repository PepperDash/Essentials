using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Core
{
    public abstract class CrestronGenericBridgeableBaseDevice : CrestronGenericBaseDevice, IBridgeAdvanced
    {
        protected CrestronGenericBridgeableBaseDevice(string key, string name, GenericBase hardware) : base(key, name, hardware)
        {
        }

        protected CrestronGenericBridgeableBaseDevice(string key, string name)
            : base(key, name)
        {
        }


        public abstract void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge);
    }
}