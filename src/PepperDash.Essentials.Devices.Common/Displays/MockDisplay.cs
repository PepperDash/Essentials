using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Displays;

/// <summary>
/// Represents a mock display device for testing and simulation purposes.
/// </summary>
public class MockDisplay : TwoWayDisplayBase, IBasicVolumeWithFeedback, IBridgeAdvanced, IHasInputs<string>, IRoutingSinkWithSwitchingWithInputPort, IHasPowerControlWithFeedback
{
    /// <inheritdoc />
    public ISelectableItems<string> Inputs { get; private set; }

    bool _PowerIsOn;
    bool _IsWarmingUp;
    bool _IsCoolingDown;

    /// <inheritdoc />
    protected override Func<bool> PowerIsOnFeedbackFunc
    {
        get
        {
            return () =>
                {
                    return _PowerIsOn;
                };
        }
    }

    /// <inheritdoc />
    protected override Func<bool> IsCoolingDownFeedbackFunc
    {
        get
        {
            return () =>
            {
                return _IsCoolingDown;
            };
        }
    }

    /// <inheritdoc />
    protected override Func<bool> IsWarmingUpFeedbackFunc
    {
        get
        {
            return () =>
            {
                return _IsWarmingUp;
            };
        }
    }

    /// <inheritdoc />
    protected override Func<string> CurrentInputFeedbackFunc { get { return () => Inputs.CurrentItem; } }

    int VolumeHeldRepeatInterval = 200;
    ushort VolumeInterval = 655;
    ushort _FakeVolumeLevel = 31768;
    bool _IsMuted;
    Timer _volumeUpTimer;
    Timer _volumeDownTimer;

    /// <summary>
    /// Constructor for MockDisplay
    /// </summary> <param name="key"></param>
    /// <param name="name"></param>
    public MockDisplay(string key, string name)
        : base(key, name)
    {
        Inputs = new MockDisplayInputs
        {
            Items = new Dictionary<string, ISelectableItem>
                {
                    { "HDMI1", new MockDisplayInput ( "HDMI1", "HDMI 1",this ) },
                    { "HDMI2", new MockDisplayInput ("HDMI2", "HDMI 2",this ) },
                    { "HDMI3", new MockDisplayInput ("HDMI3", "HDMI 3",this ) },
                    { "HDMI4", new MockDisplayInput ("HDMI4", "HDMI 4",this )},
                    { "DP", new MockDisplayInput ("DP", "DisplayPort", this ) }
                }
        };

        Inputs.CurrentItemChanged += (o, a) => CurrentInputFeedback.FireUpdate();

        var hdmiIn1 = new RoutingInputPort(RoutingPortNames.HdmiIn1, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Hdmi, "HDMI1", this);
        var hdmiIn2 = new RoutingInputPort(RoutingPortNames.HdmiIn2, eRoutingSignalType.AudioVideo,
            eRoutingPortConnectionType.Hdmi, "HDMI2", this);
        var hdmiIn3 = new RoutingInputPort(RoutingPortNames.HdmiIn3, eRoutingSignalType.AudioVideo,
            eRoutingPortConnectionType.Hdmi, "HDMI3", this);
        var hdmiIn4 = new RoutingInputPort(RoutingPortNames.HdmiIn4, eRoutingSignalType.AudioVideo,
            eRoutingPortConnectionType.Hdmi, "HDMI4", this);
        var dpIn = new RoutingInputPort(RoutingPortNames.DisplayPortIn, eRoutingSignalType.AudioVideo,
            eRoutingPortConnectionType.DisplayPort, "DP", this);
        InputPorts.AddRange(new[] { hdmiIn1, hdmiIn2, hdmiIn3, hdmiIn4, dpIn });


        VolumeLevelFeedback = new IntFeedback("volume", () => { return _FakeVolumeLevel; });
        MuteFeedback = new BoolFeedback("muteOn", () => _IsMuted);

        WarmupTime = 10000;
        CooldownTime = 10000;
    }

    /// <summary>
    /// PowerOn method
    /// </summary>
    /// <inheritdoc />
    public override void PowerOn()
    {
        if (!PowerIsOnFeedback.BoolValue && !_IsWarmingUp && !_IsCoolingDown)
        {
            _IsWarmingUp = true;
            IsWarmingUpFeedback.InvokeFireUpdate();
            // Fake power-up cycle
            WarmupTimer = new Timer(WarmupTime) { AutoReset = false };
            WarmupTimer.Elapsed += (s, e) =>
                {
                    _IsWarmingUp = false;
                    _PowerIsOn = true;
                    IsWarmingUpFeedback.InvokeFireUpdate();
                    PowerIsOnFeedback.InvokeFireUpdate();
                };
            WarmupTimer.Start();
        }
    }

    /// <inheritdoc />
    public override void PowerOff()
    {
        // If a display has unreliable-power off feedback, just override this and
        // remove this check.
        if (PowerIsOnFeedback.BoolValue && !_IsWarmingUp && !_IsCoolingDown)
        {
            _IsCoolingDown = true;
            IsCoolingDownFeedback.InvokeFireUpdate();
            // Fake cool-down cycle
            CooldownTimer = new Timer(CooldownTime) { AutoReset = false };
            CooldownTimer.Elapsed += (s, e) =>
                {
                    Debug.LogMessage(LogEventLevel.Verbose, "Cooldown timer ending", this);
                    _IsCoolingDown = false;
                    IsCoolingDownFeedback.InvokeFireUpdate();
                    _PowerIsOn = false;
                    PowerIsOnFeedback.InvokeFireUpdate();
                };
            CooldownTimer.Start();
        }
    }

