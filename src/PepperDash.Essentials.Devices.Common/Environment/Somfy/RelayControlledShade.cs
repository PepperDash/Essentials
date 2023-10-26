using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.CrestronIO;
using PepperDash.Essentials.Core.Shades;

namespace PepperDash.Essentials.Devices.Common.Environment.Somfy
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

        public string StopOrPresetButtonLabel { get; set; }

        public RelayControlledShade(string key, string name, RelayControlledShadeConfigProperties config)
            : base(key, name)
        {
            Config = config;

            RelayPulseTime = Config.RelayPulseTime;

            StopOrPresetButtonLabel = Config.StopOrPresetLabel;

        }

        public override bool CustomActivate()
        {
            //Create ISwitchedOutput objects based on props
            OpenRelay = GetSwitchedOutputFromDevice(Config.Relays.Open);
            StopOrPresetRelay = GetSwitchedOutputFromDevice(Config.Relays.StopOrPreset);
            CloseRelay = GetSwitchedOutputFromDevice(Config.Relays.Close);


            return base.CustomActivate();
        }

        public override void Open()
        {
            Debug.Console(1, this, "Opening Shade: '{0}'", this.Name);

            PulseOutput(OpenRelay, RelayPulseTime);
        }

        public override void Stop()
        {
            Debug.Console(1, this, "Stopping Shade: '{0}'", this.Name);

            PulseOutput(StopOrPresetRelay, RelayPulseTime);
        }

        public override void Close()
        {
            Debug.Console(1, this, "Closing Shade: '{0}'", this.Name);

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
                Debug.Console(1, this, "Error: Unable to get relay on port '{0}' from device with key '{1}'", relayConfig.PortNumber, relayConfig.PortDeviceKey);
                return null;
            }
        }

    }
}