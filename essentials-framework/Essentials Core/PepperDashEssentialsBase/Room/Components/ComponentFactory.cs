using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;

using PepperDash.Core;


using PepperDash.Essentials.Interfaces.Components;

namespace PepperDash.Essentials.Core.Room.Components
{
    public class ComponentFactoryWrapper
    {
        public CType CType { get; set; }
        public string Description { get; set; }
        public Func<RoomComponentConfig, IKeyed> FactoryMethod { get; set; }

        public ComponentFactoryWrapper()
        {
            CType = null;
            Description = "Not Available";
        }
    }

    /// <summary>
    /// Defines a class that is capable of loading component types
    /// </summary>
    public interface IComponentFactory : IDeviceFactory
    {

    }

    public static class ComponentFactory
    {
        static ComponentFactory()
        {
            var assy = Assembly.GetExecutingAssembly();
            PluginLoader.SetEssentialsAssembly(assy.GetName().Name, assy);

            var types = assy.GetTypes().Where(ct => typeof(IComponentFactory).IsAssignableFrom(ct) && !ct.IsInterface && !ct.IsAbstract);

            if (types != null)
            {
                foreach (var type in types)
                {
                    try
                    {
                        var factory = (IComponentFactory)Crestron.SimplSharp.Reflection.Activator.CreateInstance(type);
                        factory.LoadTypeFactories();
                    }
                    catch (Exception e)
                    {
                        Debug.Console(0, Debug.ErrorLogLevel.Error, "Unable to load type: '{1}' ComponentFactory: {0}", e, type.Name);
                    }
                }
            }
        }

        /// <summary>
        /// A dictionary of factory methods, keyed by config types, added by plugins.
        /// These methods are looked up and called by GetDevice in this class.
        /// </summary>
        static Dictionary<string, ComponentFactoryWrapper> FactoryMethods =
            new Dictionary<string, ComponentFactoryWrapper>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Adds a plugin factory method
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static void AddFactoryForType(string typeName, Func<RoomComponentConfig, IKeyed> method)
        {
            Debug.Console(0, Debug.ErrorLogLevel.Notice, "Adding factory method for type '{0}'", typeName);
            ComponentFactory.FactoryMethods.Add(typeName, new ComponentFactoryWrapper() { FactoryMethod = method });
        }

        public static void AddFactoryForType(string typeName, string description, CType cType, Func<RoomComponentConfig, IKeyed> method)
        {
            Debug.Console(0, Debug.ErrorLogLevel.Notice, "Adding factory method for type '{0}'", typeName);

            if (FactoryMethods.ContainsKey(typeName))
            {
                Debug.Console(0, Debug.ErrorLogLevel.Error, "Unable to add type: '{0}'.  Already exists in ComponentFactory", typeName);
                return;
            }

            var wrapper = new ComponentFactoryWrapper() { CType = cType, Description = description, FactoryMethod = method };
            ComponentFactory.FactoryMethods.Add(typeName, wrapper);
        }

        /// <summary>
        /// The factory method for Core components. Also iterates the Factory methods that have
        /// been loaded from plugins
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static IKeyed GetComponent(RoomComponentConfig dc)
        {
            var key = dc.Key;
            var name = dc.Name;
            var type = dc.Type;
            var properties = dc.Properties;

            var typeName = dc.Type.ToLower();

            // Check for types that have been added by plugin dlls. 
            if (FactoryMethods.ContainsKey(typeName))
            {
                Debug.Console(0, Debug.ErrorLogLevel.Notice, "Loading '{0}' from Essentials Core", dc.Type);
                return FactoryMethods[typeName].FactoryMethod(dc);
            }

            return null;
        }

        /// <summary>
        /// Prints the type names and associated metadata from the FactoryMethods collection.
        /// </summary>
        /// <param name="command"></param>
        public static void GetComponentFactoryTypes(string filter)
        {
            Dictionary<string, ComponentFactoryWrapper> types = new Dictionary<string, ComponentFactoryWrapper>();

            if (!string.IsNullOrEmpty(filter))
            {
                types = FactoryMethods.Where(k => k.Key.Contains(filter)).ToDictionary(k => k.Key, k => k.Value);
            }
            else
            {
                types = FactoryMethods;
            }

            Debug.Console(0, "Component Types:");

            foreach (var type in types.OrderBy(t => t.Key))
            {
                var description = type.Value.Description;
                var cType = "Not Specified by Plugin";

                if (type.Value.CType != null)
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


    /// <summary>
    /// Devices the basic needs for a Device Factory
    /// </summary>
    public abstract class EssentialsComponentFactory<T> : IComponentFactory where T : IComponent
    {
        #region IComponentFactory Members

        /// <summary>
        /// A list of strings that can be used in the type property of a DeviceConfig object to build an instance of this device
        /// </summary>
        public List<string> TypeNames { get; protected set; }

        /// <summary>
        /// Loads an item to the ComponentFactory.FactoryMethods dictionary for each entry in the TypeNames list
        /// </summary>
        public void LoadTypeFactories()
        {
            foreach (var typeName in TypeNames)
            {
                Debug.Console(2, "Getting Description Attribute from class: '{0}'", typeof(T).FullName);
                var descriptionAttribute = typeof(T).GetCustomAttributes(typeof(DescriptionAttribute), true) as DescriptionAttribute[];
                string description = descriptionAttribute[0].Description;
                var snippetAttribute = typeof(T).GetCustomAttributes(typeof(ConfigSnippetAttribute), true) as ConfigSnippetAttribute[];
                ComponentFactory.AddFactoryForType(typeName.ToLower(), description, typeof(T), BuildComponent);
            }
        }

        /// <summary>
        /// The method that will build the device
        /// </summary>
        /// <param name="dc">The device config</param>
        /// <returns>An instance of the device</returns>
        public abstract IComponent BuildComponent(RoomComponentConfig dc);

        #endregion
    }
}

