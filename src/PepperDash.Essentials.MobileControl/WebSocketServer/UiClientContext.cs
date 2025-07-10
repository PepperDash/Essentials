namespace PepperDash.Essentials.WebSocketServer
{
  /// <summary>
  /// Represents an instance of a UiClient and the associated Token
  /// </summary>
  public class UiClientContext
  {
    public UiClient Client { get; private set; }
    public JoinToken Token { get; private set; }

    public UiClientContext(JoinToken token)
    {
      Token = token;
    }

    public void SetClient(UiClient client)
    {
      Client = client;
    }

  }
}
