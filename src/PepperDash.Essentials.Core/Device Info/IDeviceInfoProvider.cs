using System;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceInfo
{
    /// <summary>
    /// Defines the contract for IDeviceInfoProvider
    /// </summary>
    public interface IDeviceInfoProvider:IKeyed
    {
        /// <summary>
        /// Gets the DeviceInfo
        /// </summary>
        DeviceInfo DeviceInfo { get; }

        /// <summary>
        /// Event fired when DeviceInfo changes
        /// </summary>
        event DeviceInfoChangeHandler DeviceInfoChanged;

        /// <summary>
        /// Updates the DeviceInfo
        /// </summary>
        void UpdateDeviceInfo();
    }

    /// <summary>
    /// Delegate for DeviceInfoChangeHandler
    /// </summary>
    public delegate void DeviceInfoChangeHandler(IKeyed device, DeviceInfoEventArgs args);
}