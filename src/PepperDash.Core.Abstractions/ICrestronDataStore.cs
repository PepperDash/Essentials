namespace PepperDash.Core.Abstractions;

/// <summary>
/// Abstracts <c>Crestron.SimplSharp.CrestronDataStore.CrestronDataStoreStatic</c>
/// to allow unit testing without the Crestron SDK.
/// </summary>
public interface ICrestronDataStore
{
    /// <summary>Initialises the data store. Must be called once before any other operation.</summary>
    void InitStore();

    /// <summary>Reads an integer value from the local (program-slot) store.</summary>
    /// <returns><c>true</c> if the value was found and read successfully.</returns>
    bool TryGetLocalInt(string key, out int value);

    /// <summary>Writes an integer value to the local (program-slot) store.</summary>
    /// <returns><c>true</c> on success.</returns>
    bool SetLocalInt(string key, int value);

    /// <summary>Writes an unsigned integer value to the local (program-slot) store.</summary>
    /// <returns><c>true</c> on success.</returns>
    bool SetLocalUint(string key, uint value);

    /// <summary>Reads a boolean value from the local (program-slot) store.</summary>
    /// <returns><c>true</c> if the value was found and read successfully.</returns>
    bool TryGetLocalBool(string key, out bool value);

    /// <summary>Writes a boolean value to the local (program-slot) store.</summary>
    /// <returns><c>true</c> on success.</returns>
    bool SetLocalBool(string key, bool value);
}
