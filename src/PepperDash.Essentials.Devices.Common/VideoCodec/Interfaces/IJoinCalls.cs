using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    /// <summary>
    /// Defines the contract for IJoinCalls
    /// </summary>
    public interface IJoinCalls
    {
        void JoinCall(CodecActiveCallItem activeCall);
        void JoinAllCalls();
    }
}