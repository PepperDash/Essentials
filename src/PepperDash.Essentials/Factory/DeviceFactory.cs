

using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials;

/// <summary>
/// Responsible for loading all of the device types for this library
/// </summary>
public class DeviceFactory
{

    public DeviceFactory()
    {
        var assy = Assembly.GetExecutingAssembly();
        PluginLoader.AddLoadedAssembly(assy.GetName().Name, assy);

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
                    Debug.LogMessage(Serilog.Events.LogEventLevel.Error, "Unable to load type: '{exception}' DeviceFactory: {factoryName}", e, type.Name);                        
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
            var descriptionAttribute = deviceFactory.FactoryType.GetCustomAttributes(typeof(DescriptionAttribute), true) as DescriptionAttribute[];
            string description = descriptionAttribute[0].Description;
            var snippetAttribute = deviceFactory.FactoryType.GetCustomAttributes(typeof(ConfigSnippetAttribute), true) as ConfigSnippetAttribute[];
            Core.DeviceFactory.AddFactoryForType(typeName.ToLower(), description, deviceFactory.FactoryType, deviceFactory.BuildDevice);
        }
    }
}
