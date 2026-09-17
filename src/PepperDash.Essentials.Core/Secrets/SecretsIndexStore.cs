using System;
using System.Collections.Generic;
using System.Text;
using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Reads and writes the sidecar index across chunked Data Store records.
/// </summary>
/// <remarks>
/// The Crestron-facing half of <see cref="SecretsIndex"/>, which holds the pure logic. Every method
/// here is written so that a damaged index degrades to "nothing is managed" rather than to an
/// error: the index classifies secrets, it does not store them.
/// </remarks>
public static class SecretsIndexStore
{
    /// <summary>
    /// Result of attempting to read an index.
    /// </summary>
    public class ReadResult
    {
        /// <summary>Gets the entries, empty when the index is missing or unusable.</summary>
        public IList<SecretsIndexEntry> Entries { get; internal set; }

        /// <summary>Gets "ok", "missing", or a "corrupt:*" reason.</summary>
        public string Status { get; internal set; }

        /// <summary>Gets non-fatal problems found while sanitizing entries.</summary>
        public IList<string> Warnings { get; internal set; }

        /// <summary>Gets how many chunk records the index currently occupies.</summary>
        public int ChunkCount { get; internal set; }
    }

    /// <summary>
    /// Reads the index. Never throws, never deletes, never reports failure to the caller as an error.
    /// </summary>
    public static ReadResult Read(IEnumerableSecretProvider provider)
    {
        var warnings = new List<string>();
        var result = new ReadResult
        {
            Entries = new List<SecretsIndexEntry>(),
            Status = "missing",
            Warnings = warnings,
            ChunkCount = 0
        };

        try
        {
            var headerSecret = provider.GetSecret(SecretsIndex.HeaderKey);
            var header = SecretsIndex.ParseHeader(headerSecret?.Value as string);

            if (headerSecret == null)
                return result;

            if (header == null)
            {
                result.Status = "corrupt:header";
                return result;
            }

            result.ChunkCount = header.Chunks;

            var builder = new StringBuilder(header.Length);
            for (var i = 0; i < header.Chunks; i++)
            {
                var chunk = provider.GetSecret(SecretsIndex.ChunkKey(i));
                if (chunk?.Value is not string text)
                {
                    result.Status = "corrupt:missingChunk";
                    return result;
                }
                builder.Append(text);
            }

            var document = SecretsIndex.ParseDocument(header, builder.ToString(), out var status);
            if (document == null)
            {
                result.Status = status ?? "corrupt:unparseable";
                return result;
            }

            result.Entries = SecretsIndex.Sanitize(document.Entries, warnings);
            result.Status = "ok";
            return result;
        }
        catch (Exception ex)
        {
            // A read failure must never take down a list request - report it as unusable and let
            // every key show as unmanaged.
            Debug.LogMessage(ex, "Unable to read the secrets index for {provider}", null, provider.Key);
            result.Entries = new List<SecretsIndexEntry>();
            result.Status = "corrupt:unreadable";
            return result;
        }
    }

    /// <summary>
    /// Writes the index, replacing whatever was there.
    /// </summary>
    /// <remarks>
    /// Chunks are written first and the header last. A crash partway leaves the previous header,
    /// whose length and checksum will not match the new chunk contents - so the next read reports
    /// <c>corrupt:checksum</c> and degrades to "missing" rather than handing back a document
    /// assembled from a mix of two writes.
    /// </remarks>
    /// <param name="provider">Provider owning the Data Store space.</param>
    /// <param name="entries">Entries to persist.</param>
    /// <param name="previousChunkCount">Chunk count before this write, so leftovers can be removed.</param>
    /// <returns>A failed result when the index will not fit, or the store refused a write.</returns>
    public static SecretStoreResult Write(
        IEnumerableSecretProvider provider,
        IList<SecretsIndexEntry> entries,
        int previousChunkCount)
    {
        try
        {
            var document = new SecretsIndexDocument
            {
                Version = SecretsIndex.Version,
                Entries = new List<SecretsIndexEntry>(entries ?? new List<SecretsIndexEntry>())
            };

            var payload = SecretsIndex.Serialize(document);
            var chunks = SecretsIndex.Split(payload);

            if (chunks.Count > SecretsIndex.MaxChunks)
            {
                // Refuse rather than truncate: a silently shortened index would mark real secrets
                // unmanaged, which looks like data loss.
                return SecretStoreResult.Fail(
                    "indexFull",
                    $"The secrets index needs {chunks.Count} records but only {SecretsIndex.MaxChunks} are available.");
            }

            for (var i = 0; i < chunks.Count; i++)
            {
                var write = provider.WriteSecret(SecretsIndex.ChunkKey(i), chunks[i]);
                if (!write.Success)
                    return write;
            }

            var header = new SecretsIndexHeader
            {
                Version = SecretsIndex.Version,
                Chunks = chunks.Count,
                Length = payload.Length,
                Checksum = SecretsIndex.Checksum(payload),
                UpdatedUtc = DateTime.UtcNow
            };

            var headerWrite = provider.WriteSecret(
                SecretsIndex.HeaderKey, SecretsIndex.SerializeHeader(header));
            if (!headerWrite.Success)
                return headerWrite;

            // Drop chunk records the shrunken index no longer uses. A leftover is harmless - the
            // header says how many to read - so a failure here is logged, not surfaced.
            for (var i = chunks.Count; i < previousChunkCount; i++)
            {
                var delete = provider.DeleteSecret(SecretsIndex.ChunkKey(i));
                if (!delete.Success)
                {
                    Debug.LogMessage(LogEventLevel.Debug,
                        "Left a stale secrets index chunk {index} in place for {provider}: {reason}",
                        null, i, provider.Key, delete.ErrorCode);
                }
            }

            return SecretStoreResult.Ok();
        }
        catch (Exception ex)
        {
            Debug.LogMessage(ex, "Unable to write the secrets index for {provider}", null, provider.Key);
            return SecretStoreResult.Fail("indexWriteFailed", ex.Message);
        }
    }

    /// <summary>
    /// Removes every index record, returning the provider to an unindexed state.
    /// </summary>
    public static void Clear(IEnumerableSecretProvider provider, int chunkCount)
    {
        try
        {
            provider.DeleteSecret(SecretsIndex.HeaderKey);
            for (var i = 0; i < Math.Max(chunkCount, SecretsIndex.MaxChunks); i++)
            {
                provider.DeleteSecret(SecretsIndex.ChunkKey(i));
            }
        }
        catch (Exception ex)
        {
            Debug.LogMessage(ex, "Unable to clear the secrets index for {provider}", null, provider.Key);
        }
    }
}
