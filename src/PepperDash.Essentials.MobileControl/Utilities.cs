using PepperDash.Core;
using PepperDash.Core.Logging;
using WebSocketSharp;

namespace PepperDash.Essentials
{
  /// <summary>
  /// Utility functions for logging and other common tasks.
  /// </summary>
  public static class Utilities
  {
    private static int nextClientId = 0;

    /// <summary>
    /// Get the next unique client ID
    /// </summary>
    /// <returns>Client ID</returns>
    public static int GetNextClientId()
    {
      nextClientId++;
      return nextClientId;
    }
    /// <summary>
    /// Converts a WebSocketServer LogData object to Essentials logging calls.
    /// </summary>
    /// <param name="data">The LogData object to convert.</param>
    /// <param name="message">The log message.</param>
    /// <param name="device">The device associated with the log message.</param>
    public static void ConvertWebsocketLog(LogData data, string message, IKeyed device = null)
    {

      switch (data.Level)
      {
        case LogLevel.Trace:
          if (device == null)
          {
            Debug.LogVerbose(message);
          }
          else
          {
            device.LogVerbose(message);
          }
          break;
        case LogLevel.Debug:
          if (device == null)
          {
            Debug.LogDebug(message);
          }
          else
          {
            device.LogDebug(message);
          }
          break;
        case LogLevel.Info:
          if (device == null)
          {
            Debug.LogInformation(message);
          }
          else
          {
            device.LogInformation(message);
          }
          break;
        case LogLevel.Warn:
          if (device == null)
          {
            Debug.LogWarning(message);
          }
          else
          {
            device.LogWarning(message);
          }
          break;
        case LogLevel.Error:
          if (device == null)
          {
            Debug.LogError(message);
          }
          else
          {
            device.LogError(message);
          }
          break;
        case LogLevel.Fatal:
          if (device == null)
          {
            Debug.LogFatal(message);
          }
          else
          {
            device.LogFatal(message);
          }
          break;
      }
    }
  }
}
