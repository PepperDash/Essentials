using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Devices
{
    /// <summary>
    /// This class should be inherited from when the configuration can be modified at runtime from another source other than the configuration file. 
    /// It contains the necessary properties, methods and events to allot the initial device configuration to be overridden and then notifies the 
    /// ConfigWriter to write out the new values to a local file to be read on next boot.
    /// </summary>
    public abstract class ReconfigurableDevice : Device
    {
        public event EventHandler<EventArgs> ConfigChanged;

        public DeviceConfig Config { get; private set; }

        public ReconfigurableDevice(DeviceConfig config)
            : base(config.Key)
        {
            SetNameHelper(config);

            Config = config;
        }

        /// <summary>
        /// Sets the Config, calls CustomSetConfig and fires the ConfigChanged event
        /// </summary>
        /// <param name="config"></param>
        public void SetConfig(DeviceConfig config)
        {
            Config = config;

            SetNameHelper(config);

            CustomSetConfig(config);

            var handler = ConfigChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        void SetNameHelper(DeviceConfig config)
        {
            if (!string.IsNullOrEmpty(config.Name))
                Name = config.Name;
        }

        /// <summary>
        /// Used by the extending class to allow for any custom actions to be taken (tell the ConfigWriter to write config, etc)
        /// </summary>
        /// <param name="Config"></param>
        protected virtual void CustomSetConfig(DeviceConfig config)
        {
            ConfigWriter.UpdateDeviceConfig(config);
        }
    }
}