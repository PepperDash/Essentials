using System.Collections.Generic;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Codec
{
  /// <summary>
  /// Represents a DirectoryFolder
  /// </summary>
  public class DirectoryFolder : DirectoryItem
  {

    /// <summary>
    /// Gets or sets the Contacts
    /// </summary>
    [JsonProperty("contacts")]
    public List<DirectoryContact> Contacts { get; set; }

    /// <summary>
    /// Constructor for <see cref="DirectoryFolder"/>
    /// </summary>
    public DirectoryFolder()
    {
      Contacts = new List<DirectoryContact>();
    }
  }
}