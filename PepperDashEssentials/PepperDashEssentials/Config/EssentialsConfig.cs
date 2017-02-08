using System;
using System.Collections.Generic;
using Crestron.SimplSharp.CrestronIO;

using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials
{
	/// <summary>
	/// Loads the ConfigObject from the file
	/// </summary>
	public class EssentialsConfig : BasicConfig
	{
		[JsonProperty("rooms")]
		public List<EssentialsRoomConfig> Rooms { get; private set; }
	}

	/// <summary>
	/// 
	/// </summary>
	public class SystemTemplateConfigs
	{
		public EssentialsConfig System { get; set; }

		public EssentialsConfig Template { get; set; }
	}
}