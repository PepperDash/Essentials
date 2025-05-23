﻿using Newtonsoft.Json;

namespace PepperDash.Essentials
{
    public class UserCodeChangedContent
    {
        [JsonProperty("userCode")]
        public string UserCode { get; set; }

        [JsonProperty("qrChecksum", NullValueHandling = NullValueHandling.Include)]
        public string QrChecksum { get; set; }
    }
}
