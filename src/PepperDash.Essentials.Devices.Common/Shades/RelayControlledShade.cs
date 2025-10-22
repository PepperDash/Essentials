using System.Collections.Generic;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.CrestronIO;
using PepperDash.Essentials.Core.Shades;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Shades
{
    /// <summary>
    /// Controls a single shade using three relays
    /// </summary>
    public class RelayControlledShade : ShadeBase, IShadesOpenCloseStop
    {
        RelayControlledShadeConfigProperties Config;

        ISwitchedOutput OpenRelay;
        ISwitchedOutput StopOrPresetRelay;
        ISwitchedOutput CloseRelay;

        int RelayPulseTime;

        /// <summary>
        /// Gets or sets the StopOrPresetButtonLabel
        /// </summary>
        public string StopOrPresetButtonLabel { get; set; }

        /// <summary>
        /// Initializes a new instance of the RelayControlledShade class
        /// </summary>
        /// <param name="key">The device key</param>
        /// <param name="name">The device name</param>
        /// <param name="config">The relay controlled shade configuration</param>
        public RelayControlledShade(string key, string name, RelayControlledShadeConfigProperties config)
            : base(key, name)
        {
            Config = config;

            RelayPulseTime = Config.RelayPulseTime;

            StopOrPresetButtonLabel = Config.StopOrPresetLabel;

        }

        /// <summary>
        /// CustomActivate method
        /// </summary>
        /// <inheritdoc />
        public override bool CustomActivate()
        {
            //Create ISwitchedOutput objects based on props
            OpenRelay = GetSwitchedOutputFromDevice(Config.Relays.Open);
            StopOrPresetRelay = GetSwitchedOutputFromDevice(Config.Relays.StopOrPreset);
            CloseRelay = GetSwitchedOutputFromDevice(Config.Relays.Close);


            return base.CustomActivate();
        }

        /// <summary>
        /// Open method
        /// </summary>
        /// <inheritdoc />
        public override void Open()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Opening Shade: '{0}'", this.Name);

            PulseOutput(OpenRelay, RelayPulseTime);
        }

        /// <summary>
        /// Stop method
        /// </summary>
        /// <inheritdoc />
        public override void Stop()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Stopping Shade: '{0}'", this.Name);

            PulseOutput(StopOrPresetRelay, RelayPulseTime);
        }

        /// <summary>
        /// Close method
        /// </summary>
        /// <inheritdoc />
        public override void Close()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Closing Shade: '{0}'", this.Name);

            PulseOutput(CloseRelay, RelayPulseTime);
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
        ISwitchedOutput GetSwitchedOutputFromDevice(IOPortConfig relayConfig)
        {
            var portDevice = DeviceManager.GetDeviceForKey(relayConfig.PortDeviceKey);

            if (portDevice != null)
            {
                return (portDevice as ISwitchedOutputCollection).SwitchedOutputs[relayConfig.PortNumber];
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "Error: Unable to get relay on port '{0}' from device with key '{1}'", relayConfig.PortNumber, relayConfig.PortDeviceKey);
                return null;
            }
        }

    }

    /// <summary>
    /// Represents a RelayControlledShadeConfigProperties
    /// </summary>
    public class RelayControlledShadeConfigProperties
    {
        /// <summary>
        /// Gets or sets the RelayPulseTime
        /// </summary>
        public int RelayPulseTime { get; set; }
        /// <summary>
        /// Gets or sets the Relays
        /// </summary>
        public ShadeRelaysConfig Relays { get; set; }
        /// <summary>
        /// Gets or sets the StopOrPresetLabel
        /// </summary>
        public string StopOrPresetLabel { get; set; }

        /// <summary>
        /// Represents a ShadeRelaysConfig
        /// </summary>
        public class ShadeRelaysConfig
        {
            /// <summary>
            /// Gets or sets the Open
            /// </summary>
            public IOPortConfig Open { get; set; }
            /// <summary>
            /// Gets or sets the StopOrPreset
            /// </summary>
            public IOPortConfig StopOrPreset { get; set; }
            /// <summary>
            /// Gets or sets the Close
            /// </summary>
            public IOPortConfig Close { get; set; }
        }
    }

    /// <summary>
    /// Represents a RelayControlledShadeFactory
    /// </summary>
    public class RelayControlledShadeFactory : EssentialsDeviceFactory<RelayControlledShade>
    {
        /// <summary>
        /// Initializes a new instance of the RelayControlledShadeFactory class
        /// </summary>
        public RelayControlledShadeFactory()
        {
            TypeNames = new List<string>() { "relaycontrolledshade" };
        }

        /// <summary>
        /// BuildDevice method
        /// </summary>
        /// <inheritdoc />
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new Generic Comm Device");
            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<RelayControlledShadeConfigProperties>(dc.Properties.ToString());

            return new RelayControlledShade(dc.Key, dc.Name, props);
        }
    }

}