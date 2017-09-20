using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    public enum eCodecCallStatus
    {
        Connected, 
        Connecting, 
        Dialing, 
        Disconnected,
        Disconnecting, 
        EarlyMedia, 
        Idle, 
        Incoming, 
        OnHold, 
        Ringing, 
        Preserved, 
        RemotePreserved,
        Unknown = 0
    }
}