

using System;

namespace PepperDash.Essentials.Devices.Common.Codec
{
  /// <summary>
  /// Represents a DirectoryEventArgs
  /// </summary>
  public class DirectoryEventArgs : EventArgs
  {
    /// <summary>
    /// Gets or sets the Directory
    /// </summary>
    public CodecDirectory Directory { get; set; }
    /// <summary>
    /// Gets or sets the DirectoryIsOnRoot
    /// </summary>
    public bool DirectoryIsOnRoot { get; set; }
  }
}