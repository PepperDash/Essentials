extern alias Full;
using System.Collections.Generic;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Config
{
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