﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Defines minimum volume controls for a codec device with dialing capabilities
    /// </summary>
    public interface ICodecAudio : IBasicVolumeWithFeedback, IPrivacy
    {

    }
}