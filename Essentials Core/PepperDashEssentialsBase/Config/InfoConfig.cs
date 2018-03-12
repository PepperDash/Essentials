using System;

using Crestron.SimplSharp.Reflection;

using Newtonsoft.Json;

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

		public InfoConfig()
		{
			Name = "";
			Date = DateTime.Now;
			Type = "";
			Version = "";
			Comment = "";

            RuntimeInfo = new RuntimeInfo();
		}
	}

    
    /// <summary>
    /// Represents runtime information about the program/processor
    /// </summary>
    public class RuntimeInfo
    {
        /// <summary>
        /// The name of the running application
        /// </summary>
        [JsonProperty("appName")]
        public string AppName { get; set; } 

        /// <summary>
        /// The Assembly version of the running application
        /// </summary>
        [JsonProperty("assemblyVersion")]
        public string AssemblyVersion { get; set; } 

        /// <summary>
        /// The OS Version of the processor (Firmware Version)
        /// </summary>
        [JsonProperty("osVersion")]
        public string OsVersion { get; set; } 
    }
}