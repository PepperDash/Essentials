using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Which Crestron Data Store space a provider reads and writes.
/// </summary>
public enum SecretStoreScope
{
    /// <summary>Per-program-slot storage. Not visible to other applications.</summary>
    Local,

    /// <summary>Processor-wide storage, shared by every program slot.</summary>
    Global
}

/// <summary>
/// A secret provider that can list the keys it holds and report why an operation failed.
/// </summary>
/// <remarks>
/// <para>
/// Deliberately separate from <see cref="ISecretProvider"/> rather than added to it.
/// <see cref="ISecretProvider"/> is public API that plugins outside this repository implement, so
/// adding a member would be a breaking change. Callers feature-detect with a type check instead.
/// </para>
/// <para>
/// <see cref="WriteSecret"/> and <see cref="DeleteSecret"/> exist alongside
/// <see cref="ISecretProvider.SetSecret"/> for two reasons: they return a reason for failure rather
/// than a bare <c>bool</c>, and they refuse an empty key. An empty key is not a harmless no-op -
/// <c>clearLocal("")</c> deletes every record belonging to the application - so the guard lives in
/// the provider itself, where it holds even if a caller bypasses the API's own validation.
/// </para>
/// </remarks>
public interface IEnumerableSecretProvider : ISecretProvider
{
    /// <summary>
    /// Gets the Data Store space this provider uses.
    /// </summary>
    SecretStoreScope Scope { get; }

    /// <summary>
    /// Lists the records in this provider's Data Store space. Never returns a value.
    /// </summary>
    /// <remarks>
    /// The Data Store namespace is flat and shared, so this returns every record in the space -
    /// including records written by other subsystems. Classifying them is the caller's job.
    /// </remarks>
    SecretStoreEnumeration EnumerateKeys();

    /// <summary>
    /// Writes a secret, refusing an empty key or an empty value.
    /// </summary>
    SecretStoreResult WriteSecret(string key, string value);

    /// <summary>
    /// Deletes a secret, refusing an empty key.
    /// </summary>
    SecretStoreResult DeleteSecret(string key);

    /// <summary>
    /// Reads a record's value length without exposing the value, for diagnostics.
    /// </summary>
    /// <returns>The length, or -1 when the record does not exist.</returns>
    int GetSecretLength(string key);
}

/// <summary>
/// The outcome of a Data Store write or delete.
/// </summary>
public class SecretStoreResult
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool Success { get; private set; }

    /// <summary>
    /// Gets the raw <c>CDS_ERROR</c> name when the store refused, or a validation code such as
    /// <c>emptyKey</c>. Null on success.
    /// </summary>
    public string ErrorCode { get; private set; }

    /// <summary>
    /// Gets a human-readable explanation. Never contains a secret value.
    /// </summary>
    public string Message { get; private set; }

    private SecretStoreResult(bool success, string errorCode, string message)
    {
        Success = success;
        ErrorCode = errorCode;
        Message = message;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static SecretStoreResult Ok() => new SecretStoreResult(true, null, null);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static SecretStoreResult Fail(string errorCode, string message)
        => new SecretStoreResult(false, errorCode, message);
}

/// <summary>
/// One record discovered by a Data Store walk. Carries the key and its metadata, never its value.
/// </summary>
public class CdsRecord
{
    /// <summary>
    /// Gets the record name, which is the secret key.
    /// </summary>
    public string Key { get; private set; }

    /// <summary>
    /// Gets the application that created the record. Meaningful in the global scope, which is
    /// shared across program slots - another application's records cannot be deleted.
    /// </summary>
    public string Owner { get; private set; }

    /// <summary>
    /// Gets the record's last-modified time, in UTC.
    /// </summary>
    public DateTime? LastModifiedUtc { get; private set; }

    /// <summary>
    /// Gets the Data Store type name, e.g. "String".
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CdsRecord"/> class.
    /// </summary>
    public CdsRecord(string key, string owner, DateTime? lastModifiedUtc, string type)
    {
        Key = key;
        Owner = owner;
        LastModifiedUtc = lastModifiedUtc;
        Type = type;
    }
}

/// <summary>
/// The result of walking a Data Store space.
/// </summary>
public class SecretStoreEnumeration
{
    /// <summary>
    /// Gets the records discovered, in the order the store returned them.
    /// </summary>
    public IList<CdsRecord> Records { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the walk reached the end of the table.
    /// </summary>
    /// <remarks>
    /// Load-bearing. A partial walk is a partial view of the store, so pruning the index against it
    /// would delete entries for secrets that do exist. Callers must refuse to prune unless this is
    /// true.
    /// </remarks>
    public bool Complete { get; private set; }

    /// <summary>
    /// Gets the raw <c>CDS_ERROR</c> name that ended the walk early, or null.
    /// </summary>
    public string ErrorCode { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecretStoreEnumeration"/> class.
    /// </summary>
    public SecretStoreEnumeration(IList<CdsRecord> records, bool complete, string errorCode)
    {
        Records = records ?? new List<CdsRecord>();
        Complete = complete;
        ErrorCode = errorCode;
    }
}
