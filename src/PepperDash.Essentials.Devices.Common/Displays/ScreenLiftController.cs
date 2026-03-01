using System;
using System.Collections.Generic;
using System.Timers;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.CrestronIO;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Devices.Common.Displays;

namespace PepperDash.Essentials.Devices.Common.Shades
{
    /// <summary>
    /// Enumeration for requested state
    /// </summary>
    enum RequestedState
    {
        None,
        Raise,
        Lower,
    }

    /// <summary>
    /// Controls a single shade using three relays
    /// </summary>
    public class ScreenLiftController : EssentialsDevice, IProjectorScreenLiftControl
    {
        readonly ScreenLiftControllerConfigProperties Config;
        readonly ScreenLiftRelaysConfig RaiseRelayConfig;
        readonly ScreenLiftRelaysConfig LowerRelayConfig;
        readonly ScreenLiftRelaysConfig LatchedRelayConfig;

        DisplayBase DisplayDevice;
        ISwitchedOutput RaiseRelay;
        ISwitchedOutput LowerRelay;
        ISwitchedOutput LatchedRelay;

        private bool _isMoving;
        private RequestedState _requestedState;
        private RequestedState _currentMovement;
        private Timer _movementTimer;

        /// <summary>
        /// Gets or sets the InUpPosition
        /// </summary>
        public bool InUpPosition
        {
            get { return _isInUpPosition; }
            set
            {
                if (value == _isInUpPosition)
                    return;
                _isInUpPosition = value;
                IsInUpPosition.FireUpdate();
                PositionChanged?.Invoke(this, new EventArgs());
            }
        }

        private bool _isInUpPosition { get; set; }

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public eScreenLiftControlType Type { get; private set; }

        /// <summary>
        /// Gets or sets the Mode
        /// </summary>
        public eScreenLiftControlMode Mode { get; private set; }

        /// <summary>
        /// Gets or sets the DisplayDeviceKey
        /// </summary>
        public string DisplayDeviceKey { get; private set; }

        /// <summary>
        /// Gets or sets the IsInUpPosition
        /// </summary>
        public BoolFeedback IsInUpPosition { get; private set; }

        /// <summary>
        /// Event that fires when the position changes
        /// </summary>
        public event EventHandler<EventArgs> PositionChanged;

        /// <summary>
        /// Constructor for ScreenLiftController
        /// </summary>
        public ScreenLiftController(
            string key,
            string name,
            ScreenLiftControllerConfigProperties config
        )
            : base(key, name)
        {
            Config = config;
            DisplayDeviceKey = Config.DisplayDeviceKey;
            Mode = Config.Mode;
            Type = Config.Type;

            IsInUpPosition = new BoolFeedback("isInUpPosition", () => _isInUpPosition);

            // Initialize movement timer for reuse
            _movementTimer = new Timer();
            _movementTimer.Elapsed += OnMovementComplete;
            _movementTimer.AutoReset = false;

            switch (Mode)
            {
                case eScreenLiftControlMode.momentary:
                {
                    RaiseRelayConfig = Config.Relays["raise"];
                    LowerRelayConfig = Config.Relays["lower"];
                    break;
                }
                case eScreenLiftControlMode.latched:
                {
                    LatchedRelayConfig = Config.Relays["latched"];
                    break;
                }
            }

            IsInUpPosition.OutputChange += (sender, args) =>
            {
                this.LogDebug(
                    "ScreenLiftController '{name}' IsInUpPosition changed to {position}",
                    Name,
                    IsInUpPosition.BoolValue ? "Up" : "Down"
                );

                if (!Config.MuteOnScreenUp)
                {
                    return;
                }

                if (args.BoolValue)
                {
                    return;
                }

                if (DisplayDevice is IBasicVideoMuteWithFeedback videoMute)
                {
                    this.LogInformation("Unmuting video because screen is down");
                    videoMute.VideoMuteOff();
                }
            };

            IsInUpPosition.FireUpdate();
        }

