using System;
using Crestron.SimplSharp;

namespace PepperDash.Core
{
  /// <summary>
  /// Extension methods for stream debugging
  /// </summary>
  public static class StreamDebuggingExtensions
  {
    private static readonly string app = CrestronEnvironment.DevicePlatform == eDevicePlatform.Appliance ? $"App {InitialParametersClass.ApplicationNumber}" : $"{InitialParametersClass.RoomId}";

    /// <summary>
    /// Print the sent bytes to the console
    /// </summary>
    /// <param name="comms">comms device</param>
    /// <param name="bytes">bytes to print</param>
    public static void PrintSentBytes(this IStreamDebugging comms, byte[] bytes)
    {
      if (!comms.StreamDebugging.TxStreamDebuggingIsEnabled) return;

      var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

      CrestronConsole.PrintLine($"[{timestamp}][{app}][{comms.Key}] Sending {bytes.Length} bytes: '{ComTextHelper.GetEscapedText(bytes)}'");
    }

    /// <summary>
    /// Print the received bytes to the console
    /// </summary>
    /// <param name="comms">comms device</param>
    /// <param name="bytes">bytes to print</param>
    public static void PrintReceivedBytes(this IStreamDebugging comms, byte[] bytes)
    {
      if (!comms.StreamDebugging.RxStreamDebuggingIsEnabled) return;

      var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

      CrestronConsole.PrintLine($"[{timestamp}][{app}][{comms.Key}] Received {bytes.Length} bytes: '{ComTextHelper.GetEscapedText(bytes)}'");
    }

    /// <summary>
    /// Print the sent text to the console
    /// </summary>
    /// <param name="comms">comms device</param>
    /// <param name="text">text to print</param>
    public static void PrintSentText(this IStreamDebugging comms, string text)
    {
      if (!comms.StreamDebugging.TxStreamDebuggingIsEnabled) return;

      var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

      CrestronConsole.PrintLine($"[{timestamp}][{app}][{comms.Key}] Sending Text: '{ComTextHelper.GetDebugText(text)}'");
    }

    /// <summary>
    /// Print the received text to the console
    /// </summary>
    /// <param name="comms">comms device</param>
    /// <param name="text">text to print</param>
    public static void PrintReceivedText(this IStreamDebugging comms, string text)
    {
      if (!comms.StreamDebugging.RxStreamDebuggingIsEnabled) return;

      var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

      CrestronConsole.PrintLine($"[{timestamp}][{app}][{comms.Key}] Received Text: '{ComTextHelper.GetDebugText(text)}'");
    }
  }
}
