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
/// The list of keys held by <see cref="CrestronSecureStorageSecretsProvider"/>.
/// </summary>
/// <remarks>
/// <para>
/// Crestron secure storage can store, retrieve and delete a tag, but it cannot list its tags. The
/// provider keeps this list in secure storage beside the secrets, updated on every write and delete
/// it makes, so it can answer <see cref="IEnumerableSecretProvider.EnumerateKeys"/>.
/// </para>
/// <para>
/// It lists only keys written through the provider. A tag stored by other code calling
/// <c>CrestronSecureStorage</c> directly is not in it.
/// </para>
/// <para>
/// This file has no Crestron references by design, so it can be unit tested off-target.
/// </para>
/// </remarks>
public static class SecureStorageKeyRegistry
{
    /// <summary>Current document version.</summary>
    public const int Version = 1;

    /// <summary>
    /// Tag holding the list. It starts with <see cref="SecretsIndex.HeaderKey"/>, so the secrets API
    /// already treats it as reserved and refuses to write or delete it.
    /// </summary>
    public const string RecordKey = SecretsIndex.HeaderKey + "Keys";

    private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
    {
        Formatting = Formatting.None,
        NullValueHandling = NullValueHandling.Ignore
    };

    /// <summary>
    /// Serializes a key list.
    /// </summary>
    public static string Serialize(IEnumerable<string> keys)
        => JsonConvert.SerializeObject(new SecureStorageKeyDocument
        {
            Version = Version,
            Keys = Normalize(keys)
        }, SerializerSettings);

    /// <summary>
    /// Parses a stored key list.
    /// </summary>
    /// <param name="json">The stored document.</param>
    /// <param name="keys">The keys, or an empty list when parsing fails.</param>
    /// <returns>False when the document is not a readable key list.</returns>
    public static bool TryParse(string json, out IList<string> keys)
    {
        keys = new List<string>();

        if (string.IsNullOrEmpty(json))
            return false;

        try
        {
            var document = JsonConvert.DeserializeObject<SecureStorageKeyDocument>(json);
            if (document == null || document.Version != Version || document.Keys == null)
                return false;

            keys = Normalize(document.Keys);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the list with a key added, or null when it is already present.
    /// </summary>
    public static IList<string> WithKey(IEnumerable<string> keys, string key)
    {
        var list = Normalize(keys);
        if (string.IsNullOrWhiteSpace(key) || list.Contains(key, StringComparer.Ordinal))
            return null;

        list.Add(key);
        return list;
    }

    /// <summary>
    /// Returns the list with a key removed, or null when it was not present.
    /// </summary>
    public static IList<string> WithoutKey(IEnumerable<string> keys, string key)
    {
        var list = Normalize(keys);
        return list.RemoveAll(item => string.Equals(item, key, StringComparison.Ordinal)) == 0
            ? null
            : list;
    }

    /// <remarks>
    /// Ordinal: a tag is an exact string, so "Key" and "key" are two records.
    /// </remarks>
    private static List<string> Normalize(IEnumerable<string> keys)
        => (keys ?? Enumerable.Empty<string>())
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Distinct(StringComparer.Ordinal)
            .ToList();
}

/// <summary>
/// The stored form of <see cref="SecureStorageKeyRegistry"/>.
/// </summary>
public class SecureStorageKeyDocument
{
    /// <summary>Gets or sets the document version.</summary>
    [JsonProperty("v")]
    public int Version { get; set; }

    /// <summary>Gets or sets the keys.</summary>
    [JsonProperty("keys")]
    public List<string> Keys { get; set; }
}
