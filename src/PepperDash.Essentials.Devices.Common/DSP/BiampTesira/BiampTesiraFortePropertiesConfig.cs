using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.DSP
{
	/// <summary>
	/// 
	/// </summary>
	public class BiampTesiraFortePropertiesConfig
	{
		public CommunicationMonitorConfig CommunicationMonitorProperties { get; set; }

		public ControlPropertiesConfig Control { get; set; }

		/// <summary>
		/// These are key-value pairs, string id, string type.  
		/// Valid types are level and mute.
        /// Need to include the index values somehow
		/// </summary>
		public Dictionary<string, BiampTesiraForteLevelControlBlockConfig> LevelControlBlocks { get; set; }
       // public Dictionary<string, BiampTesiraForteDialerControlBlockConfig> DialerControlBlocks {get; set;}
	}
}