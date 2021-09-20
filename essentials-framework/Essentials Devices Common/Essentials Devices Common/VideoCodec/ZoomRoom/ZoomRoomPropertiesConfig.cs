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
        public bool SupportsCameraAutoMode { get; set; }
        public bool SupportsCameraOff { get; set; }

        //if true, the layouts will be set automatically when sharing starts/ends or a call is joined
        public bool AutoDefaultLayouts { get; set; }

        /* This layout will be selected when Sharing starts (either from Far end or locally)*/
        public string DefaultSharingLayout { get; set; }

        //This layout will be selected when a call is connected and no content is being shared
        public string DefaultCallLayout { get; set; }

        public int MinutesBeforeMeetingStart { get; set; }
    }
}