// This file is also compiled into PepperDash.Essentials.Core.Tests, which enables nullable
// reference types. Core does not, so the annotations here would be read inconsistently between the
// two. Disabling explicitly keeps the behaviour identical in both, and is a no-op in Core.
#nullable disable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Usage of the processor's Data Store, read from the DATASTORESTATUS console command.
/// </summary>
/// <remarks>
/// <para>
/// The SDK has no call that reports Data Store usage, so this parses the console output, the same
/// way Essentials reads other console commands. Crestron does not document the format; this
/// matches 4-series firmware 2.8006.00110:
/// </para>
/// <code>
/// CrestronDataStore: SIMPLLOCAL
///     PepperDashEssentials    6 records    2921 bytes    29% used
/// CrestronDataStore: SIMPLGLOBAL
///     PepperDashEssentials    2 records    404 bytes    4% used
/// </code>
/// <para>
/// Usage is kept per program name, not per slot, so two programs with the same name share a row
/// and a quota. Crestron does not stop a write that goes over 100%.
/// </para>
/// <para>
/// This file has no Crestron references by design, so it can be unit tested off-target.
/// </para>
/// </remarks>
public static class DataStoreStatus
{
    private static readonly Regex StoreHeader = new Regex(
        @"^\s*CrestronDataStore:\s*SIMPL(?<store>LOCAL|GLOBAL)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex UsageRow = new Regex(
        @"^\s*(?<owner>\S.*?)\s+(?<records>\d+)\s+records?\s+(?<bytes>\d+)\s+bytes\s+(?<percent>\d+)%\s+used\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Parses DATASTORESTATUS output. Lines that match neither a store header nor a usage row are
    /// ignored.
    /// </summary>
    /// <returns>One entry per program name per store, in the order printed.</returns>
    public static IList<DataStoreUsage> Parse(string output)
    {
        var rows = new List<DataStoreUsage>();
        if (string.IsNullOrEmpty(output))
            return rows;

        string store = null;
        foreach (var rawLine in output.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');

            var header = StoreHeader.Match(line);
            if (header.Success)
            {
                store = header.Groups["store"].Value.ToLowerInvariant();
                continue;
            }

            if (store == null)
                continue;

            var row = UsageRow.Match(line);
            if (!row.Success)
                continue;

            rows.Add(new DataStoreUsage
            {
                Store = store,
                Owner = row.Groups["owner"].Value.Trim(),
                Records = int.Parse(row.Groups["records"].Value, CultureInfo.InvariantCulture),
                Bytes = long.Parse(row.Groups["bytes"].Value, CultureInfo.InvariantCulture),
                PercentUsed = int.Parse(row.Groups["percent"].Value, CultureInfo.InvariantCulture)
            });
        }

        return rows;
    }
}

/// <summary>One program's usage of one Data Store space.</summary>
public class DataStoreUsage
{
    /// <summary>Gets or sets "local" or "global".</summary>
    public string Store { get; set; }

    /// <summary>Gets or sets the program name the usage is kept under.</summary>
    public string Owner { get; set; }

    /// <summary>Gets or sets the number of records.</summary>
    public int Records { get; set; }

    /// <summary>Gets or sets the bytes used.</summary>
    public long Bytes { get; set; }

    /// <summary>Gets or sets the percentage of the quota used, as Crestron reports it.</summary>
    public int PercentUsed { get; set; }
}
