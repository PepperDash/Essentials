using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    public enum eCodecCallStatus
    {
        Unknown = 0,
        Connected, 
        Connecting, 
        Dialing, 
        Disconnected,
        Disconnecting, 
        EarlyMedia, 
        Idle,
        OnHold, 
        Ringing, 
        Preserved, 
        RemotePreserved,
    }
}