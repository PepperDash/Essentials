using System.Collections.Generic;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
    /// <summary>
    /// Used to track the current status of a ZoomRoom
    /// </summary>
    public class ZoomRoomStatus
    {
        public zStatus.Login Login { get; set; }
        public zStatus.SystemUnit SystemUnit { get; set; }
        public zStatus.Phonebook Phonebook { get; set; }
        public zStatus.Call Call { get; set; }
        public zStatus.Capabilities Capabilities { get; set; }
        public zStatus.Sharing Sharing { get; set; }
        public zStatus.NumberOfScreens NumberOfScreens { get; set; }
        public zStatus.Layout Layout { get; set; }
        public zStatus.Video Video { get; set; }
        public zStatus.CameraShare CameraShare { get; set; }
        public List<zStatus.AudioVideoInputOutputLineItem> AudioInputs { get; set; }
        public List<zStatus.AudioVideoInputOutputLineItem> AudioOuputs { get; set; }
        public List<zStatus.AudioVideoInputOutputLineItem> Cameras { get; set; }
        public zEvent.PhoneCallStatus PhoneCall { get; set; }
        public zEvent.NeedWaitForHost NeedWaitForHost { get; set; }

        public ZoomRoomStatus()
        {
            Login           = new zStatus.Login();
            SystemUnit      = new zStatus.SystemUnit();
            Phonebook       = new zStatus.Phonebook();
            Call            = new zStatus.Call();
            Capabilities    = new zStatus.Capabilities();
            Sharing         = new zStatus.Sharing();
            NumberOfScreens = new zStatus.NumberOfScreens();
            Layout          = new zStatus.Layout();
            Video           = new zStatus.Video();
            CameraShare     = new zStatus.CameraShare();
            AudioInputs     = new List<zStatus.AudioVideoInputOutputLineItem>();
            AudioOuputs     = new List<zStatus.AudioVideoInputOutputLineItem>();
            Cameras         = new List<zStatus.AudioVideoInputOutputLineItem>();
            PhoneCall       = new zEvent.PhoneCallStatus();
            NeedWaitForHost = new zEvent.NeedWaitForHost();
        }
    }
}