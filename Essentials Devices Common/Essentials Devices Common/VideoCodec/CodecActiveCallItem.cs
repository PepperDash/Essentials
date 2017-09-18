using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.VideoCodec

{
    public class CodecActiveCallItem
    {
        public string Name { get; private set; }

        public string Number { get; private set; }

        public eCodecCallType Type { get; private set; }
    }

    public enum eCodecCallType
    {
        None, Audio, Video
    }
}