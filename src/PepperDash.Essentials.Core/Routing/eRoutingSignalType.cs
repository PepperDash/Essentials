using System;

namespace PepperDash.Essentials.Core
{
    [Flags]
    public enum eRoutingSignalType
    {
        Audio = 1,
        Video = 2,     
        AudioVideo = Audio | Video,
        UsbOutput = 8,
        UsbInput = 16
    }
}