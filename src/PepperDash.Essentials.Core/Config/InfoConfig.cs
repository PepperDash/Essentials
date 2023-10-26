extern alias Full;

using System;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;

using Full.Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Config
{
	/// <summary>
	/// Represents the info section of a Config file
	/// </summary>
	public class InfoConfig
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("date")]
		public DateTime Date { get; set; }
		
		[JsonProperty("type")]
		public string Type { get; set; }
		
		[JsonProperty("version")]
		public string Version { get; set; }

        [JsonProperty("runtimeInfo")]
        public RuntimeInfo RuntimeInfo { get; set; } 
		
		[JsonProperty("comment")]
		public string Comment { get; set; }

        [JsonProperty("hostname")]
        public string HostName { get; set; }

        [JsonProperty("appNumber")]
        public uint AppNumber { get; set; }

		public InfoConfig()
		{
			Name = "";
			Date = DateTime.Now;
			Type = "";
			Version = "";
			Comment = "";
            HostName = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME, 0);
            AppNumber = InitialParametersClass.ApplicationNumber;

            RuntimeInfo = new RuntimeInfo();
		}
	}
}