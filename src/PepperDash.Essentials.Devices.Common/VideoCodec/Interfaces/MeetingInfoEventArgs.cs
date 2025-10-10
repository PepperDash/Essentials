

using System;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
  /// <summary>
  /// Represents a MeetingInfoEventArgs
  /// </summary>
  public class MeetingInfoEventArgs : EventArgs
  {
    /// <summary>
    /// Gets or sets the Info
    /// </summary>
    public MeetingInfo Info { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MeetingInfoEventArgs"/> class.
    /// </summary>
    /// <param name="info">The meeting information.</param>
    public MeetingInfoEventArgs(MeetingInfo info)
    {
      Info = info;
    }

  }
}