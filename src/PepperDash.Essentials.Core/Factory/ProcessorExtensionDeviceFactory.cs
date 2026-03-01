
using System.Reflection;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PepperDash.Essentials.Core
{

    /// <summary>
    /// Represents a ProcessorExtensionDeviceFactory
    /// </summary>
    public class ProcessorExtensionDeviceFactory
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessorExtensionDeviceFactory() {
            var assy = Assembly.GetExecutingAssembly();
            PluginLoader.SetEssentialsAssembly(assy.GetName().Name, assy);

            var extensions = assy.GetTypes().Where(ct => typeof(IProcessorExtensionDeviceFactory)
                .IsAssignableFrom(ct) && !ct.IsInterface && !ct.IsAbstract);

            if (extensions != null )
            {
                foreach ( var extension in extensions )
                {
                    try
                    {
                        var factory = (IProcessorExtensionDeviceFactory)Activator.CreateInstance(extension);
                        factory.LoadFactories();
                    }
                    catch( Exception e )
                    {
                        Debug.LogMessage(LogEventLevel.Information, "Unable to load extension device: '{1}' ProcessorExtensionDeviceFactory: {0}", e, extension.Name);
                    }
                }
            }
        }

        /// <summary>
        /// A dictionary of factory methods, keyed by config types, added by plugins.
        /// These methods are looked up and called by GetDevice in this class.
        /// </summary>
        static Dictionary<string, DeviceFactoryWrapper> ProcessorExtensionFactoryMethods =
            new Dictionary<string, DeviceFactoryWrapper>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// Adds a plugin factory method
        /// </summary>
        /// <param name="extensionName">name fo extension to add</param>
        /// <param name="method">method to add</param>
        /// <returns></returns>
        public static void AddFactoryForType(string extensionName, Func<DeviceConfig, IKeyed> method)
        {
            //Debug.LogMessage(LogEventLevel.Debug, "Adding factory method for type '{0}'", typeName);
            ProcessorExtensionDeviceFactory.ProcessorExtensionFactoryMethods.Add(extensionName, new DeviceFactoryWrapper() { FactoryMethod = method });
        }

        /// <summary>
        /// Adds a plugin factory method with type and description
        /// </summary>
        /// <param name="extensionName">name of extension to add</param>
        /// <param name="description">description of extension to add</param>
        /// <param name="Type">type of extension to add</param>
        /// <param name="method">method to add</param>
        public static void AddFactoryForType(string extensionName, string description, Type Type, Func<DeviceConfig, IKeyed> method)
        {
            //Debug.LogMessage(LogEventLevel.Debug, "Adding factory method for type '{0}'", typeName);

            if (ProcessorExtensionFactoryMethods.ContainsKey(extensionName))
            {
                Debug.LogMessage(LogEventLevel.Information, "Unable to add extension device: '{0}'.  Already exists in ProcessorExtensionDeviceFactory", extensionName);
                return;
            }

            var wrapper = new DeviceFactoryWrapper() { Type = Type, Description = description, FactoryMethod = method };
            ProcessorExtensionDeviceFactory.ProcessorExtensionFactoryMethods.Add(extensionName, wrapper);
        }

        private static void CheckForSecrets(IEnumerable<Newtonsoft.Json.Linq.JProperty> obj)
        {
            foreach (var prop in obj.Where(prop => prop.Value as Newtonsoft.Json.Linq.JObject != null))
            {
                if (prop.Name.ToLower() == "secret")
                {
                    var secret = GetSecret(prop.Children().First().ToObject<SecretsPropertiesConfig>());
                    //var secret = GetSecret(JsonConvert.DeserializeObject<SecretsPropertiesConfig>(prop.Children().First().ToString()));
                    prop.Parent.Replace(secret);
                }
                var recurseProp = prop.Value as Newtonsoft.Json.Linq.JObject;
                if (recurseProp == null) return;
                CheckForSecrets(recurseProp.Properties());
            }
        }

        private static string GetSecret(SecretsPropertiesConfig data)
        {
            var secretProvider = SecretsManager.GetSecretProviderByKey(data.Provider);
            if (secretProvider == null) return null;
            var secret = secretProvider.GetSecret(data.Key);
            if (secret != null) return (string)secret.Value;
            Debug.LogMessage(LogEventLevel.Debug,
                "Unable to retrieve secret {0}{1} - Make sure you've added it to the secrets provider",
                data.Provider, data.Key);
            return String.Empty;
        }

        /// <summary>
        /// The factory method for processor extension devices. Also iterates the Factory methods that have
        /// been loaded from plugins
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        /// <summary>
        /// GetExtensionDevice method
        /// </summary>
        public static IKeyed GetExtensionDevice(DeviceConfig dc)
        {
            try
            {
                Debug.LogMessage(LogEventLevel.Information, "Loading '{0}' from Essentials Core", dc.Type);

                var localDc = new DeviceConfig(dc);

                var key = localDc.Key;
                var name = localDc.Name;
                var type = localDc.Type;
                var properties = localDc.Properties;
                //var propRecurse = properties;

                var typeName = localDc.Type.ToLower();

                var jObject = properties as Newtonsoft.Json.Linq.JObject;
                if (jObject != null)
                {
                    var jProp = jObject.Properties();

                    CheckForSecrets(jProp);
                }

                Debug.LogMessage(LogEventLevel.Verbose, "typeName = {0}", typeName);
                // Check for types that have been added by plugin dlls. 
                return !ProcessorExtensionFactoryMethods.ContainsKey(typeName) ? null : ProcessorExtensionFactoryMethods[typeName].FactoryMethod(localDc);
            }
            catch (Exception ex)
            {
                Debug.LogMessage(LogEventLevel.Information, "Exception occurred while creating device {0}: {1}", dc.Key, ex.Message);

                Debug.LogMessage(LogEventLevel.Verbose, "{0}", ex.StackTrace);

                if (ex.InnerException == null)
                {
                    return null;
                }

                Debug.LogMessage(LogEventLevel.Information, "Inner exception while creating device {0}: {1}", dc.Key,
                    ex.InnerException.Message);
                Debug.LogMessage(LogEventLevel.Verbose, "{0}", ex.InnerException.StackTrace);
                return null;
            }
        }

    }

}
