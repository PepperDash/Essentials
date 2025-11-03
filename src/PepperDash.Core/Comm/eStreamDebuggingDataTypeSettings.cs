using System;

namespace PepperDash.Core
{
  /// <summary>
  /// The available settings for stream debugging data format types
  /// </summary>
  [Flags]
  public enum eStreamDebuggingDataTypeSettings
  {
    /// <summary>
    /// Debug data in byte format
    /// </summary>
    Bytes = 0,
    /// <summary>
    /// Debug data in text format
    /// </summary>
    Text = 1,
    /// <summary>
    /// Debug data in both byte and text formats
    /// </summary>
    Both = Bytes | Text
  }
}
