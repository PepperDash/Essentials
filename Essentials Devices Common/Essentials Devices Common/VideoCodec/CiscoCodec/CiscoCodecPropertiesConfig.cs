using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    public class CiscoCodecPropertiesConfig
    {
        public CommunicationMonitorConfig CommunicationMonitorProperties { get; set; }

        //public ControlPropertiesConfig Control { get; set; }

        public List<CodecActiveCallItem> Favorites { get; set; }

        /// <summary>
        /// Valid values: "Local" or "Corporate"
        /// </summary>
        public string PhonebookMode { get; set; }

    }
}