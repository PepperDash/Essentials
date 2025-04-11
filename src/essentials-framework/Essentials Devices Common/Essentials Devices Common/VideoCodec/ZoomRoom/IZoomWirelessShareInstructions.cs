using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

using PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
    public class ShareInfoEventArgs : EventArgs
    {
        public zStatus.Sharing SharingStatus { get; private set; }

        public ShareInfoEventArgs(zStatus.Sharing status)
        {
            SharingStatus = status;
        }
    }

    public interface IZoomWirelessShareInstructions
    {
        event EventHandler<ShareInfoEventArgs> ShareInfoChanged;

        zStatus.Sharing SharingState { get; }
    }
}