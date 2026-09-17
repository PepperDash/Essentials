// This file is also compiled into PepperDash.Essentials.Core.Tests, which enables nullable
// reference types. Core does not, so the annotations here would be read inconsistently between the
// two. Disabling explicitly keeps the behaviour identical in both, and is a no-op in Core.
#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core;

/// <summary>
/// The sidecar index that records which Data Store keys were written through the secrets API.
/// </summary>
/// <remarks>
/// <para>
/// The Data Store namespace is flat and shared, and a secret's storage key is used verbatim by
/// device configs, so it cannot be namespaced or prefixed. The index therefore lives beside the
/// secrets rather than encoding anything into them: it classifies keys and carries metadata the
/// store has no room for, and nothing else depends on it.
/// </para>
/// <para>
/// It is advisory. Losing or corrupting it means every key reports as unmanaged - a degraded
/// display, never a broken system, and never a reason to refuse a write.
/// </para>
/// <para>
/// This file has no Crestron references by design, so the chunking, checksum and merge logic can be
/// unit tested off-target. Anything touching the Data Store belongs in
/// <see cref="SecretsIndexStore"/>.
/// </para>
/// </remarks>
public static class SecretsIndex
{
    /// <summary>Current document version.</summary>
    public const int Version = 1;

    /// <summary>Record name holding the header. Also the prefix that marks a key reserved.</summary>
    public const string HeaderKey = "__essSecretsIdx";

    /// <summary>
    /// Characters per chunk. The Data Store caps a value at 1600; 1200 leaves headroom so a
    /// miscount can never silently truncate a write.
    /// </summary>
    public const int ChunkSize = 1200;

    /// <summary>
    /// Maximum chunks, bounding both the index size and the reserved key range
    /// (<c>__essSecretsIdx00</c> … <c>__essSecretsIdx15</c>, all within the 32-character key cap).
    /// </summary>
    public const int MaxChunks = 16;

    /// <summary>Longest description retained per entry, to keep the document small.</summary>
    public const int MaxDescriptionLength = 120;

    /// <summary>
    /// Builds the record name for a chunk.
    /// </summary>
    public static string ChunkKey(int index) => $"{HeaderKey}{index:00}";

    /// <summary>
    /// True when a key belongs to the index and must never be written as a secret.
    /// </summary>
    public static bool IsReservedKey(string key)
        => !string.IsNullOrEmpty(key) && key.StartsWith(HeaderKey, StringComparison.Ordinal);

    /// <remarks>
    /// <c>EscapeNonAscii</c> guarantees one character per byte, so the chunk arithmetic is correct
    /// however the Data Store counts its 1600-character limit.
    /// </remarks>
    private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
    {
        Formatting = Formatting.None,
        NullValueHandling = NullValueHandling.Ignore,
        StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
    };

    /// <summary>
    /// Serializes a document to its wire form, compactly and with only ASCII characters.
    /// </summary>
    public static string Serialize(SecretsIndexDocument document)
        => JsonConvert.SerializeObject(document, SerializerSettings);

    /// <summary>
    /// Serializes a header to its wire form.
    /// </summary>
    public static string SerializeHeader(SecretsIndexHeader header)
        => JsonConvert.SerializeObject(header, SerializerSettings);

    /// <summary>
    /// Splits a serialized document into chunk-sized pieces.
    /// </summary>
    public static IList<string> Split(string payload)
    {
        var chunks = new List<string>();
        if (string.IsNullOrEmpty(payload))
            return chunks;

        for (var offset = 0; offset < payload.Length; offset += ChunkSize)
        {
            chunks.Add(payload.Substring(offset, Math.Min(ChunkSize, payload.Length - offset)));
        }
        return chunks;
    }

    /// <summary>
    /// A non-cryptographic checksum (FNV-1a) over the serialized document.
    /// </summary>
    /// <remarks>
    /// Guards against a torn write, not against tampering. Anyone who can write the chunks can
    /// write the header too; the point is to notice when the header and the chunks disagree because
    /// a write was interrupted partway.
    /// </remarks>
    public static string Checksum(string payload)
    {
        unchecked
        {
            const uint offsetBasis = 2166136261;
            const uint prime = 16777619;

            var hash = offsetBasis;
            foreach (var c in payload ?? string.Empty)
            {
                hash ^= c;
                hash *= prime;
            }
            return hash.ToString("x8");
        }
    }

