using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Displays
{
    /// <summary>
    /// Represents a MockDisplay
    /// </summary>
    public class MockDisplay : TwoWayDisplayBase, IBasicVolumeWithFeedback, IBridgeAdvanced, IHasInputs<string>, IRoutingSinkWithSwitchingWithInputPort, IHasPowerControlWithFeedback
    {
        /// <summary>
        /// Gets or sets the Inputs
        /// </summary>
        public ISelectableItems<string> Inputs { get; private set; }

        bool _PowerIsOn;
        bool _IsWarmingUp;
        bool _IsCoolingDown;

        /// <summary>
        /// Gets the power is on feedback function
        /// </summary>
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
        /// <summary>
        /// Gets the is cooling down feedback function
        /// </summary>
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
        /// <summary>
        /// Gets the is warming up feedback function
        /// </summary>
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
        /// <summary>
        /// Gets the current input feedback function
        /// </summary>
        protected override Func<string> CurrentInputFeedbackFunc { get { return () => Inputs.CurrentItem; } }

        int VolumeHeldRepeatInterval = 200;
        ushort VolumeInterval = 655;
        ushort _FakeVolumeLevel = 31768;
        bool _IsMuted;

        /// <summary>
        /// Initializes a new instance of the MockDisplay class
        /// </summary>
        /// <param name="key">The device key</param>
        /// <param name="name">The device name</param>
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
                WarmupTimer = new CTimer(o =>
                    {
                        _IsWarmingUp = false;
                        _PowerIsOn = true;
                        IsWarmingUpFeedback.InvokeFireUpdate();
                        PowerIsOnFeedback.InvokeFireUpdate();
                    }, WarmupTime);
            }
        }

        /// <summary>
        /// PowerOff method
        /// </summary>
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
                CooldownTimer = new CTimer(o =>
                    {
                        Debug.LogMessage(LogEventLevel.Verbose, "Cooldown timer ending", this);
                        _IsCoolingDown = false;
                        IsCoolingDownFeedback.InvokeFireUpdate();
                        _PowerIsOn = false;
                        PowerIsOnFeedback.InvokeFireUpdate();
                    }, CooldownTime);
            }
        }

        /// <summary>
        /// PowerToggle method
        /// </summary>
        /// <inheritdoc />
        public override void PowerToggle()
        {
            if (PowerIsOnFeedback.BoolValue && !IsWarmingUpFeedback.BoolValue)
                PowerOff();
            else if (!PowerIsOnFeedback.BoolValue && !IsCoolingDownFeedback.BoolValue)
                PowerOn();
        }

        /// <summary>
        /// ExecuteSwitch method
        /// </summary>
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

        /// <summary>
        /// SetInput method
        /// </summary>
        public void SetInput(string selector)
        {
            ISelectableItem currentInput = null;

            try
            {
                currentInput = Inputs.Items.SingleOrDefault(Inputs => Inputs.Value.IsSelected).Value;
            }
            catch { }


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

        /// <summary>
        /// Gets or sets the VolumeLevelFeedback
        /// </summary>
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

        /// <summary>
        /// VolumeUp method
        /// </summary>
        public void VolumeUp(bool pressRelease)
        {
            //while (pressRelease)
            //{
            Debug.LogMessage(LogEventLevel.Verbose, this, "Volume Down {0}", pressRelease);
            if (pressRelease)
            {
                var newLevel = _FakeVolumeLevel + VolumeInterval;
                SetVolume((ushort)newLevel);
                CrestronEnvironment.Sleep(VolumeHeldRepeatInterval);
            }
            //}
        }

        /// <summary>
        /// VolumeDown method
        /// </summary>
        public void VolumeDown(bool pressRelease)
        {
            //while (pressRelease)
            //{
            Debug.LogMessage(LogEventLevel.Verbose, this, "Volume Up {0}", pressRelease);
            if (pressRelease)
            {
                var newLevel = _FakeVolumeLevel - VolumeInterval;
                SetVolume((ushort)newLevel);
                CrestronEnvironment.Sleep(VolumeHeldRepeatInterval);
            }
            //}
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

        /// <summary>
        /// LinkToApi method
        /// </summary>
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

        /// <summary>
        /// BuildDevice method
        /// </summary>
        /// <inheritdoc />
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new Mock Display Device");
            return new MockDisplay(dc.Key, dc.Name);
        }
    }
}