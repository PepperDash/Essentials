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

        public List<string> TypeNames { get; protected set; }

        public virtual void LoadTypeFactories()
        {
            foreach (var typeName in TypeNames)
            {
                DeviceFactory.AddFactoryForType(typeName, BuildDevice);
            }
        }

        public abstract EssentialsDevice BuildDevice(DeviceConfig dc);

        #endregion
    }

    /// <summary>
    /// Devices the basic needs for a Device Factory
    /// </summary>
    public abstract class EssentialsPluginDeviceFactory<T> : EssentialsDeviceFactory<T>, IPluginDeviceFactory where T : EssentialsDevice
    {
        public string MinimumEssentialsFrameworkVersion { get; protected set; }
    }
}