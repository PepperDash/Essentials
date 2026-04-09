using PepperDash.Core.Abstractions;

namespace PepperDash.Essentials.Tests.Fakes;

internal class FakeCrestronEnvironment : ICrestronEnvironment
{
    public DevicePlatform DevicePlatform { get; set; } = DevicePlatform.Appliance;
    public RuntimeEnvironment RuntimeEnvironment { get; set; } = RuntimeEnvironment.SimplSharpPro;
    public string NewLine { get; set; } = "\r\n";
    public uint ApplicationNumber { get; set; } = 1;
    public uint RoomId { get; set; } = 0;

    public event EventHandler<ProgramStatusEventArgs>? ProgramStatusChanged
    {
        add { }
        remove { }
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
    public void PrintLine(string message) { }
    public void Print(string message) { }
    public void ConsoleCommandResponse(string message) { }
    public void AddNewConsoleCommand(Action<string> _, string __, string ___, ConsoleAccessLevel ____) { }
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
