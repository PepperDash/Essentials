using PepperDash.Essentials.Core.CrestronIO;

namespace PepperDash.Essentials.Devices.Common.Environment.Somfy
{
    public class RelayControlledShadeConfigProperties
    {
        public int RelayPulseTime { get; set; }
        public ShadeRelaysConfig Relays { get; set; }
        public string StopOrPresetLabel { get; set; }

        public class ShadeRelaysConfig
        {
            public IOPortConfig Open { get; set; }
            public IOPortConfig StopOrPreset { get; set; }
            public IOPortConfig Close { get; set; }
        }
    }
}