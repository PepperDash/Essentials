using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Stores secrets encrypted, in Crestron secure storage's local space, kept apart per program number.
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
/// Local secure storage lives in the Data Store, which keeps records per program name: programs
/// with the same name (two Essentials programs, say) share them, and other programs can list the
/// record names but not read them. So each secret is stored under this program's number as well
/// as its key (see <see cref="Tag"/>), and each Essentials program sees only its own.
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
    /// Secure storage keeps its records in the program's local Data Store, which has a quota of
    /// about 10,000 bytes per program name (the DATASTORESTATUS console command shows bytes and
    /// percent used). Crestron does not refuse a write over the quota: on an RMC4 (firmware
    /// 2.8006.00110) one 8,192-byte secret took the program to 114%, and writes of tens of
    /// kilobytes left the Data Store unusable until a factory restore.
    /// </para>
    /// <para>
    /// So the limits leave room for everything else the program stores there. 4,096 holds an RSA
    /// private key in PEM form (1,679 characters for 2048-bit, about 3,300 for 4096-bit) or a
    /// certificate. Do not raise these limits without a need for the larger value.
    /// </para>
    /// <para>
    /// Every write is also read back (see <see cref="WriteSecret"/>).
    /// </para>
    /// </remarks>
    public const int MaxSecureValueLength = 4096;

    /// <summary>
    /// Most bytes this provider stores in total: its secrets, the Secrets API's index records and its
    /// own key list. About 60% of the program's Data Store quota (see <see cref="MaxSecureValueLength"/>).
    /// </summary>
    public const int MaxSecureTotalLength = 6144;

    /// <summary>
    /// Room kept in <see cref="MaxSecureTotalLength"/> for the index to record a new secret.
    /// </summary>
    private const int IndexReserve = 512;

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
    public const int MaxSecureSecrets = 8;

    /// <summary>
    /// Secure storage's flag for the local space (kept per program name) rather than the global one.
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

    /// <inheritdoc />
    public bool Encrypted => true;

    /// <inheritdoc />
    public int? MaxTotalLength => MaxSecureTotalLength;

    /// <inheritdoc />
    public int? MaxSecrets => MaxSecureSecrets;

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
            var listAfter = SecureStorageKeyRegistry.Serialize(withKey ?? listed);
            if (withKey != null && Encoding.UTF8.GetByteCount(listAfter) > MaxValueLength)
            {
                return SecretStoreResult.Fail("keyListFull",
                    String.Format("The list of keys for {0} has reached its {1}-byte limit; delete a secret before adding another.", Key, MaxValueLength));
            }

            // Everything this provider stores shares the program's Data Store quota, so the total is
            // capped too: every listed record, the key list, and room for the index to record this
            // secret. Lengths only - no value is logged.
            var total = StoredBytesExcept(listed, key) + Encoding.UTF8.GetByteCount(listAfter) + bytes.Length
                + (SecretsIndex.IsIndexRecord(key) ? 0 : IndexReserve);
            if (total > MaxSecureTotalLength)
            {
                return SecretStoreResult.Fail("totalTooLarge",
                    String.Format("{0} holds at most {1} bytes in total, and this would make about {2}; delete or shorten a secret first.",
                        Key, MaxSecureTotalLength, total));
            }

            var status = CrestronSecureStorage.Store(Tag(key), GlobalStore, bytes, Entropy(key));
            if (status != eCrestronSecureStorageStatus.Ok)
                return Fail(status, key);

            // Store can report Ok for a value it cannot return (seen on an RMC4 with values far
            // above MaxSecureValueLength), so read it back. An unreadable value is no use to anyone, so it is removed rather than left
            // looking like a successful write.
            if (!ReadsBack(key, value))
            {
                CrestronSecureStorage.Delete(Tag(key), GlobalStore);
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
                CrestronSecureStorage.Delete(Tag(key), GlobalStore);
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
            var status = CrestronSecureStorage.Delete(Tag(key), GlobalStore);
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

    /// <summary>
    /// Bytes held by every listed record other than <paramref name="except"/>, which is about to be
    /// replaced. A record that can't be read counts as nothing.
    /// </summary>
    private long StoredBytesExcept(System.Collections.Generic.IEnumerable<string> keys, string except)
    {
        long total = 0;
        foreach (var listedKey in keys)
        {
            if (String.Equals(listedKey, except, StringComparison.Ordinal))
                continue;

            if (Retrieve(listedKey, out var stored) == eCrestronSecureStorageStatus.Ok && stored != null)
                total += Encoding.UTF8.GetByteCount(stored);
        }

        return total;
    }

    private bool ReadsBack(string key, string expected)
        => Retrieve(key, out var actual) == eCrestronSecureStorageStatus.Ok
            && String.Equals(actual, expected, StringComparison.Ordinal);

    private eCrestronSecureStorageStatus Retrieve(string key, out string value)
    {
        value = null;

        var status = CrestronSecureStorage.Retrieve(Tag(key), GlobalStore, Entropy(key), out var data);
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
            Tag(SecureStorageKeyRegistry.RecordKey), GlobalStore, Encoding.UTF8.GetBytes(json), Entropy(SecureStorageKeyRegistry.RecordKey));

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

    /// <summary>
    /// The secure storage tag for a key: the key with this program's number in front, e.g.
    /// "1:iotKey".
    /// </summary>
    /// <remarks>
    /// The Data Store keeps local records per program name, and programs with the same name, such
    /// as two Essentials programs, share them. The program number keeps each program's secrets,
    /// key list and index apart, the same way Mobile Control names its token record
    /// ("{ProgramNumber}:{Key}-tokens"). Callers, configs and the Secrets API use the plain key.
    /// </remarks>
    private static string Tag(string key)
        => String.Format("{0}:{1}", InitialParametersClass.ApplicationNumber, key);

    /// <summary>Supplemental encryption key for a value: the tagged key.</summary>
    private static byte[] Entropy(string key) => Encoding.UTF8.GetBytes(Tag(key));
}
