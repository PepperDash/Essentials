using System.Collections.Generic;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Codec
{
  /// <summary>
  /// Represents a DirectoryContact
  /// </summary>
  public class DirectoryContact : DirectoryItem
  {

    /// <summary>
    /// Gets or sets the ContactId
    /// </summary>
    [JsonProperty("contactId")]
    public string ContactId { get; set; }

    /// <summary>
    /// Gets or sets the Title
    /// </summary>
    [JsonProperty("title")]
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the ContactMethods
    /// </summary>
    [JsonProperty("contactMethods")]
    public List<ContactMethod> ContactMethods { get; set; }

    /// <summary>
    /// Constructor for <see cref="DirectoryContact"/>
    /// </summary>
    public DirectoryContact()
    {
      ContactMethods = new List<ContactMethod>();
    }
  }
}