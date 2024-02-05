

using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
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

    
    /// <summary>
    /// Represents runtime information about the program/processor
    /// </summary>
    public class RuntimeInfo
    {
        /// <summary>
        /// The name of the running application
        /// </summary>
        [JsonProperty("appName")]
        public string AppName {get; set;}
        //{
        //    get
        //    {
        //        return Assembly.GetExecutingAssembly().GetName().Name;
        //    }
        //} 

        /// <summary>
        /// The Assembly version of the running application
        /// </summary>
        [JsonProperty("assemblyVersion")]
        public string AssemblyVersion {get; set;}
        //{
        //    get
        //    {
        //        var version = Assembly.GetExecutingAssembly().GetName().Version;
        //        return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
        //    }
        //} 

        /// <summary>, 
        /// The OS Version of the processor (Firmware Version)
        /// </summary>
        [JsonProperty("osVersion")]
        public string OsVersion {get; set;}
		//{
		//    get
		//    {
		//        return Crestron.SimplSharp.CrestronEnvironment.OSVersion.Firmware;
		//    }
		//} 

        /// <summary>
        /// The information gathered by the processor at runtime about it's NICs and their IP addresses.   
        /// </summary>
        [JsonProperty("ipInfo")]
        public Dictionary<short, EthernetAdapterInfo> IpInfo
        {
            get
            {
                return Global.EthernetAdapterInfoCollection;
            }
        }
    }

}