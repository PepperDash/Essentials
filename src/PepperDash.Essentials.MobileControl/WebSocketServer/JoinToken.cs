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

    /// <summary>
    /// Room Key this token is associated with
    /// </summary>
    public string RoomKey { get; set; }

    /// <summary>
    /// Unique ID for this token
    /// </summary>
    public string Uuid { get; set; }

    /// <summary>
    /// Touchpanel Key this token is associated with, if this is a touch panel token
    /// </summary>
    public string TouchpanelKey { get; set; } = "";

    /// <summary>
    /// Gets or sets the Token
    /// </summary>
    public string Token { get; set; } = null;
  }
}
