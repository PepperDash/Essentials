using System;

namespace Crestron.SimplSharp.CrestronSockets
{
  /// <summary>Mock implementation of Crestron SecureTCPClient for testing purposes</summary>
  public class SecureTCPClient : TCPClient
  {
    /// <summary>Initializes a new instance of the SecureTCPClient class</summary>
    /// <param name="addressToConnectTo">IP address to connect to</param>
    /// <param name="portNumber">Port number to connect to</param>
    /// <param name="bufferSize">Size of the receive buffer</param>
    public SecureTCPClient(string addressToConnectTo, int portNumber, int bufferSize)
      : base(addressToConnectTo, portNumber, bufferSize)
    {
    }

    /// <summary>Gets or sets whether to verify the host certificate</summary>
    public bool HostVerification { get; set; } = true;

    /// <summary>Gets or sets whether to verify the peer certificate</summary>
    public bool PeerVerification { get; set; } = true;

    /// <summary>Resets the client connection</summary>
    /// <param name="connectionFlag">Connection flag</param>
    public void Reset(int connectionFlag)
    {
      // Mock implementation
      DisconnectFromServer();
    }
  }
}
