using System;
using System.Net;

namespace Crestron.SimplSharp.CrestronSockets
{
  // Additional types needed for networking compatibility

  /// <summary>IP address extensions and utilities</summary>
  public static class IPAddress
  {
    /// <summary>Parse IP address string</summary>
    public static System.Net.IPAddress Parse(string ipString)
    {
      return System.Net.IPAddress.Parse(ipString);
    }

    /// <summary>Any IP address</summary>
    public static System.Net.IPAddress Any => System.Net.IPAddress.Any;
  }
}

namespace Crestron.SimplSharp
{
  /// <summary>Extensions for CrestronQueue</summary>
  public static class CrestronQueueExtensions
  {
    /// <summary>Try to enqueue item</summary>
    public static bool TryToEnqueue<T>(this CrestronQueue<T> queue, T item)
    {
      try
      {
        queue.Enqueue(item);
        return true;
      }
      catch
      {
        return false;
      }
    }
  }
}
