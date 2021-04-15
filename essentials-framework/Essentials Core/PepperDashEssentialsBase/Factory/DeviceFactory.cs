using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.GeneralIO;
using Crestron.SimplSharp.Reflection;
using PepperDash.Core;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
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
			Debug.Console(1, Debug.ErrorLogLevel.Notice, "Adding factory method for type '{0}'", typeName);
            DeviceFactory.FactoryMethods.Add(typeName, new DeviceFactoryWrapper() { FactoryMethod = method});
		}

        public static void AddFactoryForType(string typeName, string description, CType cType, Func<DeviceConfig, IKeyed> method)
        {
            Debug.Console(1, Debug.ErrorLogLevel.Notice, "Adding factory method for type '{0}'", typeName);

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
		    try
		    {
                Debug.Console(0, Debug.ErrorLogLevel.Notice, "Loading '{0}' from Essentials Core", dc.Type);

		        var localDc = new DeviceConfig(dc);

		        var key = localDc.Key;
		        var name = localDc.Name;
		        var type = localDc.Type;
                var properties = localDc.Properties;
		        //var propRecurse = properties;

		        var typeName = localDc.Type.ToLower();

                Debug.Console(2, "typeName = {0}", typeName);
		        // Check for types that have been added by plugin dlls. 
		        if (!FactoryMethods.ContainsKey(typeName)) return null;

                /*foreach (var obj in (propRecurse as JObject).FindTokens("secret").OfType<JObject>())
                {
                    Debug.Console(2, obj.ToString());
                }*/


                //look for secret in username
		        var userSecretToken = properties["control"]["tcpSshProperties"]["username"]["secret"];

		        if (userSecretToken != null)
		        {
		            Debug.Console(2, "Found a secret for {0} - attempting to retrieve it!", name);
		            var userSecretResult =
		                JsonConvert.DeserializeObject<SecretsPropertiesConfig>(userSecretToken.ToString());
		            var userProvider = SecretsManager.GetSecretProviderByKey(userSecretResult.Provider);
		            if (userProvider != null)
		            {
		                var user = userProvider.GetSecret(userSecretResult.Key);
		                if (user == null)
		                {
		                    Debug.Console(1,
		                        "Unable to retrieve secret for {0} - Make sure you've added it to the secrets provider");
		                    return null;
		                }
		                properties["control"]["tcpSshProperties"]["username"] = (string) user.Value;
		            }
		        }

                //look for secret in password
		        var passwordSecretToken = properties["control"]["tcpSshProperties"]["password"]["secret"];

		        if (passwordSecretToken != null)
		        {
		            Debug.Console(2, "Found a secret for {0} - attempting to retrieve it!", name);

		            var passwordSecretResult =
		                JsonConvert.DeserializeObject<SecretsPropertiesConfig>(passwordSecretToken.ToString());
		            var passwordProvider = SecretsManager.GetSecretProviderByKey(passwordSecretResult.Provider);
		            if (passwordProvider != null)
		            {
		                var password = passwordProvider.GetSecret(passwordSecretResult.Key);
                        if (password == null)
		                {
		                    Debug.Console(1,
		                        "Unable to retrieve secret for {0} - Make sure you've added it to the secrets provider");
		                    return null;
		                }
                        properties["control"]["tcpSshProperties"]["password"] = (string)password.Value;
		            }
		        }

                Debug.Console(0, "{0}", localDc.Properties.ToString());

		        return FactoryMethods[typeName].FactoryMethod(localDc);
		    }
		    catch (Exception ex)
		    {
		        Debug.Console(2, "Issue with getting device - {0}", ex.Message);
		        return null;
		    }
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