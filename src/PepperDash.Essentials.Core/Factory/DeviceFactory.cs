

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Crestron.SimplSharp;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Provides functionality for managing and registering device factories, including loading plugin-based factories and
    /// retrieving devices based on their configuration.
    /// </summary>
    /// <remarks>The <see cref="DeviceFactory"/> class is responsible for discovering and registering device factories
    /// from plugins, as well as providing methods to retrieve devices based on their configuration. It maintains a
    /// collection of factory methods that are keyed by device type names, allowing for extensibility through plugins. This
    /// class also handles metadata retrieval and secret management for device configurations.</remarks>
    public class DeviceFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceFactory"/> class and loads all available device factories
        /// from the current assembly.
        /// </summary>
        /// <remarks>This constructor scans the executing assembly for types that implement the <see
        /// cref="IDeviceFactory"/> interface and are not abstract or interfaces. For each valid type, an instance is
        /// created and passed to the  <c>LoadDeviceFactories</c> method for further processing.  If a type cannot be
        /// instantiated, an informational log message is generated, and the process continues with the remaining
        /// types.</remarks>
        public DeviceFactory()
        {
            var programAssemblies = Directory.GetFiles(InitialParametersClass.ProgramDirectory.ToString(), "*.dll");

            // Assemblies known to cause load errors that should be skipped
            var assembliesToSkip = new[] { "CrestronOnvif.dll" };

            foreach (var assembly in programAssemblies)
            {
                if (assembliesToSkip.Any(a => Path.GetFileName(assembly).Equals(a, StringComparison.OrdinalIgnoreCase)))
                {
                    Debug.LogMessage(LogEventLevel.Verbose, "Skipping assembly: {assemblyName}", Path.GetFileName(assembly));
                    continue;
                }

                try
                {
                    Assembly.LoadFrom(assembly);
                }
                catch (Exception e)
                {
                    Debug.LogError("Unable to load assembly: {assemblyName} - {message}", assembly, e.Message);
                }
            }

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Loop through all loaded assemblies that contain at least 1 type that implements IDeviceFactory
            foreach (var assembly in loadedAssemblies)
            {
                Debug.LogDebug("loaded assembly: {assemblyName}", assembly.GetName()?.Name ?? "Unknown");

                PluginLoader.AddLoadedAssembly(assembly.GetName()?.Name ?? "Unknown", assembly);

                var types = assembly.GetTypes().Where(ct => typeof(IDeviceFactory).IsAssignableFrom(ct) && !ct.IsInterface && !ct.IsAbstract);

                if (types == null || !types.Any())
                {
                    Debug.LogDebug("No DeviceFactory types found in assembly: {assemblyName}", assembly.GetName().Name);
                    continue;
                }

                foreach (var type in types)
                {
                    try
                    {
                        var factory = (IDeviceFactory)Activator.CreateInstance(type);
                        LoadDeviceFactories(factory);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Unable to load type: '{message}' DeviceFactory: {type}", e.Message, type.Name);
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
                string description = deviceFactory.FactoryType.GetCustomAttributes(typeof(DescriptionAttribute), true) is DescriptionAttribute[] descriptionAttribute && descriptionAttribute.Length > 0
                    ? descriptionAttribute[0].Description
                    : "No description available";

                AddFactoryForType(typeName.ToLower(), description, deviceFactory.FactoryType, deviceFactory.BuildDevice);
            }
        }

        /// <summary>
        /// A dictionary of factory methods, keyed by config types, added by plugins.
        /// These methods are looked up and called by GetDevice in this class.
        /// </summary>
        private static readonly Dictionary<string, DeviceFactoryWrapper> FactoryMethods =
            new Dictionary<string, DeviceFactoryWrapper>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Registers a factory method for creating instances of a specific type.
        /// </summary>
        /// <remarks>This method associates a type name with a factory method, allowing instances of the type to
        /// be created dynamically. The factory method is stored internally and can be retrieved or invoked as
        /// needed.</remarks>
        /// <param name="typeName">The name of the type for which the factory method is being registered. This value cannot be null or empty.</param>
        /// <param name="method">A delegate that defines the factory method. The delegate takes a <see cref="DeviceConfig"/> parameter and
        /// returns an instance of <see cref="IKeyed"/>.</param>
        public static void AddFactoryForType(string typeName, Func<DeviceConfig, IKeyed> method)
        {
            FactoryMethods.Add(typeName, new DeviceFactoryWrapper() { FactoryMethod = method });
        }

        /// <summary>
        /// Registers a factory method for creating instances of a specific device type.
        /// </summary>
        /// <remarks>If a factory method for the specified <paramref name="typeName"/> already exists, the method
        /// will not overwrite it and will log an informational message instead.</remarks>
        /// <param name="typeName">The unique name of the device type. This serves as the key for identifying the factory method.</param>
        /// <param name="description">A brief description of the device type. This is used for informational purposes.</param>
        /// <param name="Type">The <see cref="Type"/> of the device being registered. This represents the runtime type of the device.</param>
        /// <param name="method">A factory method that takes a <see cref="DeviceConfig"/> as input and returns an instance of <see
        /// cref="IKeyed"/>.</param>
        public static void AddFactoryForType(string typeName, string description, Type Type, Func<DeviceConfig, IKeyed> method)
        {
            if (FactoryMethods.ContainsKey(typeName))
            {
                Debug.LogInformation("Unable to add type: '{typeName}'. Already exists in DeviceFactory", typeName);
                return;
            }

            var wrapper = new DeviceFactoryWrapper() { Type = Type, Description = description, FactoryMethod = method };

            FactoryMethods.Add(typeName, wrapper);
        }

        private static void CheckForSecrets(IEnumerable<JProperty> obj)
        {
            foreach (var prop in obj.Where(prop => prop.Value as JObject != null))
            {
                if (prop.Name.Equals("secret", StringComparison.CurrentCultureIgnoreCase))
                {
                    var secret = GetSecret(prop.Children().First().ToObject<SecretsPropertiesConfig>());

                    prop.Parent.Replace(secret);
                }

                if (!(prop.Value is JObject recurseProp)) continue;

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
            return string.Empty;
        }


        /// <summary>
        /// Creates and returns a device instance based on the provided <see cref="DeviceConfig"/>.
        /// </summary>
        /// <remarks>This method attempts to create a device using the type specified in the <paramref name="dc"/>
        /// parameter. If the type corresponds to a registered factory method, the device is created and returned. If the
        /// type is unrecognized or an exception occurs, the method logs the error and returns <see
        /// langword="null"/>.</remarks>
        /// <param name="dc">The configuration object containing the key, name, type, and properties required to create the device.</param>
        /// <returns>An instance of a device that implements <see cref="IKeyed"/>, or <see langword="null"/> if the device type is
        /// not recognized or an error occurs during creation.</returns>
        public static IKeyed GetDevice(DeviceConfig dc)
        {
            try
            {
                var localDc = new DeviceConfig(dc);

                var key = localDc.Key;
                var name = localDc.Name;
                var type = localDc.Type;
                var properties = localDc.Properties;

                var typeName = localDc.Type.ToLower();

                if (properties is JObject jObject)
                {
                    var jProp = jObject.Properties();

                    CheckForSecrets(jProp);
                }

                if (!FactoryMethods.TryGetValue(typeName, out var wrapper))
                {
                    Debug.LogWarning("Device type '{typeName}' not found in DeviceFactory", typeName);
                    return null;
                }

                Debug.LogInformation("Loading '{type}' from {assemblyName}", typeName, wrapper.Type.Assembly.FullName);

                // Check for types that have been added by plugin dlls.
                return wrapper.FactoryMethod(localDc);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex, "Exception occurred while creating device {key}: {message}", dc.Key, ex.Message);
                Debug.LogDebug(ex, "Exception details: {stackTrace}", ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Displays a list of device factory types that match the specified filter.
        /// </summary>
        /// <remarks>The method outputs the filtered list of device factory types to the console, including their
        /// key, type, and description. If a type is not specified by the plugin, it will be displayed as "Not Specified by
        /// Plugin."</remarks>
        /// <param name="filter">A string used to filter the device factory types by their keys. If the filter is null or empty, all device
        /// factory types are displayed.</param>
        public static void GetDeviceFactoryTypes(string filter)
        {
            var types = !string.IsNullOrEmpty(filter)
                ? FactoryMethods.Where(k => k.Key.Contains(filter)).ToDictionary(k => k.Key, k => k.Value)
                : FactoryMethods;

            CrestronConsole.ConsoleCommandResponse("Device Types:");

            foreach (var type in types.OrderBy(t => t.Key))
            {
                var description = type.Value.Description;
                var Type = "Not Specified by Plugin";

                if (type.Value.Type != null)
                {
                    Type = type.Value.Type.FullName;
                }

                CrestronConsole.ConsoleCommandResponse(
                    "Type: '{0}'\r\n" +
                    "                    Type: '{1}'\r\n" +
                    "                    Description: {2}{3}", type.Key, Type, description, CrestronEnvironment.NewLine);
            }
        }

        /// <summary>
        /// Retrieves a dictionary of device factory wrappers, optionally filtered by a specified string.
        /// </summary>
        /// <param name="filter">A string used to filter the dictionary keys. Only entries with keys containing the specified filter will be
        /// included. If <see langword="null"/> or empty, all entries are returned.</param>
        /// <returns>A dictionary where the keys are strings representing device identifiers and the values are <see
        /// cref="DeviceFactoryWrapper"/> instances. The dictionary may be empty if no entries match the filter.</returns>
        public static Dictionary<string, DeviceFactoryWrapper> GetDeviceFactoryDictionary(string filter)
        {
            return string.IsNullOrEmpty(filter)
                ? FactoryMethods
                : FactoryMethods.Where(k => k.Key.Contains(filter)).ToDictionary(k => k.Key, k => k.Value);
        }
    }
}
