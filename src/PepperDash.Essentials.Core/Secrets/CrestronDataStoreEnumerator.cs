using System;
using System.Collections.Generic;
using Crestron.SimplSharp.CrestronDataStore;
using PepperDash.Core;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Walks a Crestron Data Store space and reports the records it holds.
/// </summary>
/// <remarks>
/// The only place <c>GetNextLocalTag</c> / <c>GetNextGlobalTag</c> are called. Those APIs have three
/// behaviours that are each individually enough to produce a wrong list, so they are handled once,
/// here, rather than at every call site. See <see cref="Enumerate"/>.
/// </remarks>
internal static class CrestronDataStoreEnumerator
{
    /// <summary>
    /// Upper bound on records walked, so a cursor that never terminates cannot hang a web request.
    /// Far above any realistic Data Store population.
    /// </summary>
    private const int MaxRecords = 4096;

    /// <summary>
    /// Enumerates every record in the given Data Store space.
    /// </summary>
    /// <remarks>
    /// <para>Three SDK behaviours drive the shape of this loop:</para>
    /// <para>
    /// 1. <c>info</c> proves nothing. The SDK pre-fills the out-struct with placeholders - empty
    /// owner, a String type, <c>GetLocalTime()</c> - <em>before</em> it looks anything up, so it
    /// always comes back populated whether or not a record was found. The only reliable signal that
    /// the cursor advanced is <c>name</c> changing to a value not already seen.
    /// </para>
    /// <para>
    /// 2. <c>CDS_END_OF_TABLE</c> arrives while the data is still valid. It is returned when the
    /// <em>input</em> name was the second-to-last record, so the returned name and info describe the
    /// last one. The record must be consumed before the loop stops - the obvious
    /// <c>while (err == CDS_SUCCESS)</c> silently drops the final key.
    /// </para>
    /// <para>
    /// 3. <c>info.date</c> is local time, not UTC, so it is converted before use.
    /// </para>
    /// </remarks>
    public static SecretStoreEnumeration Enumerate(SecretStoreScope scope)
    {
        var records = new List<CdsRecord>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var name = string.Empty;   // An empty seed asks for the first record.
        var complete = false;
        string errorCode = null;

        try
        {
            for (var i = 0; i < MaxRecords; i++)
            {
                var previous = name;
                CrestronDataStore.recInfo info;

                var error = scope == SecretStoreScope.Local
                    ? CrestronDataStoreStatic.GetNextLocalTag(ref name, out info)
                    : CrestronDataStoreStatic.GetNextGlobalTag(ref name, out info,
                        CrestronDataStore.CDS_ACTION.CDS_NAMEONLY);

                if (error != CrestronDataStore.CDS_ERROR.CDS_SUCCESS
                    && error != CrestronDataStore.CDS_ERROR.CDS_END_OF_TABLE)
                {
                    // CDS_RECORD_NOT_FOUND on the very first call means the database is empty. That
                    // is a complete enumeration of zero records, not a failure.
                    complete = error == CrestronDataStore.CDS_ERROR.CDS_RECORD_NOT_FOUND
                        && records.Count == 0;
                    errorCode = complete ? null : error.ToString();
                    break;
                }

                var advanced = !string.IsNullOrEmpty(name)
                    && !string.Equals(name, previous, StringComparison.Ordinal)
                    && seen.Add(name);

                if (advanced)
                    records.Add(ToRecord(name, info));

                // Consume first, then stop - see remark 2.
                if (error == CrestronDataStore.CDS_ERROR.CDS_END_OF_TABLE)
                {
                    complete = true;
                    break;
                }

                // Success but the cursor did not move: treat as the end rather than spinning.
                if (!advanced)
                {
                    complete = true;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogMessage(ex, "Secrets enumeration failed after {count} records", null, records.Count);
            errorCode = "enumerationException";
            complete = false;
        }

        return new SecretStoreEnumeration(records, complete, errorCode);
    }

    private static CdsRecord ToRecord(string name, CrestronDataStore.recInfo info)
    {
        DateTime? modified = null;
        try
        {
            // info.date is local time with an unspecified Kind; pin it before converting so the
            // result is not shifted twice.
            if (info.date != default)
                modified = DateTime.SpecifyKind(info.date, DateTimeKind.Local).ToUniversalTime();
        }
        catch (Exception)
        {
            // A placeholder or out-of-range date is not worth failing an enumeration over.
        }

        return new CdsRecord(name, info.owner, modified, info.type.ToString());
    }
}
