using System.Collections.Generic;


namespace PepperDash.Essentials.WebSocketServer
{
  /// <summary>
  /// Represents the data structure for the grant code and UiClient tokens to be stored in the secrets manager
  /// </summary>
  public class ServerTokenSecrets
  {
    public string GrantCode { get; set; }

    public Dictionary<string, JoinToken> Tokens { get; set; }

    public ServerTokenSecrets(string grantCode)
    {
      GrantCode = grantCode;
      Tokens = new Dictionary<string, JoinToken>();
    }
  }
}
