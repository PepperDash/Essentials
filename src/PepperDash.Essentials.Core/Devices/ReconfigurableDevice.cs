

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
        /// <summary>
        /// Event fired when the configuration changes
        /// </summary>
        public event EventHandler<EventArgs> ConfigChanged;

        /// <summary>
        /// Gets the current DeviceConfig
        /// </summary>
        public DeviceConfig Config { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">config of the device</param>
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
        /// <summary>
        /// SetConfig method
        /// </summary>
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
        /// <param name="config">config of the device</param>
        protected virtual void CustomSetConfig(DeviceConfig config)
        {
            ConfigWriter.UpdateDeviceConfig(config);
        }
    }

    /// <summary>
    /// A ReconfigurableDevice that is also bridgeable
    /// </summary>
    public abstract class ReconfigurableBridgableDevice : ReconfigurableDevice, IBridgeAdvanced
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">config of the device</param>
        protected ReconfigurableBridgableDevice(DeviceConfig config) : base(config)
        {
        }

        /// <summary>
        /// LinkToApi method
        /// </summary>
        /// <param name="trilist">trilist to link</param>
        /// <param name="joinStart">the join to start at</param>
        /// <param name="joinMapKey">key to the join map</param>
        /// <param name="bridge">the bridge to use</param>
        public abstract void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge);
    }
}