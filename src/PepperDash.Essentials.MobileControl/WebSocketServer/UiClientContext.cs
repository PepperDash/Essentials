namespace PepperDash.Essentials.WebSocketServer
{
  /// <summary>
  /// Represents a UiClientContext
  /// </summary>
  public class UiClientContext
  {
    /// <summary>
    /// Gets or sets the Client
    /// </summary>
    public UiClient Client { get; private set; }
    /// <summary>
    /// Gets or sets the Token
    /// </summary>
    public JoinToken Token { get; private set; }

    /// <summary>
    /// Initialize an instance of the <see cref="UiClientContext"/> class with the provided token
    /// </summary>
    /// <param name="token">token for this client</param>
    public UiClientContext(JoinToken token)
    {
      Token = token;
    }

    /// <summary>
    /// SetClient method
    /// </summary>
    public void SetClient(UiClient client)
    {
      Client = client;
    }

  }
}
