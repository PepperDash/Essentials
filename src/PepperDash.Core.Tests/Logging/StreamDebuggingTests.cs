using FluentAssertions;
using Xunit;

namespace PepperDash.Core.Tests.Logging;

/// <summary>
/// Device stream debugging settings (#1584): Debug's settings collection must exist, or every
/// setdevicestreamdebug throws a NullReferenceException after changing the setting.
/// </summary>
public class StreamDebuggingTests
{
    [Fact]
    public void SetDebuggingWithSpecificTimeout_TurnsOnWithoutThrowing()
    {
        var debugging = new CommunicationStreamDebugging("stream-test-on");

        var act = () => debugging.SetDebuggingWithSpecificTimeout(eStreamDebuggingSetting.Both, 30);

        act.Should().NotThrow();
        debugging.RxStreamDebuggingIsEnabled.Should().BeTrue();
        debugging.TxStreamDebuggingIsEnabled.Should().BeTrue();
        Debug.GetDeviceDebugSettingsForKey("stream-test-on").Should().Be(eStreamDebuggingSetting.Both);

        debugging.SetDebuggingWithDefaultTimeout(eStreamDebuggingSetting.Off);
    }

    [Fact]
    public void SetDebuggingOff_TurnsOffWithoutThrowing()
    {
        var debugging = new CommunicationStreamDebugging("stream-test-off");
        debugging.SetDebuggingWithSpecificTimeout(eStreamDebuggingSetting.Rx, 30);

        var act = () => debugging.SetDebuggingWithDefaultTimeout(eStreamDebuggingSetting.Off);

        act.Should().NotThrow();
        debugging.RxStreamDebuggingIsEnabled.Should().BeFalse();
        debugging.TxStreamDebuggingIsEnabled.Should().BeFalse();
        Debug.GetDeviceDebugSettingsForKey("stream-test-off").Should().Be(eStreamDebuggingSetting.Off);
    }

    [Fact]
    public void GetDeviceDebugSettingsForKey_ReturnsNull_ForUnknownKey()
    {
        Debug.GetDeviceDebugSettingsForKey("stream-test-never-set").Should().BeNull();
    }
}
