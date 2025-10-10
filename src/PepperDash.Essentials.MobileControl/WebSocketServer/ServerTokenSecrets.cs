using System.Collections.Generic;


namespace PepperDash.Essentials.WebSocketServer
{
  /// <summary>
  /// Represents a ServerTokenSecrets
  /// </summary>
  public class ServerTokenSecrets
  {
    /// <summary>
    /// Gets or sets the GrantCode
    /// </summary>
    public string GrantCode { get; set; }

    public Dictionary<string, JoinToken> Tokens { get; set; }

    public ServerTokenSecrets(string grantCode)
    {
      GrantCode = grantCode;
      Tokens = new Dictionary<string, JoinToken>();
    }
  }
}
