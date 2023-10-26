using PepperDash.Essentials.Devices.Common.Cameras;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
    public class ZoomRoomFarEndCamera : ZoomRoomCamera, IAmFarEndCamera
    {

        public ZoomRoomFarEndCamera(string key, string name, ZoomRoom codec, int id)
            : base(key, name, codec)
        {
            Id = id;
        }

    }
}