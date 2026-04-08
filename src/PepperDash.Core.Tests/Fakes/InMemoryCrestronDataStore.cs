using PepperDash.Core.Abstractions;

namespace PepperDash.Core.Tests.Fakes;

/// <summary>
/// In-memory ICrestronDataStore backed by a dictionary.
/// Use in unit tests to verify that keys are read from and written to the store correctly.
/// </summary>
public class InMemoryCrestronDataStore : ICrestronDataStore
{
    private readonly Dictionary<string, object> _store = new();

    public bool Initialized { get; private set; }

    public void InitStore() => Initialized = true;

    public bool TryGetLocalInt(string key, out int value)
    {
        if (_store.TryGetValue(key, out var raw) && raw is int i)
        {
            value = i;
            return true;
        }
        value = 0;
        return false;
    }

    public bool SetLocalInt(string key, int value)
    {
        _store[key] = value;
        return true;
    }

    public bool SetLocalUint(string key, uint value)
    {
        _store[key] = (int)value;
        return true;
    }

    public bool TryGetLocalBool(string key, out bool value)
    {
        if (_store.TryGetValue(key, out var raw) && raw is bool b)
        {
            value = b;
            return true;
        }
        value = false;
        return false;
    }

    public bool SetLocalBool(string key, bool value)
    {
        _store[key] = value;
        return true;
    }

    /// <summary>Seeds a key for testing read paths.</summary>
    public void Seed(string key, object value) => _store[key] = value;
}
