using Crestron.SimplSharp;
using Newtonsoft.Json;
using PepperDash.Core;
using Serilog.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Represents a DeviceJsonApi
    /// </summary>
    public class DeviceJsonApi
    {
        /// <summary>
        /// DoDeviceActionWithJson method
        /// </summary>
        /// <param name="json">json method</param>
        public static void DoDeviceActionWithJson(string json)
        {
            if (String.IsNullOrEmpty(json))
            {
                CrestronConsole.ConsoleCommandResponse(
                    "Please provide a JSON object matching the format {\"deviceKey\":\"myDevice\", \"methodName\":\"someMethod\", \"params\": [\"param1\", true]}.\r\nIf the method has no parameters, the \"params\" object may be omitted.");
                return;
            }
            try
            {
                var action = JsonConvert.DeserializeObject<DeviceActionWrapper>(json);

                DoDeviceAction(action);
            }
            catch (Exception)
            {
                CrestronConsole.ConsoleCommandResponse("Incorrect format for JSON. Please check that the format matches {\"deviceKey\":\"myDevice\", \"methodName\":\"someMethod\", \"params\": [\"param1\", true]}");
            }

        }


        /// <summary>
        /// DoDeviceAction method
        /// </summary>
        /// <param name="action">action method</param>
        public static void DoDeviceAction(DeviceActionWrapper action)
        {
            var key = action.DeviceKey;
            var obj = FindObjectOnPath(key);
            if (obj == null)
            {
                CrestronConsole.ConsoleCommandResponse("Unable to find object at path {0}", key);
                return;
            }

            if (action.Params == null)
            {
                //no params, so setting action.Params to empty array
                action.Params = new object[0];
            }

            Type t = obj.GetType();
            try
            {
                var methods = t.GetMethods().Where(m => m.Name == action.MethodName).ToList();

                var method = methods.Count == 1 ? methods[0] : methods.FirstOrDefault(m => m.GetParameters().Length == action.Params.Length);

                if (method == null)
                {
                    CrestronConsole.ConsoleCommandResponse(
                        "Unable to find method with name {0} and that matches parameters {1}", action.MethodName,
                        action.Params);
                    return;
                }
                var mParams = method.GetParameters();

                var convertedParams = mParams
                                    .Select((p, i) => ConvertType(action.Params[i], p.ParameterType))
                                    .ToArray();

                Task.Run(() =>
                    {
                        try
                        {
                            Debug.LogMessage(LogEventLevel.Verbose, "Calling method {methodName} on device {deviceKey}", null, method.Name, action.DeviceKey);
                            method.Invoke(obj, convertedParams);
                        }
                        catch (Exception e)
                        {
                            Debug.LogMessage(e, "Error invoking method {methodName} on device {deviceKey}", null, method.Name, action.DeviceKey);
                        }
                    });

                CrestronConsole.ConsoleCommandResponse("Method {0} successfully called on device {1}", method.Name,
                    action.DeviceKey);
            }
            catch (Exception ex)
            {
                CrestronConsole.ConsoleCommandResponse("Unable to call method with name {0}. {1}", action.MethodName,
                    ex.Message);
            }
        }

        /// <summary>
        /// DoDeviceActionAsync method
        /// </summary>
        /// <param name="action">action method</param>
        public static async Task DoDeviceActionAsync(DeviceActionWrapper action)
        {
            var key = action.DeviceKey;
            var obj = FindObjectOnPath(key);
            if (obj == null)
            {
                Debug.LogMessage(LogEventLevel.Warning, "Unable to find object at path {deviceKey}", null, key);
                return;
            }

            if (action.Params == null)
            {
                //no params, so setting action.Params to empty array
                action.Params = new object[0];
            }

            Type t = obj.GetType();
            try
            {
                var methods = t.GetMethods().Where(m => m.Name == action.MethodName).ToList();

                var method = methods.Count == 1 ? methods[0] : methods.FirstOrDefault(m => m.GetParameters().Length == action.Params.Length);

                if (method == null)
                {
                    Debug.LogMessage(LogEventLevel.Warning,
                        "Unable to find method with name {methodName} and that matches parameters {@parameters}", null, action.MethodName,
                        action.Params);
                    return;
                }
                var mParams = method.GetParameters();

                var convertedParams = mParams
                                    .Select((p, i) => ConvertType(action.Params[i], p.ParameterType))
                                    .ToArray();

                try
                {
                    Debug.LogMessage(LogEventLevel.Verbose, "Calling method {methodName} on device {deviceKey} with {@params}", null, method.Name, action.DeviceKey, action.Params);
                    var result = method.Invoke(obj, convertedParams);
                    
                    // If the method returns a Task, await it
                    if (result is Task task)
                    {
                        await task;
                    }
                    // If the method returns a Task<T>, await it
                    else if (result != null && result.GetType().IsGenericType && result.GetType().GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        await (Task)result;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogMessage(e, "Error invoking method {methodName} on device {deviceKey}", null, method.Name, action.DeviceKey);
                }
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Unable to call method with name {methodName} with {@parameters}", null, action.MethodName, action.Params);
            }
        }

        private static object ConvertType(object value, Type conversionType)
        {
            if (!conversionType.IsEnum)
            {
                return Convert.ChangeType(value, conversionType, System.Globalization.CultureInfo.InvariantCulture);
            }

            var stringValue = Convert.ToString(value);

            if (String.IsNullOrEmpty(stringValue))
            {
                throw new InvalidCastException(
                    String.Format("{0} cannot be converted to a string prior to conversion to enum"));
            }
            return Enum.Parse(conversionType, stringValue, true);
        }

        /// <summary>
        /// Gets the properties on a device
        /// </summary>
        /// <param name="deviceObjectPath">The path to the device object</param>
        /// <returns>A JSON string representing the properties of the device</returns>
        public static string GetProperties(string deviceObjectPath)
        {
            var obj = FindObjectOnPath(deviceObjectPath);
            if (obj == null)
                return "{ \"error\":\"No Device\"}";

            Type t = obj.GetType();
            // get the properties and set them into a new collection of NameType wrappers
            var props = t.GetProperties().Select(p => new PropertyNameType(p, obj));
            return JsonConvert.SerializeObject(props, Formatting.Indented);
        }

        /// <summary>
        /// Gets a property from a device path by name
        /// </summary>
        /// <param name="deviceObjectPath">The path to the device object</param>
        /// <param name="propertyName">The name of the property to get</param>
        /// <returns>The value of the property</returns>
        public static object GetPropertyByName(string deviceObjectPath, string propertyName)
        {
            var dev = FindObjectOnPath(deviceObjectPath);
            if (dev == null)
                return "{ \"error\":\"No Device\"}";

            object prop = dev.GetType().GetProperty(propertyName).GetValue(dev, null);

            // var prop = t.GetProperty(propertyName);
            if (prop != null)
            {
                return prop;
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Debug, "Unable to find Property: {0} on Device with path: {1}", propertyName, deviceObjectPath);
                return null;
            }
        }

        /// <summary>
        /// Gets the methods on a device
        /// </summary>
        /// <param name="deviceObjectPath">The path to the device object</param>
        /// <returns>A JSON string representing the methods of the device</returns>
        public static string GetMethods(string deviceObjectPath)
        {
            var obj = FindObjectOnPath(deviceObjectPath);
            if (obj == null)
                return "{ \"error\":\"No Device\"}";

            // Package up method names using helper objects
            Type t = obj.GetType();
            var methods = t.GetMethods()
                .Where(m => !m.IsSpecialName)
                .Select(p => new MethodNameParams(p));
            return JsonConvert.SerializeObject(methods, Formatting.Indented);
        }

        /// <summary>
        /// Gets the API methods on a device
        /// </summary>
        /// <param name="deviceObjectPath">The path to the device object</param>
        /// <returns>A JSON string representing the API methods of the device</returns>
        public static string GetApiMethods(string deviceObjectPath)
        {
            var obj = FindObjectOnPath(deviceObjectPath);
            if (obj == null)
                return "{ \"error\":\"No Device\"}";

            // Package up method names using helper objects
            Type t = obj.GetType();
            var methods = t.GetMethods()
                .Where(m => !m.IsSpecialName)
                .Where(m => m.GetCustomAttributes(typeof(ApiAttribute), true).Any())
                .Select(p => new MethodNameParams(p));
            return JsonConvert.SerializeObject(methods, Formatting.Indented);
        }


        /// <summary>
        /// FindObjectOnPath method
        /// </summary>
        /// <param name="deviceObjectPath">The path to the device object</param>
        /// <returns>The object found at the specified path</returns>
        public static object FindObjectOnPath(string deviceObjectPath)
        {
            var path = deviceObjectPath.Split('.');

            var dev = DeviceManager.GetDeviceForKey(path[0]);
            if (dev == null)
            {
                Debug.LogMessage(LogEventLevel.Information, "Device {0} not found", path[0]);
                return null;
            }

            // loop through any dotted properties
            object obj = dev;
            if (path.Length > 1)
            {
                for (int i = 1; i < path.Length; i++)
                {
                    var objName = path[i];
                    string indexStr = null;
                    var indexOpen = objName.IndexOf('[');
                    if (indexOpen != -1)
                    {
                        var indexClose = objName.IndexOf(']');
                        if (indexClose == -1)
                        {
                            Debug.LogMessage(LogEventLevel.Information, dev, "ERROR Unmatched index brackets");
                            return null;
                        }
                        // Get the index and strip quotes if any
                        indexStr = objName.Substring(indexOpen + 1, indexClose - indexOpen - 1).Replace("\"", "");
                        objName = objName.Substring(0, indexOpen);
                        Debug.LogMessage(LogEventLevel.Information, dev, "  Checking for collection '{0}', index '{1}'", objName, indexStr);
                    }

                    Type oType = obj.GetType();
                    var prop = oType.GetProperty(objName);
                    if (prop == null)
                    {
                        Debug.LogMessage(LogEventLevel.Information, dev, "Property {0} not found on {1}", objName, path[i - 1]);
                        return null;
                    }
                    // if there's an index, try to get the property
                    if (indexStr != null)
                    {
                        if (!typeof(ICollection).IsAssignableFrom(prop.PropertyType))
                        {
                            Debug.LogMessage(LogEventLevel.Information, dev, "Property {0} is not collection", objName);
                            return null;
                        }
                        var collection = prop.GetValue(obj, null) as ICollection;
                        // Get the indexed items "property"
                        var indexedPropInfo = prop.PropertyType.GetProperty("Item");
                        // These are the parameters for the indexing. Only care about one
                        var indexParams = indexedPropInfo.GetIndexParameters();
                        if (indexParams.Length > 0)
                        {
                            Debug.LogMessage(LogEventLevel.Information, "  Indexed, param type: {0}", indexParams[0].ParameterType.Name);
                            var properParam = Convert.ChangeType(indexStr, indexParams[0].ParameterType,
                                System.Globalization.CultureInfo.InvariantCulture);
                            try
                            {
                                obj = indexedPropInfo.GetValue(collection, new object[] { properParam });
                            }
                            // if the index is bad, catch it here.
                            catch (TargetInvocationException e)
                            {
                                if (e.InnerException is ArgumentOutOfRangeException)
                                    Debug.LogMessage(LogEventLevel.Information, "  Index Out of range");
                                else if (e.InnerException is KeyNotFoundException)
                                    Debug.LogMessage(LogEventLevel.Information, "  Key not found");
                                return null;
                            }
                        }

                    }
                    else
                        obj = prop.GetValue(obj, null);
                }
            }
            return obj;
        }

        /// <summary>
        /// Sets a property on an object.
        /// </summary>
        /// <param name="deviceObjectPath">The path to the device object</param>
        /// <returns>A JSON string representing the result of setting the property</returns>
        public static string SetProperty(string deviceObjectPath)
        {
            throw new NotImplementedException("This could be really useful. Finish it please");

            //var obj = FindObjectOnPath(deviceObjectPath);
            //if (obj == null)
            //    return "{\"error\":\"No object found\"}";

            //Type t = obj.GetType();


            //// get the properties and set them into a new collection of NameType wrappers
            //var props = t.GetProperties().Select(p => new PropertyNameType(p, obj));
            //return JsonConvert.SerializeObject(props, Formatting.Indented);
        }


    }

    /// <summary>
    /// Represents a DeviceActionWrapper
    /// </summary>
    public class DeviceActionWrapper
    {
        /// <summary>
        /// Gets or sets the DeviceKey
        /// </summary>
        public string DeviceKey { get; set; }

        /// <summary>
        /// Gets or sets the MethodName
        /// </summary>
        public string MethodName { get; set; }
        
        /// <summary>
        /// Gets or sets the Params
        /// </summary>
        public object[] Params { get; set; }
    }

    /// <summary>
    /// Represents a PropertyNameType
    /// </summary>
    public class PropertyNameType
    {
        private object Parent;

        /// <summary>
        /// Gets or sets the PropInfo
        /// </summary>
        [JsonIgnore]
        public PropertyInfo PropInfo { get; private set; }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get { return PropInfo.Name; } }

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public string Type { get { return PropInfo.PropertyType.Name; } }

        /// <summary>
        /// Gets or sets the Value
        /// </summary>
        public string Value
        {
            get
            {
                if (PropInfo.CanRead)
                {
                    try
                    {
                        return PropInfo.GetValue(Parent, null).ToString();
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets or sets the CanRead
        /// </summary>
        public bool CanRead { get { return PropInfo.CanRead; } }

        /// <summary>
        /// Gets or sets the CanWrite
        /// </summary>
        public bool CanWrite { get { return PropInfo.CanWrite; } }

        /// <summary>
        /// PropertyNameType constructor
        /// </summary>
        /// <param name="info">property info</param>
        /// <param name="parent">parent object</param>
        public PropertyNameType(PropertyInfo info, object parent)
        {
            PropInfo = info;
            Parent = parent;
        }
    }

    /// <summary>
    /// Represents a MethodNameParams
    /// </summary>
    public class MethodNameParams
    {
        /// <summary>
        /// Gets or sets the MethodInfo
        /// </summary>
        [JsonIgnore]
        public MethodInfo MethodInfo { get; private set; }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get { return MethodInfo.Name; } }

        /// <summary>
        /// Gets or sets the Params
        /// </summary>
        public IEnumerable<NameType> Params
        {
            get
            {
                return MethodInfo.GetParameters().Select(p =>
                    new NameType { Name = p.Name, Type = p.ParameterType.Name });
            }
        }

        /// <summary>
        /// MethodNameParams constructor
        /// </summary>
        /// <param name="info">method info</param>
        public MethodNameParams(MethodInfo info)
        {
            MethodInfo = info;
        }
    }

    /// <summary>
    /// Represents a NameType
    /// </summary>
    public class NameType
    {
        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public string Type { get; set; }
    }

    /// <summary>
    /// Represents a ApiAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class ApiAttribute : Attribute
    {

    }
}