using FluentAssertions;
using PepperDash.Core;
using Xunit;

namespace PepperDash.Core.Tests.Devices;

/// <summary>
/// Tests for <see cref="Device"/> — the base class for all PepperDash devices.
/// These run without Crestron hardware; Debug is initialized with fakes via TestInitializer.
/// </summary>
public class DeviceTests
{
    // -----------------------------------------------------------------------
    // Construction
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_SingleArg_SetsKey()
    {
        var device = new Device("my-device");

        device.Key.Should().Be("my-device");
    }

    [Fact]
    public void Constructor_SingleArg_SetsNameToEmpty()
    {
        var device = new Device("my-device");

        device.Name.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_TwoArg_SetsKeyAndName()
    {
        var device = new Device("my-device", "My Device");

        device.Key.Should().Be("my-device");
        device.Name.Should().Be("My Device");
    }

    [Fact]
    public void Constructor_KeyWithDot_StillSetsKey()
    {
        // The dot triggers a debug log warning but must not prevent construction.
        var device = new Device("parent.child");

        device.Key.Should().Be("parent.child");
    }

    // -----------------------------------------------------------------------
    // ToString
    // -----------------------------------------------------------------------

    [Fact]
    public void ToString_WithName_FormatsKeyDashName()
    {
        var device = new Device("cam-01", "Front Camera");

        device.ToString().Should().Be("cam-01 - Front Camera");
    }

    [Fact]
    public void ToString_WithoutName_UsesDashPlaceholder()
    {
        var device = new Device("cam-01");

        device.ToString().Should().Be("cam-01 - ---");
    }

    // -----------------------------------------------------------------------
    // DefaultDevice
    // -----------------------------------------------------------------------

    [Fact]
    public void DefaultDevice_IsNotNull()
    {
        Device.DefaultDevice.Should().NotBeNull();
    }

    [Fact]
    public void DefaultDevice_HasKeyDefault()
    {
        Device.DefaultDevice.Key.Should().Be("Default");
    }

    // -----------------------------------------------------------------------
    // CustomActivate / Activate / Deactivate / Initialize
    // -----------------------------------------------------------------------

    [Fact]
    public void CustomActivate_DefaultReturnTrue()
    {
        var device = new TestDevice("d1");

        device.CallCustomActivate().Should().BeTrue();
    }

    [Fact]
    public void Deactivate_DefaultReturnsTrue()
    {
        var device = new Device("d1");

        device.Deactivate().Should().BeTrue();
    }

    [Fact]
    public void Activate_CallsCustomActivate_AndReturnsItsResult()
    {
        var stub = new ActivateTrackingDevice("d1", result: false);

        stub.Activate().Should().BeFalse();
        stub.CustomActivateCalled.Should().BeTrue();
    }

    [Fact]
    public void Activate_TrueWhenCustomActivateReturnsTrue()
    {
        var stub = new ActivateTrackingDevice("d1", result: true);

        stub.Activate().Should().BeTrue();
    }

    [Fact]
    public void Initialize_DoesNotThrow()
    {
        var device = new TestDevice("d1");
        var act = () => device.CallInitialize();

        act.Should().NotThrow();
    }

    // -----------------------------------------------------------------------
    // PreActivate
    // -----------------------------------------------------------------------

    [Fact]
    public void PreActivate_NoActions_DoesNotThrow()
    {
        var device = new TestDevice("d1");
        var act = () => device.PreActivate();

        act.Should().NotThrow();
    }

    [Fact]
    public void PreActivate_RunsRegisteredActionsInOrder()
    {
        var device = new TestDevice("d1");
        var order = new List<int>();

        device.AddPreActivationAction(() => order.Add(1));
        device.AddPreActivationAction(() => order.Add(2));
        device.AddPreActivationAction(() => order.Add(3));

        device.PreActivate();

        order.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void PreActivate_ContinuesAfterFaultingAction()
    {
        var device = new TestDevice("d1");
        var reached = false;

        device.AddPreActivationAction(() => throw new InvalidOperationException("boom"));
        device.AddPreActivationAction(() => reached = true);

        var act = () => device.PreActivate();

        act.Should().NotThrow("exceptions in individual actions must be caught internally");
        reached.Should().BeTrue("actions after a faulting action must still run");
    }

    // -----------------------------------------------------------------------
    // PostActivate
    // -----------------------------------------------------------------------

    [Fact]
    public void PostActivate_NoActions_DoesNotThrow()
    {
        var device = new TestDevice("d1");
        var act = () => device.PostActivate();

        act.Should().NotThrow();
    }

    [Fact]
    public void PostActivate_RunsRegisteredActionsInOrder()
    {
        var device = new TestDevice("d1");
        var order = new List<int>();

        device.AddPostActivationAction(() => order.Add(1));
        device.AddPostActivationAction(() => order.Add(2));

        device.PostActivate();

        order.Should().Equal(1, 2);
    }

    [Fact]
    public void PostActivate_ContinuesAfterFaultingAction()
    {
        var device = new TestDevice("d1");
        var reached = false;

        device.AddPostActivationAction(() => throw new Exception("boom"));
        device.AddPostActivationAction(() => reached = true);

        var act = () => device.PostActivate();

        act.Should().NotThrow();
        reached.Should().BeTrue();
    }

    // -----------------------------------------------------------------------
    // Pre and Post actions are independent lists
    // -----------------------------------------------------------------------

    [Fact]
    public void PreActivationActions_DoNotRunOnPostActivate()
    {
        var device = new TestDevice("d1");
        var preRan = false;

        device.AddPreActivationAction(() => preRan = true);
        device.PostActivate();

        preRan.Should().BeFalse();
    }

    [Fact]
    public void PostActivationActions_DoNotRunOnPreActivate()
    {
        var device = new TestDevice("d1");
        var postRan = false;

        device.AddPostActivationAction(() => postRan = true);
        device.PreActivate();

        postRan.Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // OnFalse
    // -----------------------------------------------------------------------

    [Fact]
    public void OnFalse_FiresAction_WhenBoolIsFalse()
    {
        var device = new Device("d1");
        var fired = false;

        device.OnFalse(false, () => fired = true);

        fired.Should().BeTrue();
    }

    [Fact]
    public void OnFalse_DoesNotFireAction_WhenBoolIsTrue()
    {
        var device = new Device("d1");
        var fired = false;

        device.OnFalse(true, () => fired = true);

        fired.Should().BeFalse();
    }

    [Fact]
    public void OnFalse_DoesNotFireAction_ForNonBoolType()
    {
        var device = new Device("d1");
        var fired = false;

        device.OnFalse("not a bool", () => fired = true);
        device.OnFalse(0, () => fired = true);
        device.OnFalse(null!, () => fired = true);

        fired.Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Exposes protected Device members so test methods can call them directly.
    /// </summary>
    private class TestDevice : Device
    {
        public TestDevice(string key) : base(key) { }
        public TestDevice(string key, string name) : base(key, name) { }

        public void AddPreActivationAction(Action act) => base.AddPreActivationAction(act);
        public void AddPostActivationAction(Action act) => base.AddPostActivationAction(act);
        public bool CallCustomActivate() => base.CustomActivate();
        public void CallInitialize() => base.Initialize();
    }

    /// <summary>
    /// Records whether CustomActivate was invoked and returns a configured result.
    /// Used to verify Activate() correctly delegates to CustomActivate().
    /// </summary>
    private sealed class ActivateTrackingDevice : Device
    {
        private readonly bool _result;
        public bool CustomActivateCalled { get; private set; }

        public ActivateTrackingDevice(string key, bool result = true) : base(key)
        {
            _result = result;
        }

        protected override bool CustomActivate()
        {
            CustomActivateCalled = true;
            return _result;
        }
    }
}
