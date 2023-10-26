namespace PepperDash.Essentials.Devices.Common.DSP
{
    public class BiampTesiraForteLevelControlBlockConfig
    {
        public bool Enabled { get; set; }
        public string Label { get; set; }
        public string InstanceTag { get; set; }
        public int Index1 { get; set; }
        public int Index2 { get; set; }
        public bool HasMute { get; set; }
        public bool HasLevel { get; set; }
    }
}