extern alias Full;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.DSP;
using System;
using System.Text.RegularExpressions;

namespace PepperDash.Essentials
{
    /// <summary>
    /// 
    /// </summary>
    public class EssentialsRoomVolumesConfig
    {
        public EssentialsVolumeLevelConfig Master { get; set; }
        public EssentialsVolumeLevelConfig Program { get; set; }
        public EssentialsVolumeLevelConfig AudioCallRx { get; set; }
        public EssentialsVolumeLevelConfig AudioCallTx { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class EssentialsVolumeLevelConfig
    {
        public string DeviceKey { get; set; }
        public string Label { get; set; }
        public int Level { get; set; }
    }
}