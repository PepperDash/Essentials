namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Enumeration of eRoutingPortConnectionType values
    /// </summary>
    public enum eRoutingPortConnectionType
	{
        /// <summary>
        /// No connection type
        /// </summary>
		None, 
        
        /// <summary>
        /// Backplane only connection
        /// </summary>
        BackplaneOnly, 
        
        /// <summary>
        /// Connection via cable
        /// </summary>
        DisplayPort, 
        
        /// <summary>
        /// DVI connection
        /// </summary>
        Dvi, 
        
        /// <summary>
        /// HDMI connection
        /// </summary>
        Hdmi, 
        
        /// <summary>
        /// RGB connection
        /// </summary>
        Rgb, 
        
        /// <summary>
        /// VGA connection
        /// </summary>
        Vga, 
        
        /// <summary>
        /// Line audio connection
        /// </summary>
        LineAudio, 
        
        /// <summary>
        /// Digital audio connection
        /// </summary>
        DigitalAudio, 
        
        /// <summary>
        /// SDI connection
        /// </summary>
        Sdi, 

        /// <summary>
        /// Composite connection
        /// </summary>
		Composite, 
        
        /// <summary>
        /// Component connection
        /// </summary>
        Component, 
        
        /// <summary>
        /// DM CAT connection
        /// </summary>
        DmCat, 
        
        /// <summary>
        /// DM MM Fiber connection
        /// </summary>
        DmMmFiber, 
        
        /// <summary>
        /// DM SM Fiber connection
        /// </summary>
        DmSmFiber, 
        
        /// <summary>
        /// Speaker connection
        /// </summary>
        Speaker, 
        
        /// <summary>
        /// Microphone connection
        /// </summary>
        Streaming, 
        
        /// <summary>
        /// USB-C connection
        /// </summary>
        UsbC, 
        
        /// <summary>
        /// HDBaseT connection
        /// </summary>
        HdBaseT
	}
}