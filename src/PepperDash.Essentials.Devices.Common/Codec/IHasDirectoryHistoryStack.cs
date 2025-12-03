using System.Collections.Generic;
using PepperDash.Essentials.Devices.Common.Codec;


/// <summary>
/// Defines the contract for IHasDirectoryHistoryStack
/// </summary>
public interface IHasDirectoryHistoryStack : IHasDirectory
{
  /// <summary>
  /// Gets the DirectoryBrowseHistoryStack
  /// </summary>
  Stack<CodecDirectory> DirectoryBrowseHistoryStack { get; }
}