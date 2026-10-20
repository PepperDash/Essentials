using System;
using FluentAssertions;
using PepperDash.Essentials;
using Xunit;

namespace PepperDash.Essentials.Core.Tests.Plugins;

public class EssentialsVersionTests
{
    private static EssentialsVersion V(string value)
    {
        EssentialsVersion.TryParse(value, out var version).Should().BeTrue($"'{value}' should parse");
        return version!;
    }

    [Theory]
    [InlineData("3.0.0", 3, 0, 0, "")]
    [InlineData("3.0.0-rc.16", 3, 0, 0, "rc")]
    [InlineData("v2.43.1", 2, 43, 1, "")]
    [InlineData("3.0.0.0", 3, 0, 0, "")]
    [InlineData("3.0.0-feat-update-ihaswebview.1", 3, 0, 0, "feat-update-ihaswebview")]
    [InlineData("3.0.0-rc.16+abc123", 3, 0, 0, "rc")]
    public void Parses_ReleasePrereleaseAndAssemblyVersions(string value, int major, int minor, int patch, string label)
    {
        var version = V(value);

        version.Major.Should().Be(major);
        version.Minor.Should().Be(minor);
        version.Patch.Should().Be(patch);
        version.PrereleaseLabel.Should().Be(label);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("3.0")]
    [InlineData("three")]
    public void Rejects_UnrecognisedInput(string? value)
    {
        EssentialsVersion.TryParse(value!, out _).Should().BeFalse();
    }

    [Theory]
    [InlineData("3.0.0-rc.9", "3.0.0-rc.11")]
    [InlineData("3.0.0-rc.11", "3.0.0-rc.16")]
    [InlineData("3.0.0-rc.16", "3.0.0")]
    [InlineData("2.43.1", "3.0.0-rc.1")]
    [InlineData("3.0.0", "3.0.1")]
    [InlineData("3.0.0-rc", "3.0.0-rc.1")]
    public void OrdersBySemVerPrecedence(string older, string newer)
    {
        V(older).CompareTo(V(newer)).Should().BeNegative();
        V(newer).CompareTo(V(older)).Should().BePositive();
    }

    [Fact]
    public void CompareForCompatibility_OrdersSameLabelPrereleases()
    {
        EssentialsVersion.CompareForCompatibility(V("3.0.0-rc.16"), V("3.0.0-rc.11")).Should().BePositive();
    }

    [Theory]
    [InlineData("3.0.0-local", "3.0.0-rc.16")]
    [InlineData("3.0.0-rc.16", "3.0.0-local")]
    [InlineData("0.1.0", "3.0.0")]
    public void CompareForCompatibility_IsUnknownForLocalAndZeroMajorBuilds(string a, string b)
    {
        EssentialsVersion.CompareForCompatibility(V(a), V(b)).Should().BeNull();
    }

    [Fact]
    public void CompareForCompatibility_IsUnknownWhenPrereleaseLabelsDiffer()
    {
        // A feature-branch build can't be ordered against an rc of the same version
        EssentialsVersion.CompareForCompatibility(V("3.0.0-feat-update-ihaswebview.1"), V("3.0.0-rc.16"))
            .Should().BeNull();
    }

    [Fact]
    public void CompareForCompatibility_UsesCoreVersionWhenItDiffers()
    {
        EssentialsVersion.CompareForCompatibility(V("3.1.0-feat-x.1"), V("3.0.0-rc.16")).Should().BePositive();
    }

    [Fact]
    public void FromAssemblyVersion_DropsTheRevision()
    {
        var version = EssentialsVersion.FromAssemblyVersion(new Version(2, 21, 274, 0));

        version.Major.Should().Be(2);
        version.Minor.Should().Be(21);
        version.Patch.Should().Be(274);
        version.IsPrerelease.Should().BeFalse();
    }
}
