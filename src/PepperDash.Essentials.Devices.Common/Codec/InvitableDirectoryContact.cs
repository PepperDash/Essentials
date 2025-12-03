using Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Codec
{
  /// <summary>
  /// Represents an InvitableDirectoryContact
  /// </summary>
  public class InvitableDirectoryContact : DirectoryContact, IInvitableContact
  {
    /// <summary>
    /// Gets a value indicating whether this contact is invitable
    /// </summary>
    [JsonProperty("isInvitableContact")]
    public bool IsInvitableContact
    {
      get
      {
        return this is IInvitableContact;
      }
    }
  }
}