    /// <summary>
    /// Parses a header record. Returns null when it is absent or unusable.
    /// </summary>
    public static SecretsIndexHeader ParseHeader(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            var header = JsonConvert.DeserializeObject<SecretsIndexHeader>(json);
            if (header == null || header.Version != Version)
                return null;
            if (header.Chunks < 0 || header.Chunks > MaxChunks)
                return null;
            return header;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Validates reassembled chunks against a header and parses the document.
    /// </summary>
    /// <param name="header">The header describing the expected payload.</param>
    /// <param name="payload">The reassembled chunk text.</param>
    /// <param name="status">On failure, a <c>corrupt:*</c> reason; otherwise null.</param>
    /// <returns>The parsed document, or null when it could not be trusted.</returns>
    public static SecretsIndexDocument ParseDocument(
        SecretsIndexHeader header,
        string payload,
        out string status)
    {
        status = null;

        if (header == null)
        {
            status = "corrupt:header";
            return null;
        }

        if (payload == null)
        {
            status = "corrupt:missingChunk";
            return null;
        }

        if (payload.Length != header.Length || Checksum(payload) != header.Checksum)
        {
            // The header and the chunks disagree, which is what a torn write looks like.
            status = "corrupt:checksum";
            return null;
        }

        try
        {
            var document = JsonConvert.DeserializeObject<SecretsIndexDocument>(payload);
            if (document == null)
            {
                status = "corrupt:unparseable";
                return null;
            }
            document.Entries ??= new List<SecretsIndexEntry>();
            return document;
        }
        catch (JsonException)
        {
            status = "corrupt:unparseable";
            return null;
        }
    }

    /// <summary>
    /// Removes entries that cannot be trusted, reporting what was dropped.
    /// </summary>
    /// <remarks>
    /// An entry with no key is meaningless; a duplicate key is ambiguous. Both are dropped rather
    /// than rejecting the whole index, since a single bad entry should not hide every good one.
    /// </remarks>
    public static IList<SecretsIndexEntry> Sanitize(
        IEnumerable<SecretsIndexEntry> entries,
        IList<string> warnings)
    {
        var result = new List<SecretsIndexEntry>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries ?? Enumerable.Empty<SecretsIndexEntry>())
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.Key))
            {
                warnings?.Add("Dropped an index entry with no key.");
                continue;
            }
            if (!seen.Add(entry.Key))
            {
                warnings?.Add($"Dropped a duplicate index entry for '{entry.Key}'.");
                continue;
            }
            result.Add(entry);
        }

        return result;
    }

    /// <summary>
    /// Truncates a description to the retained length, or returns null when there is none.
    /// </summary>
    public static string NormalizeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        var trimmed = description.Trim();
        return trimmed.Length <= MaxDescriptionLength
            ? trimmed
            : trimmed.Substring(0, MaxDescriptionLength);
    }

    /// <summary>
    /// Finds index entries that no longer have a matching record in the store.
    /// </summary>
    /// <remarks>
    /// Only meaningful against a complete enumeration - a partial walk would report live secrets as
    /// stale. Callers must check <see cref="SecretStoreEnumeration.Complete"/> before pruning.
    /// </remarks>
    public static IList<SecretsIndexEntry> FindStale(
        IEnumerable<SecretsIndexEntry> entries,
        IEnumerable<string> existingKeys)
    {
        var existing = new HashSet<string>(
            existingKeys ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);

        return (entries ?? Enumerable.Empty<SecretsIndexEntry>())
            .Where(entry => entry != null && !existing.Contains(entry.Key))
            .ToList();
    }
}

/// <summary>
/// The header record, small enough to always fit one Data Store value.
/// </summary>
public class SecretsIndexHeader
{
    /// <summary>Gets or sets the document version.</summary>
    [JsonProperty("v")]
    public int Version { get; set; }

    /// <summary>Gets or sets how many chunk records make up the document.</summary>
    [JsonProperty("chunks")]
    public int Chunks { get; set; }

    /// <summary>Gets or sets the serialized document length, for torn-write detection.</summary>
    [JsonProperty("len")]
    public int Length { get; set; }

    /// <summary>Gets or sets the FNV-1a checksum of the serialized document.</summary>
    [JsonProperty("sum")]
    public string Checksum { get; set; }

    /// <summary>Gets or sets when the index was last written.</summary>
    [JsonProperty("updatedUtc")]
    public DateTime UpdatedUtc { get; set; }
}

/// <summary>
/// The index document, split across chunk records.
/// </summary>
public class SecretsIndexDocument
{
    /// <summary>Gets or sets the document version.</summary>
    [JsonProperty("version")]
    public int Version { get; set; } = SecretsIndex.Version;

    /// <summary>Gets or sets the managed entries.</summary>
    [JsonProperty("entries")]
    public List<SecretsIndexEntry> Entries { get; set; } = new List<SecretsIndexEntry>();
}

/// <summary>
/// Metadata for one key written through the secrets API. Never holds a value.
/// </summary>
public class SecretsIndexEntry
{
    /// <summary>Gets or sets the Data Store key. Stored verbatim, never prefixed.</summary>
    [JsonProperty("key")]
    public string Key { get; set; }

    /// <summary>Gets or sets an optional human note about what the secret is for.</summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }

    /// <summary>Gets or sets when the secret was first written through the API.</summary>
    [JsonProperty("createdUtc")]
    public DateTime CreatedUtc { get; set; }

    /// <summary>Gets or sets when it was last written through the API.</summary>
    [JsonProperty("updatedUtc")]
    public DateTime UpdatedUtc { get; set; }
}
