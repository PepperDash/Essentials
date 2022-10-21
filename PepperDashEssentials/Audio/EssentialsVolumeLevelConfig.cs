using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Devices.Common.DSP;

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