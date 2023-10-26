using System;
using Crestron.SimplSharp.Reflection;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
    public class DeviceFactoryWrapper
    {
        public CType CType { get; set; }
        public string Description { get; set; }
        public Func<DeviceConfig, IKeyed> FactoryMethod { get; set; }

        public DeviceFactoryWrapper()
        {
            CType       = null;
            Description = "Not Available";
        }
    }
}