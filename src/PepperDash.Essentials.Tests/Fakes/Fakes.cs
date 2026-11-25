using System.Text;
using PepperDash.Core.Abstractions;

namespace PepperDash.Essentials.Tests.Fakes;

internal class FakeCrestronEnvironment : ICrestronEnvironment
{
    public DevicePlatform DevicePlatform { get; set; } = DevicePlatform.Appliance;
    public RuntimeEnvironment RuntimeEnvironment { get; set; } = RuntimeEnvironment.SimplSharpPro;
    public string NewLine { get; set; } = "\r\n";
    public uint ApplicationNumber { get; set; } = 1;
    public uint RoomId { get; set; } = 0;

    private readonly object _handlersLock = new();
    private int _programStatusHandlers;

    /// <summary>How many handlers are subscribed to <see cref="ProgramStatusChanged"/>.</summary>
    public int ProgramStatusHandlerCount
    {
        get { lock (_handlersLock) return _programStatusHandlers; }
    }

    // Counted, never raised: a Stopping event would dispose every GenericQueue in the process,
    // including the routing queue other tests rely on.
    public event EventHandler<ProgramStatusEventArgs>? ProgramStatusChanged
    {
        add { lock (_handlersLock) _programStatusHandlers++; }
        remove { lock (_handlersLock) _programStatusHandlers--; }
    }

    public event EventHandler<PepperDashEthernetEventArgs>? EthernetEventReceived
    {
        add { }
        remove { }
    }

    public string GetApplicationRootDirectory() => System.IO.Path.GetTempPath();
    public bool IsHardwareRuntime => false;
}

internal class NoOpCrestronConsole : ICrestronConsole
{
    private readonly object _commandsLock = new();
    private readonly Dictionary<string, Action<string>> _commands = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The replies of the command <see cref="RunCommand"/> is running, on this flow only.</summary>
    private readonly AsyncLocal<StringBuilder?> _reply = new();

    /// <summary>Whether a console command with this name has been added.</summary>
    public bool HasCommand(string command)
    {
        lock (_commandsLock) return _commands.ContainsKey(command);
    }

    /// <summary>
    /// Runs a console command that was added, as typed at the console (<c>devfb</c> with <c>avr</c>), and returns
    /// what it answered with <see cref="ConsoleCommandResponse"/>. Log messages aren't part of the reply.
    /// </summary>
    public string RunCommand(string command, string args = "")
    {
        Action<string> callback;
        lock (_commandsLock) callback = _commands[command];
        var reply = new StringBuilder();
        _reply.Value = reply;
        try
        {
            callback(args);
        }
        finally
        {
            _reply.Value = null;
        }

        return reply.ToString();
    }

    public void PrintLine(string message) { }
    public void Print(string message) { }
    public void ConsoleCommandResponse(string message) => _reply.Value?.Append(message);

    public void AddNewConsoleCommand(Action<string> callback, string command, string __, ConsoleAccessLevel ___)
    {
        lock (_commandsLock) _commands[command] = callback;
    }
}

internal class InMemoryCrestronDataStore : ICrestronDataStore
{
    private readonly Dictionary<string, object> _store = new();

    public void InitStore() { }

    public bool TryGetLocalInt(string key, out int value)
    {
        if (_store.TryGetValue(key, out var raw) && raw is int i) { value = i; return true; }
        value = 0;
        return false;
    }

    public bool SetLocalInt(string key, int value) { _store[key] = value; return true; }
    public bool SetLocalUint(string key, uint value) { _store[key] = (int)value; return true; }

    public bool TryGetLocalBool(string key, out bool value)
    {
        if (_store.TryGetValue(key, out var raw) && raw is bool b) { value = b; return true; }
        value = false;
        return false;
    }

    public bool SetLocalBool(string key, bool value) { _store[key] = value; return true; }
}
