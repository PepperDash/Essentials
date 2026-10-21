using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using PepperDash.Essentials;
using Xunit;

namespace PepperDash.Essentials.Core.Tests.Plugins;

/// <summary>
/// Inspects this test assembly, which the test project stamps the same way the Essentials NuGet
/// packages stamp a plugin (see the AssemblyMetadata item in the .csproj).
/// </summary>
public class PluginAssemblyInspectorTests
{
    private static string ThisAssemblyPath => typeof(PluginAssemblyInspectorTests).Assembly.Location;

    [Fact]
    public void ReadsTheBuiltAgainstStamp()
    {
        var info = PluginAssemblyInspector.Inspect(ThisAssemblyPath);

        info.BuiltAgainst.Should().ContainKey("PepperDashEssentials")
            .WhoseValue.Should().Be("3.0.0-rc.42");
    }

    [Fact]
    public void ReadsIdentityAndInformationalVersion()
    {
        var expected = typeof(PluginAssemblyInspectorTests).Assembly.GetName();

        var info = PluginAssemblyInspector.Inspect(ThisAssemblyPath);

        info.AssemblyName.Should().Be(expected.Name);
        info.AssemblyVersion.Should().Be(expected.Version);
        info.InformationalVersion.Should().NotBeNullOrEmpty();
        info.DisplayVersion.Should().Be(info.InformationalVersion);
    }

    [Fact]
    public void ReadsAssemblyReferencesWithVersions()
    {
        var expected = typeof(FluentAssertions.AssertionExtensions).Assembly.GetName();

        var info = PluginAssemblyInspector.Inspect(ThisAssemblyPath);

        info.References.Should().Contain(r => r.Name == expected.Name && r.Version == expected.Version);
    }

    [Fact]
    public void ReadsTypeReferences()
    {
        var info = PluginAssemblyInspector.Inspect(ThisAssemblyPath);

        info.TypeReferences.Should().Contain("Xunit.FactAttribute");
    }

    [Fact]
    public void LeavesUnrelatedMetadataOut()
    {
        var info = PluginAssemblyInspector.Inspect(ThisAssemblyPath);

        info.BuiltAgainst.Keys.Should().OnlyContain(k => !k.StartsWith(PluginAssemblyInspector.BuiltAgainstMetadataPrefix));
    }

    [Fact]
    public void RejectsAFileThatIsNotAnAssembly()
    {
        var path = Path.GetTempFileName();

        try
        {
            File.WriteAllText(path, "not an assembly");

            var act = () => PluginAssemblyInspector.Inspect(path);

            act.Should().Throw<BadImageFormatException>();
        }
        finally
        {
            File.Delete(path);
        }
    }
}
