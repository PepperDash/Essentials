using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the basic needs for an EssentialsDevice to enable it to be build by an IDeviceFactory class
    /// </summary>
    public abstract class EssentialsDevice : Device
    {
        protected EssentialsDevice(string key)
            : base(key)
        {

        }

        protected EssentialsDevice(string key, string name)
            : base(key, name)
        {

        }
    }

    /// <summary>
    /// Devices the basic needs for a Device Factory
    /// </summary>
    public abstract class EssentialsDeviceFactory<T> : IDeviceFactory where T:EssentialsDevice
    {
        #region IDeviceFactory Members

        /// <summary>
        /// A list of strings that can be used in the type property of a DeviceConfig object to build an instance of this device
        /// </summary>
        public List<string> TypeNames { get; protected set; }

        /// <summary>
        /// Loads an item to the DeviceFactory.FactoryMethods dictionary for each entry in the TypeNames list
        /// </summary>
        public void LoadTypeFactories()
        {
            foreach (var typeName in TypeNames)
            {
                DeviceFactory.AddFactoryForType(typeName, BuildDevice);
            }
        }

        /// <summary>
        /// The method that will build the device
        /// </summary>
        /// <param name="dc">The device config</param>
        /// <returns>An instance of the device</returns>
        public abstract EssentialsDevice BuildDevice(DeviceConfig dc);

        #endregion
    }

    /// <summary>
    /// Devices the basic needs for a Device Factory
    /// </summary>
    public abstract class EssentialsPluginDeviceFactory<T> : EssentialsDeviceFactory<T>, IPluginDeviceFactory where T : EssentialsDevice
    {
        /// <summary>
        /// Specifies the minimum version of Essentials required for a plugin to run.  Must use the format Major.Minor.Build (ex. "1.4.33")
        /// </summary>
        public string MinimumEssentialsFrameworkVersion { get; protected set; }
    }
}