    /// <inheritdoc />
    public override void PowerToggle()
    {
        if (PowerIsOnFeedback.BoolValue && !IsWarmingUpFeedback.BoolValue)
            PowerOff();
        else if (!PowerIsOnFeedback.BoolValue && !IsCoolingDownFeedback.BoolValue)
            PowerOn();
    }

    /// <inheritdoc />
    public override void ExecuteSwitch(object selector)
    {
        try
        {
            Debug.LogMessage(LogEventLevel.Verbose, "ExecuteSwitch: {0}", this, selector);

            if (!_PowerIsOn)
            {
                PowerOn();
            }

            if (!Inputs.Items.TryGetValue(selector.ToString(), out var input))
                return;

            Debug.LogMessage(LogEventLevel.Verbose, "Selected input: {input}", this, input.Key);
            input.Select();

            var inputPort = InputPorts.FirstOrDefault(port =>
            {
                Debug.LogMessage(LogEventLevel.Verbose, "Checking input port {inputPort} with selector {portSelector} against {selector}", this, port, port.Selector, selector);
                return port.Selector.ToString() == selector.ToString();
            });

            if (inputPort == null)
            {
                Debug.LogMessage(LogEventLevel.Verbose, "Unable to find input port for selector {selector}", this, selector);
                return;
            }

            Debug.LogMessage(LogEventLevel.Verbose, "Setting current input port to {inputPort}", this, inputPort);
            CurrentInputPort = inputPort;
        }
        catch (Exception ex)
        {
            Debug.LogMessage(ex, "Error making switch: {Exception}", this, ex);
        }
    }

    /// <inheritdoc />
    public void SetInput(string selector)
    {
        ISelectableItem currentInput = null;


        if (currentInput != null)
        {
            Debug.LogMessage(LogEventLevel.Verbose, this, "SetInput: {0}", selector);
            currentInput.IsSelected = false;
        }

        if (!Inputs.Items.TryGetValue(selector, out var input))
            return;

        input.IsSelected = true;

        Inputs.CurrentItem = selector;
    }


    #region IBasicVolumeWithFeedback Members

    /// <inheritdoc />
    public IntFeedback VolumeLevelFeedback { get; private set; }

    /// <summary>
    /// SetVolume method
    /// </summary>
    public void SetVolume(ushort level)
    {
        _FakeVolumeLevel = level;
        VolumeLevelFeedback.InvokeFireUpdate();
    }

    /// <summary>
    /// MuteOn method
    /// </summary>
    public void MuteOn()
    {
        _IsMuted = true;
        MuteFeedback.InvokeFireUpdate();
    }

    /// <summary>
    /// MuteOff method
    /// </summary>
    public void MuteOff()
    {
        _IsMuted = false;
        MuteFeedback.InvokeFireUpdate();
    }

    /// <summary>
    /// Gets or sets the MuteFeedback
    /// </summary>
    public BoolFeedback MuteFeedback { get; private set; }


    #endregion

    #region IBasicVolumeControls Members

    /// <inheritdoc />
    public void VolumeUp(bool pressRelease)
    {
        if (pressRelease)
        {
            SetVolume((ushort)(_FakeVolumeLevel + VolumeInterval));
            if (_volumeUpTimer == null)
            {
                _volumeUpTimer = new Timer(VolumeHeldRepeatInterval) { AutoReset = true };
                _volumeUpTimer.Elapsed += (s, e) => SetVolume((ushort)(_FakeVolumeLevel + VolumeInterval));
                _volumeUpTimer.Start();
            }
        }
        else
        {
            _volumeUpTimer?.Stop();
            _volumeUpTimer = null;
        }
    }

    /// <inheritdoc />
    public void VolumeDown(bool pressRelease)
    {
        if (pressRelease)
        {
            SetVolume((ushort)(_FakeVolumeLevel - VolumeInterval));
            if (_volumeDownTimer == null)
            {
                _volumeDownTimer = new Timer(VolumeHeldRepeatInterval) { AutoReset = true };
                _volumeDownTimer.Elapsed += (s, e) => SetVolume((ushort)(_FakeVolumeLevel - VolumeInterval));
                _volumeDownTimer.Start();
            }
        }
        else
        {
            _volumeDownTimer?.Stop();
            _volumeDownTimer = null;
        }
    }

    /// <summary>
    /// MuteToggle method
    /// </summary>
    public void MuteToggle()
    {
        _IsMuted = !_IsMuted;
        MuteFeedback.InvokeFireUpdate();
    }

    #endregion

    /// <inheritdoc />
    public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
    {
        LinkDisplayToApi(this, trilist, joinStart, joinMapKey, bridge);
    }
}

/// <summary>
/// Represents a MockDisplayFactory
/// </summary>
public class MockDisplayFactory : EssentialsDeviceFactory<MockDisplay>
{
    /// <summary>
    /// Initializes a new instance of the MockDisplayFactory class
    /// </summary>
    public MockDisplayFactory()
    {
        TypeNames = new List<string>() { "mockdisplay", "mockdisplay2" };
    }

    /// <inheritdoc />
    public override EssentialsDevice BuildDevice(DeviceConfig dc)
    {
        Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new Mock Display Device");
        return new MockDisplay(dc.Key, dc.Name);
    }
}
