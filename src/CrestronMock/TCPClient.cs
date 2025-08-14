using System;

namespace Crestron.SimplSharp.CrestronSockets
{
  /// <summary>Mock SocketStatus enumeration</summary>
  public enum SocketStatus
  {
    /// <summary>Socket is connecting</summary>
    SOCKET_STATUS_WAITING = 1,
    /// <summary>Socket is connected</summary>
    SOCKET_STATUS_CONNECTED = 2,
    /// <summary>Socket is not connected</summary>
    SOCKET_STATUS_NOT_CONNECTED = 3,
    /// <summary>Connection broken</summary>
    SOCKET_STATUS_BROKEN_REMOTELY = 4,
    /// <summary>Connection broken locally</summary>
    SOCKET_STATUS_BROKEN_LOCALLY = 5,
    /// <summary>DNS resolution failed</summary>
    SOCKET_STATUS_DNS_RESOLUTION_FAILED = 6,
    /// <summary>Connection failed</summary>
    SOCKET_STATUS_CONNECT_FAILED = 7,
    /// <summary>Socket error</summary>
    SOCKET_STATUS_SOCKET_ERROR = 8,
    /// <summary>Secure connection failed</summary>
    SOCKET_STATUS_SSL_FAILED = 9
  }

  /// <summary>Mock ServerState enumeration</summary>
  public enum ServerState
  {
    /// <summary>Server is not listening</summary>
    SERVER_NOT_LISTENING = 0,
    /// <summary>Server is listening</summary>
    SERVER_LISTENING = 1
  }

  /// <summary>Mock event handler for TCP client status changes</summary>
  /// <param name="client">The TCP client</param>
  /// <param name="clientSocketStatus">The socket status</param>
  public delegate void TCPClientSocketStatusChangeEventHandler(TCPClient client, SocketStatus clientSocketStatus);

  /// <summary>Delegate for TCP client connect callback</summary>
  /// <param name="client">TCP client instance</param>
  public delegate void TCPClientConnectCallback(TCPClient client);

  /// <summary>Mock event handler for receiving TCP client data</summary>
  /// <param name="client">The TCP client</param>
  /// <param name="numberOfBytesReceived">Number of bytes received</param>
  public delegate void TCPClientReceiveEventHandler(TCPClient client, int numberOfBytesReceived);

  /// <summary>
  /// Mock implementation of Crestron TCPClient for testing purposes
  /// Provides the same public API surface as the real TCPClient
  /// </summary>
  public class TCPClient : IDisposable
  {
    #region Events

    /// <summary>Event fired when socket status changes</summary>
    public event TCPClientSocketStatusChangeEventHandler? SocketStatusChange;

    /// <summary>Event fired when data is received</summary>
    public event TCPClientReceiveEventHandler? DataReceived;

    #endregion

    #region Properties

    /// <summary>Gets the client socket status</summary>
    public SocketStatus ClientStatus { get; private set; } = SocketStatus.SOCKET_STATUS_NOT_CONNECTED;

    /// <summary>Gets or sets the address to connect to</summary>
    public string AddressToConnectTo { get; set; } = string.Empty;

    /// <summary>Gets or sets the port number to connect to</summary>
    public int PortNumber { get; set; } = 0;

    /// <summary>Gets the number of bytes received in the incoming data buffer</summary>
    public int IncomingDataBufferSize { get; private set; } = 0;

    /// <summary>Gets or sets the socket send timeout in milliseconds</summary>
    public int SocketSendTimeout { get; set; } = 30000;

    /// <summary>Gets or sets the socket receive timeout in milliseconds</summary>
    public int SocketReceiveTimeout { get; set; } = 30000;

    /// <summary>Gets or sets whether to keep the connection alive</summary>
    public bool KeepAlive { get; set; } = false;

    /// <summary>Gets or sets whether Nagle algorithm is enabled</summary>
    public bool EnableNagle { get; set; } = true;

    /// <summary>Gets the number of bytes available to read</summary>
    public int BytesAvailable => IncomingDataBufferSize;

