using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    public class CiscoSparkCodecPropertiesConfig
    {
        public CommunicationMonitorConfig CommunicationMonitorProperties { get; set; }

        public List<CodecActiveCallItem> Favorites { get; set; }

        /// <summary>
        /// Valid values: "Local" or "Corporate"
        /// </summary>
        public string PhonebookMode { get; set; }

        public bool ShowSelfViewByDefault { get; set; }

        public SharingProperties Sharing { get; set; }

    }

    public class SharingProperties
    {
        public bool AutoShareContentWhileInCall { get; set; }
    }
}