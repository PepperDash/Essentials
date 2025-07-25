using System;
using System.Linq;
using System.Reflection;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Represents a MobileControlFactory
    /// </summary>
    public class MobileControlFactory
    {
        public MobileControlFactory()
        {
            var assembly = Assembly.GetExecutingAssembly();

            PluginLoader.SetEssentialsAssembly(assembly.GetName().Name, assembly);

            var types = assembly.GetTypes().Where(t => typeof(IDeviceFactory).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            if (types == null)
            {
                return;
            }

            foreach (var type in types)
            {
                try
                {
                    var factory = (IDeviceFactory)Activator.CreateInstance(type);

                    LoadDeviceFactories(factory);
                }
                catch (Exception ex)
                {
                    Debug.LogMessage(ex, "Unable to load type '{type}' DeviceFactory: {factory}", null, type.Name);
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
                var descriptionAttribute = deviceFactory.FactoryType.GetCustomAttributes(typeof(DescriptionAttribute), true) as DescriptionAttribute[];
                string description = (descriptionAttribute != null && descriptionAttribute.Length > 0) 
                    ? descriptionAttribute[0].Description 
                    : string.Empty; // Default value if no DescriptionAttribute is found
                var snippetAttribute = deviceFactory.FactoryType.GetCustomAttributes(typeof(ConfigSnippetAttribute), true) as ConfigSnippetAttribute[];
                Core.DeviceFactory.AddFactoryForType(typeName.ToLower(), description, deviceFactory.FactoryType, deviceFactory.BuildDevice);
            }
        }
    }
}
