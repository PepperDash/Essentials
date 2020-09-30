using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
    public class ZoomRoomPropertiesConfig
    {
        public CommunicationMonitorConfig CommunicationMonitorProperties { get; set; }

        public bool DisablePhonebookAutoDownload { get; set; }
    }
}