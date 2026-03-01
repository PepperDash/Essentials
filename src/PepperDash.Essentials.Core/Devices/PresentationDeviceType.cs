using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.EthernetCommunication;
using Crestron.SimplSharpPro.UI;

namespace PepperDash.Essentials.Core
{
 /// <summary>
 /// Enumeration of PresentationSourceType values
 /// </summary>
	public enum PresentationSourceType
	{
		/// <summary>
		/// No source type assigned
		/// </summary>
		None, 
		
		/// <summary>
		/// DVD source type
		/// </summary>
		Dvd, 
		
		/// <summary>
		/// Document Camera source type
		/// </summary>
		Laptop, 
		
		/// <summary>
		/// PC source type
		/// </summary>
		PC, 
		
		/// <summary>
		/// Set Top Box source type
		/// </summary>
		SetTopBox, 
		
		/// <summary>
		/// VCR source type
		/// </summary>
		VCR
	}
}