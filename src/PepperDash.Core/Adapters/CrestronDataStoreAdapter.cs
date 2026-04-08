using Crestron.SimplSharp.CrestronDataStore;
using PepperDash.Core.Abstractions;

namespace PepperDash.Core.Adapters;

/// <summary>
/// Production adapter — delegates ICrestronDataStore calls to the real Crestron SDK.
/// </summary>
public sealed class CrestronDataStoreAdapter : ICrestronDataStore
{
    public void InitStore() => CrestronDataStoreStatic.InitCrestronDataStore();

    public bool TryGetLocalInt(string key, out int value)
    {
        var err = CrestronDataStoreStatic.GetLocalIntValue(key, out value);
        return err == CrestronDataStore.CDS_ERROR.CDS_SUCCESS;
    }

    public bool SetLocalInt(string key, int value)
    {
        var err = CrestronDataStoreStatic.SetLocalIntValue(key, value);
        return err == CrestronDataStore.CDS_ERROR.CDS_SUCCESS;
    }

    public bool SetLocalUint(string key, uint value)
    {
        var err = CrestronDataStoreStatic.SetLocalUintValue(key, value);
        return err == CrestronDataStore.CDS_ERROR.CDS_SUCCESS;
    }

    public bool TryGetLocalBool(string key, out bool value)
    {
        var err = CrestronDataStoreStatic.GetLocalBoolValue(key, out value);
        return err == CrestronDataStore.CDS_ERROR.CDS_SUCCESS;
    }

    public bool SetLocalBool(string key, bool value)
    {
        var err = CrestronDataStoreStatic.SetLocalBoolValue(key, value);
        return err == CrestronDataStore.CDS_ERROR.CDS_SUCCESS;
    }
}
