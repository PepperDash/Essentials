﻿using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.CrestronIO;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Devices.Common;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Shades
{
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
        public eScreenLiftControlType Type { get; private set; }
        public eScreenLiftControlMode Mode { get; private set; }

        public string DisplayDeviceKey { get; private set; }
        public BoolFeedback IsInUpPosition { get; private set; }

        public event EventHandler<EventArgs> PositionChanged;

        public ScreenLiftController(string key, string name, ScreenLiftControllerConfigProperties config)
            : base(key, name)
        {
            Config = config;
            DisplayDeviceKey = Config.DisplayDeviceKey;
            Mode = Config.Mode;
            Type = Config.Type;

            IsInUpPosition = new BoolFeedback(() => _isInUpPosition);

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

        public void Raise()
        {
            if (RaiseRelay == null && LatchedRelay == null) return;

            Debug.LogMessage(LogEventLevel.Debug, this, $"Raising {Type}");

            switch (Mode)
            {
                case eScreenLiftControlMode.momentary:
                    {
                        PulseOutput(RaiseRelay, RaiseRelayConfig.PulseTimeInMs);
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

        public void Lower()
        {
            if (LowerRelay == null && LatchedRelay == null) return;

            Debug.LogMessage(LogEventLevel.Debug, this, $"Lowering {Type}");

            switch (Mode)
            {
                case eScreenLiftControlMode.momentary:
                    {
                        PulseOutput(LowerRelay, LowerRelayConfig.PulseTimeInMs);
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

        void PulseOutput(ISwitchedOutput output, int pulseTime)
        {
            output.On();
            CTimer pulseTimer = new CTimer(new CTimerCallbackFunction((o) => output.Off()), pulseTime);
        }

        /// <summary>
        /// Attempts to get the port on teh specified device from config
        /// </summary>
        /// <param name="relayConfig"></param>
        /// <returns></returns>
        ISwitchedOutput GetSwitchedOutputFromDevice(string relayKey)
        {
            var portDevice = DeviceManager.GetDeviceForKey(relayKey);
            if (portDevice != null)
            {
                return (portDevice as ISwitchedOutput);
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

    public class ScreenLiftControllerConfigProperties
    {
        [JsonProperty("displayDeviceKey")]
        public string DisplayDeviceKey { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public eScreenLiftControlType Type { get; set; }

        [JsonProperty("mode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public eScreenLiftControlMode Mode { get; set; }

        [JsonProperty("relays")]
        public Dictionary<string,ScreenLiftRelaysConfig> Relays { get; set; }

    }
    public class ScreenLiftRelaysConfig
    {
        [JsonProperty("deviceKey")]
        public string DeviceKey { get; set; }

        [JsonProperty("pulseTimeInMs")]
        public int PulseTimeInMs { get; set; }
    }

    public class ScreenLiftControllerFactory : EssentialsDeviceFactory<RelayControlledShade>
    {
        public ScreenLiftControllerFactory()
        {
            TypeNames = new List<string>() { "screenliftcontroller" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new Generic Comm Device");
            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<ScreenLiftControllerConfigProperties>(dc.Properties.ToString());

            return new ScreenLiftController(dc.Key, dc.Name, props);
        }
    }

    public enum eScreenLiftControlMode
    {
        momentary,
        latched
    }
}