

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PepperDash.Essentials.Core.Devices
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ReconfigurableDevice : EssentialsDevice, IReconfigurableDevice
    {
        public event EventHandler<EventArgs> ConfigChanged;

        public DeviceConfig Config { get; private set; }

        protected ReconfigurableDevice(DeviceConfig config)
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
        /// <param name="config"></param>
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