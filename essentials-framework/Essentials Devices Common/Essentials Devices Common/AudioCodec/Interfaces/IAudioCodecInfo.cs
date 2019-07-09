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

    /// <summary>
    /// Stores general information about a codec
    /// </summary>
    public abstract class AudioCodecInfo
    {
        public abstract string PhoneNumber { get; set; }
    }
}