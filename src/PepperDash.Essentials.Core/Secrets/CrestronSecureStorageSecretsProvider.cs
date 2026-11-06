using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Stores secrets encrypted, in Crestron secure storage's local (per-program-slot) space.
/// </summary>
/// <remarks>
/// <para>
/// Crestron secure storage encrypts each value with a master key generated and kept on the
/// processor. Unlike the Data Store it accepts values longer than 1600 characters, such as a
/// PEM private key.
/// </para>
/// <para>
/// Secure storage cannot list its tags, so this provider keeps its own list of the keys it has
/// written (<see cref="SecureStorageKeyRegistry"/>). <see cref="EnumerateKeys"/> reads that list, so
/// it shows only keys written through this provider.
/// </para>
/// <para>
/// Each value is stored with its own key as the supplemental encryption key, the same call pattern
/// as PepperDash's OAuth client library, so values written by that pattern read back here.
/// </para>
/// <para>
/// A secret value is never logged. Anything written to the log reaches the processor's error log,
/// which is routinely copied into tickets and support threads.
/// </para>
/// </remarks>
public class CrestronSecureStorageSecretsProvider : IEnumerableSecretProvider
{
    /// <summary>
    /// The key <see cref="SecretsManager"/> registers this provider under.
    /// </summary>
    public const string DefaultKey = "CrestronSecureSecrets";

    /// <summary>
    /// Longest value accepted, in UTF-8 bytes, which is also its length in characters for ASCII.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Crestron documents no size limit for secure storage. These limits are set from what the
    /// provider is for, not from what the processor accepts: an RSA private key in PEM form
    /// (1,679 characters for 2048-bit, about 3,300 for 4096-bit) or a certificate chain of a few
    /// certificates. 8,192 holds either.
    /// </para>
    /// <para>
    /// Large secure storage writes on an RMC4 (firmware 2.8006.00110) produced CrestronDataStore errors
    /// and left the processor's local Data Store unusable for every program until a factory restore.
    /// Keep these limits small; do not raise them without a need for the larger value.
    /// </para>
    /// <para>
    /// Every write is also read back (see <see cref="WriteSecret"/>).
    /// </para>
    /// </remarks>
    public const int MaxSecureValueLength = 8192;

    /// <summary>
    /// Longest key accepted, in characters. The key is the secure storage tag and its encryption
    /// entropy. The same figure the Secrets API reports as every provider's key limit.
    /// </summary>
    public const int MaxSecureKeyLength = SecretProviderGuards.DocumentedMaxKeyLength;

    /// <summary>
    /// Most secrets this provider holds. It is for the few long secrets the Data Store cannot hold,
    /// such as a device certificate and its private key, so it bounds the total it writes.
    /// </summary>
    /// <remarks>
    /// The Secrets API's own index records (<see cref="SecretsIndex.IsReservedKey"/>) do not count;
    /// <see cref="SecretsIndex.MaxChunks"/> bounds them.
    /// </remarks>
    public const int MaxSecureSecrets = 16;

    /// <summary>
    /// Secure storage's flag for the local, per-program space rather than the global one.
    /// </summary>
    private const bool GlobalStore = false;

    private const string RecordType = "String";

    /// <summary>Error code for an unavailable store; the Secrets API maps it to storeUnavailable.</summary>
    private const string DeviceUnavailable = "DeviceUnavailable";

    private readonly object _sync = new object();

    /// <inheritdoc />
    public string Key { get; private set; }

    /// <inheritdoc />
    public string Description { get; private set; }

    /// <inheritdoc />
    public SecretStoreScope Scope => SecretStoreScope.Local;

    /// <inheritdoc />
    public int MaxValueLength => MaxSecureValueLength;

