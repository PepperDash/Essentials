namespace PepperDash.Essentials.WebSocketServer
{
  /// <summary>
  /// Represents a JoinToken
  /// </summary>
  public class JoinToken
  {
    /// <summary>
    /// Unique client ID for a client that is joining
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// Gets or sets the Code
    /// </summary>
    public string Code { get; set; }

    public string RoomKey { get; set; }

    public string Uuid { get; set; }

    public string TouchpanelKey { get; set; } = "";

    /// <summary>
    /// Gets or sets the Token
    /// </summary>
    public string Token { get; set; } = null;
  }
}
