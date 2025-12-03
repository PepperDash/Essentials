using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Devices.Common
{
  /// <summary>
  /// Represents AppleTvIrCommands
  /// </summary>
  public static class AppleTvIrCommands
  {

    /// <summary>
    /// Up command
    /// </summary>
    public static string Up = "+";

    /// <summary>
    /// Down command
    /// </summary>
    public static string Down = "-";

    /// <summary>
    /// Left command
    /// </summary>
    public static string Left = IROutputStandardCommands.IROut_TRACK_MINUS;

    /// <summary>
    /// Right command
    /// </summary>
    public static string Right = IROutputStandardCommands.IROut_TRACK_PLUS;

    /// <summary>
    /// Enter command
    /// </summary>
    public static string Enter = IROutputStandardCommands.IROut_ENTER;

    /// <summary>
    /// PlayPause command
    /// </summary>
    public static string PlayPause = "PLAY/PAUSE";

    /// <summary>
    /// Rewind command
    /// </summary>
    public static string Rewind = "REWIND";

    /// <summary>
    /// Menu command
    /// </summary>
    public static string Menu = "Menu";

    /// <summary>
    /// FastForward command
    /// </summary>
    public static string FastForward = "FASTFORWARD";
  }
}