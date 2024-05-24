

using Crestron.SimplSharp;
using System.Reflection;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PepperDash.Essentials.Core
{
    public class DeviceFactoryWrapper
    {
        public Type CType { get; set; }
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
                        Debug.LogMessage(LogEventLevel.Information, "Unable to load type: '{1}' DeviceFactory: {0}", e, type.Name);
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
            //Debug.LogMessage(LogEventLevel.Debug, "Adding factory method for type '{0}'", typeName);
            DeviceFactory.FactoryMethods.Add(typeName, new DeviceFactoryWrapper() { FactoryMethod = method});
		}

        public static void AddFactoryForType(string typeName, string description, Type cType, Func<DeviceConfig, IKeyed> method)
        {
            //Debug.LogMessage(LogEventLevel.Debug, "Adding factory method for type '{0}'", typeName);

            if(FactoryMethods.ContainsKey(typeName))
            {
                Debug.LogMessage(LogEventLevel.Information, "Unable to add type: '{0}'.  Already exists in DeviceFactory", typeName);
                return;
            }

            var wrapper = new DeviceFactoryWrapper() { CType = cType, Description = description, FactoryMethod = method };
            DeviceFactory.FactoryMethods.Add(typeName, wrapper);
        }

        private static void CheckForSecrets(IEnumerable<JProperty> obj)
        {
            foreach (var prop in obj.Where(prop => prop.Value as JObject != null))
            {
                if (prop.Name.ToLower() == "secret")
                {
                    var secret = GetSecret(prop.Children().First().ToObject<SecretsPropertiesConfig>());
                    //var secret = GetSecret(JsonConvert.DeserializeObject<SecretsPropertiesConfig>(prop.Children().First().ToString()));
                    prop.Parent.Replace(secret);
                }
                var recurseProp = prop.Value as JObject;
                if (recurseProp == null) return;
                CheckForSecrets(recurseProp.Properties());
            }
        }

        private static string GetSecret(SecretsPropertiesConfig data)
        {
            var secretProvider = SecretsManager.GetSecretProviderByKey(data.Provider);
            if (secretProvider == null) return null;
            var secret = secretProvider.GetSecret(data.Key);
            if (secret != null) return (string) secret.Value;
            Debug.LogMessage(LogEventLevel.Debug,
                "Unable to retrieve secret {0}{1} - Make sure you've added it to the secrets provider",
                data.Provider, data.Key);
            return String.Empty;
        }


        /// <summary>
        /// The factory method for Core "things". Also iterates the Factory methods that have
        /// been loaded from plugins
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static IKeyed GetDevice(DeviceConfig dc)
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


                var jObject = properties as JObject;
                if (jObject != null)
                {
                    var jProp = jObject.Properties();

                    CheckForSecrets(jProp);
                }

                Debug.LogMessage(LogEventLevel.Verbose, "typeName = {0}", typeName);
                // Check for types that have been added by plugin dlls. 
                return !FactoryMethods.ContainsKey(typeName) ? null : FactoryMethods[typeName].FactoryMethod(localDc);
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Exception occurred while creating device {0}: {1}", null, dc.Key, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Prints the type names and associated metadata from the FactoryMethods collection.
        /// </summary>
        /// <param name="filter"></param>
        public static void GetDeviceFactoryTypes(string filter)
        {
            var types = !string.IsNullOrEmpty(filter)
                ? FactoryMethods.Where(k => k.Key.Contains(filter)).ToDictionary(k => k.Key, k => k.Value)
                : FactoryMethods;

            CrestronConsole.ConsoleCommandResponse("Device Types:");

            foreach (var type in types.OrderBy(t => t.Key))
            {
                var description = type.Value.Description;
                var cType = "Not Specified by Plugin";

                if (type.Value.CType != null)
                {
                    cType = type.Value.CType.FullName;
                }

                CrestronConsole.ConsoleCommandResponse(
                    @"Type: '{0}' 
                    CType: '{1}' 
                    Description: {2}{3}", type.Key, cType, description, CrestronEnvironment.NewLine);
            }
        }

		/// <summary>
		/// Returns the device factory dictionary
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
	    public static Dictionary<string, DeviceFactoryWrapper> GetDeviceFactoryDictionary(string filter)
		{
			return string.IsNullOrEmpty(filter) 
				? FactoryMethods
				: FactoryMethods.Where(k => k.Key.Contains(filter)).ToDictionary(k => k.Key, k => k.Value);
		}
    }
}