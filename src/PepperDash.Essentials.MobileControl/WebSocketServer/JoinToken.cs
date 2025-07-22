using Independentsoft.Exchange;

namespace PepperDash.Essentials.WebSocketServer
{
  /// <summary>
  /// Represents a join token with the associated properties
  /// </summary>
  public class JoinToken
  {
    public string Code { get; set; }

    public string RoomKey { get; set; }

    public string Uuid { get; set; }

    public string TouchpanelKey { get; set; } = "";

    public string Token { get; set; } = null;

    public string Id { get; set; }
  }
}
