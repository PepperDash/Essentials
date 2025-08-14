using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Crestron.SimplSharp.CrestronSockets
{
  /// <summary>Mock TCPServer class for server-side TCP operations</summary>
  public class TCPServer : IDisposable
  {
    private TcpListener? _listener;
    private readonly List<TCPClientConnection> _clients = new List<TCPClientConnection>();
    private bool _listening;
    private readonly object _lockObject = new object();

    /// <summary>Event fired when waiting for connections</summary>
    public event TCPServerWaitingForConnectionsEventHandler? WaitingForConnections;

    /// <summary>Event fired when a client connects</summary>
    public event TCPServerClientConnectEventHandler? ClientConnected;

    /// <summary>Event fired when a client disconnects</summary>
    public event TCPServerClientDisconnectEventHandler? ClientDisconnected;

    /// <summary>Event fired when data is received from a client</summary>
    public event TCPServerReceiveDataEventHandler? ReceivedData;

    /// <summary>Gets the server state</summary>
    public SocketServerState State { get; private set; } = SocketServerState.SERVER_NOT_LISTENING;

    /// <summary>Gets the port number</summary>
    public int PortNumber { get; private set; }

    /// <summary>Gets the maximum number of clients</summary>
    public int MaxNumberOfClientSupported { get; private set; }

    /// <summary>Gets the number of connected clients</summary>
    public int NumberOfClientsConnected
    {
      get
      {
        lock (_lockObject)
        {
          return _clients.Count;
        }
      }
    }

    /// <summary>Initializes a new instance of TCPServer</summary>
    /// <param name="ipAddress">IP address to bind to</param>
    /// <param name="portNumber">Port number to listen on</param>
    /// <param name="bufferSize">Buffer size for data reception</param>
    /// <param name="ethernetAdapterToBindTo">Ethernet adapter to bind to</param>
    /// <param name="maxNumberOfClientSupported">Maximum number of clients</param>
    public TCPServer(string ipAddress, int portNumber, int bufferSize, EthernetAdapterType ethernetAdapterToBindTo, int maxNumberOfClientSupported)
    {
      PortNumber = portNumber;
      MaxNumberOfClientSupported = maxNumberOfClientSupported;
    }

    /// <summary>Initializes a new instance of TCPServer</summary>
    /// <param name="portNumber">Port number to listen on</param>
    /// <param name="bufferSize">Buffer size for data reception</param>
    /// <param name="maxNumberOfClientSupported">Maximum number of clients</param>
    public TCPServer(int portNumber, int bufferSize, int maxNumberOfClientSupported)
    {
      PortNumber = portNumber;
      MaxNumberOfClientSupported = maxNumberOfClientSupported;
    }

    /// <summary>Starts listening for client connections</summary>
    /// <returns>SocketErrorCodes indicating success or failure</returns>
    public SocketErrorCodes WaitForConnectionAsync()
    {
      if (_listening)
        return SocketErrorCodes.SOCKET_OPERATION_PENDING;

      try
      {
        _listener = new TcpListener(IPAddress.Any, PortNumber);
        _listener.Start();
        _listening = true;
        State = SocketServerState.SERVER_LISTENING;

        WaitingForConnections?.Invoke(this, new TCPServerWaitingForConnectionsEventArgs(0));

        _ = Task.Run(AcceptClientsAsync);

        return SocketErrorCodes.SOCKET_OK;
      }
      catch (Exception)
      {
        State = SocketServerState.SERVER_NOT_LISTENING;
        return SocketErrorCodes.SOCKET_CONNECTION_FAILED;
      }
    }

    /// <summary>Stops listening for connections</summary>
    /// <returns>SocketErrorCodes indicating success or failure</returns>
    public SocketErrorCodes Stop()
    {
      if (!_listening)
        return SocketErrorCodes.SOCKET_NOT_CONNECTED;

      try
      {
        _listening = false;
        _listener?.Stop();
        State = SocketServerState.SERVER_NOT_LISTENING;

        lock (_lockObject)
        {
          foreach (var client in _clients)
          {
            client.Disconnect();
          }
          _clients.Clear();
        }

        return SocketErrorCodes.SOCKET_OK;
      }
      catch (Exception)
      {
        return SocketErrorCodes.SOCKET_CONNECTION_FAILED;
      }
    }

    /// <summary>Sends data to a specific client</summary>
    /// <param name="data">Data to send</param>
    /// <param name="dataLength">Length of data</param>
    /// <param name="clientIndex">Index of client to send to</param>
    /// <returns>SocketErrorCodes indicating success or failure</returns>
    public SocketErrorCodes SendData(byte[] data, int dataLength, uint clientIndex)
    {
      lock (_lockObject)
      {
        if (clientIndex >= _clients.Count)
          return SocketErrorCodes.SOCKET_INVALID_CLIENT_INDEX;

        return _clients[(int)clientIndex].SendData(data, dataLength);
      }
    }

    /// <summary>Sends data to all connected clients</summary>
    /// <param name="data">Data to send</param>
    /// <param name="dataLength">Length of data</param>
    /// <returns>SocketErrorCodes indicating success or failure</returns>
    public SocketErrorCodes SendDataToAll(byte[] data, int dataLength)
    {
      lock (_lockObject)
      {
        var result = SocketErrorCodes.SOCKET_OK;
        foreach (var client in _clients)
        {
          var sendResult = client.SendData(data, dataLength);
          if (sendResult != SocketErrorCodes.SOCKET_OK)
            result = sendResult;
        }
        return result;
      }
    }

    /// <summary>Disconnects a specific client</summary>
    /// <param name="clientIndex">Index of client to disconnect</param>
    /// <returns>SocketErrorCodes indicating success or failure</returns>
    public SocketErrorCodes Disconnect(uint clientIndex)
    {
      lock (_lockObject)
      {
        if (clientIndex >= _clients.Count)
          return SocketErrorCodes.SOCKET_INVALID_CLIENT_INDEX;

        var client = _clients[(int)clientIndex];
        client.Disconnect();
        _clients.RemoveAt((int)clientIndex);

        ClientDisconnected?.Invoke(this, new TCPServerClientDisconnectEventArgs((uint)clientIndex));

        return SocketErrorCodes.SOCKET_OK;
      }
    }

    /// <summary>Gets the IP address of a connected client</summary>
    /// <param name="clientIndex">Index of client</param>
    /// <returns>IP address as string</returns>
    public string GetAddressServerAcceptedConnectionFromForSpecificClient(uint clientIndex)
    {
      lock (_lockObject)
      {
        if (clientIndex >= _clients.Count)
          return string.Empty;

        return _clients[(int)clientIndex].ClientIPAddress;
      }
    }

    private async Task AcceptClientsAsync()
    {
      while (_listening && _listener != null)
      {
        try
        {
          var tcpClient = await _listener.AcceptTcpClientAsync();

          lock (_lockObject)
          {
            if (_clients.Count >= MaxNumberOfClientSupported)
            {
              tcpClient.Close();
              continue;
            }

            var clientConnection = new TCPClientConnection(tcpClient, (uint)_clients.Count);
            clientConnection.DataReceived += OnClientDataReceived;
            clientConnection.Disconnected += OnClientDisconnected;
            _clients.Add(clientConnection);

            ClientConnected?.Invoke(this, new TCPServerClientConnectEventArgs((uint)(_clients.Count - 1)));
          }
        }
        catch (ObjectDisposedException)
        {
          // Server was stopped
          break;
        }
        catch (Exception)
        {
          // Handle other exceptions
          continue;
        }
      }
    }

    private void OnClientDataReceived(object? sender, TCPClientDataEventArgs e)
    {
      if (sender is TCPClientConnection client)
      {
        var args = new TCPServerReceiveDataEventArgs(e.Data, e.DataLength, client.ClientIndex);
        ReceivedData?.Invoke(this, args);
      }
    }

    private void OnClientDisconnected(object? sender, EventArgs e)
    {
      if (sender is TCPClientConnection client)
      {
        lock (_lockObject)
        {
          var index = _clients.IndexOf(client);
          if (index >= 0)
          {
            _clients.RemoveAt(index);
            ClientDisconnected?.Invoke(this, new TCPServerClientDisconnectEventArgs(client.ClientIndex));
          }
        }
      }
    }

    /// <summary>Disposes the TCPServer</summary>
    public void Dispose()
    {
      Stop();
      _listener?.Stop();
    }
  }

  /// <summary>Mock SecureTCPServer class for secure server-side TCP operations</summary>
  public class SecureTCPServer : TCPServer
  {
    /// <summary>Initializes a new instance of SecureTCPServer</summary>
    /// <param name="ipAddress">IP address to bind to</param>
    /// <param name="portNumber">Port number to listen on</param>
    /// <param name="bufferSize">Buffer size for data reception</param>
    /// <param name="ethernetAdapterToBindTo">Ethernet adapter to bind to</param>
    /// <param name="maxNumberOfClientSupported">Maximum number of clients</param>
    public SecureTCPServer(string ipAddress, int portNumber, int bufferSize, EthernetAdapterType ethernetAdapterToBindTo, int maxNumberOfClientSupported)
        : base(ipAddress, portNumber, bufferSize, ethernetAdapterToBindTo, maxNumberOfClientSupported)
    {
    }

    /// <summary>Initializes a new instance of SecureTCPServer</summary>
    /// <param name="portNumber">Port number to listen on</param>
    /// <param name="bufferSize">Buffer size for data reception</param>
    /// <param name="maxNumberOfClientSupported">Maximum number of clients</param>
    public SecureTCPServer(int portNumber, int bufferSize, int maxNumberOfClientSupported)
        : base(portNumber, bufferSize, maxNumberOfClientSupported)
    {
    }
  }

  /// <summary>Internal class representing a client connection</summary>
  internal class TCPClientConnection
  {
    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _stream;
    private readonly byte[] _buffer = new byte[4096];
    private bool _connected = true;

    public uint ClientIndex { get; }
    public string ClientIPAddress { get; }

    public event EventHandler<TCPClientDataEventArgs>? DataReceived;
    public event EventHandler? Disconnected;

    public TCPClientConnection(TcpClient tcpClient, uint clientIndex)
    {
      _tcpClient = tcpClient;
      ClientIndex = clientIndex;
      _stream = tcpClient.GetStream();

      var endpoint = tcpClient.Client.RemoteEndPoint as IPEndPoint;
      ClientIPAddress = endpoint?.Address.ToString() ?? "Unknown";

      _ = Task.Run(ReceiveDataAsync);
    }

    public SocketErrorCodes SendData(byte[] data, int dataLength)
    {
      if (!_connected)
        return SocketErrorCodes.SOCKET_NOT_CONNECTED;

      try
      {
        _stream.Write(data, 0, dataLength);
        return SocketErrorCodes.SOCKET_OK;
      }
      catch (Exception)
      {
        Disconnect();
        return SocketErrorCodes.SOCKET_CONNECTION_FAILED;
      }
    }

    public void Disconnect()
    {
      if (!_connected)
        return;

      _connected = false;
      _stream?.Close();
      _tcpClient?.Close();
      Disconnected?.Invoke(this, EventArgs.Empty);
    }

    private async Task ReceiveDataAsync()
    {
      while (_connected)
      {
        try
        {
          var bytesRead = await _stream.ReadAsync(_buffer, 0, _buffer.Length);
          if (bytesRead == 0)
          {
            Disconnect();
            break;
          }

          var data = new byte[bytesRead];
          Array.Copy(_buffer, data, bytesRead);
          DataReceived?.Invoke(this, new TCPClientDataEventArgs(data, bytesRead));
        }
        catch (Exception)
        {
          Disconnect();
          break;
        }
      }
    }
  }

  /// <summary>Event args for TCP client data</summary>
  internal class TCPClientDataEventArgs : EventArgs
  {
    public byte[] Data { get; }
    public int DataLength { get; }

    public TCPClientDataEventArgs(byte[] data, int dataLength)
    {
      Data = data;
      DataLength = dataLength;
    }
  }

  /// <summary>Server state enumeration</summary>
  public enum SocketServerState
  {
    /// <summary>Server is not listening</summary>
    SERVER_NOT_LISTENING = 0,
    /// <summary>Server is listening for connections</summary>
    SERVER_LISTENING = 1
  }

  // Event handler delegates
  public delegate void TCPServerWaitingForConnectionsEventHandler(TCPServer server, TCPServerWaitingForConnectionsEventArgs args);
  public delegate void TCPServerClientConnectEventHandler(TCPServer server, TCPServerClientConnectEventArgs args);
  public delegate void TCPServerClientDisconnectEventHandler(TCPServer server, TCPServerClientDisconnectEventArgs args);
  public delegate void TCPServerReceiveDataEventHandler(TCPServer server, TCPServerReceiveDataEventArgs args);

  // Event argument classes
  public class TCPServerWaitingForConnectionsEventArgs : EventArgs
  {
    public int ErrorCode { get; }
    public TCPServerWaitingForConnectionsEventArgs(int errorCode) { ErrorCode = errorCode; }
  }

  public class TCPServerClientConnectEventArgs : EventArgs
  {
    public uint ClientIndex { get; }
    public TCPServerClientConnectEventArgs(uint clientIndex) { ClientIndex = clientIndex; }
  }

  public class TCPServerClientDisconnectEventArgs : EventArgs
  {
    public uint ClientIndex { get; }
    public TCPServerClientDisconnectEventArgs(uint clientIndex) { ClientIndex = clientIndex; }
  }

  public class TCPServerReceiveDataEventArgs : EventArgs
  {
    public byte[] Data { get; }
    public int DataLength { get; }
    public uint ClientIndex { get; }

    public TCPServerReceiveDataEventArgs(byte[] data, int dataLength, uint clientIndex)
    {
      Data = data;
      DataLength = dataLength;
      ClientIndex = clientIndex;
    }
  }
}
