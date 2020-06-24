using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace PepperDash_Essentials_Core.Devices
{
    public class DestinationListItem
    {
        [JsonProperty("sinkYey")]
        public string SinkKey { get; set; }

        private EssentialsDevice _sinkDevice;

        public EssentialsDevice SinkDevice
        {
            get { return _sinkDevice ?? (_sinkDevice = DeviceManager.GetDeviceForKey(SinkKey) as EssentialsDevice); }
        }
    }
}