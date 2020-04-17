using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;

using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines a class that is capable of loading device types
    /// </summary>
    public interface IDeviceFactory
    {
        /// <summary>
        /// Loads all the types to the DeviceFactory
        /// </summary>
        void LoadTypeFactories();
    }
}