    /// <summary>Gets or sets the socket send or receive timeout in milliseconds</summary>
    public int SocketSendOrReceiveTimeOutInMs
    {
      get => SocketSendTimeout;
      set => SocketSendTimeout = SocketReceiveTimeout = value;
    }

    /// <summary>Gets the address the client is connected to</summary>
    public string AddressClientConnectedTo { get; private set; } = string.Empty;

    /// <summary>Gets the incoming data buffer</summary>
    public byte[] IncomingDataBuffer { get; private set; } = new byte[0];

    #endregion

    #region Constructor

    /// <summary>Initializes a new instance of the TCPClient class</summary>
    /// <param name="addressToConnectTo">IP address to connect to</param>
    /// <param name="portNumber">Port number to connect to</param>
    /// <param name="bufferSize">Size of the receive buffer</param>
    public TCPClient(string addressToConnectTo, int portNumber, int bufferSize)
    {
      AddressToConnectTo = addressToConnectTo;
      PortNumber = portNumber;
      _bufferSize = bufferSize;
      _receiveBuffer = new byte[bufferSize];
    }

    /// <summary>Initializes a new instance of the TCPClient class</summary>
    /// <param name="addressToConnectTo">IP address to connect to</param>
    /// <param name="portNumber">Port number to connect to</param>
    /// <param name="bufferSize">Size of the receive buffer</param>
    /// <param name="ethernetAdapterToBindTo">Ethernet adapter to bind to</param>
    public TCPClient(string addressToConnectTo, int portNumber, int bufferSize, EthernetAdapterType ethernetAdapterToBindTo)
    {
      AddressToConnectTo = addressToConnectTo;
      PortNumber = portNumber;
      _bufferSize = bufferSize;
      _receiveBuffer = new byte[bufferSize];
      // Note: EthernetAdapterType is ignored in mock implementation
    }

    #endregion

    #region Private Fields

    private readonly int _bufferSize;
    private readonly byte[] _receiveBuffer;
    private bool _disposed = false;

    #endregion

    #region Public Methods

    /// <summary>Connects to the remote endpoint asynchronously</summary>
    /// <returns>Status of the connection attempt</returns>
    public SocketStatus ConnectToServerAsync()
    {
      if (_disposed) return SocketStatus.SOCKET_STATUS_SOCKET_ERROR;

      // Mock connection - simulate successful connection
      ClientStatus = SocketStatus.SOCKET_STATUS_CONNECTED;
      AddressClientConnectedTo = AddressToConnectTo;
      SocketStatusChange?.Invoke(this, ClientStatus);
      return ClientStatus;
    }

    /// <summary>Connects to the remote endpoint asynchronously with callback</summary>
    /// <param name="callback">Callback to invoke when connection completes</param>
    /// <returns>Status of the connection attempt</returns>
    public SocketStatus ConnectToServerAsync(TCPClientConnectCallback callback)
    {
      var status = ConnectToServerAsync();
      callback?.Invoke(this);
      return status;
    }

    /// <summary>Connects to the remote endpoint</summary>
    /// <returns>Status of the connection attempt</returns>
    public SocketStatus ConnectToServer()
    {
      return ConnectToServerAsync();
    }

    /// <summary>Disconnects from the remote endpoint</summary>
    /// <returns>Status of the disconnection</returns>
    public SocketStatus DisconnectFromServer()
    {
      if (_disposed) return SocketStatus.SOCKET_STATUS_SOCKET_ERROR;

      ClientStatus = SocketStatus.SOCKET_STATUS_NOT_CONNECTED;
      SocketStatusChange?.Invoke(this, ClientStatus);
      return ClientStatus;
    }

    /// <summary>Sends data to the connected server</summary>
    /// <param name="dataToSend">Data to send as a string</param>
    /// <returns>Number of bytes sent, or -1 on error</returns>
    public int SendData(string dataToSend)
    {
      if (_disposed || string.IsNullOrEmpty(dataToSend)) return -1;
      if (ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED) return -1;

      // Mock send - return the length of the string as bytes sent
      return System.Text.Encoding.UTF8.GetByteCount(dataToSend);
    }

