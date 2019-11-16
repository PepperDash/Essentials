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
    public class MobileControlConfig
    {       
		[JsonProperty("serverUrl")]
        public string ServerUrl { get; set; }

		[JsonProperty("clientAppUrl")]
		public string ClientAppUrl { get; set; }
    }

	/// <summary>
	/// 
	/// </summary>
	public class MobileControlDdvc01RoomBridgePropertiesConfig
	{
		[JsonProperty("eiscId")]
		public string EiscId { get; set; }
	}
}