using System;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    public interface IHasCameraMuteWithUnmuteReqeust : IHasCameraMute
    {
        event EventHandler VideoUnmuteRequested;
    }
}