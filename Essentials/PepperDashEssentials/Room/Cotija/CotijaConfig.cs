using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core.Config;

using Newtonsoft.Json;

namespace PepperDash.Essentials
{
    public class CotijaConfig : DeviceConfig
    {       
		[JsonProperty("serverUrl")]
        public string ServerUrl { get; set; }     
    }
}