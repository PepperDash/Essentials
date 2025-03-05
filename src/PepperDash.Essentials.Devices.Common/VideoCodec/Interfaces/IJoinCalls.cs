using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    public interface IJoinCalls
    {
        void JoinCall(CodecActiveCallItem activeCall);
        void JoinAllCalls();
    }
}