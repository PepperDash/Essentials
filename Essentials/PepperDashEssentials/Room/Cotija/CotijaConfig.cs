using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core.Config;

using Newtonsoft.Json;

namespace PepperDash.Essentials
{

	/// <summary>
	/// 
	/// </summary>
    public class CotijaConfig
    {       
		[JsonProperty("serverUrl")]
        public string ServerUrl { get; set; }     
    }

	/// <summary>
	/// 
	/// </summary>
	public class CotijaDdvc01RoomBridgePropertiesConfig
	{
		[JsonProperty("eiscId")]
		public string EiscId { get; set; }
	}
}