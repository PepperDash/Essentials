using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.Codec;

public interface IHasCallHold
{
    /// <summary>
    /// Put the specified call on hold
    /// </summary>
    /// <param name="activeCall"></param>
    void HoldCall(CodecActiveCallItem activeCall);

    /// <summary>
    /// Resume the specified call
    /// </summary>
    /// <param name="activeCall"></param>
    void ResumeCall(CodecActiveCallItem activeCall);
}