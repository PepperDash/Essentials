using System;
using System.Linq;
using System.Reflection;
using PepperDash.Core;
using PepperDash.Essentials.Core.Factory;
using PepperDash.Essentials.Core.Plugins;

namespace PepperDash.Essentials.Factory
{
    /*
    /// <summary>
    /// Responsible for loading all of the device types for this library
    /// </summary>
    public class DeviceFactory
    {

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
                        factory.LoadTypeFactories();
                    }
                    catch (Exception e)
                    {
                        Debug.LogMessage(Serilog.Events.LogEventLevel.Error, "Unable to load type: '{exception}' DeviceFactory: {factoryName}", e, type.Name);                        
                    }
                }
            }
        }
    }*/
}
