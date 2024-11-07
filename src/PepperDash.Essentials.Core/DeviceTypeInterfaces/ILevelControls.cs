﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PepperDash.Essentials.Core.Devices;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    public interface ILevelControls
    {
        Dictionary<string, IBasicVolumeWithFeedback> LevelControlPoints { get; }
    }
}
