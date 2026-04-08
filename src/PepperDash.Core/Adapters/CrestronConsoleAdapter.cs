using System;
using Crestron.SimplSharp;
using PdCore = PepperDash.Core.Abstractions;

namespace PepperDash.Core.Adapters;

/// <summary>
/// Production adapter — delegates ICrestronConsole calls to the real Crestron SDK.
/// </summary>
public sealed class CrestronConsoleAdapter : PdCore.ICrestronConsole
{
    public void PrintLine(string message) => CrestronConsole.PrintLine(message);

    public void Print(string message) => CrestronConsole.Print(message);

    public void ConsoleCommandResponse(string message) =>
        CrestronConsole.ConsoleCommandResponse(message);

    public void AddNewConsoleCommand(
        Action<string> callback,
        string command,
        string helpText,
        PdCore.ConsoleAccessLevel accessLevel)
    {
        var crestronLevel = accessLevel switch
        {
            PdCore.ConsoleAccessLevel.AccessAdministrator => ConsoleAccessLevelEnum.AccessAdministrator,
            PdCore.ConsoleAccessLevel.AccessProgrammer => ConsoleAccessLevelEnum.AccessProgrammer,
            _ => ConsoleAccessLevelEnum.AccessOperator,
        };

        // Wrap Action<string> in a lambda — Crestron's delegate is not a standard Action<string>.
        CrestronConsole.AddNewConsoleCommand(s => callback(s), command, helpText, crestronLevel);
    }
}
