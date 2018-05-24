using System;
using System.Collections.Generic;
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
        ISwitchedOutput StopRelay;
        ISwitchedOutput CloseRelay;

        int RelayPulseTime;

        public RelayControlledShade(string key, string name, RelayControlledShadeConfigProperties config)
            : base(key, name)
        {
            Config = config;

            RelayPulseTime = Config.RelayPulseTime;

        }

        public override bool CustomActivate()
        {
            //Create ISwitchedOutput objects based on props
            OpenRelay = GetSwitchedOutputFromDevice(Config.Relays.Open);
            StopRelay = GetSwitchedOutputFromDevice(Config.Relays.Stop);
            CloseRelay = GetSwitchedOutputFromDevice(Config.Relays.Close);


            return base.CustomActivate();
        }

        public override void Open()
        {
            Debug.Console(1, this, "Opening Shade: '{0}'", this.Name);
            StopRelay.Off();
            CloseRelay.Off();

            OpenRelay.On();
        }

        public void Stop()
        {
            Debug.Console(1, this, "Stopping Shade: '{0}'", this.Name);
            OpenRelay.Off();
            CloseRelay.Off();

            StopRelay.On();
            CTimer stopTimer = new CTimer(new CTimerCallbackFunction((o) => StopRelay.Off()), RelayPulseTime);
        }

        public override void Close()
        {
            Debug.Console(1, this, "Closing Shade: '{0}'", this.Name);
            OpenRelay.Off();
            StopRelay.Off();

            CloseRelay.On();
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

    public class RelayControlledShadeConfigProperties
    {
        public int RelayPulseTime { get; set; }
        public ShadeRelaysConfig Relays { get; set; }

        public class ShadeRelaysConfig
        {
            public IOPortConfig Open { get; set; }
            public IOPortConfig Stop { get; set; }
            public IOPortConfig Close { get; set; }
        }
    }
}