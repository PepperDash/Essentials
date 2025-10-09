

using System;

namespace PepperDash.Essentials.Devices.Common.Codec
{
  /// <summary>
  /// Enumeration of eMeetingEventChangeType values
  /// </summary>
  [Flags]
  public enum eMeetingEventChangeType
  {
    /// <summary>
    /// Unknown change type
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// Meeting start warning
    /// </summary>
    MeetingStartWarning = 1,
    /// <summary>
    /// Meeting start
    /// </summary>
    MeetingStart = 2,
    /// <summary>
    /// Meeting end warning
    /// </summary>
    MeetingEndWarning = 4,
    /// <summary>
    /// Meeting end
    /// </summary>
    MeetingEnd = 8
  }

}
