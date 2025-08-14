using System;

namespace Crestron.SimplSharp
{
  /// <summary>Mock console command response utility</summary>
  public static class ConsoleCommandResponseUtility
  {
    /// <summary>Send console command response with response code</summary>
    /// <param name="response">The response text</param>
    /// <param name="responseCode">The response code</param>
    public static void ConsoleCommandResponse(string response, int responseCode = 0)
    {
      // Mock implementation - just log to console or ignore
      Console.WriteLine($"Console Response ({responseCode}): {response}");
    }

    /// <summary>Send console command response with additional parameter</summary>
    /// <param name="response">The response text</param>
    /// <param name="param1">First parameter</param>
    /// <param name="param2">Second parameter</param>
    public static void ConsoleCommandResponse(string response, object param1, object param2)
    {
      // Mock implementation
      Console.WriteLine($"Console Response: {response} - {param1}, {param2}");
    }
  }
}
