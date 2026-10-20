// This file is also compiled into PepperDash.Essentials.Core.Tests, which enables nullable
// reference types. Core does not, so the annotations here would be read inconsistently between the
// two. Disabling explicitly keeps the behaviour identical in both, and is a no-op in Core.
#nullable disable

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace PepperDash.Essentials;

/// <summary>
/// A semantic version as Essentials packages use them, e.g. <c>3.0.0</c>, <c>3.0.0-rc.16</c> or
/// <c>3.0.0-feat-update-ihaswebview.1</c>. Also accepts a four-part assembly version such as
/// <c>3.0.0.0</c>, ignoring the fourth part.
/// </summary>
public sealed class EssentialsVersion : IComparable<EssentialsVersion>
{
    private static readonly Regex Pattern = new Regex(
        @"^v?(\d+)\.(\d+)\.(\d+)(?:\.\d+)?(?:-([0-9A-Za-z.-]+))?(?:\+.*)?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private EssentialsVersion(string original, int major, int minor, int patch, string[] prerelease)
    {
        Original = original;
        Major = major;
        Minor = minor;
        Patch = patch;
        Prerelease = prerelease;
    }

    /// <summary>The string this version was parsed from.</summary>
    public string Original { get; }

    /// <summary>The major version.</summary>
    public int Major { get; }

    /// <summary>The minor version.</summary>
    public int Minor { get; }

    /// <summary>The patch version.</summary>
    public int Patch { get; }

    /// <summary>The dot-separated prerelease identifiers, empty for a release version.</summary>
    public string[] Prerelease { get; }

    /// <summary>True when this is a prerelease version.</summary>
    public bool IsPrerelease => Prerelease.Length > 0;

    /// <summary>
    /// The prerelease label without its counter, e.g. <c>rc</c> for <c>rc.16</c>, or empty for a release.
    /// </summary>
    public string PrereleaseLabel => IsPrerelease ? Prerelease[0] : string.Empty;

    /// <summary>
    /// False for a local developer build (<c>-local</c>) or a 0.x version. Essentials does not enforce
    /// version requirements against those, so neither does the plugin version check.
    /// </summary>
    public bool IsComparable =>
        Major != 0 && !Prerelease.Any(p => p.Equals("local", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Parses a version string. Returns false for null, empty or unrecognised input.
    /// </summary>
    public static bool TryParse(string value, out EssentialsVersion version)
    {
        version = null;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();
        var match = Pattern.Match(trimmed);

        if (!match.Success)
            return false;

        if (!int.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var major)
            || !int.TryParse(match.Groups[2].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var minor)
            || !int.TryParse(match.Groups[3].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var patch))
            return false;

        var prerelease = match.Groups[4].Success
            ? match.Groups[4].Value.Split('.')
            : Array.Empty<string>();

        version = new EssentialsVersion(trimmed, major, minor, patch, prerelease);
        return true;
    }

    /// <summary>
    /// Creates a version from an assembly version, which carries no prerelease label.
    /// </summary>
    public static EssentialsVersion FromAssemblyVersion(Version assemblyVersion)
    {
        if (assemblyVersion == null)
            return null;

        return new EssentialsVersion(assemblyVersion.ToString(), assemblyVersion.Major, assemblyVersion.Minor,
            Math.Max(assemblyVersion.Build, 0), Array.Empty<string>());
    }

    /// <summary>
    /// Compares two versions for the plugin version check. Returns null when the comparison would not
    /// mean anything: either version is not <see cref="IsComparable"/>, or they share major.minor.patch
    /// but carry different prerelease labels (a feature-branch build cannot be ordered against an rc).
    /// </summary>
    /// <returns>Negative if <paramref name="a"/> is older, positive if newer, zero if equal, or null if unknown.</returns>
    public static int? CompareForCompatibility(EssentialsVersion a, EssentialsVersion b)
    {
        if (a == null || b == null || !a.IsComparable || !b.IsComparable)
            return null;

        var core = CompareCore(a, b);
        if (core != 0)
            return core;

        if (a.IsPrerelease && b.IsPrerelease
            && !string.Equals(a.PrereleaseLabel, b.PrereleaseLabel, StringComparison.OrdinalIgnoreCase))
            return null;

        return a.CompareTo(b);
    }

    /// <summary>
    /// Compares versions using SemVer 2 precedence: major, minor, patch, then prerelease identifiers,
    /// with a release ranking above any of its prereleases.
    /// </summary>
    public int CompareTo(EssentialsVersion other)
    {
        if (other == null)
            return 1;

        var core = CompareCore(this, other);
        if (core != 0)
            return core;

        if (!IsPrerelease && !other.IsPrerelease)
            return 0;
        if (!IsPrerelease)
            return 1;
        if (!other.IsPrerelease)
            return -1;

        var count = Math.Min(Prerelease.Length, other.Prerelease.Length);
        for (var i = 0; i < count; i++)
        {
            var result = CompareIdentifier(Prerelease[i], other.Prerelease[i]);
            if (result != 0)
                return result;
        }

        return Prerelease.Length.CompareTo(other.Prerelease.Length);
    }

    /// <inheritdoc />
    public override string ToString() => Original;

    private static int CompareCore(EssentialsVersion a, EssentialsVersion b)
    {
        if (a.Major != b.Major)
            return a.Major.CompareTo(b.Major);
        if (a.Minor != b.Minor)
            return a.Minor.CompareTo(b.Minor);
        return a.Patch.CompareTo(b.Patch);
    }

    private static int CompareIdentifier(string a, string b)
    {
        var aIsNumber = long.TryParse(a, NumberStyles.None, CultureInfo.InvariantCulture, out var aNumber);
        var bIsNumber = long.TryParse(b, NumberStyles.None, CultureInfo.InvariantCulture, out var bNumber);

        if (aIsNumber && bIsNumber)
            return aNumber.CompareTo(bNumber);

        // Numeric identifiers rank below alphanumeric ones
        if (aIsNumber)
            return -1;
        if (bIsNumber)
            return 1;

        return string.CompareOrdinal(a, b);
    }
}
