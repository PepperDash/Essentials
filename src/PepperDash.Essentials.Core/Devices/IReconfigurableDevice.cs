using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core.Config;


namespace PepperDash.Essentials.Core.Devices
{
    /// <summary>
    /// Defines the contract for IReconfigurableDevice
    /// </summary>
    public interface IReconfigurableDevice
    {
        /// <summary>
        /// Event fired when the configuration changes
        /// </summary>
        event EventHandler<EventArgs> ConfigChanged;

        /// <summary>
        /// Gets the current DeviceConfig
        /// </summary>
        DeviceConfig Config { get; }

        /// <summary>
        /// Sets the DeviceConfig
        /// </summary>
        /// <param name="config">config to set</param>
        void SetConfig(DeviceConfig config);
    }
}