using System;

namespace PepperDash.Core
{
  /// <summary>
  /// The available settings for stream debugging
  /// </summary>
  [Flags]
  public enum eStreamDebuggingSetting
  {
    /// <summary>
    /// Debug off
    /// </summary>
    Off = 0,
    /// <summary>
    /// Debug received data
    /// </summary>
    Rx = 1,
    /// <summary>
    /// Debug transmitted data
    /// </summary>
    Tx = 2,
    /// <summary>
    /// Debug both received and transmitted data
    /// </summary>
    Both = Rx | Tx
  }
}
