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
    }

    public abstract class Factory : IDeviceFactory
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

        #endregion

        public abstract IKeyed BuildDevice(DeviceConfig dc);

        protected Factory()
        {
            TypeNames = new List<string>();
        }
    }
}