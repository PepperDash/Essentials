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
    public interface iCodecInfo
    {
        VideoCodecInfo CodecInfo { get; }
    }

    /// <summary>
    /// Stores general information about a codec
    /// </summary>
    public abstract class VideoCodecInfo
    {
        public abstract bool MultiSiteOptionIsEnabled { get; }
        public abstract string IpAddress { get; }
        public abstract string PhoneNumber { get; }
        public abstract string SipUri { get; }
        public abstract bool AutoAnswerEnabled { get; }
    }
}