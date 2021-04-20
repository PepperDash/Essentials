using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Devices
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ReconfigurableDevice : EssentialsDevice
    {
        public event EventHandler<EventArgs> ConfigChanged;

        public DeviceConfig Config { get; private set; }

        protected ReconfigurableDevice(DeviceConfig config)
            : this(config.Key)
        {
        }

        protected ReconfigurableDevice(string key) : base(key)
        {
            //getting config directly from ConfigReader to avoid the possiblitiy of writing secrets to file
            Config = ConfigReader.ConfigObject.GetDeviceForKey(key);

            SetNameHelper(Config);
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

    public abstract class ReconfigurableBridgableDevice : ReconfigurableDevice, IBridgeAdvanced
    {
        protected ReconfigurableBridgableDevice(DeviceConfig config) : base(config)
        {
        }

        public abstract void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge);
    }
}