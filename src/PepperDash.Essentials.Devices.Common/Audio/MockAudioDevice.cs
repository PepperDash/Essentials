using System.Collections.Generic;
using System.Timers;

using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common;

/// <summary>
/// Mock audio device for testing and simulation purposes. Behaves like a real DSP fader channel,
/// with ramping volume up/down while pressed and mute on/off/toggle, all backed by fake in-memory
/// state rather than any real hardware communication.
/// </summary>
public class MockAudioDevice : EssentialsDevice, IBasicVolumeWithFeedback
{
    private const int VolumeHeldRepeatIntervalMs = 100;
    private const ushort VolumeStep = 655;

    private ushort _volumeLevel = 32768;
    private bool _isMuted;
    private Timer _volumeUpTimer;
    private Timer _volumeDownTimer;

    /// <inheritdoc />
    public IntFeedback VolumeLevelFeedback { get; private set; }

    /// <inheritdoc />
    public BoolFeedback MuteFeedback { get; private set; }

    /// <summary>
    /// Constructor for MockAudioDevice
    /// </summary>
    /// <param name="key">Device key</param>
    /// <param name="name">Device name</param>
    public MockAudioDevice(string key, string name)
        : base(key, name)
    {
        VolumeLevelFeedback = new IntFeedback("volume", () => _volumeLevel);
        MuteFeedback = new BoolFeedback("muteOn", () => _isMuted);

        // Seed the feedback's cached value immediately so the UI reflects the starting level
        // rather than 0 until the first volume change fires an update.
        VolumeLevelFeedback.FireUpdate();
        MuteFeedback.FireUpdate();
    }

    /// <inheritdoc />
    public void SetVolume(ushort level)
    {
        _volumeLevel = level;
        VolumeLevelFeedback.InvokeFireUpdate();
        this.LogDebug("SetVolume: {Level}", _volumeLevel);
    }

    /// <inheritdoc />
    public void MuteOn()
    {
        _isMuted = true;
        MuteFeedback.InvokeFireUpdate();
    }

    /// <inheritdoc />
    public void MuteOff()
    {
        _isMuted = false;
        MuteFeedback.InvokeFireUpdate();
    }

    /// <inheritdoc />
    public void MuteToggle()
    {
        if (_isMuted)
            MuteOff();
        else
            MuteOn();
    }

    /// <inheritdoc />
    public void VolumeUp(bool pressRelease)
    {
        if (pressRelease)
        {
            RampVolume(VolumeStep);

            if (_volumeUpTimer == null)
            {
                _volumeUpTimer = new Timer(VolumeHeldRepeatIntervalMs) { AutoReset = true };
                _volumeUpTimer.Elapsed += (s, e) => RampVolume(VolumeStep);
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
            RampVolume(-VolumeStep);

            if (_volumeDownTimer == null)
            {
                _volumeDownTimer = new Timer(VolumeHeldRepeatIntervalMs) { AutoReset = true };
                _volumeDownTimer.Elapsed += (s, e) => RampVolume(-VolumeStep);
                _volumeDownTimer.Start();
            }
        }
        else
        {
            _volumeDownTimer?.Stop();
            _volumeDownTimer = null;
        }
    }

    // Clamps to ushort range so repeated ramping at the extremes doesn't wrap around.
    private void RampVolume(int delta)
    {
        var newLevel = _volumeLevel + delta;
        if (newLevel < ushort.MinValue)
            newLevel = ushort.MinValue;
        else if (newLevel > ushort.MaxValue)
            newLevel = ushort.MaxValue;

        SetVolume((ushort)newLevel);
    }
}

/// <summary>
/// Factory for building <see cref="MockAudioDevice"/> devices.
/// </summary>
public class MockAudioDeviceFactory : EssentialsDeviceFactory<MockAudioDevice>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MockAudioDeviceFactory"/> class.
    /// </summary>
    public MockAudioDeviceFactory()
    {
        TypeNames = new List<string> { "mockaudiodevice", "mockaudio" };
    }

    /// <inheritdoc />
    public override EssentialsDevice BuildDevice(DeviceConfig dc)
    {
        Debug.LogMessage(LogEventLevel.Debug, "Factory attempting to create new Mock Audio Device");
        return new MockAudioDevice(dc.Key, dc.Name);
    }
}