    /// <summary>Sends data to the connected server</summary>
    /// <param name="dataToSend">Data to send as byte array</param>
    /// <param name="lengthToSend">Number of bytes to send</param>
    /// <returns>Number of bytes sent, or -1 on error</returns>
    public int SendData(byte[] dataToSend, int lengthToSend)
    {
      if (_disposed || dataToSend == null || lengthToSend <= 0) return -1;
      if (ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED) return -1;
      if (lengthToSend > dataToSend.Length) return -1;

      // Mock send - return the requested length
      return lengthToSend;
    }

    /// <summary>Receives data from the server</summary>
    /// <param name="buffer">Buffer to receive data into</param>
    /// <param name="bufferIndex">Starting index in the buffer</param>
    /// <param name="lengthToReceive">Maximum number of bytes to receive</param>
    /// <returns>Number of bytes received, or -1 on error</returns>
    public int ReceiveData(byte[] buffer, int bufferIndex, int lengthToReceive)
    {
      if (_disposed || buffer == null || bufferIndex < 0 || lengthToReceive <= 0) return -1;
      if (ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED) return -1;
      if (bufferIndex + lengthToReceive > buffer.Length) return -1;

      // Mock receive - simulate no data available for now
      return 0;
    }

    /// <summary>Receives data from the server as a string</summary>
    /// <param name="numberOfBytesToReceive">Maximum number of bytes to receive</param>
    /// <returns>Received data as string, or empty string on error</returns>
    public string ReceiveData(int numberOfBytesToReceive)
    {
      if (_disposed || numberOfBytesToReceive <= 0) return string.Empty;
      if (ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED) return string.Empty;

      // Mock receive - return empty string (no data available)
      return string.Empty;
    }

    /// <summary>Sends data to the connected server asynchronously</summary>
    /// <param name="dataToSend">Data to send as byte array</param>
    /// <param name="lengthToSend">Number of bytes to send</param>
    /// <returns>Number of bytes sent, or -1 on error</returns>
    public int SendDataAsync(byte[] dataToSend, int lengthToSend)
    {
      return SendData(dataToSend, lengthToSend);
    }

    /// <summary>Receives data from the server asynchronously</summary>
    /// <returns>Number of bytes received, or -1 on error</returns>
    public int ReceiveDataAsync()
    {
      if (_disposed) return -1;
      if (ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED) return -1;

      // Mock receive - simulate no data available
      return 0;
    }

    /// <summary>Simulates receiving data (for testing purposes)</summary>
    /// <param name="data">Data to simulate receiving</param>
    public void SimulateDataReceived(string data)
    {
      if (_disposed || string.IsNullOrEmpty(data)) return;

      var bytes = System.Text.Encoding.UTF8.GetBytes(data);
      var bytesToCopy = Math.Min(bytes.Length, _receiveBuffer.Length);
      Array.Copy(bytes, _receiveBuffer, bytesToCopy);
      IncomingDataBufferSize = bytesToCopy;

      DataReceived?.Invoke(this, bytesToCopy);
    }

    /// <summary>Simulates a socket status change (for testing purposes)</summary>
    /// <param name="newStatus">New socket status</param>
    public void SimulateStatusChange(SocketStatus newStatus)
    {
      if (_disposed) return;

      ClientStatus = newStatus;
      SocketStatusChange?.Invoke(this, newStatus);
    }

    #endregion

    #region IDisposable Implementation

    /// <summary>Disposes the TCP client and releases resources</summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>Protected dispose method</summary>
    /// <param name="disposing">True if disposing managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
      if (!_disposed)
      {
        if (disposing)
        {
          // Disconnect if still connected
          if (ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
          {
            DisconnectFromServer();
          }
        }

        _disposed = true;
      }
    }

    #endregion
  }
}
