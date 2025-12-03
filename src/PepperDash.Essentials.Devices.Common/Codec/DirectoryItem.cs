

using System;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Codec
{
  /// <summary>
  /// Represents a DirectoryItem
  /// </summary>
  public class DirectoryItem : ICloneable
  {
    /// <summary>
    /// Clone method
    /// </summary>
    public object Clone()
    {
      return MemberwiseClone();
    }

    /// <summary>
    /// Gets or sets the FolderId
    /// </summary>
    [JsonProperty("folderId")]
    public string FolderId { get; set; }


    /// <summary>
    /// Gets or sets the Name
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the ParentFolderId
    /// </summary>
    [JsonProperty("parentFolderId")]
    public string ParentFolderId { get; set; }
  }
}