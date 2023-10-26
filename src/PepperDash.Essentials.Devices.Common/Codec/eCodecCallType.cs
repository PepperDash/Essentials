using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.Codec

{
    public enum eCodecCallType
    {
        Unknown = 0, 
        Audio, 
        Video, 
        AudioCanEscalate, 
        ForwardAllCall
    }
}