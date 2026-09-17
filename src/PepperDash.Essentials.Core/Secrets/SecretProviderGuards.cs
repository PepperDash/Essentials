using System;
using Crestron.SimplSharp.CrestronDataStore;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Validation shared by the enumerable secret providers.
/// </summary>
/// <remarks>
/// These guards live in the provider layer on purpose. The web API validates too, but the guard
/// that matters most - refusing an empty key - must hold for any caller, because
/// <c>clearLocal("")</c> / <c>clearGlobal("")</c> deletes <em>every record belonging to the
/// application</em>. That is one stray trailing space away from wiping a processor's stored
/// credentials along with Mobile Control's pairing tokens.
/// </remarks>
internal static class SecretProviderGuards
{
    /// <summary>
    /// The record name length the SDK documents for <c>CDS_NAME_TOO_BIG</c>.
    /// </summary>
    /// <remarks>
    /// Advisory only, and deliberately not enforced. The documented limit is 32, but records with
    /// longer names demonstrably exist in the local store - Mobile Control's
    /// "7:mobileControl-directServer-tokens" is 35 characters and was written through this same
    /// API. Enforcing the documented figure would refuse keys the store accepts, and would make
    /// existing records impossible to delete. The store is the authority: it returns
    /// <c>CDS_NAME_TOO_BIG</c> if it actually objects, and that is mapped to a clear error.
    /// </remarks>
    public const int DocumentedMaxKeyLength = 32;

    /// <summary>Data Store string value limit (CDS_STRING_TOO_BIG).</summary>
    public const int MaxValueLength = 1600;

    /// <summary>
    /// Validates that a key names something, for operations that act on an existing record.
    /// </summary>
    /// <remarks>
    /// The only check that applies to a delete. The key came from the store's own listing, so
    /// questions of length or character set are already settled - refusing it here would strand a
    /// record that cannot then be removed by any means.
    /// <para>
    /// The empty check stays, and is the important one: an empty key reaches
    /// <c>clearLocal("")</c>, which deletes every record belonging to the application.
    /// </para>
    /// </remarks>
    public static SecretStoreResult ValidateKeyPresent(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            // Whitespace is rejected as well as empty: " " is a legal record name, so allowing it
            // would create secrets nobody can find, and it reads as a typo either way.
            return SecretStoreResult.Fail(
                "emptyKey",
                "A secret key is required. An empty key would clear the entire data store.");
        }

        return null;
    }

    /// <summary>
    /// Validates a key that is about to be created. Returns null when it is acceptable.
    /// </summary>
    public static SecretStoreResult ValidateKeyForWrite(string key)
    {
        var present = ValidateKeyPresent(key);
        if (present != null)
            return present;

        // No length check - see DocumentedMaxKeyLength. Control characters are still refused,
        // because a key containing one cannot be typed back into a device config.
        foreach (var c in key)
        {
            if (char.IsControl(c))
            {
                return SecretStoreResult.Fail(
                    "invalidKey", "The key contains control characters.");
            }
        }

        return null;
    }

    /// <summary>
    /// Validates a key and value for a write. Returns null when both are acceptable.
    /// </summary>
    public static SecretStoreResult ValidateWrite(string key, string value)
    {
        var keyResult = ValidateKeyForWrite(key);
        if (keyResult != null)
            return keyResult;

        if (string.IsNullOrEmpty(value))
        {
            // An empty value is a delete in the underlying store, so accepting one here would make
            // a write silently destructive. Callers that mean to delete must say so.
            return SecretStoreResult.Fail(
                "emptyValue",
                "A secret value is required. Use delete to remove a secret.");
        }

        if (value.Length > MaxValueLength)
        {
            // Length only - never any part of the value.
            return SecretStoreResult.Fail(
                "valueTooLong",
                $"The value is {value.Length} characters; the Crestron Data Store limit is {MaxValueLength}.");
        }

        return null;
    }

    /// <summary>
    /// Converts a Data Store error into a result, preserving the raw code for the API to map.
    /// </summary>
    public static SecretStoreResult FromCdsError(
        CrestronDataStore.CDS_ERROR error,
        string providerKey,
        string secretKey)
    {
        var message = error switch
        {
            CrestronDataStore.CDS_ERROR.CDS_RECORD_NOT_FOUND
                => $"No secret '{secretKey}' in provider '{providerKey}'.",
            CrestronDataStore.CDS_ERROR.CDS_ACCESS_DENIED
                => $"The data store refused access to '{secretKey}'. Records created by another application cannot be changed.",
            // Reported by the store rather than pre-judged: the documented 32-character figure is
            // not what it actually enforces.
            CrestronDataStore.CDS_ERROR.CDS_NAME_TOO_BIG
                => $"The data store rejected the key '{secretKey}' as too long.",
            CrestronDataStore.CDS_ERROR.CDS_STRING_TOO_BIG
                => $"The value is longer than the {MaxValueLength}-character data store limit.",
            CrestronDataStore.CDS_ERROR.CDS_MAX_RECORDS
                => "The data store has no room for another record.",
            CrestronDataStore.CDS_ERROR.CDS_DATABASE_NOT_FOUND
                => "The data store is unavailable.",
            CrestronDataStore.CDS_ERROR.CDS_DATABASE_ERROR
                => "The data store reported an error.",
            CrestronDataStore.CDS_ERROR.CDS_WRONG_DATA_TYPE
                => $"The record '{secretKey}' is not a text value.",
            _ => $"The data store returned {error}."
        };

        return SecretStoreResult.Fail(error.ToString(), message);
    }
}
