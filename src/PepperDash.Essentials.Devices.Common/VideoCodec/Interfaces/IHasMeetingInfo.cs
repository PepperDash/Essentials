

using System;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    /// <summary>
    /// Describes a device that provides meeting information (like a ZoomRoom)
    /// </summary>
    public interface IHasMeetingInfo
    {
        /// <summary>
        /// Raised when meeting info changes
        /// </summary>
        event EventHandler<MeetingInfoEventArgs> MeetingInfoChanged;

        /// <summary>
        /// Gets the current meeting information
        /// </summary>
        MeetingInfo MeetingInfo { get; }
    }
}