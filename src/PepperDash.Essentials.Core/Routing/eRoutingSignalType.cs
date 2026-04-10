using System;


namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Enumeration of eRoutingSignalType values
    /// </summary>
    [Flags]
    public enum eRoutingSignalType
	{
        /// <summary>
        /// Audio signal type
        /// </summary>
		Audio = 1,

        /// <summary>
        /// Video signal type
        /// </summary>
		Video = 2,     

        /// <summary>
        /// AudioVideo signal type
        /// </summary>
		AudioVideo = Audio | Video,

        /// <summary>
        /// Control signal type
        /// </summary>
        Usb = 8,

	}
}
