using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.CrestronIO;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Shades
{
    /// <summary>
    /// Enumeration for requested state
    /// </summary>
    enum RequestedState
    {
        None,
        Raise,
        Lower
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

        Displays.DisplayBase DisplayDevice;
        ISwitchedOutput RaiseRelay;
        ISwitchedOutput LowerRelay;
        ISwitchedOutput LatchedRelay;

        private bool _isMoving;
        private RequestedState _requestedState;
        private CTimer _movementTimer;

        /// <summary>
        /// Gets or sets the InUpPosition
        /// </summary>
        public bool InUpPosition
        {
            get { return _isInUpPosition; }
            set
            {
                if (value == _isInUpPosition) return;
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
        public ScreenLiftController(string key, string name, ScreenLiftControllerConfigProperties config)
            : base(key, name)
        {
            Config = config;
            DisplayDeviceKey = Config.DisplayDeviceKey;
            Mode = Config.Mode;
            Type = Config.Type;

            IsInUpPosition = new BoolFeedback("isInUpPosition", () => _isInUpPosition);

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
        }

        private void IsCoolingDownFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            if (!DisplayDevice.IsCoolingDownFeedback.BoolValue && Type == eScreenLiftControlType.lift)
            {
                Raise();
                return;
            }
            if (DisplayDevice.IsCoolingDownFeedback.BoolValue && Type == eScreenLiftControlType.screen)
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
                        Debug.LogMessage(LogEventLevel.Debug, this, $"Getting relays for {Mode}");
                        RaiseRelay = GetSwitchedOutputFromDevice(RaiseRelayConfig.DeviceKey);
                        LowerRelay = GetSwitchedOutputFromDevice(LowerRelayConfig.DeviceKey);
                        break;
                    }
                case eScreenLiftControlMode.latched:
                    {
                        Debug.LogMessage(LogEventLevel.Debug, this, $"Getting relays for {Mode}");
                        LatchedRelay = GetSwitchedOutputFromDevice(LatchedRelayConfig.DeviceKey);
                        break;
                    }
            }

            Debug.LogMessage(LogEventLevel.Debug, this, $"Getting display with key {DisplayDeviceKey}");
            DisplayDevice = GetDisplayBaseFromDevice(DisplayDeviceKey);

            if (DisplayDevice != null)
            {
                Debug.LogMessage(LogEventLevel.Debug, this, $"Subscribing to {DisplayDeviceKey} feedbacks");

                DisplayDevice.IsWarmingUpFeedback.OutputChange += IsWarmingUpFeedback_OutputChange;
                DisplayDevice.IsCoolingDownFeedback.OutputChange += IsCoolingDownFeedback_OutputChange;
            }

            return base.CustomActivate();
        }

        /// <summary>
        /// Raise method
        /// </summary>
        public void Raise()
        {
            if (RaiseRelay == null && LatchedRelay == null) return;

            Debug.LogMessage(LogEventLevel.Debug, this, $"Raise called for {Type}");

            // If device is moving, bank the command
            if (_isMoving)
            {
                Debug.LogMessage(LogEventLevel.Debug, this, $"Device is moving, banking Raise command");
                _requestedState = RequestedState.Raise;
                return;
            }

            Debug.LogMessage(LogEventLevel.Debug, this, $"Raising {Type}");

            switch (Mode)
            {
                case eScreenLiftControlMode.momentary:
                    {
                        PulseOutput(RaiseRelay, RaiseRelayConfig.PulseTimeInMs);
                        
                        // Set moving flag and start timer if movement time is configured
                        if (RaiseRelayConfig.RaiseTimeInMs > 0)
                        {
                            _isMoving = true;
                            DisposeMovementTimer();
                            _movementTimer = new CTimer(OnMovementComplete, RaiseRelayConfig.RaiseTimeInMs);
                        }
                        break;
                    }
                case eScreenLiftControlMode.latched:
                    {
                        LatchedRelay.Off();
                        break;
                    }
            }
            InUpPosition = true;
        }

        /// <summary>
        /// Lower method
        /// </summary>
        public void Lower()
        {
            if (LowerRelay == null && LatchedRelay == null) return;

            Debug.LogMessage(LogEventLevel.Debug, this, $"Lower called for {Type}");

            // If device is moving, bank the command
            if (_isMoving)
            {
                Debug.LogMessage(LogEventLevel.Debug, this, $"Device is moving, banking Lower command");
                _requestedState = RequestedState.Lower;
                return;
            }

            Debug.LogMessage(LogEventLevel.Debug, this, $"Lowering {Type}");

            switch (Mode)
            {
                case eScreenLiftControlMode.momentary:
                    {
                        PulseOutput(LowerRelay, LowerRelayConfig.PulseTimeInMs);
                        
                        // Set moving flag and start timer if movement time is configured
                        if (LowerRelayConfig.LowerTimeInMs > 0)
                        {
                            _isMoving = true;
                            DisposeMovementTimer();
                            _movementTimer = new CTimer(OnMovementComplete, LowerRelayConfig.LowerTimeInMs);
                        }
                        break;
                    }
                case eScreenLiftControlMode.latched:
                    {
                        LatchedRelay.On();
                        break;
                    }
            }
            InUpPosition = false;
        }

        /// <summary>
        /// Disposes the current movement timer if it exists
        /// </summary>
        private void DisposeMovementTimer()
        {
            if (_movementTimer != null)
            {
                _movementTimer.Stop();
                _movementTimer.Dispose();
                _movementTimer = null;
            }
        }

        /// <summary>
        /// Called when movement timer completes
        /// </summary>
        private void OnMovementComplete(object o)
        {
            Debug.LogMessage(LogEventLevel.Debug, this, $"Movement complete");
            
            _isMoving = false;
            
            // Execute banked command if one exists
            if (_requestedState != RequestedState.None)
            {
                Debug.LogMessage(LogEventLevel.Debug, this, $"Executing banked command: {_requestedState}");
                
                var commandToExecute = _requestedState;
                _requestedState = RequestedState.None;
                
                // Check if current state matches what the banked command would do and execute if different
                switch (commandToExecute)
                {
                    case RequestedState.Raise:
                        if (InUpPosition)
                        {
                            Debug.LogMessage(LogEventLevel.Debug, this, $"Already in up position, ignoring banked Raise command");
                        }
                        else
                        {
                            Raise();
                        }
                        break;
                        
                    case RequestedState.Lower:
                        if (!InUpPosition)
                        {
                            Debug.LogMessage(LogEventLevel.Debug, this, $"Already in down position, ignoring banked Lower command");
                        }
                        else
                        {
                            Lower();
                        }
                        break;
                }
            }
        }

        void PulseOutput(ISwitchedOutput output, int pulseTime)
        {
            output.On();
            CTimer pulseTimer = new CTimer(new CTimerCallbackFunction((o) => output.Off()), pulseTime);
        }

        /// <summary>
        /// Attempts to get the port on teh specified device from config
        /// </summary>
        /// <param name="relayKey"></param>
        /// <returns></returns>
        ISwitchedOutput GetSwitchedOutputFromDevice(string relayKey)
        {
            var portDevice = DeviceManager.GetDeviceForKey(relayKey);
            if (portDevice != null)
            {
                return portDevice as ISwitchedOutput;
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "Error: Unable to get relay device with key '{0}'", relayKey);
                return null;
            }
        }

        Displays.DisplayBase GetDisplayBaseFromDevice(string displayKey)
        {
            var displayDevice = DeviceManager.GetDeviceForKey(displayKey);
            if (displayDevice != null)
            {
                return displayDevice as Displays.DisplayBase;
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "Error: Unable to get display device with key '{0}'", displayKey);
                return null;
            }
        }

    }

    /// <summary>
    /// Represents a ScreenLiftControllerFactory
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

        /// <summary>
        /// BuildDevice method
        /// </summary>
        /// <inheritdoc />
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new Generic Comm Device");
            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<ScreenLiftControllerConfigProperties>(dc.Properties.ToString());

            return new ScreenLiftController(dc.Key, dc.Name, props);
        }
    }
}