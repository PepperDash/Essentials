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
        event EventHandler<EventArgs> ConfigChanged;

        DeviceConfig Config { get; }

        void SetConfig(DeviceConfig config);
    }
}