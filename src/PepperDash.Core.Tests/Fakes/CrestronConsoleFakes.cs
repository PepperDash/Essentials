using PepperDash.Core.Abstractions;

namespace PepperDash.Core.Tests.Fakes;

/// <summary>
/// No-op ICrestronConsole that captures output for test assertions.
/// </summary>
public class CapturingCrestronConsole : ICrestronConsole
{
    public List<string> Lines { get; } = new();
    public List<string> CommandResponses { get; } = new();
    public List<(string Command, string HelpText)> RegisteredCommands { get; } = new();

    public void PrintLine(string message) => Lines.Add(message);
    public void Print(string message) => Lines.Add(message);
    public void ConsoleCommandResponse(string message) => CommandResponses.Add(message);

    public void AddNewConsoleCommand(
        Action<string> callback,
        string command,
        string helpText,
        ConsoleAccessLevel accessLevel)
    {
        RegisteredCommands.Add((command, helpText));
    }
}

/// <summary>
/// Minimal no-op ICrestronConsole that discards all output. Useful when you only
/// care about the system under test and not what it logs.
/// </summary>
public class NoOpCrestronConsole : ICrestronConsole
{
    public void PrintLine(string message) { }
    public void Print(string message) { }
    public void ConsoleCommandResponse(string message) { }
    public void AddNewConsoleCommand(Action<string> _, string __, string ___, ConsoleAccessLevel ____) { }
}
