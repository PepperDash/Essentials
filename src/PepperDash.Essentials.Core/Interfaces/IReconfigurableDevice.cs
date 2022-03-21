using System;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Interfaces
{
    public interface IReconfigurableDevice
    {
        event EventHandler<EventArgs> ConfigChanged;

        DeviceConfig Config { get; }

        void SetConfig(DeviceConfig config);
    }
}