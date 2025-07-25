using System;


namespace PepperDash.Essentials.Core
{
    [Flags]
    /// <summary>
    /// Enumeration of eRoutingSignalType values
    /// </summary>
    public enum eRoutingSignalType
	{
		Audio = 1,
		Video = 2,     
		AudioVideo = Audio | Video,
        UsbOutput = 8,
        UsbInput = 16,
        SecondaryAudio = 32
	}
}