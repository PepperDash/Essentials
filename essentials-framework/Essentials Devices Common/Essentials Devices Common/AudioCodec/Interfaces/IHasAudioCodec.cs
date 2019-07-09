using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.AudioCodec
{
    /// <summary>
    /// For rooms that have audio codec
    /// </summary>
    public interface IHasAudioCodec
    {
        AudioCodecBase AudioCodec { get; }
        BoolFeedback InCallFeedback { get; }

        ///// <summary>
        ///// Make this more specific
        ///// </summary>
        //List<PepperDash.Essentials.Devices.Common.Codec.CodecActiveCallItem> ActiveCalls { get; }
    }
}