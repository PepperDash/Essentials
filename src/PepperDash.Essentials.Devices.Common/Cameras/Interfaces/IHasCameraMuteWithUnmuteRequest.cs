using System;


namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Interface for devices that can mute and unmute their camera video, with an event for unmute requests
    /// </summary>
    public interface IHasCameraMuteWithUnmuteReqeust : IHasCameraMute
    {
        /// <summary>
        /// Event that is raised when a video unmute is requested, typically by the far end
        /// </summary>
        event EventHandler VideoUnmuteRequested;
    }
}
