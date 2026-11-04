using FluentAssertions;
using Serilog.Events;
using Serilog.Parsing;
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

    [Theory]
    [InlineData(null, false, true)]
    [InlineData(null, true, true)]
    [InlineData("", true, true)]
    [InlineData("display-1", false, false)]
    [InlineData("codec-1", false, true)]
    [InlineData("display-1", true, true)]
    [InlineData("codec-1", true, false)]
    public void KeyFilterAllows_FollowsIncludeExcludeMode(string? key, bool excludeAllMode, bool expected)
    {
        Debug.KeyFilterAllows(key!, excludeAllMode, k => k == "display-1").Should().Be(expected);
    }

    [Fact]
    public void ConsoleKeyFilterAllows_AppliesConsoleFilterCommands()
    {
        try
        {
            Debug.SetDebugFilterFromConsole("-display-1");
            Debug.ConsoleKeyFilterAllows(EventWithKey("Display-1")).Should().BeFalse("excluded keys match case-insensitively");
            Debug.ConsoleKeyFilterAllows(EventWithKey("codec-1")).Should().BeTrue();
            Debug.ConsoleKeyFilterAllows(EventWithKey(null)).Should().BeTrue("messages with no key always pass");

            Debug.SetDebugFilterFromConsole("-all +codec-1");
            Debug.ConsoleKeyFilterAllows(EventWithKey("codec-1")).Should().BeTrue();
            Debug.ConsoleKeyFilterAllows(EventWithKey("display-1")).Should().BeFalse();
            Debug.ConsoleKeyFilterAllows(EventWithKey(null)).Should().BeTrue();

            Debug.SetDebugFilterFromConsole("+all");
            Debug.ConsoleKeyFilterAllows(EventWithKey("display-1")).Should().BeTrue();
        }
        finally
        {
            Debug.SetDebugFilterFromConsole("+all");
        }
    }

    private static LogEvent EventWithKey(string? key)
    {
        return new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null,
            new MessageTemplateParser().Parse("test"),
            new[] { new LogEventProperty("Key", new ScalarValue(key)) });
    }
}
