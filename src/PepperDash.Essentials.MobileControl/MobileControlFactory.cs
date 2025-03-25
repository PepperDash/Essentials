using PepperDash.Core;
using PepperDash.Essentials.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.MobileControl
{
    public class MobileControlFactory
    {
        public MobileControlFactory() {
            var assembly = Assembly.GetExecutingAssembly();

            PluginLoader.SetEssentialsAssembly(assembly.GetName().Name, assembly);

            var types = assembly.GetTypes().Where(t => typeof(IDeviceFactory).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            if(types == null)
            {
                return;
            }

            foreach (var type in types)
            {
                try
                {
                    var factory = (IDeviceFactory)Activator.CreateInstance(type);

                    factory.LoadTypeFactories();
                }
                catch (Exception ex)
                {
                    Debug.LogMessage(ex, "Unable to load type '{type}' DeviceFactory: {factory}", null, type.Name);
                }
            }
        }
    }
}
