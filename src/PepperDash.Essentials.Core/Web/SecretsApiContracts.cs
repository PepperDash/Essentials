using System.Collections.Generic;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Web;

/// <summary>
/// Request and response shapes for the secrets API.
/// </summary>
/// <remarks>
/// Mirrored on the client by <c>src/store/secretsContract.ts</c> in the developer tools app.
/// <para>
/// <b>No response type here has a value field, and none may be added.</b> The processor is
/// write-only for secret values: a forgotten credential must be re-entered, never recovered. That
/// property is what caps the damage of an API that has no role model behind it.
/// </para>
/// </remarks>
public static class SecretsApiContracts
{
    /// <summary>Largest batch the bulk endpoint will consider.</summary>
    public const int MaxBulkEntries = 200;
}

// ─── Providers ───────────────────────────────────────────────────────────────

/// <summary>Describes one registered secret provider.</summary>
public class SecretProviderInfo
{
    /// <summary>Gets or sets the provider's key, as used in device configs.</summary>
    [JsonProperty("key")]
    public string Key { get; set; }

    /// <summary>Gets or sets the provider's human description.</summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }

    /// <summary>Gets or sets "local" or "global".</summary>
    [JsonProperty("scope")]
    public string Scope { get; set; }

    /// <summary>Gets or sets whether this provider can list its keys.</summary>
    [JsonProperty("enumerationSupported")]
    public bool EnumerationSupported { get; set; }

    /// <summary>Gets or sets the Data Store key length limit.</summary>
    [JsonProperty("maxKeyLength")]
    public int MaxKeyLength { get; set; }

    /// <summary>Gets or sets the Data Store value length limit.</summary>
    [JsonProperty("maxValueLength")]
    public int MaxValueLength { get; set; }
}

/// <summary>Response for the providers endpoint.</summary>
public class SecretsProvidersResponse
{
    /// <summary>Gets or sets the registered providers.</summary>
    [JsonProperty("providers")]
    public List<SecretProviderInfo> Providers { get; set; }

    /// <summary>Gets or sets the failure detail when the request could not be served.</summary>
    [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
    public SecretsApiError Error { get; set; }
}

// ─── Listing ─────────────────────────────────────────────────────────────────

/// <summary>One record found in a provider's Data Store space. Never carries a value.</summary>
public class SecretInfo
{
    /// <summary>Gets or sets the Data Store key.</summary>
    [JsonProperty("key")]
    public string Key { get; set; }

    /// <summary>Gets or sets whether this key was written through the secrets API.</summary>
    [JsonProperty("managed")]
    public bool Managed { get; set; }

    /// <summary>Gets or sets the note recorded in the index.</summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }

    /// <summary>Gets or sets when the API first wrote this secret.</summary>
    [JsonProperty("createdUtc", NullValueHandling = NullValueHandling.Ignore)]
    public string CreatedUtc { get; set; }

    /// <summary>Gets or sets when the API last wrote this secret.</summary>
    [JsonProperty("updatedUtc", NullValueHandling = NullValueHandling.Ignore)]
    public string UpdatedUtc { get; set; }

    /// <summary>Gets or sets the record's own last-modified time, independent of the index.</summary>
    [JsonProperty("lastModifiedUtc", NullValueHandling = NullValueHandling.Ignore)]
    public string LastModifiedUtc { get; set; }