    /// <summary>
    /// Gets a value indicating whether this processor's firmware supports Crestron secure storage.
    /// </summary>
    public static bool Supported
    {
        get
        {
            try
            {
                return CrestronSecureStorage.Supported;
            }
            catch (Exception ex)
            {
                Debug.LogMessage(LogEventLevel.Debug, "Crestron secure storage support check failed: {message}", null, ex.Message);
                return false;
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CrestronSecureStorageSecretsProvider"/> class.
    /// </summary>
    /// <param name="key">Provider key.</param>
    public CrestronSecureStorageSecretsProvider(string key)
    {
        Key = key;
        Description = String.Format("Encrypted secret provider (Crestron secure storage) serving Essentials Application {0}",
            InitialParametersClass.ApplicationNumber);
    }

    /// <summary>
    /// Sets a secret, or deletes it when the value is empty.
    /// </summary>
    /// <param name="key">Secret key.</param>
    /// <param name="value">Secret value, as a string.</param>
    public bool SetSecret(string key, object value)
    {
        var text = value as string;
        var result = String.IsNullOrEmpty(text) ? DeleteSecret(key) : WriteSecret(key, text);

        if (!result.Success)
            Debug.LogMessage(LogEventLevel.Information, this, "Unable to set secret for {0}:{1} - {2}", Key, key, result.ErrorCode);

        return result.Success;
    }

    /// <summary>
    /// Retrieves a secret.
    /// </summary>
    /// <param name="key">Secret key.</param>
    /// <returns>The secret, or null when it does not exist or cannot be read.</returns>
    public ISecret GetSecret(string key)
    {
        if (String.IsNullOrWhiteSpace(key))
            return null;

        var status = Retrieve(key, out var value);
        if (status == eCrestronSecureStorageStatus.Ok)
        {
            Debug.LogMessage(LogEventLevel.Verbose, this, "Secret Successfully retrieved for {0}:{1}", Key, key);
            return new CrestronSecret(key, value, this);
        }

        Debug.LogMessage(LogEventLevel.Debug, this, "Unable to retrieve secret for {0}:{1} - {2}", Key, key, status);
        return null;
    }

    /// <summary>
    /// Determines whether a secret exists, without returning it.
    /// </summary>
    /// <param name="key">Secret key.</param>
    public bool TestSecret(string key)
        => !String.IsNullOrWhiteSpace(key) && Retrieve(key, out _) == eCrestronSecureStorageStatus.Ok;

    /// <inheritdoc />
    public SecretStoreEnumeration EnumerateKeys()
    {
        lock (_sync)
        {
            if (!TryReadRegistry(out var keys, out var errorCode))
                return new SecretStoreEnumeration(null, false, errorCode);

            var records = new System.Collections.Generic.List<CdsRecord>();
            foreach (var key in keys)
                records.Add(new CdsRecord(key, null, null, RecordType));

            return new SecretStoreEnumeration(records, true, null);
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Everything here is checked before storage is touched: the Data Store rules, this provider's
    /// key and UTF-8 byte limits, and reserved names. The Secrets API's index records are the only
    /// reserved-prefix keys accepted.
    /// </remarks>
    public SecretStoreResult ValidateWrite(string key, string value)
    {
        var guard = SecretProviderGuards.ValidateWrite(key, value, MaxValueLength);
        if (guard != null)
            return guard;

        if (SecretsIndex.IsReservedKey(key) && !SecretsIndex.IsIndexRecord(key))
            return ReservedKey(key);

        if (key.Length > MaxSecureKeyLength)
        {
            return SecretStoreResult.Fail("keyTooLong",
                String.Format("The key is {0} characters; this provider's limit is {1}.", key.Length, MaxSecureKeyLength));
        }

        var byteCount = Encoding.UTF8.GetByteCount(value);
        if (byteCount > MaxValueLength)
        {
            // Length only - never any part of the value.
            return SecretStoreResult.Fail("valueTooLong",
                String.Format("The value is {0} bytes as UTF-8; this provider's limit is {1}.", byteCount, MaxValueLength));
        }

        return null;
    }

    /// <inheritdoc />
    public SecretStoreResult WriteSecret(string key, string value)
    {
        var invalid = ValidateWrite(key, value);
        if (invalid != null)
            return invalid;

        var bytes = Encoding.UTF8.GetBytes(value);

        lock (_sync)
        {
            // Checked before the secret is written, so a refused key leaves nothing behind. A key
            // already in the list is an overwrite and adds nothing.
            if (!TryReadRegistry(out var listed, out var listError) && listError == DeviceUnavailable)
                return Fail(eCrestronSecureStorageStatus.DeviceUnavailable, key);

            var withKey = SecureStorageKeyRegistry.WithKey(listed, key);
            if (withKey != null && !SecretsIndex.IsIndexRecord(key)
                && withKey.Count(listedKey => !SecretsIndex.IsIndexRecord(listedKey)) > MaxSecureSecrets)
            {
                return SecretStoreResult.Fail("tooManySecrets",
                    String.Format("{0} holds at most {1} secrets; delete one before adding another.", Key, MaxSecureSecrets));
            }

            // The key list is a secure storage record too, so it gets the same value cap. With the
            // key and count limits it stays far below it; this is the backstop.
            if (withKey != null && Encoding.UTF8.GetByteCount(SecureStorageKeyRegistry.Serialize(withKey)) > MaxValueLength)
            {
                return SecretStoreResult.Fail("keyListFull",
                    String.Format("The list of keys for {0} has reached its {1}-byte limit; delete a secret before adding another.", Key, MaxValueLength));
            }

            var status = CrestronSecureStorage.Store(key, GlobalStore, bytes, Entropy(key));
            if (status != eCrestronSecureStorageStatus.Ok)
                return Fail(status, key);

            // Store can report Ok for a value it cannot return (seen on an RMC4 with values far
            // above MaxSecureValueLength), so read it back. An unreadable value is no use to anyone, so it is removed rather than left
            // looking like a successful write.
            if (!ReadsBack(key, value))
            {
                CrestronSecureStorage.Delete(key, GlobalStore);
                Flush();
                UpdateRegistry(key, add: false);
                Debug.LogMessage(LogEventLevel.Warning, this, "Secure storage accepted {0}:{1} ({2} bytes) but could not read it back; it was removed", Key, key, bytes.Length);
                return SecretStoreResult.Fail("verifyFailed",
                    String.Format("Crestron secure storage accepted '{0}' but could not read it back, so it was not saved.", key));
            }

            Flush();

            // A new key that isn't in the list is invisible to listing and to the secret count, so
            // the write only counts once the list holds it. Otherwise it is removed again.
            if (!UpdateRegistry(key, add: true))
            {
                CrestronSecureStorage.Delete(key, GlobalStore);
                Flush();
                Debug.LogMessage(LogEventLevel.Warning, this, "Secure storage saved {0}:{1} but its key list could not be updated; the secret was removed", Key, key);
                return SecretStoreResult.Fail("keyListWriteFailed",
                    String.Format("The key list for {0} could not be updated, so '{1}' was not saved.", Key, key));
            }

            // Length only - useful for diagnosing a truncated or empty write, and not the value.
            Debug.LogMessage(LogEventLevel.Information, this, "Set secret {0}:{1} ({2} characters)", Key, key, value.Length);
            return SecretStoreResult.Ok();
        }
    }

    /// <inheritdoc />
    public SecretStoreResult DeleteSecret(string key)
    {
        var guard = SecretProviderGuards.ValidateKeyPresent(key);
        if (guard != null)
            return guard;

        if (IsRegistryKey(key))
            return ReservedKey(key);

        lock (_sync)
        {
            var status = CrestronSecureStorage.Delete(key, GlobalStore);
            if (status != eCrestronSecureStorageStatus.Ok)
                return Fail(status, key);

            Flush();
            Debug.LogMessage(LogEventLevel.Information, this, "Removed secret {0}:{1}", Key, key);

            UpdateRegistry(key, add: false);
            return SecretStoreResult.Ok();
        }
    }

    /// <inheritdoc />
    public int GetSecretLength(string key)
        => !String.IsNullOrWhiteSpace(key) && Retrieve(key, out var value) == eCrestronSecureStorageStatus.Ok
            ? value.Length
            : -1;

    private bool ReadsBack(string key, string expected)
        => Retrieve(key, out var actual) == eCrestronSecureStorageStatus.Ok
            && String.Equals(actual, expected, StringComparison.Ordinal);

    private eCrestronSecureStorageStatus Retrieve(string key, out string value)
    {
        value = null;

        var status = CrestronSecureStorage.Retrieve(key, GlobalStore, Entropy(key), out var data);
        if (status != eCrestronSecureStorageStatus.Ok)
            return status;

        if (data == null)
            return eCrestronSecureStorageStatus.RetrieveFailure;

        value = Encoding.UTF8.GetString(data);
        return status;
    }

    /// <summary>
    /// Reads the key list. A missing list is an empty one.
    /// </summary>
    private bool TryReadRegistry(out System.Collections.Generic.IList<string> keys, out string errorCode)
    {
        keys = new System.Collections.Generic.List<string>();
        errorCode = null;

        var status = Retrieve(SecureStorageKeyRegistry.RecordKey, out var json);
        if (status == eCrestronSecureStorageStatus.DeviceUnavailable)
        {
            // The store itself is down, so an empty list here would be a false "no secrets" -
            // and pruning the index against it would drop entries for secrets that exist.
            errorCode = DeviceUnavailable;
            return false;
        }

        if (status != eCrestronSecureStorageStatus.Ok)
        {
            // Secure storage has no "not found" status, so a list that was never written and one
            // that cannot be read look alike. Either way it is treated as empty; the secrets
            // themselves are untouched, and the next write writes the list again.
            Debug.LogMessage(LogEventLevel.Debug, this, "No key list read for {0} - {1}", Key, status);
            return true;
        }

        if (SecureStorageKeyRegistry.TryParse(json, out keys))
            return true;

        errorCode = "keyListUnreadable";
        Debug.LogMessage(LogEventLevel.Warning, this, "The key list for {0} could not be read; listing is unavailable until a secret is written", Key);
        return false;
    }

    /// <summary>
    /// Adds or removes a key from the key list, and reads the list back. Must be called under
    /// <see cref="_sync"/>.
    /// </summary>
    /// <returns>True when the list holds the change, including when nothing needed changing.</returns>
    /// <remarks>
    /// A failure is logged here. <see cref="WriteSecret"/> undoes a new secret the list can't hold;
    /// after a delete the secret is already gone, so a stale entry is only a stale listing.
    /// </remarks>
    private bool UpdateRegistry(string key, bool add)
    {
        if (!TryReadRegistry(out var keys, out var listError) && listError == DeviceUnavailable)
        {
            Debug.LogMessage(LogEventLevel.Warning, this, "Secure storage is unavailable; the key list for {0} was not updated after changing {1}", Key, key);
            return false;
        }

        var updated = add
            ? SecureStorageKeyRegistry.WithKey(keys, key)
            : SecureStorageKeyRegistry.WithoutKey(keys, key);

        if (updated == null)
            return true;

        var json = SecureStorageKeyRegistry.Serialize(updated);
        if (Encoding.UTF8.GetByteCount(json) > MaxValueLength)
        {
            Debug.LogMessage(LogEventLevel.Warning, this, "The key list for {0} would exceed {1} bytes; it was not updated after changing {2}", Key, MaxValueLength, key);
            return false;
        }

        var status = CrestronSecureStorage.Store(
            SecureStorageKeyRegistry.RecordKey, GlobalStore, Encoding.UTF8.GetBytes(json), Entropy(SecureStorageKeyRegistry.RecordKey));

        if (status == eCrestronSecureStorageStatus.Ok && ReadsBack(SecureStorageKeyRegistry.RecordKey, json))
        {
            Flush();
            return true;
        }

        Debug.LogMessage(LogEventLevel.Warning, this, "Unable to update the key list for {0} after changing {1} - {2}", Key, key, status);
        return false;
    }

    private void Flush()
    {
        if (!CrestronSecureStorage.Flush())
            Debug.LogMessage(LogEventLevel.Warning, this, "Crestron secure storage did not confirm the flush to permanent storage for {0}", Key);
    }

    private static bool IsRegistryKey(string key)
        => String.Equals(key, SecureStorageKeyRegistry.RecordKey, StringComparison.Ordinal);

    private static SecretStoreResult ReservedKey(string key)
        => SecretStoreResult.Fail("reservedKey", String.Format("'{0}' is reserved for internal use.", key));

    private SecretStoreResult Fail(eCrestronSecureStorageStatus status, string key)
    {
        Debug.LogMessage(LogEventLevel.Information, this, "Secure storage refused {0}:{1} - {2}", Key, key, status);
        return SecretStoreResult.Fail(status.ToString(),
            String.Format("Crestron secure storage returned {0} for '{1}'.", status, key));
    }

    private static byte[] Entropy(string key) => Encoding.UTF8.GetBytes(key);
}
