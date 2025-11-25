using System;

namespace PepperDash.Essentials.WebSocketServer
{
  /// <summary>
  /// Event Args for <see cref="UiClient"/> ConnectionClosed event 
  /// </summary>
  public class ConnectionClosedEventArgs : EventArgs
  {
    /// <summary>
    /// Client ID that is being closed
    /// </summary>
    public string ClientId { get; private set; }

    /// <summary>
    /// Initialize an instance of the <see cref="ConnectionClosedEventArgs"/> class.
    /// </summary>
    /// <param name="clientId">client that's closing</param>
    public ConnectionClosedEventArgs(string clientId)
    {
      ClientId = clientId;
    }
  }
}