    /// <summary>Gets or sets the creating application, meaningful in the global scope.</summary>
    [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
    public string Owner { get; set; }

    /// <summary>Gets or sets the Data Store record type.</summary>
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public string Type { get; set; }

    /// <summary>Gets or sets whether this is one of the API's own index records.</summary>
    [JsonProperty("reserved", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Reserved { get; set; }

    /// <summary>Gets or sets the value's character count, when explicitly requested.</summary>
    [JsonProperty("valueLength", NullValueHandling = NullValueHandling.Ignore)]
    public int? ValueLength { get; set; }
}

/// <summary>An index entry with no matching record in the store.</summary>
public class StaleIndexEntryInfo
{
    /// <summary>Gets or sets the key the index still lists.</summary>
    [JsonProperty("key")]
    public string Key { get; set; }

    /// <summary>Gets or sets the recorded description.</summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }

    /// <summary>Gets or sets when the entry was created.</summary>
    [JsonProperty("createdUtc", NullValueHandling = NullValueHandling.Ignore)]
    public string CreatedUtc { get; set; }
}

/// <summary>Counts for the listing.</summary>
public class SecretsCounts
{
    /// <summary>Gets or sets the number of records listed.</summary>
    [JsonProperty("total")]
    public int Total { get; set; }

    /// <summary>Gets or sets how many are known to the index.</summary>
    [JsonProperty("managed")]
    public int Managed { get; set; }

    /// <summary>Gets or sets how many exist but are not in the index.</summary>
    [JsonProperty("unmanaged")]
    public int Unmanaged { get; set; }

    /// <summary>Gets or sets how many index entries have no record.</summary>
    [JsonProperty("stale")]
    public int Stale { get; set; }
}

/// <summary>Response for the list endpoint.</summary>
public class SecretsListResponse
{
    /// <summary>Gets or sets the provider listed.</summary>
    [JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)]
    public string Provider { get; set; }

    /// <summary>Gets or sets the provider's Data Store scope.</summary>
    [JsonProperty("scope", NullValueHandling = NullValueHandling.Ignore)]
    public string Scope { get; set; }

    /// <summary>Gets or sets "ok", "missing", or a "corrupt:*" reason.</summary>
    [JsonProperty("indexStatus", NullValueHandling = NullValueHandling.Ignore)]
    public string IndexStatus { get; set; }

    /// <summary>
    /// Gets or sets whether the Data Store walk reached the end. When false the list is partial,
    /// and pruning is refused.
    /// </summary>
    [JsonProperty("enumerationComplete")]
    public bool EnumerationComplete { get; set; }

    /// <summary>Gets or sets the counts.</summary>
    [JsonProperty("counts", NullValueHandling = NullValueHandling.Ignore)]
    public SecretsCounts Counts { get; set; }

    /// <summary>Gets or sets the records.</summary>
    [JsonProperty("secrets", NullValueHandling = NullValueHandling.Ignore)]
    public List<SecretInfo> Secrets { get; set; }

    /// <summary>Gets or sets index entries with no matching record.</summary>
    [JsonProperty("staleIndexEntries", NullValueHandling = NullValueHandling.Ignore)]
    public List<StaleIndexEntryInfo> StaleIndexEntries { get; set; }

    /// <summary>Gets or sets non-fatal problems encountered.</summary>
    [JsonProperty("warnings", NullValueHandling = NullValueHandling.Ignore)]
    public List<string> Warnings { get; set; }

    /// <summary>Gets or sets an explanation when the list is empty by nature.</summary>
    [JsonProperty("notice", NullValueHandling = NullValueHandling.Ignore)]
    public string Notice { get; set; }

    /// <summary>Gets or sets the failure detail.</summary>
    [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
    public SecretsApiError Error { get; set; }
}

// ─── Commands ────────────────────────────────────────────────────────────────

/// <summary>A single-secret command.</summary>
public class SecretCommandRequest
{
    /// <summary>Gets or sets set, update, delete, test, pruneIndex or rebuildIndex.</summary>
    [JsonProperty("action")]
    public string Action { get; set; }

    /// <summary>Gets or sets the provider key.</summary>
    [JsonProperty("provider")]
    public string Provider { get; set; }

    /// <summary>Gets or sets the secret key.</summary>
    [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
    public string Key { get; set; }

    /// <summary>Gets or sets the secret value. Never echoed back in any response.</summary>
    [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
    public string Value { get; set; }

    /// <summary>Gets or sets an optional note about what the secret is for.</summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }

    /// <summary>Gets or sets whether a set may replace an existing key.</summary>
    [JsonProperty("overwrite")]
    public bool Overwrite { get; set; }

    /// <summary>
    /// Gets or sets keys to adopt into the index during a rebuild, marking existing records as
    /// managed without reading or changing their values.
    /// </summary>
    [JsonProperty("adoptKeys", NullValueHandling = NullValueHandling.Ignore)]
    public List<string> AdoptKeys { get; set; }
}

/// <summary>Result of a single-secret command.</summary>
public class SecretCommandResponse
{
    /// <summary>Gets or sets "ok" or "error".</summary>
    [JsonProperty("status")]
    public string Status { get; set; }

    /// <summary>Gets or sets the echoed action.</summary>
    [JsonProperty("action", NullValueHandling = NullValueHandling.Ignore)]
    public string Action { get; set; }

    /// <summary>Gets or sets the provider acted on.</summary>
    [JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)]
    public string Provider { get; set; }

    /// <summary>Gets or sets the key acted on.</summary>
    [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
    public string Key { get; set; }

    /// <summary>Gets or sets whether the key already existed.</summary>
    [JsonProperty("existedBefore", NullValueHandling = NullValueHandling.Ignore)]
    public bool? ExistedBefore { get; set; }

    /// <summary>Gets or sets the result of a test action.</summary>
    [JsonProperty("exists", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Exists { get; set; }

    /// <summary>
    /// Gets or sets whether the index was updated. False means the secret was written but its
    /// metadata was not - a warning, not a failure.
    /// </summary>
    [JsonProperty("indexUpdated", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IndexUpdated { get; set; }

    /// <summary>Gets or sets how many entries survived a rebuild.</summary>
    [JsonProperty("entriesRetained", NullValueHandling = NullValueHandling.Ignore)]
    public int? EntriesRetained { get; set; }

    /// <summary>Gets or sets how many stale entries a prune removed.</summary>
    [JsonProperty("entriesRemoved", NullValueHandling = NullValueHandling.Ignore)]
    public int? EntriesRemoved { get; set; }

    /// <summary>Gets or sets a non-fatal note.</summary>
    [JsonProperty("warning", NullValueHandling = NullValueHandling.Ignore)]
    public string Warning { get; set; }

    /// <summary>Gets or sets the failure detail.</summary>
    [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
    public SecretsApiError Error { get; set; }
}

// ─── Bulk ────────────────────────────────────────────────────────────────────

/// <summary>One entry in a bulk request.</summary>
public class BulkSecretEntry
{
    /// <summary>Gets or sets the secret key.</summary>
    [JsonProperty("key")]
    public string Key { get; set; }

    /// <summary>Gets or sets the secret value.</summary>
    [JsonProperty("value")]
    public string Value { get; set; }

    /// <summary>Gets or sets an optional per-entry provider override.</summary>
    [JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)]
    public string Provider { get; set; }

    /// <summary>Gets or sets an optional note.</summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }
}

/// <summary>A bulk apply request.</summary>
public class BulkSecretsRequest
{
    /// <summary>Gets or sets "preview" or "commit". Preview writes nothing.</summary>
    [JsonProperty("mode")]
    public string Mode { get; set; }

    /// <summary>Gets or sets the default provider for entries that do not name one.</summary>
    [JsonProperty("provider")]
    public string Provider { get; set; }

    /// <summary>Gets or sets whether existing keys may be replaced.</summary>
    [JsonProperty("overwrite")]
    public bool Overwrite { get; set; }

    /// <summary>
    /// Gets or sets whether an entry may overwrite a key the index does not manage. Off by default:
    /// this is what stops a round-tripped template destroying another subsystem's records.
    /// </summary>
    [JsonProperty("allowUnmanagedOverwrite")]
    public bool AllowUnmanagedOverwrite { get; set; }

    /// <summary>
    /// Gets or sets the entries. Accepts either an array of <see cref="BulkSecretEntry"/> or a flat
    /// key-to-value map, which is what the template endpoint emits.
    /// </summary>
    [JsonProperty("secrets")]
    public Newtonsoft.Json.Linq.JToken Secrets { get; set; }
}

/// <summary>What happened, or would happen, to one bulk entry.</summary>
public class BulkEntryResult
{
    /// <summary>Gets or sets the entry's position in the request.</summary>
    [JsonProperty("index")]
    public int Index { get; set; }

    /// <summary>Gets or sets the key.</summary>
    [JsonProperty("key")]
    public string Key { get; set; }

    /// <summary>Gets or sets the provider.</summary>
    [JsonProperty("provider")]
    public string Provider { get; set; }

    /// <summary>Gets or sets create, overwrite, skip, invalid or failed.</summary>
    [JsonProperty("action")]
    public string Action { get; set; }

    /// <summary>Gets or sets the machine-readable cause for skip, invalid and failed.</summary>
    [JsonProperty("reason", NullValueHandling = NullValueHandling.Ignore)]
    public string Reason { get; set; }

    /// <summary>Gets or sets the explanation. Never contains a value.</summary>
    [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
    public string Message { get; set; }

    /// <summary>Gets or sets whether the write actually happened. Always false in preview.</summary>
    [JsonProperty("applied")]
    public bool Applied { get; set; }
}

/// <summary>Totals for a bulk run.</summary>
public class BulkSummary
{
    /// <summary>Gets or sets the entry count.</summary>
    [JsonProperty("total")]
    public int Total { get; set; }

    /// <summary>Gets or sets how many would be, or were, created.</summary>
    [JsonProperty("create")]
    public int Create { get; set; }

    /// <summary>Gets or sets how many would be, or were, replaced.</summary>
    [JsonProperty("overwrite")]
    public int Overwrite { get; set; }

    /// <summary>Gets or sets how many were left alone.</summary>
    [JsonProperty("skip")]
    public int Skip { get; set; }

    /// <summary>Gets or sets how many failed validation.</summary>
    [JsonProperty("invalid")]
    public int Invalid { get; set; }

    /// <summary>Gets or sets how many the store refused during a commit.</summary>
    [JsonProperty("failed")]
    public int Failed { get; set; }
}

/// <summary>Result of a bulk run.</summary>
public class BulkSecretsResponse
{
    /// <summary>Gets or sets the echoed mode.</summary>
    [JsonProperty("mode", NullValueHandling = NullValueHandling.Ignore)]
    public string Mode { get; set; }

    /// <summary>Gets or sets the default provider.</summary>
    [JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)]
    public string Provider { get; set; }

    /// <summary>Gets or sets the overwrite flag in effect.</summary>
    [JsonProperty("overwrite")]
    public bool Overwrite { get; set; }

    /// <summary>Gets or sets the totals.</summary>
    [JsonProperty("summary", NullValueHandling = NullValueHandling.Ignore)]
    public BulkSummary Summary { get; set; }

    /// <summary>Gets or sets the per-entry outcomes.</summary>
    [JsonProperty("entries", NullValueHandling = NullValueHandling.Ignore)]
    public List<BulkEntryResult> Entries { get; set; }

    /// <summary>Gets or sets whether the index was rewritten after the batch.</summary>
    [JsonProperty("indexUpdated", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IndexUpdated { get; set; }

    /// <summary>Gets or sets non-fatal notes.</summary>
    [JsonProperty("warnings", NullValueHandling = NullValueHandling.Ignore)]
    public List<string> Warnings { get; set; }

    /// <summary>Gets or sets the failure detail.</summary>
    [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
    public SecretsApiError Error { get; set; }
}

// ─── Template ────────────────────────────────────────────────────────────────

/// <summary>Metadata accompanying a downloaded template.</summary>
public class SecretsTemplateMetadata
{
    /// <summary>Gets or sets the key.</summary>
    [JsonProperty("key")]
    public string Key { get; set; }

    /// <summary>Gets or sets the provider.</summary>
    [JsonProperty("provider")]
    public string Provider { get; set; }

    /// <summary>Gets or sets the recorded description.</summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }

    /// <summary>Gets or sets whether the key is managed by the API.</summary>
    [JsonProperty("managed")]
    public bool Managed { get; set; }
}

/// <summary>A fill-in-the-blanks secrets file.</summary>
public class SecretsTemplateResponse
{
    /// <summary>Gets or sets the provider the template describes.</summary>
    [JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)]
    public string Provider { get; set; }

    /// <summary>Gets or sets when it was generated.</summary>
    [JsonProperty("generatedUtc", NullValueHandling = NullValueHandling.Ignore)]
    public string GeneratedUtc { get; set; }

    /// <summary>Gets or sets a note for whoever opens the file.</summary>
    [JsonProperty("note", NullValueHandling = NullValueHandling.Ignore)]
    public string Note { get; set; }

    /// <summary>
    /// Gets or sets the flat key-to-blank-value map. Byte-for-byte what the bulk endpoint accepts,
    /// so the file round-trips: download, fill in, apply.
    /// </summary>
    [JsonProperty("secrets", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, string> Secrets { get; set; }

    /// <summary>Gets or sets the per-key metadata.</summary>
    [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
    public List<SecretsTemplateMetadata> Metadata { get; set; }

    /// <summary>Gets or sets the failure detail.</summary>
    [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
    public SecretsApiError Error { get; set; }
}

// ─── Errors ──────────────────────────────────────────────────────────────────

/// <summary>Machine-readable failure detail.</summary>
public class SecretsApiError
{
    /// <summary>Gets or sets a stable error code.</summary>
    [JsonProperty("code")]
    public string Code { get; set; }

    /// <summary>Gets or sets the explanation. Never contains a secret value.</summary>
    [JsonProperty("message")]
    public string Message { get; set; }

    /// <summary>Gets or sets the offending request field, where one applies.</summary>
    [JsonProperty("field", NullValueHandling = NullValueHandling.Ignore)]
    public string Field { get; set; }
}
