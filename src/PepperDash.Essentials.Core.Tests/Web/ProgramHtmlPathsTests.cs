using System.IO;
using FluentAssertions;
using PepperDash.Essentials.Core.Web;
using Xunit;

namespace PepperDash.Essentials.Core.Tests.Web;

/// <summary>
/// Where the htmlassets bundle and the Dev Tools go. DirectoryInfo doesn't need the folders to exist,
/// so these use paths under a fake root.
/// </summary>
public class ProgramHtmlPathsTests
{
    private static readonly string Root = Path.Combine(Path.GetTempPath(), "pld-html-paths");

    [Fact]
    public void Appliance_UsesTheProcessorRoot()
    {
        var prefix = Path.Combine(Root, "user", "program10") + Path.DirectorySeparatorChar;

        ProgramHtmlPaths.GetHtmlRoot(prefix, isVirtualControl: false)
            .Should().Be(Path.Combine(Root, "html"));
        ProgramHtmlPaths.GetDevToolsRoot(prefix, isVirtualControl: false)
            .Should().Be(Path.Combine(Root, "html", "debug"));
    }

    [Fact]
    public void VirtualControl_UsesTheRoomFolder_NotTheFolderAboveAllRooms()
    {
        var runningPrograms = Path.Combine(Root, "RunningPrograms");
        var prefix = Path.Combine(runningPrograms, "DEMOV3", "User") + Path.DirectorySeparatorChar;

        ProgramHtmlPaths.GetDevToolsRoot(prefix, isVirtualControl: true)
            .Should().Be(Path.Combine(runningPrograms, "DEMOV3", "html", "debug"));
    }

    [Fact]
    public void AcceptsAPrefixWithoutATrailingSeparator()
    {
        var prefix = Path.Combine(Root, "user", "program1");

        ProgramHtmlPaths.GetHtmlRoot(prefix, isVirtualControl: false)
            .Should().Be(Path.Combine(Root, "html"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnsNullForAMissingPrefix(string? prefix)
    {
        ProgramHtmlPaths.GetHtmlRoot(prefix!, isVirtualControl: false).Should().BeNull();
        ProgramHtmlPaths.GetDevToolsRoot(prefix!, isVirtualControl: true).Should().BeNull();
    }

    [Fact]
    public void ReturnsNullWhenThereIsNoFolderAboveThePrefix()
    {
        var filesystemRoot = Path.GetPathRoot(Path.GetTempPath())!;
        var shallow = Path.Combine(filesystemRoot, "onlyOneLevel") + Path.DirectorySeparatorChar;

        ProgramHtmlPaths.GetHtmlRoot(shallow, isVirtualControl: false).Should().BeNull();
    }
}
