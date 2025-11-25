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

    /// <summary>
    /// Gets or sets the Tokens for this server
    /// </summary>
    public Dictionary<string, JoinToken> Tokens { get; set; }

    /// <summary>
    /// Initialize a new instance of the <see cref="ServerTokenSecrets"/> class with the provided grant code
    /// </summary>
    /// <param name="grantCode">The grant code for this server</param>
    public ServerTokenSecrets(string grantCode)
    {
      GrantCode = grantCode;
      Tokens = new Dictionary<string, JoinToken>();
    }
  }
}
