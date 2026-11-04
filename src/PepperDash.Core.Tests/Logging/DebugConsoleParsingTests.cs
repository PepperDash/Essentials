using FluentAssertions;
using Serilog.Events;
using Xunit;

namespace PepperDash.Core.Tests.Logging;

/// <summary>
/// Tests for the parsing behind the appdebug and appdebugfilter console commands.
/// </summary>
public class DebugConsoleParsingTests
{
    [Theory]
    [InlineData("0", LogEventLevel.Information)]
    [InlineData("1", LogEventLevel.Debug)]
    [InlineData("2", LogEventLevel.Verbose)]
    [InlineData("3", LogEventLevel.Warning)]
    [InlineData("4", LogEventLevel.Error)]
    [InlineData("5", LogEventLevel.Fatal)]
    public void TryParseLogLevel_Number_UsesLegacyMap(string token, LogEventLevel expected)
    {
        Debug.TryParseLogLevel(token, out var level).Should().BeTrue();
        level.Should().Be(expected);
    }

    [Theory]
    [InlineData("verbose", LogEventLevel.Verbose)]
    [InlineData("Debug", LogEventLevel.Debug)]
    [InlineData("INFORMATION", LogEventLevel.Information)]
    [InlineData("warning", LogEventLevel.Warning)]
    [InlineData("Error", LogEventLevel.Error)]
    [InlineData("fatal", LogEventLevel.Fatal)]
    public void TryParseLogLevel_Name_IsCaseInsensitive(string token, LogEventLevel expected)
    {
        Debug.TryParseLogLevel(token, out var level).Should().BeTrue();
        level.Should().Be(expected);
    }

    [Theory]
    [InlineData("6")]
    [InlineData("-1")]
    [InlineData("info")]
    [InlineData("Verbose, Debug")]
    [InlineData("")]
    public void TryParseLogLevel_Invalid_ReturnsFalse(string token)
    {
        Debug.TryParseLogLevel(token, out _).Should().BeFalse();
    }

    [Fact]
    public void LogLevelHelp_ListsNumbersInOrder()
    {
        Debug.LogLevelHelp.Should().Be(
            "0 = Information, 1 = Debug, 2 = Verbose, 3 = Warning, 4 = Error, 5 = Fatal (or the level name)");
    }

    [Fact]
    public void SplitFilterKeys_SplitsOnWhitespace_AndDropsEmptyEntries()
    {
        Debug.SplitFilterKeys("  -all   +display-1\t+codec-1 ")
            .Should().Equal("-all", "+display-1", "+codec-1");
    }

    [Fact]
    public void SplitFilterKeys_Empty_ReturnsNoKeys()
    {
        Debug.SplitFilterKeys("   ").Should().BeEmpty();
    }
}
