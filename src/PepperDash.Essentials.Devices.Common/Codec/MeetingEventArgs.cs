

using System;

namespace PepperDash.Essentials.Devices.Common.Codec
{
  /// <summary>
  /// Represents a MeetingEventArgs
  /// </summary>
  public class MeetingEventArgs : EventArgs
  {
    /// <summary>
    /// Gets or sets the ChangeType
    /// </summary>
    public eMeetingEventChangeType ChangeType { get; set; }
    /// <summary>
    /// Gets or sets the Meeting
    /// </summary>
    public Meeting Meeting { get; set; }
  }

}