        private void IsCoolingDownFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            if (
                !DisplayDevice.IsCoolingDownFeedback.BoolValue
                && Type == eScreenLiftControlType.lift
            )
            {
                Raise();
                return;
            }
            if (
                DisplayDevice.IsCoolingDownFeedback.BoolValue
                && Type == eScreenLiftControlType.screen
            )
            {
                Raise();
                return;
            }
        }

        private void IsWarmingUpFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            if (DisplayDevice.IsWarmingUpFeedback.BoolValue)
            {
                Lower();
            }
        }

        /// <summary>
        /// CustomActivate method
        /// </summary>
        /// <inheritdoc />
        public override bool CustomActivate()
        {
            //Create ISwitchedOutput objects based on props
            switch (Mode)
            {
                case eScreenLiftControlMode.momentary:
                {
                    this.LogDebug("Getting relays for {mode}", Mode);
                    RaiseRelay = GetSwitchedOutputFromDevice(RaiseRelayConfig.DeviceKey);
                    LowerRelay = GetSwitchedOutputFromDevice(LowerRelayConfig.DeviceKey);
                    break;
                }
                case eScreenLiftControlMode.latched:
                {
                    this.LogDebug("Getting relays for {mode}", Mode);
                    LatchedRelay = GetSwitchedOutputFromDevice(LatchedRelayConfig.DeviceKey);
                    break;
                }
            }

            this.LogDebug("Getting display with key {displayKey}", DisplayDeviceKey);
            DisplayDevice = GetDisplayBaseFromDevice(DisplayDeviceKey);

            if (DisplayDevice != null)
            {
                this.LogDebug("Subscribing to {displayKey} feedbacks", DisplayDeviceKey);

                DisplayDevice.IsWarmingUpFeedback.OutputChange += IsWarmingUpFeedback_OutputChange;
                DisplayDevice.IsCoolingDownFeedback.OutputChange +=
                    IsCoolingDownFeedback_OutputChange;
            }

            return base.CustomActivate();
        }

        /// <summary>
        /// Raise method
        /// </summary>
        public void Raise()
        {
            if (RaiseRelay == null && LatchedRelay == null)
                return;

            this.LogDebug("Raise called for {type}", Type);

            if (Config.MuteOnScreenUp && DisplayDevice is IBasicVideoMuteWithFeedback videoMute)
            {
                this.LogInformation("Muting video because screen is going up");
                videoMute.VideoMuteOn();
            }

            // If device is moving, bank the command
            if (_isMoving)
            {
                this.LogDebug("Device is moving, banking Raise command");
                _requestedState = RequestedState.Raise;
                return;
            }

            this.LogDebug("Raising {type}", Type);

            switch (Mode)
            {
                case eScreenLiftControlMode.momentary:
                {
                    PulseOutput(RaiseRelay, RaiseRelayConfig.PulseTimeInMs);

                    // Set moving flag and start timer if movement time is configured
                    if (RaiseRelayConfig.MoveTimeInMs > 0)
                    {
                        _isMoving = true;
                        _currentMovement = RequestedState.Raise;
                        if (_movementTimer.Enabled)
                        {
                            _movementTimer.Stop();
                        }
                        _movementTimer.Interval = RaiseRelayConfig.MoveTimeInMs;
                        _movementTimer.Start();
                    }
                    else
                    {
                        InUpPosition = true;
                    }
                    break;
                }
                case eScreenLiftControlMode.latched:
                {
                    LatchedRelay.Off();
                    InUpPosition = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Lower method
        /// </summary>
        public void Lower()
        {
            if (LowerRelay == null && LatchedRelay == null)
                return;

            this.LogDebug("Lower called for {type}", Type);

            // If device is moving, bank the command
            if (_isMoving)
            {
                this.LogDebug("Device is moving, banking Lower command");
                _requestedState = RequestedState.Lower;
                return;
            }

            this.LogDebug("Lowering {type}", Type);

            switch (Mode)
            {
                case eScreenLiftControlMode.momentary:
                {
                    PulseOutput(LowerRelay, LowerRelayConfig.PulseTimeInMs);

                    // Set moving flag and start timer if movement time is configured
                    if (LowerRelayConfig.MoveTimeInMs > 0)
                    {
                        _isMoving = true;
                        _currentMovement = RequestedState.Lower;
                        if (_movementTimer.Enabled)
                        {
                            _movementTimer.Stop();
                        }
                        _movementTimer.Interval = LowerRelayConfig.MoveTimeInMs;
                        _movementTimer.Start();
                    }
                    else
                    {
                        InUpPosition = false;
                    }
                    break;
                }
                case eScreenLiftControlMode.latched:
                {
                    LatchedRelay.On();
                    InUpPosition = false;
                    break;
                }
            }
        }

        private void DisposeMovementTimer()
        {
            if (_movementTimer != null)
            {
                _movementTimer.Stop();
                _movementTimer.Elapsed -= OnMovementComplete;
                _movementTimer.Dispose();
                _movementTimer = null;
            }
        }

        /// <summary>
        /// Called when movement timer completes
        /// </summary>
        private void OnMovementComplete(object sender, ElapsedEventArgs e)
        {
            this.LogDebug("Movement complete");

            // Update position based on completed movement
            if (_currentMovement == RequestedState.Raise)
            {
                InUpPosition = true;
            }
            else if (_currentMovement == RequestedState.Lower)
            {
                InUpPosition = false;
            }

            _isMoving = false;
            _currentMovement = RequestedState.None;

            // Execute banked command if one exists
            if (_requestedState != RequestedState.None)
            {
                this.LogDebug("Executing next command: {command}", _requestedState);

                var commandToExecute = _requestedState;
                _requestedState = RequestedState.None;

                // Check if current state matches what the banked command would do and execute if different
                switch (commandToExecute)
                {
                    case RequestedState.Raise:
                        Raise();
                        break;

                    case RequestedState.Lower:
                        Lower();
                        break;
                }
            }
        }

        private void PulseOutput(ISwitchedOutput output, int pulseTime)
        {
            output.On();

            var timer = new Timer(pulseTime) { AutoReset = false };

            timer.Elapsed += (sender, e) =>
            {
                output.Off();
                timer.Dispose();
            };
            timer.Start();
        }

        private ISwitchedOutput GetSwitchedOutputFromDevice(string relayKey)
        {
            var portDevice = DeviceManager.GetDeviceForKey<ISwitchedOutput>(relayKey);
            if (portDevice != null)
            {
                return portDevice;
            }
            else
            {
                this.LogWarning(
                    "Error: Unable to get relay device with key '{relayKey}'",
                    relayKey
                );
                return null;
            }
        }

        private DisplayBase GetDisplayBaseFromDevice(string displayKey)
        {
            var displayDevice = DeviceManager.GetDeviceForKey<DisplayBase>(displayKey);
            if (displayDevice != null)
            {
                return displayDevice;
            }
            else
            {
                this.LogWarning(
                    "Error: Unable to get display device with key '{displayKey}'",
                    displayKey
                );
                return null;
            }
        }
    }

    /// <summary>
    /// Factory for ScreenLiftController devices
    /// </summary>
    public class ScreenLiftControllerFactory : EssentialsDeviceFactory<RelayControlledShade>
    {
        /// <summary>
        /// Constructor for ScreenLiftControllerFactory
        /// </summary>
        public ScreenLiftControllerFactory()
        {
            TypeNames = new List<string>() { "screenliftcontroller" };
        }

        /// <inheritdoc />
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogDebug("Factory Attempting to create new ScreenLiftController Device");
            var props = dc.Properties.ToObject<ScreenLiftControllerConfigProperties>();

            return new ScreenLiftController(dc.Key, dc.Name, props);
        }
    }
}
