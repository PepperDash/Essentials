using System.Text;
using FluentAssertions;
using PepperDash.Core.Adapters;
using Xunit;

namespace PepperDash.Core.Tests.Logging;

/// <summary>
/// The Crestron console throws ("Help is too long for command ...") on help text over 79 bytes.
/// Debug registers its commands from its static constructor, so an over-long help string there
/// aborted the constructor on a processor and left file logging uninitialized.
/// </summary>
public class ConsoleHelpTextTests
{
    [Fact]
    public void AppDebugHelp_FitsTheConsoleLimit()
    {
        Encoding.UTF8.GetByteCount(Debug.AppDebugHelp).Should().BeLessOrEqualTo(ConsoleHelpText.MaxBytes);
    }

    [Fact]
    public void Fit_LeavesTextThatFitsUnchanged()
    {
        var help = new string('a', ConsoleHelpText.MaxBytes);

        ConsoleHelpText.Fit(help).Should().BeSameAs(help);
    }

    [Fact]
    public void Fit_CutsLongTextToTheLimit()
    {
        var help = "appdebug:P [sink] [level] [timeoutMinutes]: Sets the application's debug message level. level: 0 = Information, 1 = Debug, 2 = Verbose";

        var fitted = ConsoleHelpText.Fit(help);

        Encoding.UTF8.GetByteCount(fitted).Should().Be(ConsoleHelpText.MaxBytes);
        help.Should().StartWith(fitted);
    }

    [Fact]
    public void Fit_CountsBytesNotCharacters()
    {
        // 40 two-byte characters = 80 bytes: one over the limit even though it is only 40 chars.
        var help = new string('é', 40);

        var fitted = ConsoleHelpText.Fit(help);

        Encoding.UTF8.GetByteCount(fitted).Should().BeLessOrEqualTo(ConsoleHelpText.MaxBytes);
        fitted.Should().HaveLength(39);
    }

    [Fact]
    public void Fit_NeverSplitsASurrogatePair()
    {
        // Each emoji is a surrogate pair (2 chars, 4 bytes). 19 of them = 76 bytes, then 4 more bytes.
        var help = string.Concat(System.Linq.Enumerable.Repeat("\U0001F600", 20));

        var fitted = ConsoleHelpText.Fit(help);

        Encoding.UTF8.GetByteCount(fitted).Should().Be(76);
        char.IsHighSurrogate(fitted[^1]).Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Fit_TurnsNullOrEmptyIntoEmpty(string help)
    {
        ConsoleHelpText.Fit(help).Should().BeEmpty();
    }
}
