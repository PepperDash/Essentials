using FluentAssertions;
using Xunit;

namespace PepperDash.Core.Tests;

/// <summary>
/// Tests for <see cref="ProcessorEthernetInfo"/>'s parameter validation. The adapter lookups themselves
/// need the Crestron SDK, but the sentinel handling that produced URLs such as
/// <c>wss://Invalid Value:65479/debug/join</c> is pure and testable.
/// </summary>
public class ProcessorEthernetInfoTests
{
    [Theory]
    [InlineData("Invalid Value")]
    [InlineData("invalid value")]
    [InlineData("  Invalid Value  ")]
    public void NullIfInvalid_RejectsSdkSentinel(string value)
    {
        ProcessorEthernetInfo.NullIfInvalid(value).Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NullIfInvalid_RejectsBlankValues(string? value)
    {
        ProcessorEthernetInfo.NullIfInvalid(value!).Should().BeNull();
    }

    [Fact]
    public void NullIfInvalid_ReturnsTrimmedAddress()
    {
        ProcessorEthernetInfo.NullIfInvalid(" 10.0.0.5 ").Should().Be("10.0.0.5");
    }
}
