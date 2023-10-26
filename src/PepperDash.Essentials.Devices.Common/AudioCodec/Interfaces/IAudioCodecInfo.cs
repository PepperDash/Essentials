using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.AudioCodec
{
    /// <summary>
    /// Implements a common set of data about a codec
    /// </summary>
    public interface IAudioCodecInfo
    {
        AudioCodecInfo CodecInfo { get; }
    }
}