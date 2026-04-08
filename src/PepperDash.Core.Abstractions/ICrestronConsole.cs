namespace PepperDash.Core.Abstractions;

/// <summary>
/// Abstracts <c>Crestron.SimplSharp.CrestronConsole</c> to allow unit testing
/// without the Crestron SDK.
/// </summary>
public interface ICrestronConsole
{
    /// <summary>Prints a line to the Crestron console/telnet output.</summary>
    void PrintLine(string message);

    /// <summary>Prints text (without newline) to the Crestron console/telnet output.</summary>
    void Print(string message);

    /// <summary>
    /// Sends a response string to the console for the currently-executing console command.
    /// </summary>
    void ConsoleCommandResponse(string message);

    /// <summary>
    /// Registers a new command with the Crestron console.
    /// </summary>
    /// <param name="callback">Handler invoked when the command is typed.</param>
    /// <param name="command">Command name (no spaces).</param>
    /// <param name="helpText">Help text shown by the Crestron console.</param>
    /// <param name="accessLevel">Minimum access level required to run the command.</param>
    void AddNewConsoleCommand(
        Action<string> callback,
        string command,
        string helpText,
        ConsoleAccessLevel accessLevel);
}
