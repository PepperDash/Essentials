extern alias Full;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Implements a common set of data about a codec
    /// </summary>
    public interface iVideoCodecInfo
    {
        VideoCodecInfo CodecInfo { get; }
    }
}