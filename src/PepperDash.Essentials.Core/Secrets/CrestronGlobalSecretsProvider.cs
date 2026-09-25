using System;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronDataStore;
using PepperDash.Core;
using Serilog.Events;


namespace PepperDash.Essentials.Core;

/// <summary>
/// Stores secrets in the Crestron Data Store's global space, shared by every program slot on the
/// processor. Records created by another application cannot be modified or deleted.
/// </summary>
public class CrestronGlobalSecretsProvider : ISecretProvider, IEnumerableSecretProvider
{
    public string Key { get; set; }
    //Added for reference
    public string Description { get; private set; }

    /// <inheritdoc />
    public SecretStoreScope Scope => SecretStoreScope.Global;

    public CrestronGlobalSecretsProvider(string key)
    {
        Key = key;
        Description = String.Format("Default secret provider serving all local applications");

    }

    static CrestronGlobalSecretsProvider()
    {
        //Added for future encrypted reference
        var secureSupported = CrestronSecureStorage.Supported;

        CrestronDataStoreStatic.InitCrestronDataStore();
        if (secureSupported)
        {
            //doThingsFuture
        }
    }

    /// <summary>
    /// Set secret for item in the CrestronSecretsProvider
    /// </summary>
    /// <param name="key">Secret Key</param>
    /// <param name="value">Secret Value</param>
    /// <remarks>
    /// The secret value must never be logged. Anything written here reaches the processor's error
    /// log, which is routinely copied into tickets and support threads.
    /// </remarks>
    public bool SetSecret(string key, object value)
    {
        var secret = value as string;
        CrestronDataStore.CDS_ERROR returnCode;

        if (String.IsNullOrEmpty(secret))
        {
            returnCode = CrestronDataStoreStatic.clearGlobal(key);
            if (returnCode == CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
            {
                Debug.LogMessage(LogEventLevel.Information, this, "Removed secret {0}:{1}", Key, key);
                return true;
            }
        }

        else
        {
            returnCode = CrestronDataStoreStatic.SetGlobalStringValue(key, secret);
            if (returnCode == CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
            {
                // Length only - useful for diagnosing a truncated or empty write, and not the value.
                Debug.LogMessage(LogEventLevel.Information, this, "Set secret {0}:{1} ({2} characters)", Key, key, secret.Length);
                return true;
            }
        }

        Debug.LogMessage(LogEventLevel.Information, this, "Unable to set secret for {0}:{1} - {2}", Key, key, returnCode.ToString());
        return false;
    }

    /// <summary>
    /// Retrieve secret for item in the CrestronSecretsProvider
    /// </summary>
    /// <param name="key">Secret Key</param>
    /// <returns>ISecret Object containing key, provider, and value</returns>
    public ISecret GetSecret(string key)
    {
        string mySecret;
        var getErrorCode = CrestronDataStoreStatic.GetGlobalStringValue(key, out mySecret);

        switch (getErrorCode)
        {
            case CrestronDataStore.CDS_ERROR.CDS_SUCCESS:
                Debug.LogMessage(LogEventLevel.Verbose, this, "Secret Successfully retrieved for {0}:{1}", Key, key);
                return new CrestronSecret(key, mySecret, this);
            default:
                Debug.LogMessage(LogEventLevel.Information, this, "Unable to retrieve secret for {0}:{1} - {2}",
                    Key, key, getErrorCode.ToString());
                return null;
        }
    }

    /// <summary>
    /// Determine if a secret is present within the provider without retrieving it
    /// </summary>
    /// <param name="key">Secret Key</param>
    /// <returns>bool if present</returns>
    public bool TestSecret(string key)
    {
        string mySecret;
        return CrestronDataStoreStatic.GetGlobalStringValue(key, out mySecret) == CrestronDataStore.CDS_ERROR.CDS_SUCCESS;
    }

    /// <inheritdoc />
    public SecretStoreEnumeration EnumerateKeys()
        => CrestronDataStoreEnumerator.Enumerate(SecretStoreScope.Global);

    /// <inheritdoc />
    public SecretStoreResult WriteSecret(string key, string value)
    {
        var guard = SecretProviderGuards.ValidateWrite(key, value);
        if (guard != null)
            return guard;

        var returnCode = CrestronDataStoreStatic.SetGlobalStringValue(key, value);
        if (returnCode == CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
        {
            Debug.LogMessage(LogEventLevel.Information, this, "Set secret {0}:{1} ({2} characters)", Key, key, value.Length);
            return SecretStoreResult.Ok();
        }

        return SecretProviderGuards.FromCdsError(returnCode, Key, key);
    }

    /// <inheritdoc />
    public SecretStoreResult DeleteSecret(string key)
    {
        // Present-only: the key came from the store, so its length and character set are already
        // settled. Rejecting it here would make an existing record impossible to remove.
        var guard = SecretProviderGuards.ValidateKeyPresent(key);
        if (guard != null)
            return guard;

        var returnCode = CrestronDataStoreStatic.clearGlobal(key);
        if (returnCode == CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
        {
            Debug.LogMessage(LogEventLevel.Information, this, "Removed secret {0}:{1}", Key, key);
            return SecretStoreResult.Ok();
        }

        return SecretProviderGuards.FromCdsError(returnCode, Key, key);
    }

    /// <inheritdoc />
    public int GetSecretLength(string key)
    {
        string value;
        return CrestronDataStoreStatic.GetGlobalStringValue(key, out value) == CrestronDataStore.CDS_ERROR.CDS_SUCCESS
            ? value?.Length ?? 0
            : -1;
    }
}
