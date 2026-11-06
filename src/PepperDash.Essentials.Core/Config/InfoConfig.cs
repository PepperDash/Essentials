

using Crestron.SimplSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PdCore = PepperDash.Core.Abstractions;

namespace PepperDash.Essentials.Core.Config;

/// <summary>
/// Represents the info section of a Config file
/// </summary>
public class InfoConfig
	{
        /// <summary>
        /// Gets or sets the Name
        /// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Date
        /// </summary>
		[JsonProperty("date")]
		public DateTime Date { get; set; }
		
        /// <summary>
        /// Gets or sets the Type
        /// </summary>
		[JsonProperty("type")]
		public string Type { get; set; }
		
        /// <summary>
        /// Gets or sets the Version
        /// </summary>
		[JsonProperty("version")]
		public string Version { get; set; }

        /// <summary>
        /// Gets or sets the RuntimeInfo
        /// </summary>
        [JsonProperty("runtimeInfo")]
        public RuntimeInfo RuntimeInfo { get; set; } 
		
        /// <summary>
        /// Gets or sets the Comment
        /// </summary>
		[JsonProperty("comment")]
		public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the HostName
        /// </summary>
        [JsonProperty("hostname")]
        public string HostName { get; set; }

        /// <summary>
        /// Gets or sets the AppNumber
        /// </summary>
        [JsonProperty("appNumber")]
        public uint AppNumber { get; set; }

        /// <summary>
        /// InfoConfig Constructor
        /// </summary>
        /// <remarks>
        /// The app number comes from the <see cref="PdCore.ICrestronEnvironment"/> registered with
        /// <see cref="PdCore.DebugServiceRegistration"/>, and the host name is only read from the
        /// processor when that environment is real hardware (or none is registered). So a config can be
        /// created, or deserialized, off a processor - in a test, the host name is left empty.
        /// </remarks>
		public InfoConfig()
		{
			Name = "";
			Date = DateTime.Now;
			Type = "";
			Version = "";
			Comment = "";
        HostName = "";

        var environment = PdCore.DebugServiceRegistration.Environment;
        if (environment == null)
        {
            ReadFromProcessor();
        }
        else
        {
            AppNumber = environment.ApplicationNumber;
            if (environment.IsHardwareRuntime)
                HostName = ReadHostName();
        }

        RuntimeInfo = new RuntimeInfo();
		}

        // Kept out of the constructor, and never inlined, so constructing an InfoConfig off a processor
        // doesn't load the Crestron SDK classes these use.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ReadFromProcessor()
        {
            HostName = ReadHostName();
            AppNumber = InitialParametersClass.ApplicationNumber;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string ReadHostName() =>
            CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME, 0);
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