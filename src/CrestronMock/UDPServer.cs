using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Crestron.SimplSharp.CrestronSockets
{
  /// <summary>Mock UDPServer class for UDP communication</summary>
  public class UDPServer : IDisposable
  {
    private UdpClient? _udpClient;
    private bool _listening;
    private readonly object _lockObject = new object();
    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>Event fired when data is received</summary>
    public event UDPServerReceiveDataEventHandler? ReceivedData;

    /// <summary>Gets the server state</summary>
    public SocketServerState State { get; private set; } = SocketServerState.SERVER_NOT_LISTENING;

    /// <summary>Gets the port number</summary>
    public int PortNumber { get; private set; }

    /// <summary>Gets the buffer size</summary>
    public int BufferSize { get; private set; }

    /// <summary>Initializes a new instance of UDPServer</summary>
    /// <param name="ipAddress">IP address to bind to</param>
    /// <param name="portNumber">Port number to listen on</param>
    /// <param name="bufferSize">Buffer size for data reception</param>
    /// <param name="ethernetAdapterToBindTo">Ethernet adapter to bind to</param>
    public UDPServer(string ipAddress, int portNumber, int bufferSize, EthernetAdapterType ethernetAdapterToBindTo)
    {
      PortNumber = portNumber;
      BufferSize = bufferSize;
    }

    /// <summary>Initializes a new instance of UDPServer</summary>
    /// <param name="portNumber">Port number to listen on</param>
    /// <param name="bufferSize">Buffer size for data reception</param>
    public UDPServer(int portNumber, int bufferSize)
    {
      PortNumber = portNumber;
      BufferSize = bufferSize;
    }

    /// <summary>Starts listening for UDP packets</summary>
    /// <returns>SocketErrorCodes indicating success or failure</returns>
    public SocketErrorCodes EnableUDPServer()
    {
      if (_listening)
        return SocketErrorCodes.SOCKET_OPERATION_PENDING;

      try
      {
        _udpClient = new UdpClient(PortNumber);
        _listening = true;
        State = SocketServerState.SERVER_LISTENING;
        _cancellationTokenSource = new CancellationTokenSource();

        _ = Task.Run(() => ReceiveDataAsync(_cancellationTokenSource.Token));

        return SocketErrorCodes.SOCKET_OK;
      }
      catch (Exception)
      {
        State = SocketServerState.SERVER_NOT_LISTENING;
        return SocketErrorCodes.SOCKET_CONNECTION_FAILED;
      }
    }

    /// <summary>Stops listening for UDP packets</summary>
    /// <returns>SocketErrorCodes indicating success or failure</returns>
    public SocketErrorCodes DisableUDPServer()
    {
      if (!_listening)
        return SocketErrorCodes.SOCKET_NOT_CONNECTED;

      try
      {
        _listening = false;
        _cancellationTokenSource?.Cancel();
        _udpClient?.Close();
        State = SocketServerState.SERVER_NOT_LISTENING;

        return SocketErrorCodes.SOCKET_OK;
      }
      catch (Exception)
      {
        return SocketErrorCodes.SOCKET_CONNECTION_FAILED;
      }
    }

    /// <summary>Sends data to a specific endpoint</summary>
    /// <param name="data">Data to send</param>
    /// <param name="dataLength">Length of data</param>
    /// <param name="ipAddress">Target IP address</param>
    /// <param name="portNumber">Target port number</param>
    /// <returns>SocketErrorCodes indicating success or failure</returns>
    public SocketErrorCodes SendData(byte[] data, int dataLength, string ipAddress, int portNumber)
    {
      if (!_listening || _udpClient == null)
        return SocketErrorCodes.SOCKET_NOT_CONNECTED;

      try
      {
        var endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), portNumber);
        _udpClient.Send(data, dataLength, endpoint);
        return SocketErrorCodes.SOCKET_OK;
      }
      catch (Exception)
      {
        return SocketErrorCodes.SOCKET_CONNECTION_FAILED;
      }
    }

    /// <summary>Sends data to a specific endpoint</summary>
    /// <param name="data">Data to send</param>
    /// <param name="dataLength">Length of data</param>
    /// <param name="endpoint">Target endpoint</param>
    /// <returns>SocketErrorCodes indicating success or failure</returns>
    public SocketErrorCodes SendData(byte[] data, int dataLength, IPEndPoint endpoint)
    {
      if (!_listening || _udpClient == null)
        return SocketErrorCodes.SOCKET_NOT_CONNECTED;

      try
      {
        _udpClient.Send(data, dataLength, endpoint);
        return SocketErrorCodes.SOCKET_OK;
      }
      catch (Exception)
      {
        return SocketErrorCodes.SOCKET_CONNECTION_FAILED;
      }
    }

    private async Task ReceiveDataAsync(CancellationToken cancellationToken)
    {
      while (_listening && _udpClient != null && !cancellationToken.IsCancellationRequested)
      {
        try
        {
          var result = await _udpClient.ReceiveAsync();

          var args = new UDPServerReceiveDataEventArgs(
              result.Buffer,
              result.Buffer.Length,
              result.RemoteEndPoint.Address.ToString(),
              result.RemoteEndPoint.Port);

          ReceivedData?.Invoke(this, args);
        }
        catch (ObjectDisposedException)
        {
          // UDP client was disposed
          break;
        }
        catch (Exception)
        {
          // Handle other exceptions
          continue;
        }
      }
    }

    /// <summary>Disposes the UDPServer</summary>
    public void Dispose()
    {
      DisableUDPServer();
      _cancellationTokenSource?.Dispose();
      _udpClient?.Dispose();
    }
  }

  /// <summary>Mock SecureTCPClient class for secure TCP client operations</summary>
  public class SecureTCPClient : TCPClient
  {
    /// <summary>Initializes a new instance of SecureTCPClient</summary>
    /// <param name="ipAddress">Server IP address</param>
    /// <param name="portNumber">Server port number</param>
    /// <param name="bufferSize">Buffer size for data reception</param>
    /// <param name="ethernetAdapterToBindTo">Ethernet adapter to bind to</param>
    public SecureTCPClient(string ipAddress, int portNumber, int bufferSize, EthernetAdapterType ethernetAdapterToBindTo)
        : base(ipAddress, portNumber, bufferSize, ethernetAdapterToBindTo)
    {
    }

    /// <summary>Initializes a new instance of SecureTCPClient</summary>
    /// <param name="ipAddress">Server IP address</param>
    /// <param name="portNumber">Server port number</param>
    /// <param name="bufferSize">Buffer size for data reception</param>
    public SecureTCPClient(string ipAddress, int portNumber, int bufferSize)
        : base(ipAddress, portNumber, bufferSize)
    {
    }

    /// <summary>Sets the SSL/TLS settings (mock implementation)</summary>
    /// <param name="context">SSL context</param>
    public void SetSSLContext(object context)
    {
      // Mock implementation - does nothing in test environment
    }

    /// <summary>Validates server certificate (mock implementation)</summary>
    /// <param name="certificate">Server certificate</param>
    /// <returns>Always returns true in mock implementation</returns>
    public bool ValidateServerCertificate(object certificate)
    {
      // Mock implementation - always accept certificate
      return true;
    }
  }

  // Event handler delegates for UDP
  public delegate void UDPServerReceiveDataEventHandler(UDPServer server, UDPServerReceiveDataEventArgs args);

  // Event argument classes for UDP
  public class UDPServerReceiveDataEventArgs : EventArgs
  {
    /// <summary>Gets the received data</summary>
    public byte[] Data { get; }

    /// <summary>Gets the length of received data</summary>
    public int DataLength { get; }

    /// <summary>Gets the sender's IP address</summary>
    public string IPAddress { get; }

    /// <summary>Gets the sender's port number</summary>
    public int Port { get; }

    /// <summary>Initializes a new instance of UDPServerReceiveDataEventArgs</summary>
    /// <param name="data">Received data</param>
    /// <param name="dataLength">Length of received data</param>
    /// <param name="ipAddress">Sender's IP address</param>
    /// <param name="port">Sender's port number</param>
    public UDPServerReceiveDataEventArgs(byte[] data, int dataLength, string ipAddress, int port)
    {
      Data = data;
      DataLength = dataLength;
      IPAddress = ipAddress;
      Port = port;
    }
  }
}
