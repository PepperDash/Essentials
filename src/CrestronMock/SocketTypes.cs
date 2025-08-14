namespace Crestron.SimplSharp.CrestronSockets
{
  /// <summary>Mock EthernetAdapterType enumeration</summary>
  public enum EthernetAdapterType
  {
    /// <summary>Ethernet adapter 1</summary>
    EthernetLANAdapter = 0,
    /// <summary>Ethernet adapter 2</summary>
    EthernetAdapter2 = 1,
    /// <summary>Auto-detect adapter</summary>
    EthernetAdapterAuto = 2
  }

  /// <summary>Mock SocketErrorCodes enumeration</summary>
  public enum SocketErrorCodes
  {
    /// <summary>Operation completed successfully</summary>
    SOCKET_OK = 0,
    /// <summary>Socket operation pending</summary>
    SOCKET_OPERATION_PENDING = 1,
    /// <summary>Socket not connected</summary>
    SOCKET_NOT_CONNECTED = 2,
    /// <summary>Connection failed</summary>
    SOCKET_CONNECTION_FAILED = 3,
    /// <summary>Invalid client index</summary>
    SOCKET_INVALID_CLIENT_INDEX = 4,
    /// <summary>DNS lookup failed</summary>
    SOCKET_DNS_LOOKUP_FAILED = 5,
    /// <summary>Invalid address</summary>
    SOCKET_INVALID_ADDRESS = 6,
    /// <summary>Connection timed out</summary>
    SOCKET_CONNECTION_TIMEOUT = 7,
    /// <summary>Send data failed</summary>
    SOCKET_SEND_DATA_FAILED = 8,
    /// <summary>Receive data failed</summary>
    SOCKET_RECEIVE_DATA_FAILED = 9,
    /// <summary>Socket closed</summary>
    SOCKET_CLOSED = 10,
    /// <summary>Socket disconnected</summary>
    SOCKET_DISCONNECTED = 11,
    /// <summary>Max connections reached</summary>
    SOCKET_MAX_CONNECTIONS_REACHED = 12,
    /// <summary>Permission denied</summary>
    SOCKET_PERMISSION_DENIED = 13,
    /// <summary>Address already in use</summary>
    SOCKET_ADDRESS_IN_USE = 14,
    /// <summary>Invalid parameter</summary>
    SOCKET_INVALID_PARAMETER = 15
  }
}
