using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.GeneralIO;
using Crestron.SimplSharp.Reflection;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.CrestronIO;
using PepperDash.Essentials.Core.Touchpanels;

namespace PepperDash.Essentials.Core
{
    public class DeviceFactoryWrapper
    {
        public CType CType { get; set; }
        public string Description { get; set; }
        public Func<DeviceConfig, IKeyed> FactoryMethod { get; set; }

        public DeviceFactoryWrapper()
        {
            CType = null;
            Description = "Not Available";
        }
    }

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
                        var factory = (IDeviceFactory)Crestron.SimplSharp.Reflection.Activator.CreateInstance(type);
                        factory.LoadTypeFactories();
                    }
                    catch (Exception e)
                    {
                        Debug.Console(0, Debug.ErrorLogLevel.Error, "Unable to load type: '{1}' DeviceFactory: {0}", e, type.Name);
                    }
                }
            }
        }

		/// <summary>
		/// A dictionary of factory methods, keyed by config types, added by plugins.
		/// These methods are looked up and called by GetDevice in this class.
		/// </summary>
		static Dictionary<string, DeviceFactoryWrapper> FactoryMethods =
            new Dictionary<string, DeviceFactoryWrapper>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Adds a plugin factory method
		/// </summary>
		/// <param name="dc"></param>
		/// <returns></returns>
		public static void AddFactoryForType(string typeName, Func<DeviceConfig, IKeyed> method) 
		{
			Debug.Console(0, Debug.ErrorLogLevel.Notice, "Adding factory method for type '{0}'", typeName);
            DeviceFactory.FactoryMethods.Add(typeName, new DeviceFactoryWrapper() { FactoryMethod = method});
		}

        public static void AddFactoryForType(string typeName, string description, CType cType, Func<DeviceConfig, IKeyed> method)
        {
            Debug.Console(0, Debug.ErrorLogLevel.Notice, "Adding factory method for type '{0}'", typeName);

            if(FactoryMethods.ContainsKey(typeName))
            {
                Debug.Console(0, Debug.ErrorLogLevel.Error, "Unable to add type: '{0}'.  Already exists in DeviceFactory", typeName);
                return;
            }

            var wrapper = new DeviceFactoryWrapper() { CType = cType, Description = description, FactoryMethod = method };
            DeviceFactory.FactoryMethods.Add(typeName, wrapper);
        }

		/// <summary>
		/// The factory method for Core "things". Also iterates the Factory methods that have
		/// been loaded from plugins
		/// </summary>
		/// <param name="dc"></param>
		/// <returns></returns>
        public static IKeyed GetDevice(DeviceConfig dc)
        {
            var key = dc.Key;
            var name = dc.Name;
            var type = dc.Type;
            var properties = dc.Properties;	

            var typeName = dc.Type.ToLower();

            // Check for types that have been added by plugin dlls. 
            if (FactoryMethods.ContainsKey(typeName))
            {
                Debug.Console(0, Debug.ErrorLogLevel.Notice, "Loading '{0}' from plugin", dc.Type);
                return FactoryMethods[typeName].FactoryMethod(dc);
            }

            return null;
        }

        /// <summary>
        /// Prints the type names and associated metadata from the FactoryMethods collection.
        /// </summary>
        /// <param name="command"></param>
        public static void GetDeviceFactoryTypes(string filter)
        {
            Dictionary<string, DeviceFactoryWrapper> types = new Dictionary<string, DeviceFactoryWrapper>();

            if (!string.IsNullOrEmpty(filter))
            {
                types = FactoryMethods.Where(k => k.Key.Contains(filter)).ToDictionary(k => k.Key, k => k.Value);
            }
            else
            {
                types = FactoryMethods;
            }

            Debug.Console(0, "Device Types:");

            foreach (var type in types.OrderBy(t => t.Key))
            {
                var description = type.Value.Description;
                var cType = "Not Specified by Plugin";

                if(type.Value.CType != null)
                {
                    cType = type.Value.CType.FullName;
                }

                Debug.Console(0, 
                    @"Type: '{0}' 
                    CType: '{1}' 
                    Description: {2}", type.Key, cType, description);
            }
        }
    }
}