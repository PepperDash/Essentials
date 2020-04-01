using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines a class that is capable of loading device types
    /// </summary>
    public interface IDeviceFactory
    {
        /// <summary>
        /// Will be called when the plugin is loaded by Essentials.  Must add any new types to the DeviceFactory using DeviceFactory.AddFactoryForType() for each new type
        /// </summary>
        void LoadTypeFactories();
    }
}