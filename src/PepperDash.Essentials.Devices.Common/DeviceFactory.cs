

using System;
using System.Linq;
using System.Reflection;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common
{
    /// <summary>
    /// Represents a DeviceFactory
    /// </summary>
    public class DeviceFactory
    {
        /// <summary>
        /// Initializes a new instance of the DeviceFactory class
        /// </summary>
        public DeviceFactory()
        {
            var assy = Assembly.GetExecutingAssembly();
            PluginLoader.SetEssentialsAssembly(assy.GetName().Name, assy);

            var types = assy.GetTypes().Where(ct => typeof(IDeviceFactory).IsAssignableFrom(ct) && !ct.IsInterface && !ct.IsAbstract);

            if (types != null)
            {
                foreach (var type in types)
                {
                    try
                    {
                        var factory = (IDeviceFactory)Activator.CreateInstance(type);
                        LoadDeviceFactories(factory);
                    }
                    catch (Exception e)
                    {
                        Debug.LogMessage(LogEventLevel.Information, "Unable to load type: '{1}' DeviceFactory: {0}", e, type.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Loads device factories from the specified plugin device factory and registers them for use.
        /// </summary>
        /// <remarks>This method retrieves metadata from the provided <paramref name="deviceFactory"/>, including
        /// type names, descriptions, and configuration snippets, and registers the factory for each device type. The type
        /// names are converted to lowercase for registration.</remarks>
        /// <param name="deviceFactory">The plugin device factory that provides the device types, descriptions, and factory methods to be registered.</param>
        private static void LoadDeviceFactories(IDeviceFactory deviceFactory)
        {
            foreach (var typeName in deviceFactory.TypeNames)
            {
                //Debug.LogMessage(LogEventLevel.Verbose, "Getting Description Attribute from class: '{0}'", typeof(T).FullName);
                string description = (deviceFactory.FactoryType.GetCustomAttributes(typeof(DescriptionAttribute), true) is DescriptionAttribute[] descriptionAttribute && descriptionAttribute.Length > 0)
                    ? descriptionAttribute[0].Description
                    : "No description available";

                Core.DeviceFactory.AddFactoryForType(typeName.ToLower(), description, deviceFactory.FactoryType, deviceFactory.BuildDevice);
            }
        }
    }
}