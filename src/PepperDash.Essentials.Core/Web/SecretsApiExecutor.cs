using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.Web;

/// <summary>
/// Validates and executes secrets API requests.
/// </summary>
/// <remarks>
/// <para>
/// HTTP-free, so all the logic reads in one place, following the same split as
/// <see cref="SecretsCommandRequestHandler"/>'s routing-command counterpart.
/// </para>
/// <para>
/// Every operation runs under one lock covering the whole walk, index read, mutate and index write
/// sequence. The console commands and CWS request handlers run on different threads and both reach
/// the same Data Store, so without this a list could observe an index mid-rewrite. No events fire
/// inside the lock, so there is no re-entrancy path.
/// </para>
/// </remarks>
public static class SecretsApiExecutor
{
    private static readonly object SyncRoot = new object();

    private const string ActionSet = "set";
    private const string ActionUpdate = "update";
    private const string ActionDelete = "delete";
    private const string ActionTest = "test";
    private const string ActionPruneIndex = "pruneIndex";
    private const string ActionRebuildIndex = "rebuildIndex";

    // ─── Providers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Lists the registered providers and their limits.
    /// </summary>
    public static (int StatusCode, SecretsProvidersResponse Response) ListProviders()
    {
        try
        {
            var providers = SecretsManager.Secrets
                .Where(pair => pair.Value != null)
                .Select(pair => new SecretProviderInfo
                {
                    Key = pair.Key,
                    Description = pair.Value.Description,
                    Scope = (pair.Value as IEnumerableSecretProvider)?.Scope.ToString().ToLowerInvariant(),
                    EnumerationSupported = pair.Value is IEnumerableSecretProvider,
                    MaxKeyLength = SecretProviderGuards.DocumentedMaxKeyLength,
                    MaxValueLength = SecretProviderGuards.MaxValueLength
                })
                .OrderBy(info => info.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return (200, new SecretsProvidersResponse { Providers = providers });
        }
        catch (Exception ex)
        {
            Debug.LogMessage(ex, "Error listing secret providers: {Exception}");
            return (500, new SecretsProvidersResponse
            {
                Error = Error("executionError", ex.Message)
            });
        }
    }

    // ─── Listing ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Lists the keys held by a provider, annotated with index metadata. Never returns a value.
    /// </summary>
    /// <param name="providerKey">Provider to list.</param>
    /// <param name="includeReserved">Include the API's own index records.</param>
    /// <param name="includeSizes">Include each value's character count.</param>
    public static (int StatusCode, SecretsListResponse Response) List(
        string providerKey,
        bool includeReserved,
        bool includeSizes)
    {
        if (string.IsNullOrWhiteSpace(providerKey))
        {
            return (400, new SecretsListResponse
            {
                Error = Error("missingField", "A provider is required.", "provider")
            });
        }

        var provider = SecretsManager.GetSecretProviderByKey(providerKey);
        if (provider == null)
        {
            return (404, new SecretsListResponse
            {
                Provider = providerKey,
                Error = Error("providerNotFound", $"No secret provider '{providerKey}'.", "provider")
            });
        }

        if (provider is not IEnumerableSecretProvider enumerable)
        {
            // Writable but not listable. Report that plainly rather than an empty list, which would
            // read as "no secrets stored".
            return (200, new SecretsListResponse
            {
                Provider = providerKey,
                EnumerationComplete = false,
                Secrets = new List<SecretInfo>(),
                Counts = new SecretsCounts(),
                Notice = $"Provider '{providerKey}' cannot list its keys."
            });
        }

        try
        {
            lock (SyncRoot)
            {
                var enumeration = enumerable.EnumerateKeys();
                var index = SecretsIndexStore.Read(enumerable);

                var byKey = index.Entries.ToDictionary(
                    entry => entry.Key, StringComparer.OrdinalIgnoreCase);

                var secrets = new List<SecretInfo>();
                foreach (var record in enumeration.Records)
                {
                    var reserved = SecretsIndex.IsReservedKey(record.Key);
                    if (reserved && !includeReserved)
                        continue;

                    var info = new SecretInfo
                    {
                        Key = record.Key,
                        Managed = byKey.ContainsKey(record.Key),
                        Owner = string.IsNullOrEmpty(record.Owner) ? null : record.Owner,
                        Type = record.Type,
                        LastModifiedUtc = FormatUtc(record.LastModifiedUtc),
                        Reserved = reserved ? true : null
                    };

                    if (byKey.TryGetValue(record.Key, out var entry))
                    {
                        info.Description = entry.Description;
                        info.CreatedUtc = FormatUtc(entry.CreatedUtc);
                        info.UpdatedUtc = FormatUtc(entry.UpdatedUtc);
                    }

                    if (includeSizes)
                    {
                        // Only the length crosses this boundary - never the value itself.
                        var length = enumerable.GetSecretLength(record.Key);
                        if (length >= 0) info.ValueLength = length;
                    }

                    secrets.Add(info);
                }

                secrets = secrets
                    .OrderBy(info => info.Key, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // Only meaningful against a complete walk: a truncated one would report live
                // secrets as stale.
                var stale = enumeration.Complete
                    ? SecretsIndex.FindStale(
                        index.Entries, enumeration.Records.Select(record => record.Key))
                    : new List<SecretsIndexEntry>();

                var warnings = new List<string>(index.Warnings ?? new List<string>());
                if (!enumeration.Complete)
                {
                    warnings.Add(enumeration.ErrorCode == null
                        ? "The data store listing was cut short; this list may be incomplete."
                        : $"The data store listing stopped early ({enumeration.ErrorCode}); this list may be incomplete.");
                }

                return (200, new SecretsListResponse
                {
                    Provider = providerKey,
                    Scope = enumerable.Scope.ToString().ToLowerInvariant(),
                    IndexStatus = index.Status,
                    EnumerationComplete = enumeration.Complete,
                    Counts = new SecretsCounts
                    {
                        Total = secrets.Count,
                        Managed = secrets.Count(info => info.Managed),
                        Unmanaged = secrets.Count(info => !info.Managed),
                        Stale = stale.Count
                    },
                    Secrets = secrets,
                    StaleIndexEntries = stale.Count == 0 ? null : stale
                        .Select(entry => new StaleIndexEntryInfo
                        {
                            Key = entry.Key,
                            Description = entry.Description,
                            CreatedUtc = FormatUtc(entry.CreatedUtc)
                        })
                        .ToList(),
                    Warnings = warnings.Count == 0 ? null : warnings
                });
            }
        }
        catch (Exception ex)
        {
            Debug.LogMessage(ex, "Error listing secrets: {Exception}");
            return (500, new SecretsListResponse
            {
                Provider = providerKey,
                Error = Error("executionError", ex.Message)
            });
        }
    }

    // ─── Commands ────────────────────────────────────────────────────────────

    /// <summary>
    /// Runs a single-secret command.
    /// </summary>
    public static (int StatusCode, SecretCommandResponse Response) ExecuteCommand(
        SecretCommandRequest request)
    {
        if (request == null)
            return CommandError(400, "invalidJson", "Request body was empty or could not be parsed.");

        if (string.IsNullOrWhiteSpace(request.Action))
            return CommandError(400, "missingField", "An action is required.", "action");

        if (string.IsNullOrWhiteSpace(request.Provider))
            return CommandError(400, "missingField", "A provider is required.", "provider");

        var provider = SecretsManager.GetSecretProviderByKey(request.Provider);
        if (provider == null)
        {
            return CommandError(404, "providerNotFound",
                $"No secret provider '{request.Provider}'.", "provider");
        }

        if (provider is not IEnumerableSecretProvider enumerable)
        {
            return CommandError(422, "providerNotFound",
                $"Provider '{request.Provider}' does not support managed secrets.", "provider");
        }

        try
        {
            lock (SyncRoot)
            {
                return request.Action.ToLowerInvariant() switch
                {
                    ActionSet => Set(enumerable, request, allowExisting: request.Overwrite),
                    ActionUpdate => Update(enumerable, request),
                    ActionDelete => Delete(enumerable, request),
                    ActionTest => Test(enumerable, request),
                    "pruneindex" => PruneIndex(enumerable, request),
                    "rebuildindex" => RebuildIndex(enumerable, request),
                    _ => CommandError(400, "unknownAction",
                            $"Unknown action '{request.Action}'.", "action")
                };
            }
        }
        catch (Exception ex)
        {
            Debug.LogMessage(ex, "Error executing secrets command: {Exception}");
            return CommandError(500, "executionError", ex.Message);
        }
    }

    private static (int, SecretCommandResponse) Set(
        IEnumerableSecretProvider provider,
        SecretCommandRequest request,
        bool allowExisting)
    {
        var validation = ValidateKeyAndValue(request.Key, request.Value);
        if (validation != null)
            return validation.Value;

        var existed = provider.TestSecret(request.Key);
        if (existed && !allowExisting)
        {
            return CommandError(409, "alreadyExists",
                $"'{request.Key}' already exists. Use update, or set overwrite.", "key");
        }

        return WriteAndIndex(provider, request, existed, ActionSet);
    }

    private static (int, SecretCommandResponse) Update(
        IEnumerableSecretProvider provider,
        SecretCommandRequest request)
    {
        var validation = ValidateKeyAndValue(request.Key, request.Value);
        if (validation != null)
            return validation.Value;

        if (!provider.TestSecret(request.Key))
        {
            return CommandError(404, "notFound",
                $"No secret '{request.Key}' to update. Use set to create it.", "key");
        }

        return WriteAndIndex(provider, request, existedBefore: true, action: ActionUpdate);
    }

    /// <summary>
    /// Writes the secret, then records it in the index.
    /// </summary>
    /// <remarks>
    /// Order matters. The secret is authoritative and the index is advisory, so the secret is
    /// written first. If the index write then fails the request still succeeds with a warning -
    /// returning an error would invite the operator to retry a write that already happened.
    /// </remarks>
    private static (int, SecretCommandResponse) WriteAndIndex(
        IEnumerableSecretProvider provider,
        SecretCommandRequest request,
        bool existedBefore,
        string action)
    {
        var write = provider.WriteSecret(request.Key, request.Value);
        if (!write.Success)
            return CommandError(StatusForStoreError(write.ErrorCode), MapStoreError(write.ErrorCode), write.Message, "key");

        var index = SecretsIndexStore.Read(provider);
        var entries = new List<SecretsIndexEntry>(index.Entries);
        var now = DateTime.UtcNow;

        var existing = entries.FirstOrDefault(
            entry => string.Equals(entry.Key, request.Key, StringComparison.OrdinalIgnoreCase));

        if (existing == null)
        {
            entries.Add(new SecretsIndexEntry
            {
                Key = request.Key,
                Description = SecretsIndex.NormalizeDescription(request.Description),
                CreatedUtc = now,
                UpdatedUtc = now
            });
        }
        else
        {
            existing.UpdatedUtc = now;
            if (!string.IsNullOrWhiteSpace(request.Description))
                existing.Description = SecretsIndex.NormalizeDescription(request.Description);
        }

        var indexWrite = SecretsIndexStore.Write(provider, entries, index.ChunkCount);

        return (200, new SecretCommandResponse
        {
            Status = "ok",
            Action = action,
            Provider = provider.Key,
            Key = request.Key,
            ExistedBefore = existedBefore,
            IndexUpdated = indexWrite.Success,
            Warning = indexWrite.Success
                ? null
                : $"The secret was saved, but its details could not be recorded: {indexWrite.Message}"
        });
    }

    private static (int, SecretCommandResponse) Delete(
        IEnumerableSecretProvider provider,
        SecretCommandRequest request)
    {
        // Present-only - a delete targets a record that already exists, so the key's length and
        // character set were settled when it was written.
        var keyResult = SecretProviderGuards.ValidateKeyPresent(request.Key);
        if (keyResult != null)
            return CommandError(400, keyResult.ErrorCode, keyResult.Message, "key");

        if (SecretsIndex.IsReservedKey(request.Key))
        {
            return CommandError(400, "reservedKey",
                $"'{request.Key}' is reserved for internal use.", "key");
        }

        var existed = provider.TestSecret(request.Key);
        var delete = existed ? provider.DeleteSecret(request.Key) : SecretStoreResult.Ok();

        if (!delete.Success)
        {
            return CommandError(StatusForStoreError(delete.ErrorCode),
                MapStoreError(delete.ErrorCode), delete.Message, "key");
        }

        // Drop the index entry even when the record was already gone: that is exactly the stale
        // case, and a delete is the natural moment to clean it up.
        var index = SecretsIndexStore.Read(provider);
        var entries = index.Entries
            .Where(entry => !string.Equals(entry.Key, request.Key, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var removed = entries.Count != index.Entries.Count;
        var indexWrite = removed
            ? SecretsIndexStore.Write(provider, entries, index.ChunkCount)
            : SecretStoreResult.Ok();

        if (!existed)
        {
            return (404, new SecretCommandResponse
            {
                Status = "error",
                Action = ActionDelete,
                Provider = provider.Key,
                Key = request.Key,
                IndexUpdated = removed && indexWrite.Success,
                Error = Error("notFound", $"No secret '{request.Key}'.", "key")
            });
        }

        return (200, new SecretCommandResponse
        {
            Status = "ok",
            Action = ActionDelete,
            Provider = provider.Key,
            Key = request.Key,
            ExistedBefore = true,
            IndexUpdated = indexWrite.Success
        });
    }

    private static (int, SecretCommandResponse) Test(
        IEnumerableSecretProvider provider,
        SecretCommandRequest request)
    {
        var keyResult = SecretProviderGuards.ValidateKeyPresent(request.Key);
        if (keyResult != null)
            return CommandError(400, keyResult.ErrorCode, keyResult.Message, "key");

        return (200, new SecretCommandResponse
        {
            Status = "ok",
            Action = ActionTest,
            Provider = provider.Key,
            Key = request.Key,
            Exists = provider.TestSecret(request.Key)
        });
    }

    private static (int, SecretCommandResponse) PruneIndex(
        IEnumerableSecretProvider provider,
        SecretCommandRequest request)
    {
        var enumeration = provider.EnumerateKeys();
        if (!enumeration.Complete)
        {
            // Pruning against a partial view would delete entries for secrets that do exist.
            return CommandError(409, "enumerationIncomplete",
                "The data store listing was incomplete, so stale entries cannot be identified safely.");
        }

        var index = SecretsIndexStore.Read(provider);
        var existing = enumeration.Records.Select(record => record.Key);
        var stale = SecretsIndex.FindStale(index.Entries, existing);

        if (stale.Count == 0)
        {
            return (200, new SecretCommandResponse
            {
                Status = "ok",
                Action = ActionPruneIndex,
                Provider = provider.Key,
                EntriesRemoved = 0,
                IndexUpdated = false
            });
        }

        var staleKeys = new HashSet<string>(
            stale.Select(entry => entry.Key), StringComparer.OrdinalIgnoreCase);
        var kept = index.Entries.Where(entry => !staleKeys.Contains(entry.Key)).ToList();

        var write = SecretsIndexStore.Write(provider, kept, index.ChunkCount);
        if (!write.Success)
            return CommandError(500, "executionError", write.Message);

        return (200, new SecretCommandResponse
        {
            Status = "ok",
            Action = ActionPruneIndex,
            Provider = provider.Key,
            EntriesRemoved = stale.Count,
            EntriesRetained = kept.Count,
            IndexUpdated = true
        });
    }

    /// <summary>
    /// Rewrites the index, optionally adopting existing keys as managed.
    /// </summary>
    /// <remarks>
    /// Repairs a corrupt index, and is the migration path for secrets set before this API existed:
    /// adopting a key records metadata about it without reading or changing its value.
    /// </remarks>
    private static (int, SecretCommandResponse) RebuildIndex(
        IEnumerableSecretProvider provider,
        SecretCommandRequest request)
    {
        var index = SecretsIndexStore.Read(provider);
        var entries = new List<SecretsIndexEntry>(index.Entries);
        var now = DateTime.UtcNow;
        var adopted = 0;

        foreach (var key in request.AdoptKeys ?? new List<string>())
        {
            if (string.IsNullOrWhiteSpace(key) || SecretsIndex.IsReservedKey(key))
                continue;
            if (!provider.TestSecret(key))
                continue;
            if (entries.Any(entry => string.Equals(entry.Key, key, StringComparison.OrdinalIgnoreCase)))
                continue;

            entries.Add(new SecretsIndexEntry
            {
                Key = key,
                Description = null,
                CreatedUtc = now,
                UpdatedUtc = now
            });
            adopted++;
        }

        var write = SecretsIndexStore.Write(provider, entries, index.ChunkCount);
        if (!write.Success)
            return CommandError(500, "executionError", write.Message);

        return (200, new SecretCommandResponse
        {
            Status = "ok",
            Action = ActionRebuildIndex,
            Provider = provider.Key,
            EntriesRetained = entries.Count,
            IndexUpdated = true,
            Warning = index.Status == "ok"
                ? null
                : $"The previous index was {index.Status}; {adopted} key(s) adopted and {entries.Count} retained."
        });
    }

    // ─── Bulk ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Previews or applies a batch of secrets.
    /// </summary>
    /// <remarks>
    /// Preview writes nothing at all. A commit validates the whole batch first and refuses outright
    /// if anything is invalid, so the common failure - a malformed file - leaves the store
    /// untouched. The Data Store has no transactions, so a mid-batch store failure can still leave
    /// partial state; those entries are reported as "failed" rather than pretended away.
    /// </remarks>
    public static (int StatusCode, BulkSecretsResponse Response) ExecuteBulk(BulkSecretsRequest request)
    {
        if (request == null)
        {
            return (400, new BulkSecretsResponse
            {
                Error = Error("invalidJson", "Request body was empty or could not be parsed.")
            });
        }

        var preview = !string.Equals(request.Mode, "commit", StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(request.Provider))
        {
            return (400, new BulkSecretsResponse
            {
                Error = Error("missingField", "A provider is required.", "provider")
            });
        }

        var provider = SecretsManager.GetSecretProviderByKey(request.Provider);
        if (provider is not IEnumerableSecretProvider enumerable)
        {
            return (provider == null ? 404 : 422, new BulkSecretsResponse
            {
                Provider = request.Provider,
                Error = Error("providerNotFound",
                    $"No usable secret provider '{request.Provider}'.", "provider")
            });
        }

        if (!TryReadEntries(request.Secrets, out var entries, out var shapeError))
        {
            return (400, new BulkSecretsResponse
            {
                Provider = request.Provider,
                Error = Error("invalidJson", shapeError, "secrets")
            });
        }

        if (entries.Count > SecretsApiContracts.MaxBulkEntries)
        {
            return (400, new BulkSecretsResponse
            {
                Provider = request.Provider,
                Error = Error("batchTooLarge",
                    $"The batch has {entries.Count} entries; the limit is {SecretsApiContracts.MaxBulkEntries}.",
                    "secrets")
            });
        }

        try
        {
            lock (SyncRoot)
            {
                var index = SecretsIndexStore.Read(enumerable);
                var managed = new HashSet<string>(
                    index.Entries.Select(entry => entry.Key), StringComparer.OrdinalIgnoreCase);

                var results = Classify(enumerable, entries, request, managed);
                var summary = Summarize(results);

                if (preview)
                {
                    return (200, new BulkSecretsResponse
                    {
                        Mode = "preview",
                        Provider = request.Provider,
                        Overwrite = request.Overwrite,
                        Summary = summary,
                        Entries = results,
                        IndexUpdated = false
                    });
                }

                if (summary.Invalid > 0)
                {
                    // All-or-nothing for the common failure: nothing is written.
                    return (422, new BulkSecretsResponse
                    {
                        Mode = "commit",
                        Provider = request.Provider,
                        Overwrite = request.Overwrite,
                        Summary = summary,
                        Entries = results,
                        IndexUpdated = false,
                        Error = Error("validationFailed",
                            $"{summary.Invalid} entr(y/ies) are invalid; nothing was written.")
                    });
                }

                return (200, Apply(enumerable, entries, request, results, index));
            }
        }
        catch (Exception ex)
        {
            Debug.LogMessage(ex, "Error applying bulk secrets: {Exception}");
            return (500, new BulkSecretsResponse
            {
                Provider = request.Provider,
                Error = Error("executionError", ex.Message)
            });
        }
    }

    /// <summary>
    /// Decides what would happen to each entry, writing nothing.
    /// </summary>
    private static List<BulkEntryResult> Classify(
        IEnumerableSecretProvider provider,
        IList<BulkSecretEntry> entries,
        BulkSecretsRequest request,
        HashSet<string> managed)
    {
        var results = new List<BulkEntryResult>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var result = new BulkEntryResult
            {
                Index = i,
                Key = entry.Key ?? string.Empty,
                Provider = entry.Provider ?? request.Provider,
                Applied = false
            };

            var validation = SecretProviderGuards.ValidateWrite(entry.Key, entry.Value);
            if (validation != null)
            {
                result.Action = "invalid";
                result.Reason = validation.ErrorCode;
                result.Message = validation.Message;
            }
            else if (SecretsIndex.IsReservedKey(entry.Key))
            {
                result.Action = "invalid";
                result.Reason = "reservedKey";
                result.Message = $"'{entry.Key}' is reserved for internal use.";
            }
            else if (!seen.Add(entry.Key))
            {
                // Last-write-wins would be nondeterministic from the caller's point of view.
                result.Action = "invalid";
                result.Reason = "duplicateKeyInBatch";
                result.Message = $"'{entry.Key}' appears more than once in this batch.";
            }
            else if (!provider.TestSecret(entry.Key))
            {
                result.Action = "create";
            }
            else if (!request.Overwrite)
            {
                result.Action = "skip";
                result.Reason = "alreadyExists";
                result.Message = "Already exists; enable overwrite to replace it.";
            }
            else if (!managed.Contains(entry.Key) && !request.AllowUnmanagedOverwrite)
            {
                // The guard that stops a round-tripped template destroying another subsystem's
                // records - Mobile Control's pairing tokens being the one that matters.
                result.Action = "skip";
                result.Reason = "unmanagedTarget";
                result.Message = "Exists but is not managed by this API; overwriting it must be confirmed.";
            }
            else
            {
                result.Action = "overwrite";
            }

            results.Add(result);
        }

        return results;
    }

    private static BulkSecretsResponse Apply(
        IEnumerableSecretProvider provider,
        IList<BulkSecretEntry> entries,
        BulkSecretsRequest request,
        List<BulkEntryResult> results,
        SecretsIndexStore.ReadResult index)
    {
        var indexEntries = new List<SecretsIndexEntry>(index.Entries);
        var now = DateTime.UtcNow;
        var wrote = false;

        foreach (var result in results)
        {
            if (result.Action != "create" && result.Action != "overwrite")
                continue;

            var entry = entries[result.Index];
            var write = provider.WriteSecret(entry.Key, entry.Value);

            if (!write.Success)
            {
                result.Action = "failed";
                result.Reason = write.ErrorCode;
                result.Message = write.Message;
                continue;
            }

            result.Applied = true;
            wrote = true;

            var existing = indexEntries.FirstOrDefault(
                item => string.Equals(item.Key, entry.Key, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                indexEntries.Add(new SecretsIndexEntry
                {
                    Key = entry.Key,
                    Description = SecretsIndex.NormalizeDescription(entry.Description),
                    CreatedUtc = now,
                    UpdatedUtc = now
                });
            }
            else
            {
                existing.UpdatedUtc = now;
                if (!string.IsNullOrWhiteSpace(entry.Description))
                    existing.Description = SecretsIndex.NormalizeDescription(entry.Description);
            }
        }

        // One index write for the whole batch - the reason this endpoint exists rather than N
        // separate command calls.
        var indexWrite = wrote
            ? SecretsIndexStore.Write(provider, indexEntries, index.ChunkCount)
            : SecretStoreResult.Ok();

        var warnings = new List<string>();
        if (wrote && !indexWrite.Success)
            warnings.Add($"The secrets were saved, but their details could not be recorded: {indexWrite.Message}");

        return new BulkSecretsResponse
        {
            Mode = "commit",
            Provider = request.Provider,
            Overwrite = request.Overwrite,
            Summary = Summarize(results),
            Entries = results,
            IndexUpdated = wrote && indexWrite.Success,
            Warnings = warnings.Count == 0 ? null : warnings
        };
    }

    private static BulkSummary Summarize(IEnumerable<BulkEntryResult> results)
    {
        var list = results.ToList();
        return new BulkSummary
        {
            Total = list.Count,
            Create = list.Count(r => r.Action == "create"),
            Overwrite = list.Count(r => r.Action == "overwrite"),
            Skip = list.Count(r => r.Action == "skip"),
            Invalid = list.Count(r => r.Action == "invalid"),
            Failed = list.Count(r => r.Action == "failed")
        };
    }

    /// <summary>
    /// Accepts either an array of entries or a flat key-to-value map.
    /// </summary>
    /// <remarks>
    /// The flat map is what <c>secrets/template</c> emits, so supporting both is what makes the
    /// download-fill-apply round trip work without the user reshaping the file.
    /// </remarks>
    private static bool TryReadEntries(JToken token, out IList<BulkSecretEntry> entries, out string error)
    {
        entries = new List<BulkSecretEntry>();
        error = null;

        if (token == null || token.Type == JTokenType.Null)
        {
            error = "No secrets were supplied.";
            return false;
        }

        try
        {
            if (token.Type == JTokenType.Array)
            {
                entries = token.ToObject<List<BulkSecretEntry>>() ?? new List<BulkSecretEntry>();
                return true;
            }

            if (token.Type == JTokenType.Object)
            {
                var list = new List<BulkSecretEntry>();
                foreach (var property in ((JObject)token).Properties())
                {
                    list.Add(new BulkSecretEntry
                    {
                        Key = property.Name,
                        Value = property.Value?.Type == JTokenType.String
                            ? property.Value.ToString()
                            : null
                    });
                }
                entries = list;
                return true;
            }

            error = "'secrets' must be a list of entries or a key-to-value object.";
            return false;
        }
        catch (Exception)
        {
            // Deliberately does not include the exception message: a deserializer error can quote
            // the offending JSON, which here would be a credential.
            error = "'secrets' could not be read as a list of entries or a key-to-value object.";
            return false;
        }
    }

    // ─── Template ────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a key-only template for a provider. Values are always blank.
    /// </summary>
    public static (int StatusCode, SecretsTemplateResponse Response) BuildTemplate(
        string providerKey,
        bool includeUnmanaged)
    {
        var (status, list) = List(providerKey, includeReserved: false, includeSizes: false);
        if (status != 200 || list.Error != null)
        {
            return (status, new SecretsTemplateResponse
            {
                Provider = providerKey,
                Error = list.Error ?? Error("executionError", "Unable to read the secrets list.")
            });
        }

        var included = (list.Secrets ?? new List<SecretInfo>())
            .Where(info => includeUnmanaged || info.Managed)
            .ToList();

        return (200, new SecretsTemplateResponse
        {
            Provider = providerKey,
            GeneratedUtc = FormatUtc(DateTime.UtcNow),
            Note = "Values are intentionally blank. Fill them in, then apply this file.",
            Secrets = included.ToDictionary(info => info.Key, _ => string.Empty),
            Metadata = included
                .Select(info => new SecretsTemplateMetadata
                {
                    Key = info.Key,
                    Provider = providerKey,
                    Description = info.Description,
                    Managed = info.Managed
                })
                .ToList()
        });
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static (int, SecretCommandResponse)? ValidateKeyAndValue(string key, string value)
    {
        var result = SecretProviderGuards.ValidateWrite(key, value);
        if (result != null)
        {
            var field = result.ErrorCode == "emptyValue" || result.ErrorCode == "valueTooLong"
                ? "value"
                : "key";
            return CommandError(400, result.ErrorCode, result.Message, field);
        }

        if (SecretsIndex.IsReservedKey(key))
            return CommandError(400, "reservedKey", $"'{key}' is reserved for internal use.", "key");

        return null;
    }

    /// <summary>
    /// Maps a raw CDS_ERROR name onto the API's stable error codes.
    /// </summary>
    private static string MapStoreError(string cdsError) => cdsError switch
    {
        "CDS_RECORD_NOT_FOUND" => "notFound",
        "CDS_ACCESS_DENIED" => "accessDenied",
        "CDS_NAME_TOO_BIG" => "keyTooLong",
        "CDS_STRING_TOO_BIG" => "valueTooLong",
        "CDS_MAX_RECORDS" => "storeFull",
        "CDS_DATABASE_NOT_FOUND" => "storeUnavailable",
        "CDS_DATABASE_ERROR" => "storeUnavailable",
        null => "executionError",
        _ => cdsError
    };

    /// <summary>
    /// 404 means the key is wrong, 422 that the request is impossible on this device, 5xx that the
    /// store itself refused.
    /// </summary>
    private static int StatusForStoreError(string cdsError) => cdsError switch
    {
        "emptyKey" or "invalidKey" or "keyTooLong" or "emptyValue" or "valueTooLong" => 400,
        "CDS_NAME_TOO_BIG" or "CDS_STRING_TOO_BIG" => 400,
        "CDS_RECORD_NOT_FOUND" => 404,
        "CDS_ACCESS_DENIED" => 403,
        "CDS_MAX_RECORDS" => 507,
        "CDS_DATABASE_NOT_FOUND" or "CDS_DATABASE_ERROR" => 503,
        _ => 500
    };

    private static string FormatUtc(DateTime? value)
        => value.HasValue ? value.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : null;

    private static string FormatUtc(DateTime value)
        => value == default ? null : value.ToString("yyyy-MM-ddTHH:mm:ssZ");

    private static SecretsApiError Error(string code, string message, string field = null)
        => new SecretsApiError { Code = code, Message = message, Field = field };

    private static (int, SecretCommandResponse) CommandError(
        int statusCode, string code, string message, string field = null)
        => (statusCode, new SecretCommandResponse
        {
            Status = "error",
            Error = Error(code, message, field)
        });
}
